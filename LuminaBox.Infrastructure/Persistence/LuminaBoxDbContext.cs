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
        public DbSet<ProductItem> ProductItems => Set<ProductItem>();
        public DbSet<ProductItemImage> ProductItemImages => Set<ProductItemImage>();
        public DbSet<StallProductItem> StallProductItems => Set<StallProductItem>();
        public DbSet<StallBoxItem> StallBoxItems => Set<StallBoxItem>();
        public DbSet<PriceTier> PriceTiers => Set<PriceTier>();
        public DbSet<BoxItem> BoxItems => Set<BoxItem>();
        public DbSet<BoxItemImage> BoxItemImages => Set<BoxItemImage>();
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
        public DbSet<Province> Provinces => Set<Province>();
        public DbSet<District> Districts => Set<District>();
        public DbSet<Ward> Wards => Set<Ward>();
        public DbSet<Address> Addresses => Set<Address>();

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
            configurationBuilder.Properties<FulfillmentType>().HaveConversion<string>();
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
            configurationBuilder.Properties<AddressType>().HaveConversion<string>();        }

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

            modelBuilder.Entity<Province>(e =>
            {
                e.Property(x => x.Name).HasMaxLength(100);
                e.Property(x => x.Code).HasMaxLength(20);

                e.HasIndex(x => x.Code).IsUnique().HasFilter(NotDeletedFilter);
            });

            modelBuilder.Entity<District>(e =>
            {
                e.HasOne(x => x.Province)
                    .WithMany(p => p.Districts)
                    .HasForeignKey(x => x.ProvinceId);

                e.Property(x => x.Name).HasMaxLength(100);
                e.Property(x => x.Code).HasMaxLength(20);

                e.HasIndex(x => new { x.ProvinceId, x.Code }).IsUnique().HasFilter(NotDeletedFilter);
            });

            modelBuilder.Entity<Ward>(e =>
            {
                e.HasOne(x => x.District)
                    .WithMany(d => d.Wards)
                    .HasForeignKey(x => x.DistrictId);

                e.Property(x => x.Name).HasMaxLength(100);
                e.Property(x => x.Code).HasMaxLength(20);
                e.Property(x => x.GhnWardCode).HasMaxLength(20);

                e.HasIndex(x => new { x.DistrictId, x.Code }).IsUnique().HasFilter(NotDeletedFilter);
            });

            modelBuilder.Entity<Address>(e =>
            {
                e.HasOne(x => x.User)
                    .WithMany()
                    .HasForeignKey(x => x.UserId);

                e.HasOne(x => x.Ward)
                    .WithMany()
                    .HasForeignKey(x => x.WardId);

                e.Property(x => x.ContactName).HasMaxLength(100);
                e.Property(x => x.Phone).HasMaxLength(20);
                e.Property(x => x.AddressLine).HasMaxLength(500);
                e.Property(x => x.Latitude).HasPrecision(9, 6);
                e.Property(x => x.Longitude).HasPrecision(9, 6);
                e.Property(x => x.MapRefId).HasMaxLength(255);
                e.Property(x => x.NewProvinceName).HasMaxLength(100);
                e.Property(x => x.NewWardName).HasMaxLength(100);

                e.HasIndex(x => x.UserId);

                e.HasIndex(x => new { x.UserId, x.Type })
                    .IsUnique()
                    .HasFilter($"\"IsDefault\" = true AND {NotDeletedFilter}")
                    .HasDatabaseName("UX_Addresses_User_Type_Default");
            });

            modelBuilder.Entity<Stall>(e =>
            {
                e.HasOne(x => x.Ward)
                    .WithMany()
                    .HasForeignKey(x => x.WardId);

                e.Property(x => x.AddressLine).HasMaxLength(500);
                e.Property(x => x.Latitude).HasPrecision(9, 6);
                e.Property(x => x.Longitude).HasPrecision(9, 6);
                e.Property(x => x.MapRefId).HasMaxLength(255);
                e.Property(x => x.NewProvinceName).HasMaxLength(100);
                e.Property(x => x.NewWardName).HasMaxLength(100);

                e.HasIndex(x => x.Code).IsUnique().HasFilter(NotDeletedFilter);
                e.HasIndex(x => x.WardId);
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
                e.Property(x => x.Name).HasMaxLength(100);
                e.Property(x => x.Description).HasMaxLength(500);

                e.HasIndex(x => x.Name).IsUnique().HasFilter(NotDeletedFilter);
            });

            modelBuilder.Entity<Filling>(e =>
            {
                e.Property(x => x.Name).HasMaxLength(100);
                e.Property(x => x.Description).HasMaxLength(500);

                e.HasIndex(x => x.Name).IsUnique().HasFilter(NotDeletedFilter);
            });

            modelBuilder.Entity<Product>(e =>
            {
                e.HasOne(x => x.Organizer)
                    .WithMany(o => o.Products)
                    .HasForeignKey(x => x.OrganizerId);

                e.Property(x => x.Name).HasMaxLength(200);

                e.HasIndex(x => x.Status);
                e.HasIndex(x => x.OrganizerId);
            });

            modelBuilder.Entity<ProductItem>(e =>
            {
                e.HasOne(x => x.Product)
                    .WithMany(p => p.Items)
                    .HasForeignKey(x => x.ProductId);

                e.HasOne(x => x.Category)
                    .WithMany(c => c.ProductItems)
                    .HasForeignKey(x => x.CategoryId);

                e.HasOne(x => x.Filling)
                    .WithMany(f => f.ProductItems)
                    .HasForeignKey(x => x.FillingId);

                e.Property(x => x.Name).HasMaxLength(200);
                e.Property(x => x.LengthCm).HasPrecision(6, 2);
                e.Property(x => x.WidthCm).HasPrecision(6, 2);
                e.Property(x => x.HeightCm).HasPrecision(6, 2);

                e.HasIndex(x => new { x.ProductId, x.CategoryId, x.FillingId, x.EggCount, x.Weight })
                    .IsUnique()
                    .HasFilter(NotDeletedFilter);

                e.HasIndex(x => x.CategoryId);
                e.HasIndex(x => x.FillingId);

                e.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_ProductItems_Price", "\"Price\" > 0");
                    t.HasCheckConstraint("CK_ProductItems_EggCount", "\"EggCount\" >= 0");
                    t.HasCheckConstraint("CK_ProductItems_Weight", "\"Weight\" > 0");
                    t.HasCheckConstraint("CK_ProductItems_Dimensions", "\"LengthCm\" > 0 AND \"WidthCm\" > 0 AND \"HeightCm\" > 0");
                });
            });

            modelBuilder.Entity<BoxItem>(e =>
            {
                e.HasOne(x => x.Organizer)
                    .WithMany(o => o.BoxItems)
                    .HasForeignKey(x => x.OrganizerId);

                e.Property(x => x.Name).HasMaxLength(200);
                e.Property(x => x.LengthCm).HasPrecision(6, 2);
                e.Property(x => x.WidthCm).HasPrecision(6, 2);
                e.Property(x => x.HeightCm).HasPrecision(6, 2);

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

                e.HasOne(x => x.ProductItem)
                    .WithMany()
                    .HasForeignKey(x => x.ProductItemId);

                e.HasOne(x => x.BoxItem)
                    .WithMany()
                    .HasForeignKey(x => x.BoxItemId);

                e.HasOne(x => x.Stall)
                    .WithMany(s => s.CartItems)
                    .HasForeignKey(x => x.StallId);

                e.HasIndex(x => x.CartId);
                e.HasIndex(x => x.StallId);

                // Same product/box can appear once per stall; online (StallId null) once per cart
                e.HasIndex(x => new { x.CartId, x.ProductItemId, x.StallId })
                    .IsUnique()
                    .HasFilter($"{NotDeletedFilter} AND \"ProductItemId\" IS NOT NULL AND \"StallId\" IS NOT NULL");
                e.HasIndex(x => new { x.CartId, x.ProductItemId })
                    .IsUnique()
                    .HasFilter($"{NotDeletedFilter} AND \"ProductItemId\" IS NOT NULL AND \"StallId\" IS NULL")
                    .HasDatabaseName("IX_CartItems_CartId_ProductItemId_Online");
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
                        "(\"ItemType\" = 'Product' AND \"ProductItemId\" IS NOT NULL AND \"BoxItemId\" IS NULL) OR (\"ItemType\" = 'Box' AND \"BoxItemId\" IS NOT NULL AND \"ProductItemId\" IS NULL)");
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

                e.HasOne(x => x.Address)
                    .WithMany()
                    .HasForeignKey(x => x.AddressId);

                e.HasOne(x => x.PickupAddress)
                    .WithMany()
                    .HasForeignKey(x => x.PickupAddressId);

                e.Property(x => x.ShippingFee).HasPrecision(18, 2);
                e.Property(x => x.GhnOrderCode).HasMaxLength(50);
                e.Property(x => x.GhnStatus).HasMaxLength(50);

                e.HasIndex(x => x.OrderCode).IsUnique().HasFilter(NotDeletedFilter);
                e.HasIndex(x => x.GhnOrderCode);
                e.HasIndex(x => new { x.UserId, x.Status });
                e.HasIndex(x => new { x.OrganizerId, x.Status });
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
                    .WithMany()
                    .HasForeignKey(x => x.ProductId);

                e.HasOne(x => x.ProductItem)
                    .WithMany()
                    .HasForeignKey(x => x.ProductItemId);

                e.HasOne(x => x.BoxItem)
                    .WithMany()
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
                    .WithMany()
                    .HasForeignKey(x => x.ProductId);

                e.HasOne(x => x.Category)
                    .WithMany()
                    .HasForeignKey(x => x.CategoryId);

                e.HasOne(x => x.Filling)
                    .WithMany()
                    .HasForeignKey(x => x.FillingId);

                e.HasOne(x => x.ProductItem)
                    .WithMany()
                    .HasForeignKey(x => x.ProductItemId);

                e.HasIndex(x => new { x.OrderItemId, x.ProductItemId })
                    .IsUnique()
                    .HasFilter(NotDeletedFilter);

                e.ToTable(t => t.HasCheckConstraint("CK_OrderBoxSelections_Quantity", "\"Quantity\" > 0"));
            });


            modelBuilder.Entity<StallProductItem>().Property(x => x.Version).IsRowVersion();
            modelBuilder.Entity<StallBoxItem>().Property(x => x.Version).IsRowVersion();
            modelBuilder.Entity<Wallet>().Property(x => x.Version).IsRowVersion();

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

            modelBuilder.Entity<ProductItemImage>(e =>
            {
                e.HasOne(x => x.ProductItem)
                    .WithMany(i => i.Images)
                    .HasForeignKey(x => x.ProductItemId);

                e.Property(x => x.Url).HasMaxLength(1000);

                e.HasIndex(x => new { x.ProductItemId, x.SortOrder });
            });

            modelBuilder.Entity<BoxItemImage>(e =>
            {
                e.HasOne(x => x.BoxItem)
                    .WithMany(b => b.Images)
                    .HasForeignKey(x => x.BoxItemId);

                e.Property(x => x.Url).HasMaxLength(1000);

                e.HasIndex(x => new { x.BoxItemId, x.SortOrder });
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

            modelBuilder.Entity<StallProductItem>(e =>
            {
                e.HasOne(x => x.Stall)
                    .WithMany(s => s.StallProductItems)
                    .HasForeignKey(x => x.StallId);

                e.HasOne(x => x.ProductItem)
                    .WithMany(i => i.Stocks)
                    .HasForeignKey(x => x.ProductItemId);

                e.HasIndex(x => new { x.StallId, x.ProductItemId })
                    .IsUnique()
                    .HasFilter($"{NotDeletedFilter} AND \"StallId\" IS NOT NULL");

                e.HasIndex(x => x.ProductItemId)
                    .IsUnique()
                    .HasFilter($"{NotDeletedFilter} AND \"StallId\" IS NULL")
                    .HasDatabaseName("UX_StallProductItems_ProductItem_Online");

                e.ToTable(t => t.HasCheckConstraint("CK_StallProductItems_StockQuantity", "\"StockQuantity\" >= 0"));
            });

            modelBuilder.Entity<StallBoxItem>(e =>
            {
                e.HasOne(x => x.Stall)
                    .WithMany(s => s.StallBoxItems)
                    .HasForeignKey(x => x.StallId);

                e.HasOne(x => x.BoxItem)
                    .WithMany(b => b.Stocks)
                    .HasForeignKey(x => x.BoxItemId);

                e.HasIndex(x => new { x.StallId, x.BoxItemId })
                    .IsUnique()
                    .HasFilter($"{NotDeletedFilter} AND \"StallId\" IS NOT NULL");

                e.HasIndex(x => x.BoxItemId)
                    .IsUnique()
                    .HasFilter($"{NotDeletedFilter} AND \"StallId\" IS NULL")
                    .HasDatabaseName("UX_StallBoxItems_BoxItem_Online");

                e.ToTable(t => t.HasCheckConstraint("CK_StallBoxItems_StockQuantity", "\"StockQuantity\" >= 0"));
            });

            modelBuilder.Entity<PriceTier>(e =>
            {
                e.HasOne(x => x.ProductItem)
                    .WithMany(i => i.PriceTiers)
                    .HasForeignKey(x => x.ProductItemId);

                e.HasOne(x => x.BoxItem)
                    .WithMany(b => b.PriceTiers)
                    .HasForeignKey(x => x.BoxItemId);

                e.HasIndex(x => new { x.ProductItemId, x.MinQuantity })
                    .IsUnique()
                    .HasFilter($"{NotDeletedFilter} AND \"ProductItemId\" IS NOT NULL");

                e.HasIndex(x => new { x.BoxItemId, x.MinQuantity })
                    .IsUnique()
                    .HasFilter($"{NotDeletedFilter} AND \"BoxItemId\" IS NOT NULL");

                e.ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_PriceTiers_ItemType_Target",
                        "(\"ItemType\" = 'Product' AND \"ProductItemId\" IS NOT NULL AND \"BoxItemId\" IS NULL) OR (\"ItemType\" = 'Box' AND \"BoxItemId\" IS NOT NULL AND \"ProductItemId\" IS NULL)");
                    t.HasCheckConstraint("CK_PriceTiers_MinQuantity", "\"MinQuantity\" >= 2");
                    t.HasCheckConstraint("CK_PriceTiers_UnitPrice", "\"UnitPrice\" > 0");
                });
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
