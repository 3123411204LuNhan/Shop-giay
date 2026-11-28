using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ShopGiay.Web.Infrastructure;
using ShopGiay.Web.Services;

namespace ShopGiay.Web.Controllers;
[ApiController]
[Route("api")]
public sealed class OrderController(OrderService service) : ShopController
{
    // Lịch sử mua hàng: React GET /api/orders → service.ListMine → Repository → MySQL.
    [HttpGet("orders")]
    [ApiAccess("customer")]
    public async Task<object> ListMine() => await service.ListMine();
    // Chi tiết và theo dõi đơn: React GET /api/orders/{id} → service.Details → Repository → MySQL.
    [HttpGet("orders/{id}")]
    [ApiAccess("order-detail")]
    public async Task<object> Details([FromRoute] long id) => await service.Details(id);
    // Khách hủy đơn: React POST /api/orders/{id}/cancel → service.Cancel → Repository → MySQL.
    [HttpPost("orders/{id}/cancel")]
    [ApiAccess("customer")]
    public async Task<object> Cancel([FromRoute] long id, [FromBody] JsonElement body) => await service.Cancel(id, Row.From(body));
    // Danh sách đơn: React GET /api/manage/orders → service.ListAll → Repository → MySQL.
    [HttpGet("manage/orders")]
    [ApiAccess("manager")]
    public async Task<object> ListAll() => await service.ListAll();
    // Chuyển trạng thái: React POST /api/manage/orders/{id}/status → service.Advance → Repository → MySQL.
    [HttpPost("manage/orders/{id}/status")]
    [ApiAccess("manager")]
    public async Task<object> Advance([FromRoute] long id, [FromBody] JsonElement body) => await service.Advance(id, Row.From(body));
    // Đối soát khoản thu: React POST /api/manage/payments/{id}/confirm → service.ConfirmPayment → Repository → MySQL.
    [HttpPost("manage/payments/{id}/confirm")]
    [ApiAccess("manager")]
    public async Task<object> ConfirmPayment([FromRoute] long id, [FromBody] JsonElement body) => await service.ConfirmPayment(id, Row.From(body));
    // Đơn của khách hàng: React GET /api/manage/customers/{id}/orders → service.CustomerHistory → Repository → MySQL.
    [HttpGet("manage/customers/{id}/orders")]
    [ApiAccess("manager")]
    public async Task<object> CustomerHistory([FromRoute] long id) => await service.CustomerHistory(id);
    // Xử lý vận chuyển: React POST /api/manage/orders/{id}/delivery → service.UpdateDelivery → Repository → MySQL.
    [HttpPost("manage/orders/{id}/delivery")]
    [ApiAccess("manager")]
    public async Task<object> UpdateDelivery([FromRoute] long id, [FromBody] JsonElement body) => await service.UpdateDelivery(id, Row.From(body));
    // Nhận hàng giao thất bại: React POST /api/manage/orders/{id}/receive-delivery-return → service.ReceiveDeliveryReturn → Repository → MySQL.
    [HttpPost("manage/orders/{id}/receive-delivery-return")]
    [ApiAccess("manager")]
    public async Task<object> ReceiveDeliveryReturn([FromRoute] long id, [FromBody] JsonElement body) => await service.ReceiveDeliveryReturn(id, Row.From(body));
    // Đối soát tiền đến muộn: React POST /api/manage/orders/{id}/late-payment → service.RecordLatePayment → Repository → MySQL.
    [HttpPost("manage/orders/{id}/late-payment")]
    [ApiAccess("manager")]
    public async Task<object> RecordLatePayment([FromRoute] long id, [FromBody] JsonElement body) => await service.RecordLatePayment(id, Row.From(body));
}
