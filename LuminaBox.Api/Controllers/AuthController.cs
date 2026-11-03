using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LuminaBox.Application.Dtos.Auth;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using Swashbuckle.AspNetCore.Annotations;

namespace LuminaBox.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [SwaggerOperation(
            Summary = "Register a new account",
            Description = "Creates a new account and sends an email verification OTP.")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            await _authService.RegisterAsync(request);
            return Ok(ApiResult.Success(message: "Registration successful. Please check your email for the OTP code."));
        }

        [HttpPost("verify-otp")]
        [SwaggerOperation(
            Summary = "Verify email with OTP",
            Description = "Verifies the account email using the OTP code.")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            await _authService.VerifyOtpAsync(request);
            return Ok(ApiResult.Success(message: "Email verified successfully. Please log in."));
        }

        [HttpPost("resend-otp")]
        [SwaggerOperation(
            Summary = "Resend email verification OTP",
            Description = "Sends a new email verification OTP.")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest request)
        {
            await _authService.ResendOtpAsync(request);
            return Ok(ApiResult.Success(message: "A new OTP code has been sent to your email."));
        }

        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "Log in",
            Description = "Logs in with email and password and returns access and refresh tokens.")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return Ok(ApiResult<LoginResponse>.Success(result));
        }

        [HttpPost("refresh-token")]
        [SwaggerOperation(
            Summary = "Refresh tokens",
            Description = "Issues new access and refresh tokens from a valid refresh token.")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request);
            return Ok(ApiResult<LoginResponse>.Success(result));
        }

        [HttpPost("forgot-password")]
        [SwaggerOperation(
            Summary = "Request a password reset OTP",
            Description = "Sends a password reset OTP to the account email.")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            await _authService.ForgotPasswordAsync(request);
            return Ok(ApiResult.Success(message: "An OTP code has been sent to your email."));
        }

        [HttpPost("reset-password")]
        [SwaggerOperation(
            Summary = "Reset password with OTP",
            Description = "Resets the password using the password reset OTP.")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            await _authService.ResetPasswordAsync(request);
            return Ok(ApiResult.Success(message: "Password has been reset successfully. Please log in."));
        }

        [Authorize]
        [HttpPost("logout")]
        [SwaggerOperation(
            Summary = "Log out",
            Description = "Logs out the current user. Requires authentication.")]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return Ok(ApiResult.Success(message: "Logged out successfully."));
        }
    }
}
