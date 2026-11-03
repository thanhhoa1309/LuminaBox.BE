using LuminaBox.Application.Dtos.Auth;

namespace LuminaBox.Application.Interface
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterRequest request);
        Task VerifyOtpAsync(VerifyOtpRequest request);
        Task ResendOtpAsync(ResendOtpRequest request);
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task ForgotPasswordAsync(ForgotPasswordRequest request);
        Task ResetPasswordAsync(ResetPasswordRequest request);
        Task LogoutAsync();
    }
}
