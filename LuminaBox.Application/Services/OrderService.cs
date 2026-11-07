using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Order;
using LuminaBox.Application.Dtos.Shipping;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Notifications;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    public class OrderService : IOrderService
    {
        private const int MaxPlaceOrderAttempts = 6;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;
        private readonly ICurrentTime _currentTime;
        private readonly IShippingFeeService _shippingFeeService;
        private readonly IVoucherService _voucherService;
        private readonly IPaymentService _paymentService;
        private readonly IWalletService _walletService;
        private readonly INotificationService _notificationService;
        private readonly IOrderStockService _orderStockService;

        public OrderService(
            IUnitOfWork unitOfWork,
            IClaimsService claimsService,
            ICurrentTime currentTime,
            IShippingFeeService shippingFeeService,
            IVoucherService voucherService,
            IPaymentService paymentService,
            IWalletService walletService,
            INotificationService notificationService,
            IOrderStockService orderStockService)
        {
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
            _currentTime = currentTime;
            _shippingFeeService = shippingFeeService;
            _voucherService = voucherService;
            _paymentService = paymentService;
            _walletService = walletService;
            _notificationService = notificationService;
            _orderStockService = orderStockService;
        }

        public async Task<List<OrderResponse>> CreateOrderAsync(CreateOrderRequest request)
        {
            var userId = _claimsService.GetRequiredUserId();

            return await CreateOrdersInternalAsync(
                userId: userId,
                paymentMethod: request.PaymentMethod,
                note: request.Note,
                requestedItems: request.Items,
                requestedVoucherIds: request.VoucherIds,
                cartItemsToRemove: null,
                cartSnapshotPrices: null);
        }

        public async Task<List<OrderResponse>> CreateOrderFromCartAsync(CreateOrderFromCartRequest request)
        {
            var userId = _claimsService.GetRequiredUserId();

            var cart = await _unitOfWork.Carts.GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId)
                ?? throw ErrorHelper.NotFound("Cart not found.");

            var distinctCartItemIds = request.CartItemIds.Distinct().ToList();

            var cartItems = await _unitOfWork.CartItems.GetQueryable()
                .Include(ci => ci.ProductItem)
                .Where(ci => ci.CartId == cart.Id && distinctCartItemIds.Contains(ci.Id))
                .ToListAsync();

            if (cartItems.Count != distinctCartItemIds.Count)
                throw ErrorHelper.BadRequest("One or more selected cart items were not found in your cart.");

            var requestedItems = new List<CreateOrderItemRequest>();
            var cartSnapshotPrices = new Dictionary<int, decimal>();

            for (var idx = 0; idx < cartItems.Count; idx++)
            {
                var ci = cartItems[idx];

                if (ci.ItemType == OrderItemType.Product)
                {
                    var pi = ci.ProductItem
                        ?? await _unitOfWork.ProductItems.GetByIdAsync(ci.ProductItemId!.Value)
                        ?? throw ErrorHelper.NotFound("Product item in cart no longer exists.");

                    requestedItems.Add(new CreateOrderItemRequest
                    {
                        ItemType = OrderItemType.Product,
                        ProductItemId = pi.Id,
                        Quantity = ci.Quantity,
                        StallId = ci.StallId
                    });
                    cartSnapshotPrices[idx] = ci.UnitPrice;
                }
                else
                {
                    requestedItems.Add(new CreateOrderItemRequest
                    {
                        ItemType = OrderItemType.Box,
                        BoxItemId = ci.BoxItemId,
                        Quantity = ci.Quantity,
                        StallId = ci.StallId
                    });
                    cartSnapshotPrices[idx] = ci.UnitPrice;
                }
            }

            return await CreateOrdersInternalAsync(
                userId: userId,
                paymentMethod: request.PaymentMethod,
                note: request.Note,
                requestedItems: requestedItems,
                requestedVoucherIds: request.VoucherIds,
                cartItemsToRemove: cartItems,
                cartSnapshotPrices: cartSnapshotPrices);
        }

        public async Task<Pagination<OrderSummaryResponse>> GetMyOrdersAsync(OrderListQuery filter)
        {
            var userId = _claimsService.GetRequiredUserId();

            var query = _unitOfWork.Orders.GetQueryable()
                .AsNoTracking()
                .Where(o => o.UserId == userId);

            if (filter.Status.HasValue)
                query = query.Where(o => o.Status == filter.Status.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(o => o.CreatedAt <= filter.ToDate.Value);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var keyword = filter.Search.Trim().ToLower();
                query = query.Where(o =>
                    o.OrderCode.ToLower().Contains(keyword) ||
                    o.Organizer.BrandName.ToLower().Contains(keyword) ||
                    o.ReceiverName.ToLower().Contains(keyword));
            }

            var totalCount = await query.CountAsync();

            query = filter.SortBy?.ToLower() switch
            {
                "ordercode" => filter.IsDescending ? query.OrderByDescending(o => o.OrderCode) : query.OrderBy(o => o.OrderCode),
                "totalamount" => filter.IsDescending ? query.OrderByDescending(o => o.TotalAmount) : query.OrderBy(o => o.TotalAmount),
                "status" => filter.IsDescending ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status),
                _ => filter.IsDescending ? query.OrderByDescending(o => o.CreatedAt) : query.OrderBy(o => o.CreatedAt)
            };

            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(o => new OrderSummaryResponse
                {
                    Id = o.Id,
                    OrderCode = o.OrderCode,
                    UserId = o.UserId,
                    BuyerName = o.User.FullName,
                    OrganizerId = o.OrganizerId,
                    OrganizerBrandName = o.Organizer.BrandName,
                    StallId = o.StallId,
                    StallCode = o.Stall != null ? o.Stall.Code : null,
                    StallName = o.Stall != null ? o.Stall.Name : null,
                    ReceiverName = o.ReceiverName,
                    ReceiverPhone = o.ReceiverPhone,
                    SubtotalAmount = o.SubtotalAmount,
                    PlatformDiscount = o.PlatformDiscount,
                    OrganizerDiscount = o.OrganizerDiscount,
                    ShippingFee = o.ShippingFee,
                    TotalAmount = o.TotalAmount,
                    ItemCount = o.Items.Sum(i => i.Quantity),
                    Status = o.Status,
                    PaymentMethod = o.Payment != null ? o.Payment.Method : null,
                    PaymentStatus = o.Payment != null ? o.Payment.Status : null,
                    PaymentId = o.Payment != null ? o.Payment.Id : null,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();

            return new Pagination<OrderSummaryResponse>(items, totalCount, filter.Page, filter.PageSize);
        }

        public async Task<Pagination<OrderSummaryResponse>> GetMyOrganizerOrdersAsync(OrganizerOrderListQuery filter)
        {
            var userId = _claimsService.GetRequiredUserId();

            var organizer = await _unitOfWork.Organizers.GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.UserId == userId)
                ?? throw ErrorHelper.Forbidden("Organizer profile not found.");

            var query = _unitOfWork.Orders.GetQueryable()
                .AsNoTracking()
                .Where(o => o.OrganizerId == organizer.Id);

            if (filter.Status.HasValue)
                query = query.Where(o => o.Status == filter.Status.Value);

            if (filter.StallId.HasValue)
                query = query.Where(o => o.StallId == filter.StallId.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(o => o.CreatedAt <= filter.ToDate.Value);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var keyword = filter.Search.Trim().ToLower();
                query = query.Where(o =>
                    o.OrderCode.ToLower().Contains(keyword) ||
                    o.ReceiverName.ToLower().Contains(keyword) ||
                    o.ReceiverPhone.ToLower().Contains(keyword));
            }

            var totalCount = await query.CountAsync();

            query = filter.SortBy?.ToLower() switch
            {
                "ordercode" => filter.IsDescending ? query.OrderByDescending(o => o.OrderCode) : query.OrderBy(o => o.OrderCode),
                "totalamount" => filter.IsDescending ? query.OrderByDescending(o => o.TotalAmount) : query.OrderBy(o => o.TotalAmount),
                "status" => filter.IsDescending ? query.OrderByDescending(o => o.Status) : query.OrderBy(o => o.Status),
                _ => filter.IsDescending ? query.OrderByDescending(o => o.CreatedAt) : query.OrderBy(o => o.CreatedAt)
            };

            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(o => new OrderSummaryResponse
                {
                    Id = o.Id,
                    OrderCode = o.OrderCode,
                    UserId = o.UserId,
                    BuyerName = o.User.FullName,
                    OrganizerId = o.OrganizerId,
                    OrganizerBrandName = o.Organizer.BrandName,
                    StallId = o.StallId,
                    StallCode = o.Stall != null ? o.Stall.Code : null,
                    StallName = o.Stall != null ? o.Stall.Name : null,
                    ReceiverName = o.ReceiverName,
                    ReceiverPhone = o.ReceiverPhone,
                    SubtotalAmount = o.SubtotalAmount,
                    PlatformDiscount = o.PlatformDiscount,
                    OrganizerDiscount = o.OrganizerDiscount,
                    ShippingFee = o.ShippingFee,
                    TotalAmount = o.TotalAmount,
                    ItemCount = o.Items.Sum(i => i.Quantity),
                    Status = o.Status,
                    PaymentMethod = o.Payment != null ? o.Payment.Method : null,
                    PaymentStatus = o.Payment != null ? o.Payment.Status : null,
                    PaymentId = o.Payment != null ? o.Payment.Id : null,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();

            return new Pagination<OrderSummaryResponse>(items, totalCount, filter.Page, filter.PageSize);
        }

        public async Task<OrderResponse> GetOrderByDetail(Guid id)
        {
            var userId = _claimsService.GetRequiredUserId();

            var order = await _unitOfWork.Orders.GetQueryable()
                .AsNoTracking()
                .Where(o => o.Id == id)
                .Select(o => new
                {
                    o.UserId,
                    OrganizerUserId = o.Organizer.UserId,
                    Response = new OrderResponse
                    {
                        Id = o.Id,
                        OrderCode = o.OrderCode,
                        UserId = o.UserId,
                        BuyerName = o.User.FullName,
                        BuyerEmail = o.User.Email,
                        OrganizerId = o.OrganizerId,
                        OrganizerBrandName = o.Organizer.BrandName,
                        StallId = o.StallId,
                        StallCode = o.Stall != null ? o.Stall.Code : null,
                        StallName = o.Stall != null ? o.Stall.Name : null,
                        AddressId = o.AddressId,
                        PickupAddressId = o.PickupAddressId,
                        ReceiverName = o.ReceiverName,
                        ReceiverPhone = o.ReceiverPhone,
                        ShippingAddress = o.ShippingAddress,
                        Note = o.Note,
                        SubtotalAmount = o.SubtotalAmount,
                        PlatformDiscount = o.PlatformDiscount,
                        OrganizerDiscount = o.OrganizerDiscount,
                        ShippingFee = o.ShippingFee,
                        TotalAmount = o.TotalAmount,
                        Status = o.Status,
                        CreatedAt = o.CreatedAt,
                        ConfirmedAt = o.ConfirmedAt,
                        DeliveredAt = o.DeliveredAt,
                        CancelledAt = o.CancelledAt,
                        CancelReason = o.CancelReason,
                        Payment = o.Payment == null ? null : new OrderPaymentSummaryResponse
                        {
                            Id = o.Payment.Id,
                            Code = o.Payment.Code,
                            Amount = o.Payment.Amount,
                            Currency = o.Payment.Currency,
                            Method = o.Payment.Method,
                            Status = o.Payment.Status,
                            CheckoutSessionId = o.Payment.CheckoutSessionId,
                            TransactionId = o.Payment.TransactionId,
                            PaidAt = o.Payment.PaidAt,
                            RefundedAt = o.Payment.RefundedAt
                        },
                        Items = o.Items.Select(i => new OrderItemResponse
                        {
                            Id = i.Id,
                            ItemType = i.ItemType,
                            ProductId = i.ProductId,
                            ProductName = i.Product != null ? i.Product.Name : null,
                            BoxItemId = i.BoxItemId,
                            BoxItemName = i.BoxItem != null ? i.BoxItem.Name : null,
                            CategoryId = i.CategoryId,
                            CategoryName = i.Category != null ? i.Category.Name : null,
                            FillingId = i.FillingId,
                            FillingName = i.Filling != null ? i.Filling.Name : null,
                            Quantity = i.Quantity,
                            UnitPrice = i.UnitPrice,
                            Subtotal = i.Subtotal,
                            HasFeedback = i.Feedback != null,
                            BoxSelections = i.BoxSelections.Select(bs => new OrderBoxSelectionResponse
                            {
                                Id = bs.Id,
                                ProductId = bs.ProductId,
                                ProductName = bs.Product.Name,
                                CategoryId = bs.CategoryId,
                                CategoryName = bs.Category.Name,
                                FillingId = bs.FillingId,
                                FillingName = bs.Filling.Name,
                                Quantity = bs.Quantity
                            }).ToList()
                        }).ToList(),
                        VoucherUsages = o.VoucherUsages.Select(vu => new OrderVoucherUsageResponse
                        {
                            Id = vu.Id,
                            VoucherId = vu.VoucherId,
                            VoucherCode = vu.Voucher.Code,
                            Scope = vu.Voucher.Scope,
                            DiscountAmount = vu.DiscountAmount,
                            UsedAt = vu.UsedAt
                        }).ToList(),
                        StatusHistories = o.StatusHistories
                            .OrderBy(sh => sh.ChangedAt)
                            .Select(sh => new OrderStatusHistoryResponse
                            {
                                Id = sh.Id,
                                FromStatus = sh.FromStatus,
                                ToStatus = sh.ToStatus,
                                ChangedBy = sh.ChangedBy,
                                ChangedAt = sh.ChangedAt
                            }).ToList()
                    }
                })
                .FirstOrDefaultAsync()
                ?? throw ErrorHelper.NotFound("Order not found.");

            var isManager = await _unitOfWork.Users.GetQueryable()
                .AsNoTracking()
                .AnyAsync(u => u.Id == userId && u.Role == UserRole.Manager);

            if (order.UserId != userId && order.OrganizerUserId != userId && !isManager)
                throw ErrorHelper.Forbidden("You do not have permission to view this order.");

            return order.Response;
        }

        public async Task<OrderResponse> ConfirmOrderAsync(Guid id)
        {
            var userId = _claimsService.GetRequiredUserId();
            var now = _currentTime.GetCurrentTime();

            var order = await _unitOfWork.Orders.GetQueryable()
                .Include(o => o.Organizer)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == id)
                ?? throw ErrorHelper.NotFound("Order not found.");

            if (order.Organizer.UserId != userId)
                throw ErrorHelper.Forbidden("You do not own this order.");

            if (order.Status != OrderStatus.Pending)
                throw ErrorHelper.Conflict("Only pending orders can be confirmed.");

            if (order.Payment is not null &&
                order.Payment.Method == PaymentMethod.Stripe &&
                order.Payment.Status != PaymentStatus.Success)
            {
                throw ErrorHelper.Conflict("Cannot confirm a Stripe order before payment succeeds.");
            }

            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var previousStatus = order.Status;
                order.Status = OrderStatus.Confirmed;
                order.ConfirmedAt = now;
                await _unitOfWork.Orders.Update(order);

                await _unitOfWork.OrderStatusHistories.AddAsync(new OrderStatusHistory
                {
                    OrderId = order.Id,
                    FromStatus = previousStatus,
                    ToStatus = OrderStatus.Confirmed,
                    ChangedBy = userId,
                    ChangedAt = now
                });

                await _unitOfWork.SaveChangesAsync();

                if (order.Payment is not null &&
                    order.Payment.Method == PaymentMethod.Stripe &&
                    order.Payment.Status == PaymentStatus.Success)
                {
                    await _walletService.SettleOrderEarningAsync(order.Id);
                }

                return true;
            });

            await _notificationService.SendAsync(
                NotificationCatalog.OrderConfirmed(order.UserId, order.Id, order.OrderCode, order.Organizer.BrandName));
            await _unitOfWork.SaveChangesAsync();

            return await GetOrderByDetail(order.Id);
        }

        public async Task<OrderResponse> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusRequest request)
        {
            return request.Status switch
            {
                OrderStatus.Confirmed => await ConfirmOrderAsync(id),
                OrderStatus.Delivering => await ShipOrderAsync(id),
                OrderStatus.Delivered => await MarkOrderDeliveredAsync(id),
                _ => throw ErrorHelper.BadRequest($"Use the dedicated reject or cancel endpoint to change status to '{request.Status}'.")
            };
        }

        public async Task<OrderResponse> RejectOrderAsync(Guid id, RejectOrderRequest request)
        {
            var userId = _claimsService.GetRequiredUserId();
            var now = _currentTime.GetCurrentTime();

            var order = await _unitOfWork.Orders.GetQueryable()
                .Include(o => o.Organizer)
                .Include(o => o.Payment)
                .Include(o => o.Items)
                .Include(o => o.VoucherUsages)
                    .ThenInclude(vu => vu.Voucher)
                .FirstOrDefaultAsync(o => o.Id == id)
                ?? throw ErrorHelper.NotFound("Order not found.");

            if (order.Organizer.UserId != userId)
                throw ErrorHelper.Forbidden("You do not own this order.");

            if (order.Status != OrderStatus.Pending)
                throw ErrorHelper.Conflict("Only pending orders can be rejected.");

            var isPaidStripe = order.Payment is not null &&
                               order.Payment.Method == PaymentMethod.Stripe &&
                               order.Payment.Status == PaymentStatus.Success;

            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var previousStatus = order.Status;
                order.Status = OrderStatus.Cancelled;
                order.CancelledAt = now;
                order.CancelReason = request.Reason;
                await _unitOfWork.Orders.Update(order);

                if (!isPaidStripe && order.Payment is not null && order.Payment.Status == PaymentStatus.Pending)
                {
                    order.Payment.Status = PaymentStatus.Cancelled;
                    await _unitOfWork.Payments.Update(order.Payment);
                }

                if (isPaidStripe)
                {
                    await _walletService.ReverseOrderEarningAsync(order.Id);
                }
                else
                {
                    await _voucherService.RestoreOrderVouchersAsync(order, now);
                }

                await _orderStockService.RestoreAsync(order.Id);

                await _unitOfWork.OrderStatusHistories.AddAsync(new OrderStatusHistory
                {
                    OrderId = order.Id,
                    FromStatus = previousStatus,
                    ToStatus = OrderStatus.Cancelled,
                    ChangedBy = userId,
                    ChangedAt = now
                });

            // Runs only after the cancellation is committed, so a failed save can never leave a refunded order open
            if (isPaidStripe)
            {
                await _paymentService.TryRefundOrderPaymentAsync(order.Id);
            }
            else
            {
                await _paymentService.CloseCheckoutSessionAsync(order.Id);
            }

                await _unitOfWork.SaveChangesAsync();
                return true;
            });

            await _notificationService.SendAsync(
                NotificationCatalog.OrderCancelled(
                    order.UserId,
                    order.Organizer.UserId,
                    order.Id,
                    order.OrderCode,
                    order.Organizer.BrandName,
                    cancelledByOrganizer: true,
                    reason: request.Reason));
            await _unitOfWork.SaveChangesAsync();

            return await GetOrderByDetail(order.Id);
        }

        public async Task<OrderResponse> CancelOrderAsync(Guid id, CancelOrderRequest request)
        {
            var userId = _claimsService.GetRequiredUserId();
            var now = _currentTime.GetCurrentTime();

            var order = await _unitOfWork.Orders.GetQueryable()
                .Include(o => o.Organizer)
                .Include(o => o.Payment)
                .Include(o => o.Items)
                .Include(o => o.VoucherUsages)
                    .ThenInclude(vu => vu.Voucher)
                .FirstOrDefaultAsync(o => o.Id == id)
                ?? throw ErrorHelper.NotFound("Order not found.");

            var isBuyer = order.UserId == userId;
            var isOrganizer = order.Organizer.UserId == userId;

            if (!isBuyer && !isOrganizer)
                throw ErrorHelper.Forbidden("You do not have permission to cancel this order.");

            if (isBuyer && !isOrganizer)
            {
                if (order.Status != OrderStatus.Pending)
                    throw ErrorHelper.Conflict("Buyer can only cancel pending orders.");

                if (order.Payment is not null &&
                    order.Payment.Method == PaymentMethod.Stripe &&
                    order.Payment.Status == PaymentStatus.Success)
                {
                    throw ErrorHelper.Conflict("Buyer cannot cancel an order after Stripe payment has succeeded.");
                }
            }
            else
            {
                if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
                    throw ErrorHelper.Conflict("Organizer can only cancel pending or confirmed orders.");
            }

            var isPaidStripe = order.Payment is not null &&
                               order.Payment.Method == PaymentMethod.Stripe &&
                               order.Payment.Status == PaymentStatus.Success;

            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var previousStatus = order.Status;
                order.Status = OrderStatus.Cancelled;
                order.CancelledAt = now;
                order.CancelReason = request.Reason;
                await _unitOfWork.Orders.Update(order);

                if (!isPaidStripe && order.Payment is not null && order.Payment.Status == PaymentStatus.Pending)
                {
                    order.Payment.Status = PaymentStatus.Cancelled;
                    await _unitOfWork.Payments.Update(order.Payment);
                }

                if (isPaidStripe)
                {
                    await _walletService.ReverseOrderEarningAsync(order.Id);
                }
                else
                {
                    await _voucherService.RestoreOrderVouchersAsync(order, now);
                }

                await _orderStockService.RestoreAsync(order.Id);

                await _unitOfWork.OrderStatusHistories.AddAsync(new OrderStatusHistory
                {
                    OrderId = order.Id,
                    FromStatus = previousStatus,
                    ToStatus = OrderStatus.Cancelled,
                    ChangedBy = userId,
                    ChangedAt = now
                });

            // Runs only after the cancellation is committed, so a failed save can never leave a refunded order open
            if (isPaidStripe)
            {
                await _paymentService.TryRefundOrderPaymentAsync(order.Id);
            }
            else
            {
                await _paymentService.CloseCheckoutSessionAsync(order.Id);
            }

                await _unitOfWork.SaveChangesAsync();
                return true;
            });

            await _notificationService.SendAsync(
                NotificationCatalog.OrderCancelled(
                    order.UserId,
                    order.Organizer.UserId,
                    order.Id,
                    order.OrderCode,
                    order.Organizer.BrandName,
                    cancelledByOrganizer: isOrganizer,
                    reason: request.Reason));
            await _unitOfWork.SaveChangesAsync();

            return await GetOrderByDetail(order.Id);
        }

        public async Task<OrderResponse> ShipOrderAsync(Guid id)
        {
            var userId = _claimsService.GetRequiredUserId();
            var now = _currentTime.GetCurrentTime();

            var order = await _unitOfWork.Orders.GetQueryable()
                .Include(o => o.Organizer)
                .FirstOrDefaultAsync(o => o.Id == id)
                ?? throw ErrorHelper.NotFound("Order not found.");

            if (order.Organizer.UserId != userId)
                throw ErrorHelper.Forbidden("You do not own this order.");

            if (order.Status != OrderStatus.Confirmed)
                throw ErrorHelper.Conflict("Only confirmed orders can be marked as delivering.");

            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var previousStatus = order.Status;
                order.Status = OrderStatus.Delivering;
                await _unitOfWork.Orders.Update(order);

                await _unitOfWork.OrderStatusHistories.AddAsync(new OrderStatusHistory
                {
                    OrderId = order.Id,
                    FromStatus = previousStatus,
                    ToStatus = OrderStatus.Delivering,
                    ChangedBy = userId,
                    ChangedAt = now
                });

                await _unitOfWork.SaveChangesAsync();
                return true;
            });

            await _notificationService.SendAsync(
                NotificationCatalog.OrderDelivering(order.UserId, order.Id, order.OrderCode, order.Organizer.BrandName));
            await _unitOfWork.SaveChangesAsync();

            return await GetOrderByDetail(order.Id);
        }

        public async Task<OrderResponse> MarkOrderDeliveredAsync(Guid id)
        {
            var userId = _claimsService.GetRequiredUserId();
            var now = _currentTime.GetCurrentTime();

            var order = await _unitOfWork.Orders.GetQueryable()
                .Include(o => o.Organizer)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == id)
                ?? throw ErrorHelper.NotFound("Order not found.");

            if (order.Organizer.UserId != userId)
                throw ErrorHelper.Forbidden("You do not own this order.");

            if (order.Status != OrderStatus.Delivering)
                throw ErrorHelper.Conflict("Order must be in Delivering status to be marked as delivered.");

            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var previousStatus = order.Status;
                order.Status = OrderStatus.Delivered;
                order.DeliveredAt = now;
                await _unitOfWork.Orders.Update(order);

                if (order.Payment is { Method: PaymentMethod.CashOnDelivery, Status: PaymentStatus.Pending })
                {
                    order.Payment.Status = PaymentStatus.Success;
                    order.Payment.PaidAt = now;
                    await _unitOfWork.Payments.Update(order.Payment);
                }

                await _unitOfWork.OrderStatusHistories.AddAsync(new OrderStatusHistory
                {
                    OrderId = order.Id,
                    FromStatus = previousStatus,
                    ToStatus = OrderStatus.Delivered,
                    ChangedBy = userId,
                    ChangedAt = now
                });

                await _unitOfWork.SaveChangesAsync();
                return true;
            });

            await _notificationService.SendAsync(
                NotificationCatalog.OrderDelivered(order.UserId, order.Id, order.OrderCode, order.Organizer.BrandName));
            await _unitOfWork.SaveChangesAsync();

            return await GetOrderByDetail(order.Id);
        }

        // Stock rows are versioned: when two buyers collide, the loser re-reads the stock and tries again
        private async Task<List<OrderResponse>> CreateOrdersInternalAsync(
            Guid userId,
            PaymentMethod paymentMethod,
            string? note,
            List<CreateOrderItemRequest> requestedItems,
            List<Guid>? requestedVoucherIds,
            List<CartItem>? cartItemsToRemove,
            Dictionary<int, decimal>? cartSnapshotPrices)
        {
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    return await PlaceOrdersAsync(
                        userId,
                        paymentMethod,
                        note,
                        requestedItems,
                        requestedVoucherIds,
                        cartItemsToRemove,
                        cartSnapshotPrices);
                }
                catch (DbUpdateConcurrencyException) when (attempt < MaxPlaceOrderAttempts)
                {
                    // A random wait keeps colliding buyers from retrying in lockstep
                    await Task.Delay(Random.Shared.Next(30, 120) * attempt);
                }
            }
        }

        private async Task<List<OrderResponse>> PlaceOrdersAsync(
            Guid userId,
            PaymentMethod paymentMethod,
            string? note,
            List<CreateOrderItemRequest> requestedItems,
            List<Guid>? requestedVoucherIds,
            List<CartItem>? cartItemsToRemove,
            Dictionary<int, decimal>? cartSnapshotPrices)
        {
            const FulfillmentType fulfillmentType = FulfillmentType.Delivery;
            var now = _currentTime.GetCurrentTime();

            var buyerAddress = await _unitOfWork.Addresses.GetQueryable()
                .AsNoTracking()
                .Include(a => a.Ward)
                    .ThenInclude(w => w.District)
                        .ThenInclude(d => d.Province)
                .FirstOrDefaultAsync(a => a.UserId == userId
                                       && a.Type == AddressType.Shipping
                                       && a.IsDefault)
                ?? throw ErrorHelper.BadRequest("Default shipping address not found. Please set a default shipping address before placing an order.");

            var receiverName = buyerAddress.ContactName;
            var receiverPhone = buyerAddress.Phone;
            var fullShippingAddressText =
                $"{buyerAddress.AddressLine}, {buyerAddress.Ward.Name}, {buyerAddress.Ward.District.Name}, {buyerAddress.Ward.District.Province.Name}";

            var preparedLines = new List<PreparedOrderLine>();
            var requiredProductStock = new Dictionary<(Guid ProductItemId, Guid? StallId), int>();
            var requiredBoxStock = new Dictionary<(Guid BoxItemId, Guid? StallId), int>();

            for (var itemIdx = 0; itemIdx < requestedItems.Count; itemIdx++)
            {
                var reqItem = requestedItems[itemIdx];
                var hasCartSnapshotPrice = cartSnapshotPrices is not null &&
                                           cartSnapshotPrices.TryGetValue(itemIdx, out var cartUnitPrice) &&
                                           cartUnitPrice > 0;
                var resolvedCartPrice = hasCartSnapshotPrice ? cartSnapshotPrices![itemIdx] : 0m;

                if (reqItem.ItemType == OrderItemType.Product)
                {
                    var productItem = await ResolveProductItemAsync(reqItem.ProductItemId!.Value);

                    var product = productItem.Product;

                    if (product.Status != ProductStatus.Active)
                        throw ErrorHelper.BadRequest($"Product '{product.Name}' is not active for sale.");

                    if (product.Organizer.Status != OrganizerStatus.Active)
                        throw ErrorHelper.Conflict($"Organizer '{product.Organizer.BrandName}' is currently suspended.");

                    await EnsureOnlineApprovedAsync(product.OrganizerId, product.Organizer.BrandName);

                    var activeStocks = productItem.Stocks.Where(s => s.IsActive).ToList();
                    var selectedStock = ResolveProductStock(activeStocks, reqItem.StallId, productItem.Name);
                    var resolvedStallId = selectedStock.StallId;

                    if (resolvedStallId.HasValue)
                        await EnsureActiveStallRentalAsync(product.OrganizerId, resolvedStallId.Value);

                    if (fulfillmentType == FulfillmentType.Pickup && !resolvedStallId.HasValue)
                        throw ErrorHelper.BadRequest($"Product '{product.Name}' is sold online only and cannot be picked up at a stall.");

                    var stockKey = (productItem.Id, resolvedStallId);
                    requiredProductStock[stockKey] = requiredProductStock.GetValueOrDefault(stockKey) + reqItem.Quantity;

                    var unitPrice = hasCartSnapshotPrice
                        ? resolvedCartPrice
                        : CartService.ResolveUnitPrice(productItem.Price, productItem.PriceTiers, reqItem.Quantity);

                    preparedLines.Add(new PreparedOrderLine
                    {
                        OrganizerId = product.OrganizerId,
                        OrganizerUserId = product.Organizer.UserId,
                        OrganizerBrandName = product.Organizer.BrandName,
                        StallId = resolvedStallId,
                        ItemType = OrderItemType.Product,
                        ProductItemId = productItem.Id,
                        ProductId = product.Id,
                        CategoryId = productItem.CategoryId,
                        FillingId = productItem.FillingId,
                        Quantity = reqItem.Quantity,
                        UnitPrice = unitPrice,
                        Subtotal = unitPrice * reqItem.Quantity
                    });
                }
                else
                {
                    var box = await _unitOfWork.BoxItems.GetQueryable()
                        .Include(b => b.Organizer)
                        .Include(b => b.PriceTiers)
                        .Include(b => b.Stocks)
                        .FirstOrDefaultAsync(b => b.Id == reqItem.BoxItemId)
                        ?? throw ErrorHelper.NotFound($"Box item '{reqItem.BoxItemId}' not found.");

                    if (box.Status != ProductStatus.Active)
                        throw ErrorHelper.BadRequest($"Box item '{box.Name}' is not active for sale.");

                    if (box.Organizer.Status != OrganizerStatus.Active)
                        throw ErrorHelper.Conflict($"Organizer '{box.Organizer.BrandName}' is currently suspended.");

                    await EnsureOnlineApprovedAsync(box.OrganizerId, box.Organizer.BrandName);

                    var activeBoxStocks = box.Stocks.Where(s => s.IsActive).ToList();
                    var selectedBoxStock = ResolveBoxStock(activeBoxStocks, reqItem.StallId, box.Name);
                    var resolvedStallId = selectedBoxStock.StallId;

                    if (resolvedStallId.HasValue)
                        await EnsureActiveStallRentalAsync(box.OrganizerId, resolvedStallId.Value);

                    if (fulfillmentType == FulfillmentType.Pickup && !resolvedStallId.HasValue)
                        throw ErrorHelper.BadRequest($"Box item '{box.Name}' is sold online only and cannot be picked up at a stall.");

                    var boxStockKey = (box.Id, resolvedStallId);
                    requiredBoxStock[boxStockKey] = requiredBoxStock.GetValueOrDefault(boxStockKey) + reqItem.Quantity;

                    var boxUnitPrice = hasCartSnapshotPrice
                        ? resolvedCartPrice
                        : CartService.ResolveUnitPrice(box.Price, box.PriceTiers, reqItem.Quantity);

                    preparedLines.Add(new PreparedOrderLine
                    {
                        OrganizerId = box.OrganizerId,
                        OrganizerUserId = box.Organizer.UserId,
                        OrganizerBrandName = box.Organizer.BrandName,
                        StallId = resolvedStallId,
                        ItemType = OrderItemType.Box,
                        BoxItemId = box.Id,
                        Quantity = reqItem.Quantity,
                        UnitPrice = boxUnitPrice,
                        Subtotal = boxUnitPrice * reqItem.Quantity
                    });
                }
            }

            foreach (var ((productItemId, stallId), neededQty) in requiredProductStock)
            {
                var stockRow = await _unitOfWork.StallProductItems.GetQueryable()
                    .Include(s => s.ProductItem)
                    .FirstOrDefaultAsync(s => s.ProductItemId == productItemId && s.StallId == stallId && s.IsActive)
                    ?? throw ErrorHelper.Conflict("Selected product item stock is not available.");

                if (stockRow.StockQuantity < neededQty)
                {
                    throw ErrorHelper.Conflict(
                        $"Insufficient stock for '{stockRow.ProductItem.Name}'. Required: {neededQty}, Available: {stockRow.StockQuantity}.");
                }
            }

            foreach (var ((boxItemId, stallId), neededQty) in requiredBoxStock)
            {
                var stockRow = await _unitOfWork.StallBoxItems.GetQueryable()
                    .Include(s => s.BoxItem)
                    .FirstOrDefaultAsync(s => s.BoxItemId == boxItemId && s.StallId == stallId && s.IsActive)
                    ?? throw ErrorHelper.Conflict("Selected box item stock is not available.");

                if (stockRow.StockQuantity < neededQty)
                {
                    throw ErrorHelper.Conflict(
                        $"Insufficient stock for box '{stockRow.BoxItem.Name}'. Required: {neededQty}, Available: {stockRow.StockQuantity}.");
                }
            }

            var groups = preparedLines
                .GroupBy(l => new { l.OrganizerId, l.OrganizerUserId, l.OrganizerBrandName, l.StallId })
                .ToList();

            if (paymentMethod == PaymentMethod.Stripe && groups.Count > 1)
                throw ErrorHelper.Conflict("Stripe payment only supports checking out a single organizer and stall group per checkout.");

            var groupInputs = groups
                .Select((g, idx) => (GroupIndex: idx, g.Key.OrganizerId, Subtotal: g.Sum(l => l.Subtotal)))
                .ToList();

            var voucherPlans = await _voucherService.ValidateAndCalculateOrderVouchersAsync(
                userId,
                requestedVoucherIds,
                groupInputs,
                now);

            var groupPlans = new List<PreparedOrderGroupPlan>();

            for (var idx = 0; idx < groups.Count; idx++)
            {
                var group = groups[idx];
                var subtotal = group.Sum(l => l.Subtotal);
                var voucherPlan = voucherPlans[idx];

                decimal shippingFee = 0m;
                Guid? pickupAddressId = null;

                if (fulfillmentType == FulfillmentType.Delivery)
                {
                    var shippingRequest = new ShippingFeeRequest
                    {
                        OrganizerId = group.Key.OrganizerId,
                        StallId = group.Key.StallId,
                        AddressId = buyerAddress!.Id,
                        Items = group
                            .Where(l => l.ItemType == OrderItemType.Product && l.ProductItemId.HasValue)
                            .Select(l => new ShippingItemRequest
                            {
                                ProductItemId = l.ProductItemId!.Value,
                                Quantity = l.Quantity
                            })
                            .ToList(),
                        Boxes = group
                            .Where(l => l.ItemType == OrderItemType.Box && l.BoxItemId.HasValue)
                            .Select(l => new ShippingBoxRequest
                            {
                                BoxItemId = l.BoxItemId!.Value,
                                Quantity = l.Quantity
                            })
                            .ToList()
                    };

                    var feeEstimate = await _shippingFeeService.EstimateAsync(shippingRequest);
                    shippingFee = feeEstimate.Fee;
                    pickupAddressId = feeEstimate.PickupAddressId;
                }

                var merchandiseAfterDiscount = Math.Max(0m, subtotal - voucherPlan.OrganizerDiscount - voucherPlan.PlatformDiscount);
                var totalAmount = merchandiseAfterDiscount + shippingFee;

                groupPlans.Add(new PreparedOrderGroupPlan
                {
                    OrganizerId = group.Key.OrganizerId,
                    OrganizerUserId = group.Key.OrganizerUserId,
                    OrganizerBrandName = group.Key.OrganizerBrandName,
                    StallId = group.Key.StallId,
                    PickupAddressId = pickupAddressId,
                    SubtotalAmount = subtotal,
                    OrganizerDiscount = voucherPlan.OrganizerDiscount,
                    PlatformDiscount = voucherPlan.PlatformDiscount,
                    ShippingFee = shippingFee,
                    TotalAmount = totalAmount,
                    AppliedOrganizerUserVoucher = voucherPlan.AppliedOrganizerUserVoucher,
                    AppliedPlatformUserVoucher = voucherPlan.AppliedPlatformUserVoucher,
                    Lines = group.ToList()
                });
            }

            var createdOrderIds = new List<Guid>();
            var notificationItems = new List<(Guid OrderId, string OrderCode, Guid OrganizerUserId, string BrandName)>();

            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                foreach (var ((productItemId, stallId), neededQty) in requiredProductStock)
                {
                    var stockRow = await _unitOfWork.StallProductItems.GetQueryable()
                        .FirstOrDefaultAsync(s => s.ProductItemId == productItemId && s.StallId == stallId && s.IsActive)
                        ?? throw ErrorHelper.Conflict("Selected product item stock is not available.");

                    if (stockRow.StockQuantity < neededQty)
                        throw ErrorHelper.Conflict("Insufficient product item stock.");

                    stockRow.StockQuantity -= neededQty;
                    await _unitOfWork.StallProductItems.Update(stockRow);
                }

                foreach (var ((boxItemId, stallId), neededQty) in requiredBoxStock)
                {
                    var stockRow = await _unitOfWork.StallBoxItems.GetQueryable()
                        .FirstOrDefaultAsync(s => s.BoxItemId == boxItemId && s.StallId == stallId && s.IsActive)
                        ?? throw ErrorHelper.Conflict("Selected box item stock is not available.");

                    if (stockRow.StockQuantity < neededQty)
                        throw ErrorHelper.Conflict("Insufficient box item stock.");

                    stockRow.StockQuantity -= neededQty;
                    await _unitOfWork.StallBoxItems.Update(stockRow);
                }

                foreach (var plan in groupPlans)
                {
                    var orderCode = $"ORD{now:yyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";

                    var order = new Order
                    {
                        OrderCode = orderCode,
                        UserId = userId,
                        OrganizerId = plan.OrganizerId,
                        StallId = plan.StallId,
                        AddressId = fulfillmentType == FulfillmentType.Delivery ? buyerAddress?.Id : null,
                        PickupAddressId = plan.PickupAddressId,
                        FulfillmentType = fulfillmentType,
                        ReceiverName = receiverName,
                        ReceiverPhone = receiverPhone,
                        ShippingAddress = fullShippingAddressText,
                        Note = note,
                        SubtotalAmount = plan.SubtotalAmount,
                        PlatformDiscount = plan.PlatformDiscount,
                        OrganizerDiscount = plan.OrganizerDiscount,
                        ShippingFee = plan.ShippingFee,
                        TotalAmount = plan.TotalAmount,
                        Status = OrderStatus.Pending
                    };

                    await _unitOfWork.Orders.AddAsync(order);

                    foreach (var line in plan.Lines)
                    {
                        var orderItem = new OrderItem
                        {
                            OrderId = order.Id,
                            ItemType = line.ItemType,
                            ProductItemId = line.ProductItemId,
                            ProductId = line.ProductId,
                            BoxItemId = line.BoxItemId,
                            CategoryId = line.CategoryId,
                            FillingId = line.FillingId,
                            Quantity = line.Quantity,
                            UnitPrice = line.UnitPrice,
                            Subtotal = line.Subtotal
                        };

                        await _unitOfWork.OrderItems.AddAsync(orderItem);

                    }

                    await _unitOfWork.OrderStatusHistories.AddAsync(new OrderStatusHistory
                    {
                        OrderId = order.Id,
                        FromStatus = null,
                        ToStatus = OrderStatus.Pending,
                        ChangedBy = userId,
                        ChangedAt = now
                    });

                    var payment = new Payment
                    {
                        Code = $"PAY{now:yyMMddHHmmss}{Random.Shared.Next(1000, 9999)}",
                        UserId = userId,
                        OrderId = order.Id,
                        Amount = order.TotalAmount,
                        Currency = "VND",
                        Method = paymentMethod,
                        Status = PaymentStatus.Pending
                    };
                    await _unitOfWork.Payments.AddAsync(payment);

                    if (plan.AppliedOrganizerUserVoucher is not null)
                    {
                        await _voucherService.ConsumeVoucherAsync(
                            plan.AppliedOrganizerUserVoucher,
                            order.Id,
                            userId,
                            plan.OrganizerDiscount,
                            now);
                    }

                    if (plan.AppliedPlatformUserVoucher is not null)
                    {
                        await _voucherService.ConsumeVoucherAsync(
                            plan.AppliedPlatformUserVoucher,
                            order.Id,
                            userId,
                            plan.PlatformDiscount,
                            now);
                    }

                    createdOrderIds.Add(order.Id);
                    notificationItems.Add((order.Id, order.OrderCode, plan.OrganizerUserId, plan.OrganizerBrandName));
                }

                if (cartItemsToRemove is not null && cartItemsToRemove.Count > 0)
                {
                    await _unitOfWork.CartItems.SoftRemoveRange(cartItemsToRemove);
                }

                await _unitOfWork.SaveChangesAsync();
                return true;
            });

            foreach (var item in notificationItems)
            {
                await _notificationService.SendAsync(
                    NotificationCatalog.OrderCreated(userId, item.OrganizerUserId, item.OrderId, item.OrderCode, item.BrandName));
            }

            await _unitOfWork.SaveChangesAsync();

            var responses = new List<OrderResponse>();
            foreach (var orderId in createdOrderIds)
            {
                responses.Add(await GetOrderByDetail(orderId));
            }

            return responses;
        }

        private async Task<ProductItem> ResolveProductItemAsync(Guid productItemId)
        {
            return await _unitOfWork.ProductItems.GetQueryable()
                .Include(pi => pi.Product)
                    .ThenInclude(p => p.Organizer)
                .Include(pi => pi.PriceTiers)
                .Include(pi => pi.Stocks)
                .FirstOrDefaultAsync(pi => pi.Id == productItemId)
                ?? throw ErrorHelper.NotFound($"Product item '{productItemId}' not found.");
        }

        private async Task EnsureOnlineApprovedAsync(Guid organizerId, string brandName)
        {
            var hasOnlineApproved = await _unitOfWork.OrganizerRegistrations.GetQueryable()
                .AnyAsync(r => r.OrganizerId == organizerId
                            && r.Type == RegistrationType.Online
                            && r.Status == RegistrationStatus.Approved);

            if (!hasOnlineApproved)
                throw ErrorHelper.Conflict($"Organizer '{brandName}' is not authorized for online sales.");
        }

        private async Task EnsureActiveStallRentalAsync(Guid organizerId, Guid stallId)
        {
            var hasActiveRental = await _unitOfWork.StallRentals.GetQueryable()
                .AnyAsync(r => r.OrganizerId == organizerId
                            && r.StallId == stallId
                            && r.Status == RentalStatus.Active);

            if (!hasActiveRental)
                throw ErrorHelper.Conflict("The organizer does not have an active rental at the selected stall.");
        }

        private static StallProductItem ResolveProductStock(
            List<StallProductItem> activeStocks,
            Guid? requestedStallId,
            string itemName)
        {
            if (activeStocks.Count == 0)
                throw ErrorHelper.Conflict($"'{itemName}' is not available for sale.");

            if (requestedStallId.HasValue)
            {
                return activeStocks.FirstOrDefault(s => s.StallId == requestedStallId.Value)
                    ?? throw ErrorHelper.BadRequest($"Selected stall does not sell '{itemName}'.");
            }

            var onlineStock = activeStocks.FirstOrDefault(s => s.StallId == null);
            if (onlineStock is not null)
                return onlineStock;

            if (activeStocks.Count == 1)
                return activeStocks[0];

            throw ErrorHelper.BadRequest($"StallId is required for '{itemName}'.");
        }

        private static StallBoxItem ResolveBoxStock(
            List<StallBoxItem> activeStocks,
            Guid? requestedStallId,
            string boxName)
        {
            if (activeStocks.Count == 0)
                throw ErrorHelper.Conflict($"Box '{boxName}' is not available for sale.");

            if (requestedStallId.HasValue)
            {
                return activeStocks.FirstOrDefault(s => s.StallId == requestedStallId.Value)
                    ?? throw ErrorHelper.BadRequest($"Selected stall does not sell box '{boxName}'.");
            }

            var onlineStock = activeStocks.FirstOrDefault(s => s.StallId == null);
            if (onlineStock is not null)
                return onlineStock;

            if (activeStocks.Count == 1)
                return activeStocks[0];

            throw ErrorHelper.BadRequest($"StallId is required for box '{boxName}'.");
        }

        private sealed class PreparedOrderLine
        {
            public Guid OrganizerId { get; set; }
            public Guid OrganizerUserId { get; set; }
            public string OrganizerBrandName { get; set; } = string.Empty;
            public Guid? StallId { get; set; }
            public OrderItemType ItemType { get; set; }
            public Guid? ProductItemId { get; set; }
            public Guid? ProductId { get; set; }
            public Guid? BoxItemId { get; set; }
            public Guid? CategoryId { get; set; }
            public Guid? FillingId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Subtotal { get; set; }
        }

        private sealed class PreparedOrderGroupPlan
        {
            public Guid OrganizerId { get; set; }
            public Guid OrganizerUserId { get; set; }
            public string OrganizerBrandName { get; set; } = string.Empty;
            public Guid? StallId { get; set; }
            public Guid? PickupAddressId { get; set; }
            public decimal SubtotalAmount { get; set; }
            public decimal OrganizerDiscount { get; set; }
            public decimal PlatformDiscount { get; set; }
            public decimal ShippingFee { get; set; }
            public decimal TotalAmount { get; set; }
            public UserVoucher? AppliedOrganizerUserVoucher { get; set; }
            public UserVoucher? AppliedPlatformUserVoucher { get; set; }
            public List<PreparedOrderLine> Lines { get; set; } = new();
        }
    }
}
