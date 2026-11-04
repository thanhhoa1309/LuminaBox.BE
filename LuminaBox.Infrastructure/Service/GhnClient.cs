using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Shipping;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;

namespace LuminaBox.Infrastructure.Service
{
    public class GhnClient : IGhnClient
    {
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(15);

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly GhnSettings _settings;
        private readonly ILogger<GhnClient> _logger;

        public GhnClient(IHttpClientFactory httpClientFactory, IOptions<GhnSettings> settings, ILogger<GhnClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<decimal> CalculateFeeAsync(GhnFeeRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_settings.Token))
            {
                if (_settings.UseFallbackFee)
                    return _settings.FallbackFeeVnd;

                throw ErrorHelper.BadGateway("GHN is not configured.", "E18.1");
            }

            var client = _httpClientFactory.CreateClient(GeoSyncService.GhnClientName);

            using var message = new HttpRequestMessage(HttpMethod.Post, "shiip/public-api/v2/shipping-order/fee")
            {
                Content = JsonContent.Create(new GhnFeeBody
                {
                    ServiceTypeId = _settings.ServiceTypeId,
                    FromDistrictId = request.FromDistrictId,
                    FromWardCode = request.FromWardCode,
                    ToDistrictId = request.ToDistrictId,
                    ToWardCode = request.ToWardCode,
                    Weight = request.WeightGrams,
                    Length = request.LengthCm,
                    Width = request.WidthCm,
                    Height = request.HeightCm
                })
            };
            message.Headers.Add("ShopId", _settings.ShopId.ToString(CultureInfo.InvariantCulture));

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(RequestTimeout);

            try
            {
                using var response = await client.SendAsync(message, timeout.Token);

                if ((int)response.StatusCode >= 500)
                {
                    _logger.LogWarning("GHN fee request failed with status {Status}.", (int)response.StatusCode);
                    throw ErrorHelper.BadGateway("GHN is unavailable.", "E18.1");
                }

                var body = await response.Content.ReadFromJsonAsync<GhnFeeEnvelope>(timeout.Token);

                if (!response.IsSuccessStatusCode || body?.Data is null)
                {
                    _logger.LogWarning("GHN rejected a fee request: {Message}", body?.Message);
                    throw ErrorHelper.BadRequest("GHN does not support this route.", "E18.1");
                }

                return body.Data.Total;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogWarning(ex, "GHN fee request could not be completed.");
                throw ErrorHelper.BadGateway("GHN is unavailable.", "E18.1");
            }
        }

        private sealed class GhnFeeBody
        {
            [JsonPropertyName("service_type_id")] public int ServiceTypeId { get; set; }
            [JsonPropertyName("from_district_id")] public int FromDistrictId { get; set; }
            [JsonPropertyName("from_ward_code")] public string FromWardCode { get; set; } = string.Empty;
            [JsonPropertyName("to_district_id")] public int ToDistrictId { get; set; }
            [JsonPropertyName("to_ward_code")] public string ToWardCode { get; set; } = string.Empty;
            [JsonPropertyName("weight")] public int Weight { get; set; }
            [JsonPropertyName("length")] public int Length { get; set; }
            [JsonPropertyName("width")] public int Width { get; set; }
            [JsonPropertyName("height")] public int Height { get; set; }
        }

        private sealed class GhnFeeEnvelope
        {
            [JsonPropertyName("message")] public string? Message { get; set; }
            [JsonPropertyName("data")] public GhnFeeData? Data { get; set; }
        }

        private sealed class GhnFeeData
        {
            [JsonPropertyName("total")] public decimal Total { get; set; }
        }
    }
}
