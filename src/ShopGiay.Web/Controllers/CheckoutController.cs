using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ShopGiay.Web.Infrastructure;
using ShopGiay.Web.Services;

namespace ShopGiay.Web.Controllers;
[ApiController]
[Route("api")]
public sealed class CheckoutController(CheckoutService service) : ShopController
{
    // Xem giỏ: React GET /api/cart → service.Cart → Repository → MySQL.
    [HttpGet("cart")]
    [ApiAccess("customer")]
    public async Task<object> Cart() => await service.Cart();
    // Cập nhật giỏ: React PUT /api/cart → service.UpdateCart → Repository → MySQL.
    [HttpPut("cart")]
    [ApiAccess("customer")]
    public async Task<object> UpdateCart([FromBody] JsonElement body) => await service.UpdateCart(Row.From(body));
    // Kiểm tra thông tin đơn: React POST /api/checkout/quote → service.Quote → Repository → MySQL.
    [HttpPost("checkout/quote")]
    [ApiAccess("customer")]
    public async Task<object> Quote([FromBody] JsonElement body) => await service.Quote(Row.From(body));
    // Tạo đơn và giữ kho: React POST /api/orders → service.PlaceOrder → Repository → MySQL.
    [HttpPost("orders")]
    [ApiAccess("customer")]
    public async Task<object> PlaceOrder([FromBody] JsonElement body) => await service.PlaceOrder(Row.From(body));
}
