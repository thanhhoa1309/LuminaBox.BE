using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Interface;
using LuminaBox.Domain.Enums;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    public class OrderStockService : IOrderStockService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderStockService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task RestoreAsync(Guid orderId)
        {
            var order = await _unitOfWork.Orders.GetQueryable()
                .AsNoTracking()
                .Where(o => o.Id == orderId)
                .Select(o => new
                {
                    o.StallId,
                    Items = o.Items.Select(i => new
                    {
                        i.ItemType,
                        i.ProductItemId,
                        i.BoxItemId,
                        i.Quantity,
                        Selections = i.BoxSelections.Select(s => new { s.ProductItemId, s.Quantity }).ToList()
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (order is null)
                return;

            var productQuantities = new Dictionary<Guid, int>();
            var boxQuantities = new Dictionary<Guid, int>();

            foreach (var item in order.Items)
            {
                if (item.ItemType == OrderItemType.Product && item.ProductItemId.HasValue)
                {
                    AddQuantity(productQuantities, item.ProductItemId.Value, item.Quantity);
                    continue;
                }

                if (item.ItemType != OrderItemType.Box || !item.BoxItemId.HasValue)
                    continue;

                AddQuantity(boxQuantities, item.BoxItemId.Value, item.Quantity);

                foreach (var selection in item.Selections.Where(s => s.ProductItemId.HasValue))
                    AddQuantity(productQuantities, selection.ProductItemId!.Value, selection.Quantity * item.Quantity);
            }

            foreach (var (productItemId, quantity) in productQuantities)
            {
                var stock = await _unitOfWork.StallProductItems.GetQueryable()
                    .FirstOrDefaultAsync(s => s.ProductItemId == productItemId && s.StallId == order.StallId);

                if (stock is null)
                    continue;

                stock.StockQuantity += quantity;
                await _unitOfWork.StallProductItems.Update(stock);
            }

            foreach (var (boxItemId, quantity) in boxQuantities)
            {
                var stock = await _unitOfWork.StallBoxItems.GetQueryable()
                    .FirstOrDefaultAsync(s => s.BoxItemId == boxItemId && s.StallId == order.StallId);

                if (stock is null)
                    continue;

                stock.StockQuantity += quantity;
                await _unitOfWork.StallBoxItems.Update(stock);
            }
        }

        private static void AddQuantity(Dictionary<Guid, int> quantities, Guid id, int quantity)
            => quantities[id] = quantities.GetValueOrDefault(id) + quantity;
    }
}
