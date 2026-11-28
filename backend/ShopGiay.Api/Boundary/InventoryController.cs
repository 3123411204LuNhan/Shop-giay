using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ShopGiay.Control;
using ShopGiay.Infrastructure;
using static ShopGiay.Infrastructure.ApiInput;

namespace ShopGiay.Boundary;

[ApiController]
[Route("api/manage")]
public sealed class InventoryController(InventoryControl inventory,SessionControl session) : ControllerBase
{
    [HttpGet("stock")]
    public Task<List<dynamic>> Stock(string? productId,CancellationToken token)
    {
        session.Require("manager");
        return inventory.List(productId,token);
    }

    [HttpPost("stock")]
    public IActionResult RejectDirectAdjustment()
    {
        session.Require("manager");
        throw new ApiException("Vui lòng lập phiếu kho nháp rồi xác nhận tại mục Phiếu kho.",409);
    }

    [HttpGet("stock-history")]
    public Task<List<dynamic>> History(string? variantId,CancellationToken token)
    {
        session.Require("manager");
        return inventory.History(variantId,token);
    }

    [HttpGet("receipts")]
    public Task<List<dynamic>> Receipts(CancellationToken token)
    {
        session.Require("manager");
        return inventory.Receipts(token);
    }

    [HttpGet("receipts/{id}")]
    public Task<object> Receipt(string id,CancellationToken token)
    {
        session.Require("manager");
        return inventory.Receipt(Id(id),token);
    }

    // Lấy người quản lý từ phiên và gửi phiếu nháp mới cho Control.
    [HttpPost("receipts")]
    public Task<object> Save(JsonElement body,CancellationToken token) => inventory.Save(null,session.Require("manager").Id,body,token);

    // Xác định phiếu nháp để Control kiểm tra và cập nhật các dòng.
    [HttpPut("receipts/{id}")]
    public Task<object> Update(string id,JsonElement body,CancellationToken token) => inventory.Save(Id(id),session.Require("manager").Id,body,token);

    // Chuyển yêu cầu xác nhận của quản lý để ghi sổ phiếu kho.
    [HttpPost("receipts/{id}/confirm")]
    public Task<object> Confirm(string id,CancellationToken token) => inventory.Confirm(Id(id),session.Require("manager").Id,token);

    // Chuyển mã phiếu và lý do hủy của người quản lý.
    [HttpPost("receipts/{id}/cancel")]
    public Task<object> Cancel(string id,JsonElement body,CancellationToken token) => inventory.Cancel(Id(id),session.Require("manager").Id,body,token);

    // Nhận ngưỡng tồn mới và chuyển cho Control ghi kèm nhật ký.
    [HttpPut("stock/{id}/warning")]
    public Task<object> Warning(string id,JsonElement body,CancellationToken token) => inventory.Warning(Id(id),session.Require("manager").Id,body,token);
}
