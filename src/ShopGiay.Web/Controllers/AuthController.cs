using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ShopGiay.Web.Infrastructure;
using ShopGiay.Web.Services;

namespace ShopGiay.Web.Controllers;
[ApiController]
[Route("api")]
public sealed class AuthController(AuthService service, PasswordResetService reset) : ShopController
{
    // Thông tin phiên hiện tại: React GET /api/me → service.Me → Repository → MySQL.
    [HttpGet("me")]
    [ApiAccess("public")]
    public object Me() => service.Me();
    // Đăng nhập theo khu vực: React POST /api/auth/login → service.Login → Repository → MySQL.
    [HttpPost("auth/login")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")]
    [ApiAccess("public")]
    public async Task<IActionResult> Login([FromBody] JsonElement body)
    {
        // Service trả kết quả dự kiến cho sai mật khẩu/sai khu vực, không ném ApiException.
        var result = await service.Login(Row.From(body));
        // React nhận đúng HTTP 200/401/403 để thông báo và cho người dùng nhập lại.
        return StatusCode(result.StatusCode, result.Body);
    }
    // Đăng ký khách hàng: React POST /api/auth/register → service.Register → Repository → MySQL.
    [HttpPost("auth/register")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")]
    [ApiAccess("public")]
    public async Task<object> Register([FromBody] JsonElement body) => await service.Register(Row.From(body));
    // Thu hồi phiên hiện tại: React POST /api/auth/logout → service.Logout → Repository → MySQL.
    [HttpPost("auth/logout")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")]
    [ApiAccess("public")]
    public async Task<object> Logout() => await service.Logout();
    // Yêu cầu OTP: React POST /api/auth/forgot → reset.Request → Repository → MySQL.
    [HttpPost("auth/forgot")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")]
    [ApiAccess("public")]
    public async Task<object> RequestPasswordReset([FromBody] JsonElement body) => await reset.Request(Row.From(body));
    // Xác minh OTP: React POST /api/auth/verify-otp → reset.Verify → Repository → MySQL.
    [HttpPost("auth/verify-otp")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")]
    [ApiAccess("public")]
    public async Task<object> VerifyOtp([FromBody] JsonElement body) => await reset.Verify(Row.From(body));
    // Đặt mật khẩu mới: React POST /api/auth/reset → reset.Reset → Repository → MySQL.
    [HttpPost("auth/reset")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("auth")]
    [ApiAccess("public")]
    public async Task<object> ResetPassword([FromBody] JsonElement body) => await reset.Reset(Row.From(body));
    // Nhân sự tự đổi mật khẩu: React POST /api/staff/password → service.ChangeStaffPassword → Repository → MySQL.
    [HttpPost("staff/password")]
    [ApiAccess("staff")]
    public async Task<object> ChangeStaffPassword([FromBody] JsonElement body) => await service.ChangeStaffPassword(Row.From(body));
    // Cập nhật hồ sơ: React PUT /api/profile → service.UpdateProfile → Repository → MySQL.
    [HttpPut("profile")]
    [ApiAccess("customer")]
    public async Task<object> UpdateProfile([FromBody] JsonElement body) => await service.UpdateProfile(Row.From(body));
}
