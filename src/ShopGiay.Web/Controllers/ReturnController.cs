using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ShopGiay.Web.Infrastructure;
using ShopGiay.Web.Services;

namespace ShopGiay.Web.Controllers;
[ApiController]
[Route("api")]
public sealed class ReturnController(ReturnService service) : ShopController
{
    // Đánh giá, trả hàng hoặc hoàn tiền: React POST /api/reviews → service.Review → Repository → MySQL.
    [HttpPost("reviews")]
    [ApiAccess("customer")]
    public async Task<object> Review([FromBody] JsonElement body) => await service.Review(Row.From(body));
    // Đánh giá, trả hàng hoặc hoàn tiền: React GET /api/returns → service.ListMine → Repository → MySQL.
    [HttpGet("returns")]
    [ApiAccess("customer")]
    public async Task<object> ListMine() => await service.ListMine();
    // Đánh giá, trả hàng hoặc hoàn tiền: React POST /api/returns → service.RequestReturn → Repository → MySQL.
    [HttpPost("returns")]
    [ApiAccess("customer")]
    public async Task<object> RequestReturn([FromBody] JsonElement body) => await service.RequestReturn(Row.From(body));
    // Đánh giá, trả hàng hoặc hoàn tiền: React GET /api/returns/{id} → service.Detail → Repository → MySQL.
    [HttpGet("returns/{id}")]
    [ApiAccess("customer")]
    public async Task<object> Detail([FromRoute] long id) => await service.Detail(id);
    // Đánh giá, trả hàng hoặc hoàn tiền: React GET /api/manage/returns → service.ListAll → Repository → MySQL.
    [HttpGet("manage/returns")]
    [ApiAccess("manager")]
    public async Task<object> ListAll() => await service.ListAll();
    // Đánh giá, trả hàng hoặc hoàn tiền: React POST /api/manage/returns/{id}/action → service.ProcessReturn → Repository → MySQL.
    [HttpPost("manage/returns/{id}/action")]
    [ApiAccess("manager")]
    public async Task<object> ProcessReturn([FromRoute] long id, [FromBody] JsonElement body) => await service.ProcessReturn(id, Row.From(body));
    // Đánh giá, trả hàng hoặc hoàn tiền: React GET /api/manage/refunds → service.Refunds → Repository → MySQL.
    [HttpGet("manage/refunds")]
    [ApiAccess("manager")]
    public async Task<object> Refunds() => await service.Refunds();
    // Đánh giá, trả hàng hoặc hoàn tiền: React POST /api/manage/refunds/{id}/confirm → service.ConfirmRefund → Repository → MySQL.
    [HttpPost("manage/refunds/{id}/confirm")]
    [ApiAccess("manager")]
    public async Task<object> ConfirmRefund([FromRoute] long id, [FromBody] JsonElement body) => await service.ConfirmRefund(id, Row.From(body));
    // Đánh giá, trả hàng hoặc hoàn tiền: React POST /api/manage/refunds/{id}/retry → service.RefundAction → Repository → MySQL.
    [HttpPost("manage/refunds/{id}/retry")]
    [ApiAccess("manager")]
    public async Task<object> RetryRefund([FromRoute] long id, [FromBody] JsonElement body) => await service.RefundAction(id, Row.From(body), true);
    // Đánh giá, trả hàng hoặc hoàn tiền: React POST /api/manage/refunds/{id}/failure → service.RefundAction → Repository → MySQL.
    [HttpPost("manage/refunds/{id}/failure")]
    [ApiAccess("manager")]
    public async Task<object> MarkRefundFailed([FromRoute] long id, [FromBody] JsonElement body) => await service.RefundAction(id, Row.From(body), false);
}
