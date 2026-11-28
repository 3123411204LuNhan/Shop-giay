using Microsoft.AspNetCore.Mvc;
using ShopGiay.Control;
namespace ShopGiay.Boundary;
public sealed record CartChange(long VariantId, int? Quantity);

[ApiController]
[Route("/api/cart")]
public sealed class CartController(SessionControl session,CartControl cart) : ControllerBase
{
    // Kiểm tra phiên thuộc khu vực khách trước khi đọc hoặc cập nhật giỏ.
    private IActionResult? CheckCustomer() => session.User == null
        ? Unauthorized(new { message = "Vui lòng đăng nhập để tiếp tục." })
        : session.Area != "customer" ? StatusCode(403,new { message = "Chức năng mua sắm chỉ dành cho khách hàng." }) : null;

    // Trả các dòng giỏ của khách đang đăng nhập.
    [HttpGet]
    public async Task<IActionResult> Read(CancellationToken token) =>
        CheckCustomer() ?? Ok(await cart.Read(session.User!.Id,token));

    // Kiểm tra biến thể và số lượng, gọi Control cập nhật rồi trả kết quả cho giao diện.
    [HttpPut]
    public async Task<IActionResult> Update(CartChange change,CancellationToken token)
    {
        if (CheckCustomer() is { } denied) return denied;
        if (change.VariantId is < 1 or > 100000 || change.Quantity is null or < 0 or > 100)
            return BadRequest(new { message = "Biến thể hoặc số lượng không hợp lệ." });
        var updated = await cart.Update(session.User!.Id,change.VariantId,change.Quantity.Value,token);
        return updated ? Ok(new { ok = true }) : BadRequest(new { message = "Biến thể không đủ tồn kho." });
    }
}
