using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Services
{
    public partial class SeedService
    {
        public const string ManagerEmail = "manager@luminabox.com";
        public const string UserEmail = "user@luminabox.com";
        public const string User2Email = "user2@luminabox.com";
        public const string User3Email = "user3@luminabox.com";
        public const string User4Email = "user4@luminabox.com";
        public const string Org1Email = "org1@luminabox.com";
        public const string Org2Email = "org2@luminabox.com";
        public const string Org3Email = "org3@luminabox.com";
        public const string Org4Email = "org4@luminabox.com";
        public const string Org5Email = "org5@luminabox.com";

        public const string ManagerPassword = "Manager@123";
        public const string UserPassword = "User@123";
        public const string OrganizationPassword = "Organization@123";

        private async Task SeedUsersAsync()
        {
            await EnsureUserAsync(ManagerEmail, "Quản trị LuminaBox", "0901000001", UserRole.Manager, ManagerPassword);
            await EnsureUserAsync(UserEmail, "Nguyễn Minh Anh", "0901000002", UserRole.User, UserPassword);
            await EnsureUserAsync(User2Email, "Trần Gia Hân", "0901000005", UserRole.User, UserPassword);
            await EnsureUserAsync(User3Email, "Lê Hoàng Nam", "0901000006", UserRole.User, UserPassword);
            await EnsureUserAsync(User4Email, "Phạm Thu Trang", "0901000007", UserRole.User, UserPassword);
            await EnsureUserAsync(Org1Email, "Chủ Moonlight Bakery", "0901000003", UserRole.Organization, OrganizationPassword);
            await EnsureUserAsync(Org2Email, "Chủ Luna Online Cakes", "0901000004", UserRole.Organization, OrganizationPassword);
            await EnsureUserAsync(Org3Email, "Chủ Trăng Vàng Heritage", "0901000008", UserRole.Organization, OrganizationPassword);
            await EnsureUserAsync(Org4Email, "Chủ Hoa Sen Patisserie", "0901000009", UserRole.Organization, OrganizationPassword);
            await EnsureUserAsync(Org5Email, "Chủ Mây Trà Artisan", "0901000010", UserRole.Organization, OrganizationPassword);
            await SaveAsync();
        }

        private async Task EnsureUserAsync(
            string email,
            string fullName,
            string phone,
            UserRole role,
            string password,
            UserStatus status = UserStatus.Active,
            bool isEmailVerified = true)
        {
            var existing = await _unitOfWork.Users.GetQueryable()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (existing is not null)
                return;

            await _unitOfWork.Users.AddAsync(new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FullName = fullName,
                Phone = phone,
                Address = "LuminaBox Demo Address",
                Role = role,
                Status = status,
                IsEmailVerified = isEmailVerified,
                PasswordHash = PasswordHash.HashPassword(password)
            });
        }

        private async Task<User> RequireUserByEmailAsync(string email)
        {
            return await _unitOfWork.Users.GetQueryable()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower())
                ?? throw ErrorHelper.Internal($"Seed user '{email}' not found.");
        }
    }
}
