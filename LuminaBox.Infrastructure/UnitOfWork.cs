using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Interface;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Interfaces;
using LuminaBox.Infrastructure.Persistence;
using LuminaBox.Infrastructure.Realtime;
using LuminaBox.Infrastructure.Repositories;

namespace LuminaBox.Infrastructure
{
    /// <summary>
    /// Repositories are created lazily and cached per scope, so they all share the same
    /// <see cref="LuminaBoxDbContext"/> and are committed together by <see cref="SaveChangesAsync"/>.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LuminaBoxDbContext _context;
        private readonly ICurrentTime _currentTime;
        private readonly IClaimsService _claimsService;
        private readonly SignalRRealtimePublisher _realtimePublisher;
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(
            LuminaBoxDbContext context,
            ICurrentTime currentTime,
            IClaimsService claimsService,
            SignalRRealtimePublisher realtimePublisher)
        {
            _context = context;
            _currentTime = currentTime;
            _claimsService = claimsService;
            _realtimePublisher = realtimePublisher;
        }

        public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
        {
            if (!_repositories.TryGetValue(typeof(TEntity), out var repository))
            {
                repository = new GenericRepository<TEntity>(_context, _currentTime, _claimsService);
                _repositories[typeof(TEntity)] = repository;
            }

            return (IGenericRepository<TEntity>)repository;
        }

        public IGenericRepository<User> Users => Repository<User>();
        public IGenericRepository<Organizer> Organizers => Repository<Organizer>();
        public IGenericRepository<OrganizerRegistration> OrganizerRegistrations => Repository<OrganizerRegistration>();
        public IGenericRepository<Stall> Stalls => Repository<Stall>();
        public IGenericRepository<StallRental> StallRentals => Repository<StallRental>();
        public IGenericRepository<CakeCategory> CakeCategories => Repository<CakeCategory>();
        public IGenericRepository<Filling> Fillings => Repository<Filling>();
        public IGenericRepository<Product> Products => Repository<Product>();
        public IGenericRepository<ProductVariant> ProductVariants => Repository<ProductVariant>();
        public IGenericRepository<ProductImage> ProductImages => Repository<ProductImage>();
        public IGenericRepository<BoxItem> BoxItems => Repository<BoxItem>();
        public IGenericRepository<BoxImage> BoxImages => Repository<BoxImage>();
        public IGenericRepository<Cart> Carts => Repository<Cart>();
        public IGenericRepository<CartItem> CartItems => Repository<CartItem>();
        public IGenericRepository<Voucher> Vouchers => Repository<Voucher>();
        public IGenericRepository<VoucherUsage> VoucherUsages => Repository<VoucherUsage>();
        public IGenericRepository<UserVoucher> UserVouchers => Repository<UserVoucher>();
        public IGenericRepository<Order> Orders => Repository<Order>();
        public IGenericRepository<OrderItem> OrderItems => Repository<OrderItem>();
        public IGenericRepository<OrderBoxSelection> OrderBoxSelections => Repository<OrderBoxSelection>();
        public IGenericRepository<OrderStatusHistory> OrderStatusHistories => Repository<OrderStatusHistory>();
        public IGenericRepository<Payment> Payments => Repository<Payment>();
        public IGenericRepository<Feedback> Feedbacks => Repository<Feedback>();
        public IGenericRepository<FeedbackImage> FeedbackImages => Repository<FeedbackImage>();
        public IGenericRepository<Commission> Commissions => Repository<Commission>();
        public IGenericRepository<Wallet> Wallets => Repository<Wallet>();
        public IGenericRepository<WalletTransaction> WalletTransactions => Repository<WalletTransaction>();
        public IGenericRepository<Event> Events => Repository<Event>();
        public IGenericRepository<EventImage> EventImages => Repository<EventImage>();
        public IGenericRepository<Notification> Notifications => Repository<Notification>();
        public IGenericRepository<EmailOtp> EmailOtps => Repository<EmailOtp>();
        public IGenericRepository<ShippingAddress> ShippingAddresses => Repository<ShippingAddress>();

        public async Task<int> SaveChangesAsync()
        {
            var result = await _context.SaveChangesAsync();

            // Inside a transaction the events wait for the commit in ExecuteTransactionAsync
            if (_context.Database.CurrentTransaction is null)
                await FlushAfterCommitAsync();

            return result;
        }

        private async Task FlushAfterCommitAsync()
        {
            await _realtimePublisher.FlushAsync();
        }

        public Task<TResult> ExecuteTransactionAsync<TResult>(Func<Task<TResult>> operation)
        {
            // A retrying execution strategy rejects user-initiated transactions unless they run inside it
            var strategy = _context.Database.CreateExecutionStrategy();

            return strategy.ExecuteAsync(async () =>
            {
                TResult result;

                await using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        result = await operation();
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        _context.ChangeTracker.Clear();
                        // Retries re-run the operation, which queues its events again
                        _realtimePublisher.Clear();
                        throw;
                    }
                }

                await FlushAfterCommitAsync();
                return result;
            });
        }

        public async Task TruncateAllApplicationTablesAsync()
        {
            var tables = _context.Model.GetEntityTypes()
                .Select(t => (Schema: t.GetSchema(), Table: t.GetTableName()))
                .Where(t => t.Table is not null)
                .Distinct()
                .Select(t => t.Schema is null ? $"\"{t.Table}\"" : $"\"{t.Schema}\".\"{t.Table}\"")
                .ToList();

            if (tables.Count == 0)
            {
                return;
            }

#pragma warning disable EF1002 // Table names come from the EF model, not from user input
            await _context.Database.ExecuteSqlRawAsync(
                $"TRUNCATE TABLE {string.Join(", ", tables)} RESTART IDENTITY CASCADE;");
#pragma warning restore EF1002

            _context.ChangeTracker.Clear();
        }

        // The DbContext is owned and disposed by the DI scope
        public void Dispose()
        {
            _repositories.Clear();
            GC.SuppressFinalize(this);
        }
    }
}
