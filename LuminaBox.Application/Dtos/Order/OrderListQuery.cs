using LuminaBox.Application.Commons;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Order
{
    public class OrderListQuery : ListQuery
    {
        private DateTime? _fromDate;
        private DateTime? _toDate;

        public OrderStatus? Status { get; set; }

        public DateTime? FromDate
        {
            get => _fromDate;
            set => _fromDate = ToUtc(value);
        }

        public DateTime? ToDate
        {
            get => _toDate;
            set => _toDate = ToUtc(value);
        }

        // Npgsql only accepts UTC values for timestamptz columns
        private static DateTime? ToUtc(DateTime? value)
        {
            if (!value.HasValue)
                return null;

            return value.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                : value.Value.ToUniversalTime();
        }
    }

    public class OrganizerOrderListQuery : OrderListQuery
    {
        public Guid? StallId { get; set; }
    }
}
