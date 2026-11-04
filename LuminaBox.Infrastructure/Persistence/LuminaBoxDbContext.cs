using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Infrastructure.Persistence
{
    public class LuminaBoxDbContext : DbContext
    {
        // Unique indexes must ignore soft-deleted rows so values can be reused
        private const string NotDeletedFilter = "\"IsDeleted\" = false";

        public LuminaBoxDbContext(DbContextOptions<LuminaBoxDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Organizer> Organizers => Set<Organizer>();
        public DbSet<OrganizerRegistration> OrganizerRegistrations => Set<OrganizerRegistration>();
        public DbSet<Stall> Stalls => Set<Stall>();
        public DbSet<StallRental> StallRentals => Set<StallRental>();
        public DbSet<CakeCategory> CakeCategories => Set<CakeCategory>();
        public DbSet<Filling> Fillings => Set<Filling>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<BoxItem> BoxItems => Set<BoxItem>();
        public DbSet<BoxImage> BoxImages => Set<BoxImage>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Voucher> Vouchers => Set<Voucher>();
        public DbSet<VoucherUsage> VoucherUsages => Set<VoucherUsage>();
        public DbSet<UserVoucher> UserVouchers => Set<UserVoucher>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<OrderBoxSelection> OrderBoxSelections => Set<OrderBoxSelection>();
        public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Feedback> Feedbacks => Set<Feedback>();
        public DbSet<FeedbackImage> FeedbackImages => Set<FeedbackImage>();
        public DbSet<Commission> Commissions => Set<Commission>();
        public DbSet<Wallet> Wallets => Set<Wallet>();
        public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<EventImage> EventImages => Set<EventImage>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<EmailOtp> EmailOtps => Set<EmailOtp>();
        public DbSet<ShippingAddress> ShippingAddresses => Set<ShippingAddress>();

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<decimal>().HavePrecision(12, 2);

            configurationBuilder.Properties<UserRole>().HaveConversion<string>();
            configurationBuilder.Properties<UserStatus>().HaveConversion<string>();
            configurationBuilder.Properties<OrganizerStatus>().HaveConversion<string>();
            configurationBuilder.Properties<RegistrationStatus>().HaveConversion<string>();
            configurationBuilder.Properties<RegistrationType>().HaveConversion<string>();
            configurationBuilder.Properties<StallStatus>().HaveConversion<string>();
            configurationBuilder.Properties<RentalStatus>().HaveConversion<string>();
            configurationBuilder.Properties<ProductStatus>().HaveConversion<string>();
            configurationBuilder.Properties<OrderStatus>().HaveConversion<string>();
            configurationBuilder.Properties<OrderItemType>().HaveConversion<string>();
            configurationBuilder.Properties<PaymentMethod>().HaveConversion<string>();
            configurationBuilder.Properties<PaymentStatus>().HaveConversion<string>();
            configurationBuilder.Properties<VoucherScope>().HaveConversion<string>();
            configurationBuilder.Properties<DiscountType>().HaveConversion<string>();
            configurationBuilder.Properties<VoucherStatus>().HaveConversion<string>();
            configurationBuilder.Properties<FeedbackStatus>().HaveConversion<string>();
            configurationBuilder.Properties<CommissionStatus>().HaveConversion<string>();
            configurationBuilder.Properties<WalletTransactionType>().HaveConversion<string>();
            configurationBuilder.Properties<EventStatus>().HaveConversion<string>();
            configurationBuilder.Properties<NotificationType>().HaveConversion<string>();
            configurationBuilder.Properties<NotificationChannel>().HaveConversion<string>();
            configurationBuilder.Properties<SendStatus>().HaveConversion<string>();
            configurationBuilder.Properties<OtpPurpose>().HaveConversion<string>();
            configurationBuilder.Properties<AddressOwnerType>().HaveConversion<string>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(e =>
            {
                e.HasIndex(x => x.Email).IsUnique().HasFilter(NotDeletedFilter);
            });

            modelBuilder.Entity<Organizer>(e =>
            {
                e.HasOne(x => x.User)
                    .WithOne(u => u.Organizer)
                    .HasForeignKey<Organizer>(x => x.UserId);

                e.HasOne(x => x.Approver)
                    .WithMany()
                    .HasForeignKey(x => x.ApprovedBy);

                e.HasIndex(x => x.UserId).IsUnique().HasFilter(NotDeletedFilter);
            });

            modelBuilder.Entity<Stall>(e =>
            {
                e.HasIndex(x => x.Code).IsUnique().HasFilter(NotDeletedFilter);
            });

            modelBuilder.Entity<OrganizerRegistration>(e =>
            {
                e.Property(x => x.CommissionRate).HasPrecision(5, 2);

                e.HasOne(x => x.User)
                    .WithMany(u => u.OrganizerRegistrations)
                    .HasForeignKey(x => x.UserId);

                e.HasOne(x => x.Stall)
                    .WithMany(s => s.Registrations)
                    .HasForeignKey(x => x.StallId);

                e.HasOne(x => x.Organizer)
                    .WithMany(o => o.Registrations)
                    .HasForeignKey(x => x.OrganizerId);

                e.HasOne(x => x.Reviewer)
                    .WithMany()
                    .HasForeignKey(x => x.ReviewedBy);

                e.HasIndex(x => new { x.UserId, x.Status });
                e.HasIndex(x => new { x.StallId, x.Status });
                e.HasIndex(x => new { x.Type, x.Status });

                // Guard against duplicate submits racing past the service checks
                e.HasIndex(x => new { x.UserId, x.StallId })
                    .IsUnique()
                    .HasFilter($"\"Type\" = 'Stall' AND \"Status\" = 'Pending' AND {NotDeletedFilter}")
                    .HasDatabaseName("UX_OrganizerRegistrations_UserId_StallId_Pending");

                e.HasIndex(x => x.UserId)
                    .IsUnique()
                    .HasFilter($"\"Type\" = 'Online' AND \"Status\" IN ('Pending', 'AwaitingConfirmation', 'Approved') AND {NotDeletedFilter}")
                    .HasDatabaseName("UX_OrganizerRegistrations_UserId_OpenOnline");

                e.ToTable(t => t.HasCheckConstraint(
                    "CK_OrganizerRegistrations_Type_Stall",
                    "(\"Type\" = 'Stall' AND \"StallId\" IS NOT NULL AND \"StartDate\" IS NOT NULL AND \"EndDate\" IS NOT NULL) OR (\"Type\" = 'Online' AND \"StallId\" IS NULL AND \"StartDate\" IS NULL AND \"EndDate\" IS NULL)"));

                e.ToTable(t => t.HasCheckConstraint(
                    "CK_OrganizerRegistrations_CommissionRate_Online",
                    "\"Type\" = 'Online' OR \"CommissionRate\" IS NULL"));
            });

            modelBuilder.Entity<StallRental>(e =>
            {
                e.HasOne(x => x.Stall)
                    .WithMany(s => s.Rentals)
                    .HasForeignKey(x => x.StallId);

                e.HasOne(x => x.Organizer)
                    .WithMany(o => o.StallRentals)
                    .HasForeignKey(x => x.OrganizerId);

                e.HasOne(x => x.Registration)
                    .WithOne(r => r.StallRental)
                    .HasForeignKey<StallRental>(x => x.RegistrationId);

                e.HasIndex(x => x.RegistrationId).IsUnique().HasFilter(NotDeletedFilter);
                e.HasIndex(x => new { x.StallId, x.Status });
                e.HasIndex(x => x.OrganizerId);
            });

            modelBuilder.Entity<CakeCategory>(e =>
            {
                e.HasOne(x => x.Organizer)
                    .WithMany(o => o.CakeCategories)
                    .HasForeignKey(x => x.OrganizerId);

                e.HasIndex(x => new { x.OrganizerId, x.Name }).IsUnique().HasFilter(NotDeletedFilter);
            });

            modelBuilder.Entity<Filling>(e =>
            {
                e.HasOne(x => x.Organizer)
                    .WithMany(o => o.Fillings)
                    .HasForeignKey(x => x.OrganizerId);

                e.HasIndex(x => new { x.OrganizerId, x.Name }).IsUnique().HasFilter(NotDeletedFilter);
            });

            modelBuilder.Entity<Product>(e =>
            {
                e.HasOne(x => x.Organizer)
                    .WithMany(o => o.Products)
                    .HasForeignKey(x => x.OrganizerId);

                e.HasMany(x => x.Stalls)
                    .WithMany(s => s.Products)
                    .UsingEntity<Dictionary<string, object>>(
                        "ProductStalls",
                        j => j.HasOne<Stall>().WithMany().HasForeignKey("StallId"),
                        j => j.HasOne<Product>().WithMany().HasForeignKey("ProductId"),
                        j =>
                        {
                            j.HasKey("ProductId", "StallId");
                            j.ToTable("ProductStalls");
                        });

                e.HasIndex(x => x.Status);
                e.HasIndex(x => x.OrganizerId);
            });

            modelBuilder.Entity<ProductVariant>(e =>
            {
                e.HasOne(x => x.Product)
                    .WithMany(p => p.Variants)
                    .HasForeignKey(x => x.ProductId);

                e.HasOne(x => x.Category)
                    .WithMany(c => c.Variants)
                    .HasForeignKey(x => x.CategoryId);

                e.HasOne(x => x.Filling)
                    .WithMany(f => f.Variants)
                    .HasForeignKey(x => x.FillingId);

                e.HasIndex(x => new { x.ProductId, x.CategoryId, x.FillingId })
                    .IsUnique()
                    .HasFilter(NotDeletedFilter);

                e.HasIndex(x => x.ProductId);
                e.HasIndex(x => x.CategoryId);
                e.HasIndex(x => x.FillingId);

                e.ToTable(t => t.HasCheckConstraint("CK_ProductVariants_Price", "\"Price\" > 0"));
            });

            modelBuilder.Entity<BoxItem>(e =>
            {
                e.HasOne(x => x.Organizer)
                    .WithMany(o => o.BoxItems)
                    .HasForeignKey(x => x.OrganizerId);

                e.HasMany(x => x.Stalls)
                    .WithMany(s => s.BoxItems)
                    .UsingEntity<Dictionary<string, object>>(
                        "BoxItemStalls",
                        j => j.HasOne<Stall>().WithMany().HasForeignKey("StallId"),
                        j => j.HasOne<BoxItem>().WithMany().HasForeignKey("BoxItemId"),
                        j =>
                        {
                            j.HasKey("BoxItemId", "StallId");
                            j.ToTable("BoxItemStalls");
                        });

                e.HasMany(x => x.Products)
                    .WithMany(p => p.BoxItems)
                    .UsingEntity<Dictionary<string, object>>(
                        "BoxItemProducts",
                        j => j.HasOne<Product>().WithMany().HasForeignKey("ProductId"),
                        j => j.HasOne<BoxItem>().WithMany().HasForeignKey("BoxItemId"),
                        j =>
                        {
                            j.HasKey("BoxItemId", "ProductId");
                            j.ToTable("BoxItemProducts");
                        });

                e.HasIndex(x => x.Status);
                e.HasIndex(x => x.OrganizerId);

                e.ToTable(t => t.HasCheckConstraint("CK_BoxItems_Capacity", "\"Capacity\" > 0"));
            });

            modelBuilder.Entity<Cart>(e =>
            {
                e.HasOne(x => x.User)
                    .WithOne(u => u.Cart)
                    .HasForeignKey<Cart>(x => x.UserId);

                e.HasIndex(x => x.UserId).IsUnique().HasFilter(NotDeletedFilter);
            });

            modelBuilder.Entity<CartItem>(e =>
            {
                e.HasOne(x => x.Cart)
                    .WithMany(c => c.Items)
                    .HasForeignKey(x => x.CartId);

                e.HasOne(x => x.Product)
                    .WithMany(p => p.CartItems)
                    .HasForeignKey(x => x.ProductId);

                e.HasOne(x => x.BoxItem)
                    .WithMany(b => b.CartItems)
                    .HasForeignKey(x => x.BoxItemId);

                e.HasOne(x => x.Category)
                    .WithMany()
                    .HasForeignKey(x => x.CategoryId);

                e.HasOne(x => x.Filling)
                    .WithMany()
                    .HasForeignKey(x => x.FillingId);

                e.HasOne(x => x.Stall)
                    .WithMany(s => s.CartItems)
                    .HasForeignKey(x => x.StallId);

                e.HasIndex(x => x.CartId);
                e.HasIndex(x => x.StallId);
                e.HasIndex(x => x.CategoryId);
                e.HasIndex(x => x.FillingId);

                // Same product variant (Category×Filling) once per stall; online once per cart
                e.HasIndex(x => new { x.CartId, x.ProductId, x.CategoryId, x.FillingId, x.StallId })
                    .IsUnique()
                    .HasFilter($"{NotDeletedFilter} AND \"ProductId\" IS NOT NULL AND \"StallId\" IS NOT NULL")
                    .HasDatabaseName("IX_CartItems_CartId_Product_Variant_Stall");
                e.HasIndex(x => new { x.CartId, x.ProductId, x.CategoryId, x.FillingId })
                    .IsUnique()
                    .HasFilter($"{NotDeletedFilter} AND \"ProductId\" IS NOT NULL AND \"StallId\" IS NULL")
                    .HasDatabaseName("IX_CartItems_CartId_Product_Variant_Online");
                e.HasIndex(x => new { x.CartId, x.BoxItemId, x.StallId })
                    .IsUnique()
                    .HasFilter($"{NotDeletedFilter} AND \"BoxItemId\" IS NOT NULL AND \"StallId\" IS NOT NULL");
                e.HasIndex(x => new { x.CartId, x.BoxItemId })
                    .IsUnique()
                    .HasFilter($"{NotDeletedFilter} AND \"BoxItemId\" IS NOT NULL AND \"StallId\" IS NULL")
                    .HasDatabaseName("IX_CartItems_CartId_BoxItemId_Online");

                e.ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_CartItems_ItemType_Target",
                        "(\"ItemType\" = 'Product' AND \"ProductId\" IS NOT NULL AND \"BoxItemId\" IS NULL AND \"CategoryId\" IS NOT NULL AND \"FillingId\" IS NOT NULL) OR (\"ItemType\" = 'Box' AND \"BoxItemId\" IS NOT NULL AND \"ProductId\" IS NULL AND \"CategoryId\" IS NULL AND \"FillingId\" IS NULL)");
                    t.HasCheckConstraint("CK_CartItems_Quantity", "\"Quantity\" > 0");
                    t.HasCheckConstraint("CK_CartItems_UnitPrice", "\"UnitPrice\" > 0");
                });
            });


            modelBuilder.Entity<Voucher>(e =>
            {
                e.Property(x => x.MaxDiscountAmount).HasPrecision(12, 2);

                e.HasOne(x => x.Organizer)
                    .WithMany(o => o.Vouchers)
                    .HasForeignKey(x => x.OrganizerId);

                e.HasOne(x => x.Creator)
                    .WithMany()
                    .HasForeignKey(x => x.CreatedBy);

                e.HasIndex(x => x.Code).IsUnique().HasFilter(NotDeletedFilter);
                e.HasIndex(x => new { x.Scope, x.Status });
                e.HasIndex(x => x.OrganizerId);

                e.ToTable(t => t.HasCheckConstraint(
                    "CK_Vouchers_Scope_Organizer",
                    "(\"Scope\" = 'Platform' AND \"OrganizerId\" IS NULL) OR (\"Scope\" = 'Organizer' AND \"OrganizerId\" IS NOT NULL)"));
            });

            modelBuilder.Entity<VoucherUsage>(e =>
            {
                e.HasOne(x => x.Voucher)
                    .WithMany(v => v.Usages)
                    .HasForeignKey(x => x.VoucherId);

                e.HasOne(x => x.User)
                    .WithMany(u => u.VoucherUsages)
                    .HasForeignKey(x => x.UserId);

                e.HasOne(x => x.Order)
                    .WithMany(o => o.VoucherUsages)
                    .HasForeignKey(x => x.OrderId);

                e.HasIndex(x => new { x.VoucherId, x.UserId });
                e.HasIndex(x => new { x.VoucherId, x.OrderId }).IsUnique().HasFilter(NotDeletedFilter);
            });

            modelBuilder.Entity<UserVoucher>(e =>
            {
                e.HasOne(x => x.User)
                    .WithMany(u => u.UserVouchers)
                    .HasForeignKey(x => x.UserId);

                e.HasOne(x => x.Voucher)
                    .WithMany(v => v.UserVouchers)
                    .HasForeignKey(x => x.VoucherId);

                e.HasIndex(x => new { x.UserId, x.VoucherId }).IsUnique().HasFilter(NotDeletedFilter);
                e.HasIndex(x => x.VoucherId);
            });

            modelBuilder.Entity<Order>(e =>
            {
                e.HasOne(x => x.User)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(x => x.UserId);

                e.HasOne(x => x.Stall)
                    .WithMany(s => s.Orders)
                    .HasForeignKey(x => x.StallId);

                e.HasOne(x => x.Organizer)
                    .WithMany(o => o.Orders)
                    .HasForeignKey(x => x.OrganizerId);

                e.HasOne(x => x.ShippingAddressRef)
                    .WithMany()
                    .HasForeignKey(x => x.ShippingAddressId);

                e.Property(x => x.ShippingFee).HasPrecision(18, 2);
                e.Property(x => x.ToProvinceName).HasMaxLength(100);
                e.Property(x => x.ToWardName).HasMaxLength(100);
                e.Property(x => x.AddressDetail).HasMaxLength(500);
                e.Property(x => x.GhnOrderCode).HasMaxLength(50);
                e.Property(x => x.GhnStatus).HasMaxLength(50);

                e.HasIndex(x => x.OrderCode).IsUnique().HasFilter(NotDeletedFilter);
                e.HasIndex(x => new { x.UserId, x.Status });
                e.HasIndex(x => new { x.OrganizerId, x.Status });
                e.HasIndex(x => x.GhnOrderCode).HasFilter(NotDeletedFilter);
            });

            modelBuilder.Entity<Payment>(e =>
            {
                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId);

                e.HasOne(x => x.Order)
                    .WithOne(o => o.Payment)
                    .HasForeignKey<Payment>(x => x.OrderId);

                e.HasIndex(x => x.Code).IsUnique().HasFilter(NotDeletedFilter);
                e.HasIndex(x => x.OrderId).IsUnique().HasFilter(NotDeletedFilter);
                e.HasIndex(x => x.CheckoutSessionId).HasFilter(NotDeletedFilter);
                e.HasIndex(x => new { x.Status, x.CreatedAt });
            });

            modelBuilder.Entity<OrderItem>(e =>
            {
                e.HasOne(x => x.Order)
                    .WithMany(o => o.Items)
                    .HasForeignKey(x => x.OrderId);

                e.HasOne(x => x.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(x => x.ProductId);

                e.HasOne(x => x.BoxItem)
                    .WithMany(b => b.OrderItems)
                    .HasForeignKey(x => x.BoxItemId);

                e.HasOne(x => x.Category)
                    .WithMany()
                    .HasForeignKey(x => x.CategoryId);

                e.HasOne(x => x.Filling)
                    .WithMany()
                    .HasForeignKey(x => x.FillingId);

                e.ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_OrderItems_ItemType_Target",
                        "(\"ItemType\" = 'Product' AND \"ProductId\" IS NOT NULL AND \"BoxItemId\" IS NULL AND \"CategoryId\" IS NOT NULL AND \"FillingId\" IS NOT NULL) OR (\"ItemType\" = 'Box' AND \"BoxItemId\" IS NOT NULL AND \"ProductId\" IS NULL AND \"CategoryId\" IS NULL AND \"FillingId\" IS NULL)");
                    t.HasCheckConstraint("CK_OrderItems_Quantity", "\"Quantity\" > 0");
                });
            });

            modelBuilder.Entity<OrderBoxSelection>(e =>
            {
                e.HasOne(x => x.OrderItem)
                    .WithMany(oi => oi.BoxSelections)
                    .HasForeignKey(x => x.OrderItemId);

                e.HasOne(x => x.Product)
                    .WithMany(p => p.BoxSelections)
                    .HasForeignKey(x => x.ProductId);

                e.HasOne(x => x.Category)
                    .WithMany()
                    .HasForeignKey(x => x.CategoryId);

                e.HasOne(x => x.Filling)
                    .WithMany()
                    .HasForeignKey(x => x.FillingId);

                e.HasIndex(x => new { x.OrderItemId, x.ProductId, x.CategoryId, x.FillingId })
                    .IsUnique()
                    .HasFilter(NotDeletedFilter);

                e.ToTable(t => t.HasCheckConstraint("CK_OrderBoxSelections_Quantity", "\"Quantity\" > 0"));
            });


            modelBuilder.Entity<OrderStatusHistory>(e =>
            {
                e.Property(x => x.FromStatus).HasConversion<string>();

                e.HasOne(x => x.Order)
                    .WithMany(o => o.StatusHistories)
                    .HasForeignKey(x => x.OrderId);

                e.HasOne(x => x.Changer)
                    .WithMany()
                    .HasForeignKey(x => x.ChangedBy);

                e.HasIndex(x => x.OrderId);
            });

            modelBuilder.Entity<Feedback>(e =>
            {
                e.HasOne(x => x.OrderItem)
                    .WithOne(oi => oi.Feedback)
                    .HasForeignKey<Feedback>(x => x.OrderItemId);

                e.HasOne(x => x.User)
                    .WithMany(u => u.Feedbacks)
                    .HasForeignKey(x => x.UserId);

                e.HasOne(x => x.Organizer)
                    .WithMany(o => o.Feedbacks)
                    .HasForeignKey(x => x.OrganizerId);

                e.HasIndex(x => x.OrderItemId).IsUnique().HasFilter(NotDeletedFilter);
                e.HasIndex(x => x.OrganizerId);
                e.HasIndex(x => x.UserId);

                e.ToTable(t => t.HasCheckConstraint("CK_Feedbacks_Rating", "\"Rating\" BETWEEN 1 AND 5"));
            });

            modelBuilder.Entity<FeedbackImage>(e =>
            {
                e.HasOne(x => x.Feedback)
                    .WithMany(f => f.Images)
                    .HasForeignKey(x => x.FeedbackId);
            });

            modelBuilder.Entity<ProductImage>(e =>
            {
                e.HasOne(x => x.Product)
                    .WithMany(p => p.Images)
                    .HasForeignKey(x => x.ProductId);

                e.HasIndex(x => x.ProductId);
                e.HasIndex(x => new { x.ProductId, x.IsPrimary });
            });

            modelBuilder.Entity<BoxImage>(e =>
            {
                e.HasOne(x => x.BoxItem)
                    .WithMany(b => b.Images)
                    .HasForeignKey(x => x.BoxItemId);

                e.HasIndex(x => x.BoxItemId);
                e.HasIndex(x => new { x.BoxItemId, x.IsPrimary });
            });

            modelBuilder.Entity<Event>(e =>
            {
                e.HasOne(x => x.Organizer)
                    .WithMany(o => o.Events)
                    .HasForeignKey(x => x.OrganizerId);

                e.HasIndex(x => x.OrganizerId);
                e.HasIndex(x => x.Status);
                e.HasIndex(x => new { x.StartAt, x.EndAt });
            });

            modelBuilder.Entity<EventImage>(e =>
            {
                e.HasOne(x => x.Event)
                    .WithMany(ev => ev.Images)
                    .HasForeignKey(x => x.EventId);

                e.HasIndex(x => x.EventId);
                e.HasIndex(x => new { x.EventId, x.IsPrimary });
            });

            modelBuilder.Entity<Commission>(e =>
            {
                e.Property(x => x.Rate).HasPrecision(5, 2);

                e.HasOne(x => x.Order)
                    .WithOne(o => o.Commission)
                    .HasForeignKey<Commission>(x => x.OrderId);

                e.HasOne(x => x.Organizer)
                    .WithMany(o => o.Commissions)
                    .HasForeignKey(x => x.OrganizerId);

                e.HasIndex(x => x.OrderId).IsUnique().HasFilter(NotDeletedFilter);
            });

            modelBuilder.Entity<Wallet>(e =>
            {
                e.HasOne(x => x.User)
                    .WithOne(u => u.Wallet)
                    .HasForeignKey<Wallet>(x => x.UserId);

                e.HasIndex(x => x.UserId).IsUnique().HasFilter(NotDeletedFilter);
            });

            modelBuilder.Entity<WalletTransaction>(e =>
            {
                e.HasOne(x => x.Wallet)
                    .WithMany(w => w.Transactions)
                    .HasForeignKey(x => x.WalletId);

                e.HasOne(x => x.Order)
                    .WithMany()
                    .HasForeignKey(x => x.OrderId);

                e.HasOne(x => x.Commission)
                    .WithMany()
                    .HasForeignKey(x => x.CommissionId);

                e.HasIndex(x => x.WalletId);
                e.HasIndex(x => x.OrderId);
                e.HasIndex(x => new { x.OrderId, x.Type }).HasFilter(NotDeletedFilter);
            });

            modelBuilder.Entity<Notification>(e =>
            {
                e.HasOne(x => x.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(x => x.UserId);

                e.HasOne(x => x.Order)
                    .WithMany(o => o.Notifications)
                    .HasForeignKey(x => x.OrderId);

                e.HasOne(x => x.ActorUser)
                    .WithMany()
                    .HasForeignKey(x => x.ActorUserId);

                e.Property(x => x.EntityType).HasMaxLength(50);

                e.HasIndex(x => new { x.UserId, x.IsRead });
                e.HasIndex(x => new { x.EntityType, x.EntityId });
            });

            modelBuilder.Entity<EmailOtp>(e =>
            {
                e.HasOne(x => x.User)
                    .WithMany(u => u.EmailOtps)
                    .HasForeignKey(x => x.UserId);

                e.HasIndex(x => new { x.UserId, x.Purpose });
            });

            modelBuilder.Entity<ShippingAddress>(e =>
            {
                e.Property(x => x.Label).HasMaxLength(50);
                e.Property(x => x.ReceiverName).HasMaxLength(100);
                e.Property(x => x.ReceiverPhone).HasMaxLength(20);
                e.Property(x => x.ToProvinceName).HasMaxLength(100);
                e.Property(x => x.ToWardName).HasMaxLength(100);
                e.Property(x => x.AddressDetail).HasMaxLength(500);

                e.HasIndex(x => new { x.OwnerType, x.OwnerId, x.IsDefault });

                // Organizer and Stall may have only one active address each
                e.HasIndex(x => new { x.OwnerType, x.OwnerId })
                    .IsUnique()
                    .HasFilter($"\"OwnerType\" IN ('Organizer', 'Stall') AND {NotDeletedFilter}")
                    .HasDatabaseName("UX_ShippingAddresses_Owner_Single");
            });

            // Data is soft-deleted, so a hard delete must never silently cascade
            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }

            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                         .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var notDeleted = Expression.Lambda(
                    Expression.Not(Expression.Property(parameter, nameof(BaseEntity.IsDeleted))),
                    parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(notDeleted);
            }
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            ApplyAuditTimestamps();
            return base.SaveChanges();
        }

        // Npgsql maps DateTime to timestamptz, which only accepts UTC values
        private void ApplyAuditTimestamps()
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        if (entry.Entity.CreatedAt == default)
                        {
                            entry.Entity.CreatedAt = now;
                        }
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = now;
                        break;
                }
            }
        }
    }
}
