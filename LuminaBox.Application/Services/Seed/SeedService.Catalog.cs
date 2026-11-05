using Microsoft.EntityFrameworkCore;
using LuminaBox.Domain.Entities;

namespace LuminaBox.Application.Services
{
    public partial class SeedService
    {
        private static readonly (string Name, string Description)[] CakeCategorySeeds =
        {
            ("Bánh Nướng", "Traditional baked mooncakes"),
            ("Bánh Dẻo", "Soft snow-skin style cakes"),
            ("Bánh Hiện Đại", "Modern flavored mooncakes"),
            ("Mini Cake", "Bite-size cakes for gifts"),
            ("Bánh Tinh Tế", "Premium handcrafted cakes")
        };

        private static readonly (string Name, string Description)[] FillingSeeds =
        {
            ("Đậu xanh", "Classic mung bean filling"),
            ("Hạt sen", "Lotus seed filling"),
            ("Thập cẩm", "Mixed savory filling"),
            ("Chocolate", "Rich chocolate filling"),
            ("Trà xanh", "Matcha green tea filling")
        };

        private async Task SeedCakeCategoriesAsync()
        {
            foreach (var (name, description) in CakeCategorySeeds)
            {
                var exists = await _unitOfWork.CakeCategories.GetQueryable()
                    .AnyAsync(c => c.Name.ToLower() == name.ToLower());
                if (exists)
                    continue;

                await _unitOfWork.CakeCategories.AddAsync(new CakeCategory
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = description
                });
            }

            await SaveAsync();
        }

        private async Task SeedFillingsAsync()
        {
            foreach (var (name, description) in FillingSeeds)
            {
                var exists = await _unitOfWork.Fillings.GetQueryable()
                    .AnyAsync(f => f.Name.ToLower() == name.ToLower());
                if (exists)
                    continue;

                await _unitOfWork.Fillings.AddAsync(new Filling
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = description
                });
            }

            await SaveAsync();
        }
    }
}
