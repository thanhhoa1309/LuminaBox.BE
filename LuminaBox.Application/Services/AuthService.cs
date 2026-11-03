using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using LuminaBox.Application.Dtos.Auth;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    public class AuthService : IAuthService
    {
        private const int OtpExpirationMinutes = 5;
        private const int MaxOtpFailedAttempts = 5;
        private const int DefaultAccessTokenExpirationMinutes = 60;
        private const int DefaultRefreshTokenExpirationDays = 7;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ICurrentTime _currentTime;
        private readonly IClaimsService _claimsService;
        private readonly IConfiguration _configuration;

        public AuthService(IUnitOfWork unitOfWork, IEmailService emailService, ICurrentTime currentTime,
            IClaimsService claimsService, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _currentTime = currentTime;
            _claimsService = claimsService;
            _configuration = configuration;
        }

        public async Task RegisterAsync(RegisterRequest request)
        {
            var email = NormalizeEmail(request.Email);

            var existingUser = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser is not null)
                throw ErrorHelper.Conflict("Email already exists.");

            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var user = new User
                {
                    Email = email,
                    PasswordHash = PasswordHash.HashPassword(request.Password),
                    FullName = request.FullName.Trim(),
                    Phone = request.Phone.Trim(),
                    Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim()
                };
                await _unitOfWork.Users.AddAsync(user);

                await IssueOtpAsync(user, OtpPurpose.EmailVerification);
                return true;
            });
        }

        public async Task VerifyOtpAsync(VerifyOtpRequest request)
        {
            var user = await GetUserByEmailAsync(request.Email);

            if (user.IsEmailVerified)
                throw ErrorHelper.BadRequest("Email is already verified.");

            await ConsumeOtpAsync(user.Id, OtpPurpose.EmailVerification, request.OtpCode);

            user.IsEmailVerified = true;
            await _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ResendOtpAsync(ResendOtpRequest request)
        {
            var user = await GetUserByEmailAsync(request.Email);

            if (user.IsEmailVerified)
                throw ErrorHelper.BadRequest("Email is already verified.");

            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                await IssueOtpAsync(user, OtpPurpose.EmailVerification);
                return true;
            });
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var email = NormalizeEmail(request.Email);
            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user is null || !PasswordHash.VerifyPassword(request.Password, user.PasswordHash))
                throw ErrorHelper.Unauthorized("Email or password is incorrect.");

            if (!user.IsEmailVerified)
                throw ErrorHelper.Forbidden("Email is not verified.");

            if (user.Status == UserStatus.Locked)
                throw ErrorHelper.Forbidden("Account is locked.");

            return await GenerateTokensAsync(user);
        }

        public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            if (user is null || user.RefreshTokenExpiryTime is null || user.RefreshTokenExpiryTime <= _currentTime.GetCurrentTime())
                throw ErrorHelper.Unauthorized("Refresh token is invalid or has expired.");

            if (user.Status == UserStatus.Locked)
                throw ErrorHelper.Forbidden("Account is locked.");

            return await GenerateTokensAsync(user);
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var user = await GetUserByEmailAsync(request.Email);

            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                await IssueOtpAsync(user, OtpPurpose.ResetPassword);
                return true;
            });
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await GetUserByEmailAsync(request.Email);

            await ConsumeOtpAsync(user.Id, OtpPurpose.ResetPassword, request.OtpCode);

            user.PasswordHash = PasswordHash.HashPassword(request.NewPassword);
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task LogoutAsync()
        {
            var userId = _claimsService.GetRequiredUserId();

            var user = await _unitOfWork.Users.GetByIdAsync(userId)
                ?? throw ErrorHelper.NotFound("User not found.");

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

        private async Task<User> GetUserByEmailAsync(string email)
        {
            var normalizedEmail = NormalizeEmail(email);
            return await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail)
                ?? throw ErrorHelper.NotFound("User not found.");
        }

        private async Task IssueOtpAsync(User user, OtpPurpose purpose)
        {
            var activeOtps = await _unitOfWork.EmailOtps.GetAllAsync(o =>
                o.UserId == user.Id && o.Purpose == purpose && !o.IsUsed);
            if (activeOtps.Count > 0)
            {
                activeOtps.ForEach(o => o.IsUsed = true);
                await _unitOfWork.EmailOtps.UpdateRange(activeOtps);
            }

            var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
            await _unitOfWork.EmailOtps.AddAsync(new EmailOtp
            {
                UserId = user.Id,
                CodeHash = PasswordHash.HashPassword(code),
                Purpose = purpose,
                ExpiresAt = _currentTime.GetCurrentTime().AddMinutes(OtpExpirationMinutes),
                IsUsed = false,
                FailedAttempts = 0
            });
            await _unitOfWork.SaveChangesAsync();

            var subject = purpose == OtpPurpose.EmailVerification
                ? "LuminaBox - Verify your email"
                : "LuminaBox - Reset your password";
            var htmlBody =
                $"<p>Hello {user.FullName},</p>" +
                $"<p>Your OTP code is: <strong>{code}</strong></p>" +
                $"<p>This code will expire in {OtpExpirationMinutes} minutes. Do not share it with anyone.</p>";

            await _emailService.SendEmailAsync(user.Email, subject, htmlBody);
        }

        private async Task ConsumeOtpAsync(Guid userId, OtpPurpose purpose, string otpCode)
        {
            var otp = (await _unitOfWork.EmailOtps.GetAllAsync(o =>
                    o.UserId == userId && o.Purpose == purpose && !o.IsUsed))
                .MaxBy(o => o.CreatedAt);

            if (otp is null || otp.ExpiresAt <= _currentTime.GetCurrentTime())
                throw ErrorHelper.BadRequest("OTP is invalid or has expired.");

            if (otp.FailedAttempts >= MaxOtpFailedAttempts)
                throw ErrorHelper.BadRequest("OTP has exceeded the maximum number of attempts. Please request a new one.");

            if (!PasswordHash.VerifyPassword(otpCode, otp.CodeHash))
            {
                otp.FailedAttempts++;
                await _unitOfWork.EmailOtps.Update(otp);
                await _unitOfWork.SaveChangesAsync();
                throw ErrorHelper.BadRequest("OTP is incorrect.");
            }

            otp.IsUsed = true;
            await _unitOfWork.EmailOtps.Update(otp);
        }

        private async Task<LoginResponse> GenerateTokensAsync(User user)
        {
            var accessTokenMinutes = int.TryParse(_configuration["JWT:AccessTokenExpirationMinutes"], out var minutes)
                ? minutes
                : DefaultAccessTokenExpirationMinutes;
            var refreshTokenDays = int.TryParse(_configuration["JWT:RefreshTokenExpirationDays"], out var days)
                ? days
                : DefaultRefreshTokenExpirationDays;

            var accessToken = JwtUtils.GenerateJwtToken(user.Id, user.Email, user.Role.ToString(), _configuration,
                TimeSpan.FromMinutes(accessTokenMinutes));
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = _currentTime.GetCurrentTime().AddDays(refreshTokenDays);
            await _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
    }
}
