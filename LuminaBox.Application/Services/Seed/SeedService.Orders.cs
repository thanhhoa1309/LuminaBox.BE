using Microsoft.EntityFrameworkCore;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Services
{
    public partial class SeedService
    {
        private const decimal SeedShippingFee = 30_000m;

        private static readonly (string Code, string Email, string Brand, int Quantity, OrderStatus Status)[] SampleOrderSeeds =
        {
            ("LBDEMO0001", UserEmail, Moonlight, 2, OrderStatus.Delivered),
            ("LBDEMO0002", UserEmail, Luna, 3, OrderStatus.Delivering),
            ("LBDEMO0003", UserEmail, Moonlight, 1, OrderStatus.Confirmed),
            ("LBDEMO0004", UserEmail, TrangVang, 2, OrderStatus.Confirmed),
            ("LBDEMO0005", UserEmail, HoaSen, 1, OrderStatus.Cancelled),
            ("LBWAYBILL01", UserEmail, Moonlight, 2, OrderStatus.Confirmed),
            ("LBWAYBILL02", UserEmail, Luna, 1, OrderStatus.Confirmed),
            ("LBDEMO0006", User2Email, Moonlight, 4, OrderStatus.Pending),
            ("LBDEMO0007", User2Email, Luna, 2, OrderStatus.Confirmed),
            ("LBDEMO0008", User3Email, Moonlight, 1, OrderStatus.Delivering),
            ("LBDEMO0009", User3Email, MayTra, 2, OrderStatus.Delivered),
            ("LBDEMO0010", User4Email, HoaSen, 3, OrderStatus.Pending)
        };

        private async Task SeedSampleOrdersAsync()
        {
            foreach (var seed in SampleOrderSeeds)
                await EnsureSampleOrderAsync(seed.Code, seed.Email, seed.Brand, seed.Quantity, seed.Status);
        }

        private async Task EnsureSampleOrderAsync(string code, string email, string brand, int quantity, OrderStatus status)
        {
            if (await _unitOfWork.Orders.GetQueryable().AnyAsync(o => o.OrderCode == code))
                return;

            var buyer = await RequireUserByEmailAsync(email);
            var organizer = await RequireOrganizerByBrandAsync(brand);

            var shippingAddresses = _unitOfWork.Addresses.GetQueryable()
                .Include(a => a.Ward)
                    .ThenInclude(w => w.District)
                        .ThenInclude(d => d.Province)
                .Where(a => a.UserId == buyer.Id && a.Type == AddressType.Shipping);

            var buyerAddress = await shippingAddresses
                    .FirstOrDefaultAsync(a => a.AddressLine == User1VinhomesAddressLine)
                ?? await shippingAddresses.FirstAsync(a => a.IsDefault);

            var pickupAddress = await _unitOfWork.Addresses.GetQueryable()
                .FirstAsync(a => a.UserId == organizer.UserId && a.Type == AddressType.Pickup && a.IsDefault);

            var item = await _unitOfWork.ProductItems.GetQueryable()
                .Where(i => i.Product.OrganizerId == organizer.Id)
                .OrderBy(i => i.Name)
                .FirstAsync();

            var subtotal = item.Price * quantity;

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderCode = code,
                UserId = buyer.Id,
                OrganizerId = organizer.Id,
                AddressId = buyerAddress.Id,
                PickupAddressId = pickupAddress.Id,
                FulfillmentType = FulfillmentType.Delivery,
                ReceiverName = buyerAddress.ContactName,
                ReceiverPhone = buyerAddress.Phone,
                ShippingAddress = $"{buyerAddress.AddressLine}, {buyerAddress.Ward.Name}, {buyerAddress.Ward.District.Name}, {buyerAddress.Ward.District.Province.Name}",
                SubtotalAmount = subtotal,
                ShippingFee = SeedShippingFee,
                TotalAmount = subtotal + SeedShippingFee,
                Status = status,
                ConfirmedAt = status is OrderStatus.Confirmed or OrderStatus.Delivering or OrderStatus.Delivered ? _seedNow.AddDays(-2) : null,
                DeliveredAt = status == OrderStatus.Delivered ? _seedNow.AddDays(-1) : null,
                CancelledAt = status == OrderStatus.Cancelled ? _seedNow.AddDays(-1) : null,
                CancelReason = status == OrderStatus.Cancelled ? "Khách đổi ý" : null
            };

            await _unitOfWork.Orders.AddAsync(order);

            await _unitOfWork.OrderItems.AddAsync(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ItemType = OrderItemType.Product,
                ProductItemId = item.Id,
                ProductId = item.ProductId,
                CategoryId = item.CategoryId,
                FillingId = item.FillingId,
                Quantity = quantity,
                UnitPrice = item.Price,
                Subtotal = subtotal
            });

            await _unitOfWork.Payments.AddAsync(new Payment
            {
                Id = Guid.NewGuid(),
                Code = $"PAY{code}",
                UserId = buyer.Id,
                OrderId = order.Id,
                Amount = order.TotalAmount,
                Currency = "VND",
                Method = PaymentMethod.CashOnDelivery,
                Status = status switch
                {
                    OrderStatus.Delivered => PaymentStatus.Success,
                    OrderStatus.Cancelled => PaymentStatus.Cancelled,
                    _ => PaymentStatus.Pending
                },
                PaidAt = status == OrderStatus.Delivered ? _seedNow.AddDays(-1) : null
            });

            await AddSampleHistoryAsync(order.Id, status);

            if (status != OrderStatus.Cancelled)
            {
                var stock = await _unitOfWork.StallProductItems.GetQueryable()
                    .FirstOrDefaultAsync(s => s.ProductItemId == item.Id && s.StallId == null);

                if (stock is not null)
                    stock.StockQuantity = Math.Max(0, stock.StockQuantity - quantity);
            }

            await SaveAsync();
        }

        private async Task AddSampleHistoryAsync(Guid orderId, OrderStatus status)
        {
            var path = status == OrderStatus.Cancelled
                ? new[] { OrderStatus.Pending, OrderStatus.Cancelled }
                : new[] { OrderStatus.Pending, OrderStatus.Confirmed, OrderStatus.Delivering, OrderStatus.Delivered }
                    .TakeWhile(s => s <= status)
                    .ToArray();

            OrderStatus? previous = null;

            for (var i = 0; i < path.Length; i++)
            {
                await _unitOfWork.OrderStatusHistories.AddAsync(new OrderStatusHistory
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    FromStatus = previous,
                    ToStatus = path[i],
                    ChangedAt = _seedNow.AddDays(i - path.Length)
                });

                previous = path[i];
            }
        }
    }
}
