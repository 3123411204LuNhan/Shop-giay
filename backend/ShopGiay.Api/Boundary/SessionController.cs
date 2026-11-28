using Microsoft.AspNetCore.Mvc;
using ShopGiay.Control;

namespace ShopGiay.Boundary;

[ApiController]
public sealed class SessionController(SessionControl session) : ControllerBase
{
    [HttpGet("/api/me")]
    public IActionResult Me() => Ok(new { user = session.User });
}
