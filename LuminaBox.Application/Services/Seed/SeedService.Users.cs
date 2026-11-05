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

        public const string ManagerPassword = "Manager@123";
        public const string UserPassword = "User@123";
        public const string OrganizationPassword = "Organization@123";

        private async Task SeedUsersAsync()
        {
            await EnsureUserAsync(ManagerEmail, "Lumina Manager", "0901000001", UserRole.Manager, ManagerPassword);
            await EnsureUserAsync(UserEmail, "Demo Customer", "0901000002", UserRole.User, UserPassword);
            await EnsureUserAsync(User2Email, "Demo Customer 2", "0901000005", UserRole.User, UserPassword);
            await EnsureUserAsync(User3Email, "Demo Customer 3", "0901000006", UserRole.User, UserPassword);
            await EnsureUserAsync(User4Email, "Demo Customer 4", "0901000007", UserRole.User, UserPassword);
            await EnsureUserAsync(Org1Email, "Moonlight Bakery Owner", "0901000003", UserRole.Organization, OrganizationPassword);
            await EnsureUserAsync(Org2Email, "Luna Online Owner", "0901000004", UserRole.Organization, OrganizationPassword);
            await SaveAsync();
        }

        private async Task EnsureUserAsync(
            string email,
            string fullName,
            string phone,
            UserRole role,
            string password)
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
                Status = UserStatus.Active,
                IsEmailVerified = true,
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
