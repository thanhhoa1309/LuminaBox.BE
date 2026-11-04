using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    public class PriceTier : BaseEntity
    {
        public Guid? ProductItemId { get; set; }
        public ProductItem? ProductItem { get; set; }

        public Guid? BoxItemId { get; set; }
        public BoxItem? BoxItem { get; set; }

        public OrderItemType ItemType { get; set; }
        public int MinQuantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
