using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ShopGiay.Control;
using static ShopGiay.Infrastructure.ApiInput;
namespace ShopGiay.Boundary;
[ApiController]
[Route("api")]
public sealed class OrderController(OrderControl orders,SessionControl session) : ControllerBase
{
    [HttpGet("orders")]
    public Task<List<dynamic>> Mine(CancellationToken token) => orders.List(session.Require("customer").Id,token);

    [HttpGet("orders/{id}")]
    public Task<object> Details(string id,CancellationToken token) =>
        orders.Details(Id(id),session.Require(session.Area == "manager" ? "manager" : "customer"),session.Area == "manager",token);

    // Xác thực khách hàng rồi chuyển mã đơn và lý do hủy cho Control.
    [HttpPost("orders/{id}/cancel")]
    public Task<object> Cancel(string id,JsonElement body,CancellationToken token) => orders.Cancel(Id(id),session.Require("customer").Id,body,token);

    [HttpGet("manage/orders")]
    public Task<List<dynamic>> All(CancellationToken token)
    {
        session.Require("manager");
        return orders.List(null,token);
    }

    [HttpGet("manage/customers/{id}/orders")]
    public Task<List<dynamic>> Customer(string id,CancellationToken token)
    {
        session.Require("manager");
        return orders.List(Id(id),token);
    }

    // Kiểm tra quyền quản lý rồi gửi trạng thái mới hoặc yêu cầu hủy cho Control.
    [HttpPost("manage/orders/{id}/status")]
    public Task<object> Advance(string id,JsonElement body,CancellationToken token) => orders.Advance(Id(id),session.Require("manager").Id,body,token);

    // Kiểm tra quyền quản lý rồi chuyển bằng chứng xác nhận thu cho Control.
    [HttpPost("manage/payments/{id}/confirm")]
    public Task<object> Confirm(string id,JsonElement body,CancellationToken token) => orders.ConfirmPayment(Id(id),session.Require("manager").Id,body,token);

    // Lấy người quản lý và chuyển trạng thái vận chuyển, thông tin theo dõi cho Control.
    [HttpPost("manage/orders/{id}/delivery")]
    public Task<object> Delivery(string id,JsonElement body,CancellationToken token) => orders.Delivery(Id(id),session.Require("manager").Id,body,token);

    // Nhận kết quả kiểm hàng giao hoàn của người quản lý.
    [HttpPost("manage/orders/{id}/receive-delivery-return")]
    public Task<object> Receive(string id,JsonElement body,CancellationToken token) => orders.Receive(Id(id),session.Require("manager").Id,body,token);

    // Chuyển số tiền và bằng chứng chuyển khoản đến sau lúc hủy cho Control.
    [HttpPost("manage/orders/{id}/late-payment")]
    public Task<object> Late(string id,JsonElement body,CancellationToken token) => orders.LatePayment(Id(id),session.Require("manager").Id,body,token);
}
