using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Geography;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Infrastructure.Service
{
    public class GeoSyncService : IGeoSyncService
    {
        public const string GovernmentClientName = "geo-government";
        public const string GhnClientName = "geo-ghn";

        private static readonly SemaphoreSlim SyncGate = new(1, 1);

        private static readonly Regex AdministrativePrefix = new(
            @"^(thanh pho|tinh|quan|huyen|thi xa|thi tran|phuong|xa|tp)\s+",
            RegexOptions.Compiled);

        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly GhnSettings _ghn;
        private readonly ILogger<GeoSyncService> _logger;

        public GeoSyncService(
            IUnitOfWork unitOfWork,
            IHttpClientFactory httpClientFactory,
            IOptions<GhnSettings> ghn,
            ILogger<GeoSyncService> logger)
        {
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
            _ghn = ghn.Value;
            _logger = logger;
        }

        public async Task<GeoSyncReport> SyncAsync(CancellationToken cancellationToken = default)
        {
            if (!await SyncGate.WaitAsync(0, cancellationToken))
                throw ErrorHelper.Conflict("A geography sync is already running.", "E16.5");

            try
            {
                var report = new GeoSyncReport();

                await ImportGovernmentAsync(report, cancellationToken);

                if (string.IsNullOrWhiteSpace(_ghn.Token))
                {
                    report.GhnMatchingSkipped = true;
                    _logger.LogWarning("ShippingProviders:ghn:Token is empty. Skipping GHN id matching.");
                }
                else
                {
                    await MatchGhnAsync(report, cancellationToken);
                }

                report.GhnDistrictsUnmatched = await _unitOfWork.Districts.GetQueryable().CountAsync(d => d.GhnDistrictId == null, cancellationToken);
                report.GhnWardsUnmatched = await _unitOfWork.Wards.GetQueryable().CountAsync(w => w.GhnWardCode == null, cancellationToken);

                return report;
            }
            finally
            {
                SyncGate.Release();
            }
        }

        private async Task ImportGovernmentAsync(GeoSyncReport report, CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient(GovernmentClientName);
            var source = await client.GetFromJsonAsync<List<GovProvince>>("api/v1/?depth=3", cancellationToken)
                ?? throw new InvalidOperationException("Government geography source returned no data.");

            var provinces = await _unitOfWork.Provinces.GetQueryable().ToDictionaryAsync(p => p.Code, cancellationToken);
            var districts = (await _unitOfWork.Districts.GetQueryable().ToListAsync(cancellationToken))
                .ToDictionary(d => (d.ProvinceId, d.Code));
            var wards = (await _unitOfWork.Wards.GetQueryable().Select(w => new { w.DistrictId, w.Code }).ToListAsync(cancellationToken))
                .Select(w => (w.DistrictId, w.Code))
                .ToHashSet();

            foreach (var gp in source)
            {
                var provinceCode = gp.Code.ToString(CultureInfo.InvariantCulture);

                if (!provinces.TryGetValue(provinceCode, out var province))
                {
                    province = new Province { Id = Guid.NewGuid(), Name = gp.Name, Code = provinceCode };
                    await _unitOfWork.Provinces.AddAsync(province);
                    provinces[provinceCode] = province;
                    report.ProvincesAdded++;
                }

                foreach (var gd in gp.Districts ?? new())
                {
                    var districtCode = gd.Code.ToString(CultureInfo.InvariantCulture);

                    if (!districts.TryGetValue((province.Id, districtCode), out var district))
                    {
                        district = new District { Id = Guid.NewGuid(), ProvinceId = province.Id, Name = gd.Name, Code = districtCode };
                        await _unitOfWork.Districts.AddAsync(district);
                        districts[(province.Id, districtCode)] = district;
                        report.DistrictsAdded++;
                    }

                    var newWards = new List<Ward>();
                    foreach (var gw in gd.Wards ?? new())
                    {
                        var wardCode = gw.Code.ToString(CultureInfo.InvariantCulture);
                        if (wards.Add((district.Id, wardCode)))
                            newWards.Add(new Ward { Id = Guid.NewGuid(), DistrictId = district.Id, Name = gw.Name, Code = wardCode });
                    }

                    if (newWards.Count > 0)
                    {
                        await _unitOfWork.Wards.AddRangeAsync(newWards);
                        report.WardsAdded += newWards.Count;
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

        private async Task MatchGhnAsync(GeoSyncReport report, CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient(GhnClientName);

            var ghnProvinces = await FetchGhnAsync<GhnProvince>(client, "province", null, cancellationToken);
            var provinces = await _unitOfWork.Provinces.GetQueryable().Include(p => p.Districts).ToListAsync(cancellationToken);

            report.GhnProvincesMatched = AssignByName(
                provinces, ghnProvinces, p => p.Code, p => p.Name, g => g.ProvinceName, g => g.NameExtension,
                (province, match) => province.GhnProvinceId = match?.ProvinceId);

            foreach (var province in provinces.Where(p => p.GhnProvinceId is not null))
            {
                var ghnDistricts = await FetchGhnAsync<GhnDistrict>(client, "district", ("province_id", province.GhnProvinceId!.Value), cancellationToken);
                var dbDistricts = province.Districts.Where(d => !d.IsDeleted).ToList();

                report.GhnDistrictsMatched += AssignByName(
                    dbDistricts, ghnDistricts, d => d.Code, d => d.Name, g => g.DistrictName, g => g.NameExtension,
                    (district, match) => district.GhnDistrictId = match?.DistrictId);

                var districtIds = dbDistricts.Select(d => d.Id).ToList();
                var dbWards = await _unitOfWork.Wards.GetQueryable()
                    .Where(w => districtIds.Contains(w.DistrictId))
                    .ToListAsync(cancellationToken);

                foreach (var group in dbWards.GroupBy(w => w.DistrictId))
                {
                    var districtGhnId = dbDistricts.First(d => d.Id == group.Key).GhnDistrictId;
                    var ghnWards = districtGhnId is null
                        ? new List<GhnWard>()
                        : await FetchGhnAsync<GhnWard>(client, "ward", ("district_id", districtGhnId.Value), cancellationToken);

                    report.GhnWardsMatched += AssignByName(
                        group.ToList(), ghnWards, w => w.Code, w => w.Name, g => g.WardName, g => g.NameExtension,
                        (ward, match) => ward.GhnWardCode = match?.WardCode);
                }

                await _unitOfWork.SaveChangesAsync();
            }

            await _unitOfWork.SaveChangesAsync();
        }

        private static async Task<List<T>> FetchGhnAsync<T>(
            HttpClient client,
            string resource,
            (string Key, int Value)? parent,
            CancellationToken cancellationToken)
        {
            var path = $"shiip/public-api/master-data/{resource}";

            using var post = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = JsonContent.Create(parent is null
                    ? new Dictionary<string, int>()
                    : new Dictionary<string, int> { [parent.Value.Key] = parent.Value.Value })
            };
            using var response = await client.SendAsync(post, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadFromJsonAsync<GhnEnvelope<T>>(cancellationToken);
                return body?.Data ?? new();
            }

            var query = parent is null ? string.Empty : $"?{parent.Value.Key}={parent.Value.Value}";
            using var fallback = await client.GetAsync(path + query, cancellationToken);
            fallback.EnsureSuccessStatusCode();
            var fallbackBody = await fallback.Content.ReadFromJsonAsync<GhnEnvelope<T>>(cancellationToken);
            return fallbackBody?.Data ?? new();
        }

        // Primary names beat aliases, and exact names (with "Huyen", "Thi xa"...) beat stripped ones, so same-named units stay apart.
        // A GHN record is used once and entities go in a fixed order, so repeated syncs give the same ids and never share one.
        private static int AssignByName<TEntity, TGhn>(
            List<TEntity> entities,
            List<TGhn> candidates,
            Func<TEntity, string> entityCode,
            Func<TEntity, string> entityName,
            Func<TGhn, string?> primaryName,
            Func<TGhn, List<string>?> extensions,
            Action<TEntity, TGhn?> assign) where TGhn : class
        {
            if (candidates.Count == 0)
                return 0;

            var ordered = entities.OrderBy(entityCode, StringComparer.Ordinal).ToList();
            var used = new HashSet<TGhn>();
            var matched = new Dictionary<int, TGhn>();

            var passes = new (Func<string?, string> Normalize, bool UseExtensions)[]
            {
                (NormalizeFull, false), (Normalize, false), (NormalizeFull, true), (Normalize, true)
            };

            foreach (var (normalize, useExtensions) in passes)
            {
                for (var i = 0; i < ordered.Count; i++)
                {
                    if (matched.ContainsKey(i))
                        continue;

                    var target = normalize(entityName(ordered[i]));
                    if (target.Length == 0)
                        continue;

                    var match = candidates.FirstOrDefault(c => !used.Contains(c) && (useExtensions
                        ? extensions(c)?.Any(e => normalize(e) == target) == true
                        : normalize(primaryName(c)) == target));

                    if (match is null)
                        continue;

                    used.Add(match);
                    matched[i] = match;
                }
            }

            for (var i = 0; i < ordered.Count; i++)
                assign(ordered[i], matched.GetValueOrDefault(i));

            return matched.Count;
        }

        private static string Normalize(string? value)
        {
            return AdministrativePrefix.Replace(Clean(value), string.Empty).Replace(" ", string.Empty);
        }

        private static string NormalizeFull(string? value)
        {
            return Clean(value).Replace(" ", string.Empty);
        }

        private static string Clean(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var decomposed = value.Trim().ToLowerInvariant().Replace('đ', 'd').Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);
            foreach (var ch in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
                    continue;
                builder.Append(char.IsLetterOrDigit(ch) ? ch : ' ');
            }

            var cleaned = Regex.Replace(builder.ToString(), @"\s+", " ").Trim();
            return Regex.Replace(cleaned, @"0+(\d)", "$1");
        }

        private sealed class GovProvince
        {
            [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
            [JsonPropertyName("code")] public int Code { get; set; }
            [JsonPropertyName("districts")] public List<GovDistrict>? Districts { get; set; }
        }

        private sealed class GovDistrict
        {
            [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
            [JsonPropertyName("code")] public int Code { get; set; }
            [JsonPropertyName("wards")] public List<GovWard>? Wards { get; set; }
        }

        private sealed class GovWard
        {
            [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
            [JsonPropertyName("code")] public int Code { get; set; }
        }

        private sealed class GhnEnvelope<T>
        {
            [JsonPropertyName("data")] public List<T>? Data { get; set; }
        }

        private sealed class GhnProvince
        {
            [JsonPropertyName("ProvinceID")] public int ProvinceId { get; set; }
            [JsonPropertyName("ProvinceName")] public string? ProvinceName { get; set; }
            [JsonPropertyName("NameExtension")] public List<string>? NameExtension { get; set; }
        }

        private sealed class GhnDistrict
        {
            [JsonPropertyName("DistrictID")] public int DistrictId { get; set; }
            [JsonPropertyName("DistrictName")] public string? DistrictName { get; set; }
            [JsonPropertyName("NameExtension")] public List<string>? NameExtension { get; set; }
        }

        private sealed class GhnWard
        {
            [JsonPropertyName("WardCode")] public string WardCode { get; set; } = string.Empty;
            [JsonPropertyName("WardName")] public string? WardName { get; set; }
            [JsonPropertyName("NameExtension")] public List<string>? NameExtension { get; set; }
        }
    }
}
