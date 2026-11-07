namespace LuminaBox.Application.Interface
{
    public interface IOrderStockService
    {
        /// <summary>Returns everything the order took from stock. Call it once, when the order is cancelled.</summary>
        Task RestoreAsync(Guid orderId);
    }
}
