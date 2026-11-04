namespace LuminaBox.Application.Interface
{
    public interface IWalletService
    {
        /// <summary>
        /// Credits the organizer owner's wallet for a paid Stripe order on confirm. Idempotent.
        /// </summary>
        Task SettleOrderEarningAsync(Guid orderId);

        /// <summary>
        /// Reverses a settled Stripe earning before refund. Idempotent.
        /// </summary>
        Task ReverseOrderEarningAsync(Guid orderId);
    }
}
