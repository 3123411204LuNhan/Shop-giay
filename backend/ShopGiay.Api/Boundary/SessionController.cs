using Microsoft.AspNetCore.Mvc;
using ShopGiay.Control;

namespace ShopGiay.Boundary;

[ApiController]
public sealed class SessionController(SessionControl session) : ControllerBase
{
    // Trả hồ sơ phiên mà SessionControl đã xác định cho khu vực hiện tại.
    [HttpGet("/api/me")]
    public IActionResult Me() => Ok(new { user = session.User });
}
