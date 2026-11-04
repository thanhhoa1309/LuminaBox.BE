using System.Linq.Expressions;
using LuminaBox.Application.Dtos.Product;
using LuminaBox.Domain.Entities;

namespace LuminaBox.Application.Services
{
    internal static class ProductProjections
    {
        public static readonly Expression<Func<ProductItem, ProductItemResponse>> Item = i => new ProductItemResponse
        {
            Id = i.Id,
            ProductId = i.ProductId,
            CategoryId = i.CategoryId,
            CategoryName = i.Category.Name,
            FillingId = i.FillingId,
            FillingName = i.Filling.Name,
            Name = i.Name,
            EggCount = i.EggCount,
            Weight = i.Weight,
            LengthCm = i.LengthCm,
            WidthCm = i.WidthCm,
            HeightCm = i.HeightCm,
            Price = i.Price,
            Images = i.Images
                .OrderBy(im => im.SortOrder)
                .Select(im => new ProductImageResponse { Id = im.Id, Url = im.Url, SortOrder = im.SortOrder })
                .ToList(),
            PriceTiers = i.PriceTiers
                .OrderBy(t => t.MinQuantity)
                .Select(t => new PriceTierResponse { MinQuantity = t.MinQuantity, UnitPrice = t.UnitPrice })
                .ToList(),
            Stocks = i.Stocks
                .OrderBy(s => s.StallId)
                .Select(s => new StockResponse
                {
                    Id = s.Id,
                    StallId = s.StallId,
                    StallName = s.Stall != null ? s.Stall.Name : null,
                    StockQuantity = s.StockQuantity,
                    IsActive = s.IsActive
                })
                .ToList()
        };

        public static readonly Expression<Func<Product, ProductResponse>> Detail = p => new ProductResponse
        {
            Id = p.Id,
            OrganizerId = p.OrganizerId,
            OrganizerBrandName = p.Organizer.BrandName,
            Name = p.Name,
            Description = p.Description,
            Status = p.Status,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };

        public static readonly Expression<Func<Product, ProductSummaryResponse>> Summary = p => new ProductSummaryResponse
        {
            Id = p.Id,
            OrganizerId = p.OrganizerId,
            OrganizerBrandName = p.Organizer.BrandName,
            Name = p.Name,
            Description = p.Description,
            Status = p.Status,
            ItemCount = p.Items.Count,
            MinPrice = p.Items.Min(i => (decimal?)i.Price),
            ThumbnailUrl = p.Items
                .OrderBy(i => i.CreatedAt)
                .SelectMany(i => i.Images)
                .OrderBy(im => im.SortOrder)
                .Select(im => im.Url)
                .FirstOrDefault(),
            CreatedAt = p.CreatedAt
        };
    }
}
