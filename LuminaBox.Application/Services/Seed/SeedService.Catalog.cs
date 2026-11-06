using Microsoft.EntityFrameworkCore;
using LuminaBox.Domain.Entities;

namespace LuminaBox.Application.Services
{
    public partial class SeedService
    {
        private static readonly (string Name, string Description)[] CakeCategorySeeds =
        {
            ("Bánh Nướng", "Bánh nướng truyền thống"),
            ("Bánh Dẻo", "Bánh dẻo vỏ mềm"),
            ("Bánh Hiện Đại", "Bánh trung thu vị mới"),
            ("Mini Cake", "Bánh nhỏ làm quà"),
            ("Bánh Tinh Tế", "Dòng thủ công cao cấp")
        };

        private static readonly (string Name, string Description)[] FillingSeeds =
        {
            ("Đậu xanh", "Nhân đậu xanh truyền thống"),
            ("Hạt sen", "Nhân hạt sen bùi nhẹ"),
            ("Thập cẩm", "Nhân thập cẩm mặn ngọt"),
            ("Chocolate", "Nhân chocolate đậm vị"),
            ("Trà xanh", "Nhân trà xanh matcha"),
            ("Sầu riêng", "Nhân sầu riêng thơm béo"),
            ("Khoai môn", "Nhân khoai môn tím"),
            ("Trứng muối", "Nhân trứng muối mặn bùi")
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
