using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ShopGiay.Control;
using static ShopGiay.Infrastructure.ApiInput;

namespace ShopGiay.Boundary;

[ApiController]
[Route("api/admin/accounts")]
public sealed class UserController(UserControl users,SessionControl session) : ControllerBase
{
    [HttpGet]
    public Task<List<dynamic>> List(CancellationToken token)
    {
        session.Require("admin");
        return users.List(token);
    }

    // Chỉ cho quản trị tạo tài khoản rồi chuyển nội dung cho Control gán vai trò.
    [HttpPost]
    public Task<object> Create(JsonElement body,CancellationToken token) => users.Create(session.Require("admin").Id,body,token);

    // Chuyển yêu cầu quản trị cập nhật tài khoản và phân quyền cho Control.
    [HttpPut("{id}")]
    public Task<object> Update(string id,JsonElement body,CancellationToken token) => users.Update(Id(id),session.Require("admin").Id,body,token);

    // Chuyển yêu cầu xóa tài khoản của quản trị cho Control.
    [HttpDelete("{id}")]
    public Task<object> Delete(string id,CancellationToken token) => users.Delete(Id(id),session.Require("admin").Id,token);
}
