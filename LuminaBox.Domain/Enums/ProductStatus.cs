namespace LuminaBox.Domain.Enums
{
    public enum ProductStatus
    {
        /// <summary>Organizer is drafting; not visible to users.</summary>
        Draft,

        /// <summary>On sale; users can order.</summary>
        Active,

        /// <summary>Hidden / stopped selling; not visible to users.</summary>
        Inactive
    }
}
