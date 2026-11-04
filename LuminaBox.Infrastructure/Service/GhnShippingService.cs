using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Infrastructure.Service
{
    public class GhnShippingService : IGhnShippingService
    {
        private readonly GhnSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<GhnShippingService> _logger;

        public GhnShippingService(
            IOptions<GhnSettings> settings,
            HttpClient httpClient,
            ILogger<GhnShippingService> logger)
        {
            _settings = settings.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<decimal> CalculateFeeAsync(
            ShippingAddress fromAddress,
            ShippingAddress toAddress,
            decimal insuranceValue,
            int itemCount)
        {
            if (string.IsNullOrWhiteSpace(_settings.Token) || _settings.ShopId <= 0)
            {
                return _settings.FallbackFeeVnd > 0 ? _settings.FallbackFeeVnd : 30000m;
            }

            var weight = Math.Max(200, _settings.DefaultWeightGrams * Math.Max(1, itemCount));
            var cappedInsurance = (int)Math.Min(5000000m, Math.Max(0m, insuranceValue));

            var payload = new
            {
                service_type_id = _settings.ServiceTypeId,
                from_ward_code = fromAddress.ToWardId.ToString(),
                to_ward_code = toAddress.ToWardId.ToString(),
                weight,
                length = _settings.DefaultLengthCm,
                width = _settings.DefaultWidthCm,
                height = _settings.DefaultHeightCm,
                insurance_value = cappedInsurance
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_settings.BaseUrl.TrimEnd('/')}/shiip/public-api/v2/shipping-order/fee")
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Add("Token", _settings.Token);
            request.Headers.Add("ShopId", _settings.ShopId.ToString());

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                if (_settings.UseFallbackFee)
                {
                    _logger.LogWarning("[GHN] Fee API returned {StatusCode}, using fallback fee.", response.StatusCode);
                    return _settings.FallbackFeeVnd > 0 ? _settings.FallbackFeeVnd : 30000m;
                }

                var errorBody = await response.Content.ReadAsStringAsync();
                throw ErrorHelper.BadRequest($"Failed to calculate GHN shipping fee: {errorBody}");
            }

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            if (doc.RootElement.TryGetProperty("data", out var dataElement) &&
                dataElement.TryGetProperty("total", out var totalElement) &&
                totalElement.TryGetDecimal(out var totalFee))
            {
                return totalFee;
            }

            return _settings.FallbackFeeVnd > 0 ? _settings.FallbackFeeVnd : 30000m;
        }

        public async Task<(string orderCode, string status)> CreateShippingOrderAsync(
            Order order,
            ShippingAddress fromAddress,
            int totalItemQuantity)
        {
            if (_settings.UseFakeWaybill || string.IsNullOrWhiteSpace(_settings.Token) || _settings.ShopId <= 0)
            {
                var fakeCode = $"GHN{DateTime.UtcNow:yyMMddHHmmss}{Random.Shared.Next(100, 999)}";
                return (fakeCode, "ready_to_pick");
            }

            var codAmount = order.Payment?.Method == PaymentMethod.CashOnDelivery
                ? (int)Math.Round(order.TotalAmount, 0)
                : 0;

            var weight = Math.Max(200, _settings.DefaultWeightGrams * Math.Max(1, totalItemQuantity));

            var payload = new
            {
                payment_type_id = 1,
                note = order.Note ?? string.Empty,
                required_note = _settings.RequiredNote,
                from_name = fromAddress.ReceiverName,
                from_phone = fromAddress.ReceiverPhone,
                from_address = fromAddress.AddressDetail,
                from_ward_name = fromAddress.ToWardName,
                from_province_name = fromAddress.ToProvinceName,
                to_name = order.ReceiverName,
                to_phone = order.ReceiverPhone,
                to_address = order.AddressDetail,
                to_ward_code = order.ToWardId.ToString(),
                to_ward_name = order.ToWardName,
                to_province_name = order.ToProvinceName,
                cod_amount = codAmount,
                weight,
                length = _settings.DefaultLengthCm,
                width = _settings.DefaultWidthCm,
                height = _settings.DefaultHeightCm,
                service_type_id = _settings.ServiceTypeId,
                items = new[]
                {
                    new
                    {
                        name = $"LuminaBox order {order.OrderCode}",
                        quantity = Math.Max(1, totalItemQuantity),
                        weight
                    }
                }
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_settings.BaseUrl.TrimEnd('/')}/shiip/public-api/v2/shipping-order/create")
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Add("Token", _settings.Token);
            request.Headers.Add("ShopId", _settings.ShopId.ToString());

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw ErrorHelper.BadRequest($"Failed to create GHN shipping order: {errorBody}");
            }

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            var ghnOrderCode = doc.RootElement.GetProperty("data").GetProperty("order_code").GetString()
                ?? $"GHN{DateTime.UtcNow:yyMMddHHmmss}";

            return (ghnOrderCode, "ready_to_pick");
        }

        public async Task<string> GetShippingOrderStatusAsync(string ghnOrderCode)
        {
            if (_settings.UseFakeWaybill || string.IsNullOrWhiteSpace(_settings.Token))
            {
                return "delivering";
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_settings.BaseUrl.TrimEnd('/')}/shiip/public-api/v2/shipping-order/detail")
            {
                Content = JsonContent.Create(new { order_code = ghnOrderCode })
            };
            request.Headers.Add("Token", _settings.Token);

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                throw ErrorHelper.BadRequest($"Failed to sync GHN shipping status: {errorBody}");
            }

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            if (doc.RootElement.TryGetProperty("data", out var dataElement) &&
                dataElement.TryGetProperty("status", out var statusElement))
            {
                return statusElement.GetString() ?? "delivering";
            }

            return "delivering";
        }
    }
}

