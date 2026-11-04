using LuminaBox.Domain.Entities;

namespace LuminaBox.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // Generic method - access any repository
        IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;

        IGenericRepository<User> Users { get; }
        IGenericRepository<Organizer> Organizers { get; }
        IGenericRepository<OrganizerRegistration> OrganizerRegistrations { get; }
        IGenericRepository<Stall> Stalls { get; }
        IGenericRepository<StallRental> StallRentals { get; }
        IGenericRepository<CakeCategory> CakeCategories { get; }
        IGenericRepository<Filling> Fillings { get; }
        IGenericRepository<Product> Products { get; }
        IGenericRepository<ProductItem> ProductItems { get; }
        IGenericRepository<ProductItemImage> ProductItemImages { get; }
        IGenericRepository<StallProductItem> StallProductItems { get; }
        IGenericRepository<StallBoxItem> StallBoxItems { get; }
        IGenericRepository<PriceTier> PriceTiers { get; }
        IGenericRepository<BoxItem> BoxItems { get; }
        IGenericRepository<BoxItemImage> BoxItemImages { get; }
        IGenericRepository<Cart> Carts { get; }
        IGenericRepository<CartItem> CartItems { get; }
        IGenericRepository<Voucher> Vouchers { get; }
        IGenericRepository<VoucherUsage> VoucherUsages { get; }
        IGenericRepository<UserVoucher> UserVouchers { get; }
        IGenericRepository<Order> Orders { get; }
        IGenericRepository<OrderItem> OrderItems { get; }
        IGenericRepository<OrderBoxSelection> OrderBoxSelections { get; }
        IGenericRepository<OrderStatusHistory> OrderStatusHistories { get; }
        IGenericRepository<Payment> Payments { get; }
        IGenericRepository<Feedback> Feedbacks { get; }
        IGenericRepository<FeedbackImage> FeedbackImages { get; }
        IGenericRepository<Commission> Commissions { get; }
        IGenericRepository<Wallet> Wallets { get; }
        IGenericRepository<WalletTransaction> WalletTransactions { get; }
        IGenericRepository<Event> Events { get; }
        IGenericRepository<EventImage> EventImages { get; }
        IGenericRepository<Notification> Notifications { get; }
        IGenericRepository<EmailOtp> EmailOtps { get; }
        IGenericRepository<Province> Provinces { get; }
        IGenericRepository<District> Districts { get; }
        IGenericRepository<Ward> Wards { get; }
        IGenericRepository<Address> Addresses { get; }

        Task<int> SaveChangesAsync();

        /// <summary>Runs the operation inside a database transaction; rolls back if it throws.</summary>
        Task<TResult> ExecuteTransactionAsync<TResult>(Func<Task<TResult>> operation);

        /// <summary>Dev-only: truncate all application tables (keeps EF migration history).</summary>
        Task TruncateAllApplicationTablesAsync();
    }
}
