using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ShopGiay.Control;

namespace ShopGiay.Boundary;

[ApiController]
[Route("api")]
public sealed class AuthController(AuthControl auth,SessionControl session) : ControllerBase
{
    // Nhận thông tin đăng nhập đã giới hạn tần suất rồi chuyển cho Control của phiên hiện tại.
    [HttpPost("auth/login"), EnableRateLimiting("auth")]
    public Task<object> Login(JsonElement body) => auth.Login(HttpContext,body);

    // Nhận dữ liệu đăng ký và chuyển cho Control tạo tài khoản khách và phiên.
    [HttpPost("auth/register"), EnableRateLimiting("auth")]
    public Task<object> Register(JsonElement body) => auth.Register(HttpContext,body);

    // Chuyển cookie phiên hiện tại cho Control thu hồi và đăng xuất.
    [HttpPost("auth/logout"), EnableRateLimiting("auth")]
    public Task<object> Logout() => auth.Logout(HttpContext);

    // Nhận yêu cầu OTP, chuyển Control kiểm tra giới hạn và gửi mã.
    [HttpPost("auth/forgot"), EnableRateLimiting("auth")]
    public Task<object> Forgot(JsonElement body,CancellationToken token) => auth.Forgot(body,token);

    // Nhận challenge và mã rồi yêu cầu Control xác minh để cấp token đặt lại.
    [HttpPost("auth/verify-otp"), EnableRateLimiting("auth")]
    public Task<object> Verify(JsonElement body,CancellationToken token) => auth.Verify(body,token);

    // Nhận token đã xác minh cùng mật khẩu mới và chuyển Control xử lý.
    [HttpPost("auth/reset"), EnableRateLimiting("auth")]
    public Task<object> Reset(JsonElement body,CancellationToken token) => auth.Reset(body,token);

    // Kiểm tra phiên nhân sự trước khi chuyển yêu cầu đổi mật khẩu cho Control.
    [HttpPost("staff/password")]
    public Task<object> StaffPassword(JsonElement body) => auth.StaffPassword(HttpContext,session.RequireStaff().Id,body);

    // Lấy tài khoản khách từ phiên rồi chuyển dữ liệu hồ sơ cho Control.
    [HttpPut("profile")]
    public Task<object> Profile(JsonElement body,CancellationToken token) => auth.Profile(session.Require("customer").Id,body,token);
}
