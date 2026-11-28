using Microsoft.AspNetCore.Mvc;
using ShopGiay.Control;

namespace ShopGiay.Boundary;

[ApiController]
public sealed class CheckoutController(SessionControl session,CheckoutControl checkout) : ControllerBase
{
    [HttpPost("/api/checkout/quote")]
    public Task<IActionResult> Quote(CheckoutInput input,CancellationToken token) => Process(input,false,token);

    [HttpPost("/api/orders")]
    public Task<IActionResult> Place(CheckoutInput input,CancellationToken token) => Process(input,true,token);

    private async Task<IActionResult> Process(CheckoutInput input,bool place,CancellationToken token)
    {
        if (session.User == null) return Unauthorized(new { message = "Vui lòng đăng nhập để tiếp tục." });
        if (session.Area != "customer") return StatusCode(403,new { message = "Chức năng mua sắm chỉ dành cho khách hàng." });
        try { return Ok(await checkout.Process(session.User.Id,input,place,token)); }
        catch (CheckoutException e) { return StatusCode(e.Status,new { message = e.Message }); }
    }
}
