using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Order;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Interface
{
    public interface IOrderService
    {
        Task<List<OrderResponse>> CreateOrderAsync(CreateOrderRequest request);
        Task<List<OrderResponse>> CreateOrderFromCartAsync(CreateOrderFromCartRequest request);
        Task<Pagination<OrderSummaryResponse>> GetMyOrdersAsync(
            string? search,
            string? sortBy,
            bool isDescending,
            int page,
            int pageSize,
            OrderStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);
        Task<Pagination<OrderSummaryResponse>> GetMyOrganizerOrdersAsync(
            string? search,
            string? sortBy,
            bool isDescending,
            int page,
            int pageSize,
            OrderStatus? status = null,
            Guid? stallId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);
        Task<OrderResponse> GetOrderByDetail(Guid id);
        Task<OrderResponse> ConfirmOrderAsync(Guid id);
        Task<OrderResponse> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusRequest request);
        Task<OrderResponse> RejectOrderAsync(Guid id, RejectOrderRequest request);
        Task<OrderResponse> CancelOrderAsync(Guid id, CancelOrderRequest request);
        Task<OrderResponse> ShipOrderAsync(Guid id);
        Task<OrderResponse> MarkOrderDeliveredAsync(Guid id);
    }
}
