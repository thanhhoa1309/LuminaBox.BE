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
            EnsureConfiguredOrFallback(out var useFallback);
            if (useFallback)
                return _settings.FallbackFeeVnd;

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
            AddShopIdHeader(message);

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

        public async Task<GhnCreateWaybillResponse> CreateWaybillAsync(
            GhnCreateWaybillRequest request,
            CancellationToken cancellationToken = default)
        {
            if (_settings.UseFakeWaybill)
            {
                return new GhnCreateWaybillResponse
                {
                    OrderCode = $"FAKE{DateTime.UtcNow:yyMMddHHmmss}{Random.Shared.Next(1000, 9999)}",
                    Status = "ready_to_pick"
                };
            }

            if (string.IsNullOrWhiteSpace(_settings.Token) || _settings.ShopId <= 0)
                throw ErrorHelper.BadGateway("GHN is not configured.", "E18.1");

            var client = _httpClientFactory.CreateClient(GeoSyncService.GhnClientName);

            using var message = new HttpRequestMessage(HttpMethod.Post, "shiip/public-api/v2/shipping-order/create")
            {
                Content = JsonContent.Create(new GhnCreateOrderBody
                {
                    PaymentTypeId = 1,
                    RequiredNote = _settings.RequiredNote,
                    ClientOrderCode = request.ClientOrderCode,
                    ToName = request.ToName,
                    ToPhone = request.ToPhone,
                    ToAddress = request.ToAddress,
                    ToWardCode = request.ToWardCode,
                    ToDistrictId = request.ToDistrictId,
                    FromName = request.FromName,
                    FromPhone = request.FromPhone,
                    FromAddress = request.FromAddress,
                    FromWardCode = request.FromWardCode,
                    FromDistrictId = request.FromDistrictId,
                    CodAmount = request.CodAmount,
                    Weight = request.WeightGrams > 0 ? request.WeightGrams : _settings.DefaultWeightGrams,
                    Length = request.LengthCm > 0 ? request.LengthCm : _settings.DefaultLengthCm,
                    Width = request.WidthCm > 0 ? request.WidthCm : _settings.DefaultWidthCm,
                    Height = request.HeightCm > 0 ? request.HeightCm : _settings.DefaultHeightCm,
                    ServiceTypeId = _settings.ServiceTypeId,
                    Note = request.Note,
                    Items = request.Items.Select(i => new GhnCreateOrderItemBody
                    {
                        Name = i.Name,
                        Quantity = i.Quantity,
                        Weight = i.WeightGrams > 0 ? i.WeightGrams : _settings.DefaultWeightGrams
                    }).ToList()
                })
            };
            AddShopIdHeader(message);

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(RequestTimeout);

            try
            {
                using var response = await client.SendAsync(message, timeout.Token);

                if ((int)response.StatusCode >= 500)
                {
                    _logger.LogWarning("GHN create waybill failed with status {Status}.", (int)response.StatusCode);
                    throw ErrorHelper.BadGateway("GHN is unavailable.", "E18.1");
                }

                var body = await response.Content.ReadFromJsonAsync<GhnCreateOrderEnvelope>(timeout.Token);

                if (!response.IsSuccessStatusCode || body?.Data is null || string.IsNullOrWhiteSpace(body.Data.OrderCode))
                {
                    _logger.LogWarning("GHN rejected create waybill: {Code} {Message}", body?.Code, body?.Message);
                    throw ErrorHelper.BadRequest(
                        string.IsNullOrWhiteSpace(body?.Message)
                            ? "GHN could not create the shipping order."
                            : $"GHN could not create the shipping order: {body.Message}",
                        "E18.1");
                }

                return new GhnCreateWaybillResponse
                {
                    OrderCode = body.Data.OrderCode,
                    Status = body.Data.Status
                };
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogWarning(ex, "GHN create waybill could not be completed.");
                throw ErrorHelper.BadGateway("GHN is unavailable.", "E18.1");
            }
        }

        private void EnsureConfiguredOrFallback(out bool useFallback)
        {
            useFallback = false;
            if (!string.IsNullOrWhiteSpace(_settings.Token) && _settings.ShopId > 0)
                return;

            if (_settings.UseFallbackFee)
            {
                useFallback = true;
                return;
            }

            throw ErrorHelper.BadGateway("GHN is not configured.", "E18.1");
        }

        private void AddShopIdHeader(HttpRequestMessage message)
        {
            message.Headers.Add("ShopId", _settings.ShopId.ToString(CultureInfo.InvariantCulture));
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

        private sealed class GhnCreateOrderBody
        {
            [JsonPropertyName("payment_type_id")] public int PaymentTypeId { get; set; }
            [JsonPropertyName("required_note")] public string RequiredNote { get; set; } = string.Empty;
            [JsonPropertyName("client_order_code")] public string ClientOrderCode { get; set; } = string.Empty;
            [JsonPropertyName("to_name")] public string ToName { get; set; } = string.Empty;
            [JsonPropertyName("to_phone")] public string ToPhone { get; set; } = string.Empty;
            [JsonPropertyName("to_address")] public string ToAddress { get; set; } = string.Empty;
            [JsonPropertyName("to_ward_code")] public string ToWardCode { get; set; } = string.Empty;
            [JsonPropertyName("to_district_id")] public int ToDistrictId { get; set; }
            [JsonPropertyName("from_name")] public string FromName { get; set; } = string.Empty;
            [JsonPropertyName("from_phone")] public string FromPhone { get; set; } = string.Empty;
            [JsonPropertyName("from_address")] public string FromAddress { get; set; } = string.Empty;
            [JsonPropertyName("from_ward_code")] public string FromWardCode { get; set; } = string.Empty;
            [JsonPropertyName("from_district_id")] public int FromDistrictId { get; set; }
            [JsonPropertyName("cod_amount")] public int CodAmount { get; set; }
            [JsonPropertyName("weight")] public int Weight { get; set; }
            [JsonPropertyName("length")] public int Length { get; set; }
            [JsonPropertyName("width")] public int Width { get; set; }
            [JsonPropertyName("height")] public int Height { get; set; }
            [JsonPropertyName("service_type_id")] public int ServiceTypeId { get; set; }
            [JsonPropertyName("note")] public string? Note { get; set; }
            [JsonPropertyName("items")] public List<GhnCreateOrderItemBody> Items { get; set; } = new();
        }

        private sealed class GhnCreateOrderItemBody
        {
            [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
            [JsonPropertyName("quantity")] public int Quantity { get; set; }
            [JsonPropertyName("weight")] public int Weight { get; set; }
        }

        private sealed class GhnCreateOrderEnvelope
        {
            [JsonPropertyName("code")] public int Code { get; set; }
            [JsonPropertyName("message")] public string? Message { get; set; }
            [JsonPropertyName("data")] public GhnCreateOrderData? Data { get; set; }
        }

        private sealed class GhnCreateOrderData
        {
            [JsonPropertyName("order_code")] public string OrderCode { get; set; } = string.Empty;
            [JsonPropertyName("status")] public string? Status { get; set; }
        }
    }
}
