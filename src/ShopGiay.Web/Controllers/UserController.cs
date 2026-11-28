using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ShopGiay.Web.Infrastructure;
using ShopGiay.Web.Services;

namespace ShopGiay.Web.Controllers;
[ApiController]
[Route("api")]
public sealed class UserController(UserService service) : ShopController
{
    // Quản trị tài khoản và phân quyền: React GET /api/admin/accounts → service.List → Repository → MySQL.
    [HttpGet("admin/accounts")]
    [ApiAccess("admin")]
    public async Task<object> List() => await service.List();
    // Quản trị tài khoản và phân quyền: React POST /api/admin/accounts → service.Create → Repository → MySQL.
    [HttpPost("admin/accounts")]
    [ApiAccess("admin")]
    public async Task<object> Create([FromBody] JsonElement body) => await service.Create(Row.From(body));
    // Quản trị tài khoản và phân quyền: React PUT /api/admin/accounts/{id} → service.Update → Repository → MySQL.
    [HttpPut("admin/accounts/{id}")]
    [ApiAccess("admin")]
    public async Task<object> Update([FromRoute] long id, [FromBody] JsonElement body) => await service.Update(id, Row.From(body));
    // Quản trị tài khoản và phân quyền: React DELETE /api/admin/accounts/{id} → service.Remove → Repository → MySQL.
    [HttpDelete("admin/accounts/{id}")]
    [ApiAccess("admin")]
    public async Task<object> Remove([FromRoute] long id) => await service.Remove(id);
}
