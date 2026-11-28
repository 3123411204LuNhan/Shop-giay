using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ShopGiay.Web.Infrastructure;
using ShopGiay.Web.Services;

namespace ShopGiay.Web.Controllers;
[ApiController]
[Route("api")]
public sealed class InventoryController(InventoryService service) : ShopController
{
    // Phiếu kho, cảnh báo hoặc lịch sử kho: React POST /api/manage/stock → throw new ApiException → Repository → MySQL.
    [HttpPost("manage/stock")]
    [ApiAccess("manager")]
    public object RejectDirectAdjustment() => throw new ApiException("Vui lòng lập phiếu kho nháp rồi xác nhận tại mục Phiếu kho.", 409);
    // Phiếu kho, cảnh báo hoặc lịch sử kho: React GET /api/manage/receipts → service.Receipts → Repository → MySQL.
    [HttpGet("manage/receipts")]
    [ApiAccess("manager")]
    public async Task<object> Receipts() => await service.Receipts();
    // Phiếu kho, cảnh báo hoặc lịch sử kho: React GET /api/manage/receipts/{id} → service.Receipt → Repository → MySQL.
    [HttpGet("manage/receipts/{id}")]
    [ApiAccess("manager")]
    public async Task<object> Receipt([FromRoute] long id) => await service.Receipt(id);
    // Phiếu kho, cảnh báo hoặc lịch sử kho: React POST /api/manage/receipts → service.CreateReceipt → Repository → MySQL.
    [HttpPost("manage/receipts")]
    [ApiAccess("manager")]
    public async Task<object> CreateReceipt([FromBody] JsonElement body) => await service.CreateReceipt(Row.From(body));
    // Phiếu kho, cảnh báo hoặc lịch sử kho: React PUT /api/manage/receipts/{id} → service.UpdateReceipt → Repository → MySQL.
    [HttpPut("manage/receipts/{id}")]
    [ApiAccess("manager")]
    public async Task<object> UpdateReceipt([FromRoute] long id, [FromBody] JsonElement body) => await service.UpdateReceipt(id, Row.From(body));
    // Phiếu kho, cảnh báo hoặc lịch sử kho: React POST /api/manage/receipts/{id}/confirm → service.ConfirmReceipt → Repository → MySQL.
    [HttpPost("manage/receipts/{id}/confirm")]
    [ApiAccess("manager")]
    public async Task<object> ConfirmReceipt([FromRoute] long id) => await service.ConfirmReceipt(id);
    // Phiếu kho, cảnh báo hoặc lịch sử kho: React POST /api/manage/receipts/{id}/cancel → service.CancelReceipt → Repository → MySQL.
    [HttpPost("manage/receipts/{id}/cancel")]
    [ApiAccess("manager")]
    public async Task<object> CancelReceipt([FromRoute] long id, [FromBody] JsonElement body) => await service.CancelReceipt(id, Row.From(body));
    // Phiếu kho, cảnh báo hoặc lịch sử kho: React PUT /api/manage/stock/{id}/warning → service.SetWarning → Repository → MySQL.
    [HttpPut("manage/stock/{id}/warning")]
    [ApiAccess("manager")]
    public async Task<object> SetWarning([FromRoute] long id, [FromBody] JsonElement body) => await service.SetWarning(id, Row.From(body));
    // Phiếu kho, cảnh báo hoặc lịch sử kho: React GET /api/manage/stock-history → service.StockHistory → Repository → MySQL.
    [HttpGet("manage/stock-history")]
    [ApiAccess("manager")]
    public async Task<object> StockHistory() => await service.StockHistory(Query());
}
