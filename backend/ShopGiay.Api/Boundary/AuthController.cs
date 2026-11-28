using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ShopGiay.Control;

namespace ShopGiay.Boundary;

[ApiController]
[Route("api")]
public sealed class AuthController(AuthControl auth,SessionControl session) : ControllerBase
{
    [HttpPost("auth/login"), EnableRateLimiting("auth")]
    public Task<object> Login(JsonElement body) => auth.Login(HttpContext,body);

    [HttpPost("auth/register"), EnableRateLimiting("auth")]
    public Task<object> Register(JsonElement body) => auth.Register(HttpContext,body);

    [HttpPost("auth/logout"), EnableRateLimiting("auth")]
    public Task<object> Logout() => auth.Logout(HttpContext);

    [HttpPost("auth/forgot"), EnableRateLimiting("auth")]
    public Task<object> Forgot(JsonElement body,CancellationToken token) => auth.Forgot(body,token);

    [HttpPost("auth/verify-otp"), EnableRateLimiting("auth")]
    public Task<object> Verify(JsonElement body,CancellationToken token) => auth.Verify(body,token);

    [HttpPost("auth/reset"), EnableRateLimiting("auth")]
    public Task<object> Reset(JsonElement body,CancellationToken token) => auth.Reset(body,token);

    [HttpPost("staff/password")]
    public Task<object> StaffPassword(JsonElement body) => auth.StaffPassword(HttpContext,session.RequireStaff().Id,body);

    [HttpPut("profile")]
    public Task<object> Profile(JsonElement body,CancellationToken token) => auth.Profile(session.Require("customer").Id,body,token);
}
