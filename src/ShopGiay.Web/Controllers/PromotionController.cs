using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ShopGiay.Web.Infrastructure;
using ShopGiay.Web.Services;

namespace ShopGiay.Web.Controllers;
[ApiController]
[Route("api")]
public sealed class PromotionController(PromotionService service) : ShopController
{
    // Khuyến mãi: React GET /api/promotions → service.Published → Repository → MySQL.
    [HttpGet("promotions")]
    [ApiAccess("public")]
    public async Task<object> Published() => await service.Published();
    // Khuyến mãi: React GET /api/manage/promotions → service.List → Repository → MySQL.
    [HttpGet("manage/promotions")]
    [ApiAccess("manager")]
    public async Task<object> List() => await service.List();
    // Khuyến mãi: React POST /api/manage/promotions → service.Create → Repository → MySQL.
    [HttpPost("manage/promotions")]
    [ApiAccess("manager")]
    public async Task<object> Create([FromBody] JsonElement body) => await service.Create(Row.From(body));
    // Khuyến mãi: React PUT /api/manage/promotions/{id} → service.Update → Repository → MySQL.
    [HttpPut("manage/promotions/{id}")]
    [ApiAccess("manager")]
    public async Task<object> Update([FromRoute] long id, [FromBody] JsonElement body) => await service.Update(id, Row.From(body));
    // Khuyến mãi: React DELETE /api/manage/promotions/{id} → service.Delete → Repository → MySQL.
    [HttpDelete("manage/promotions/{id}")]
    [ApiAccess("manager")]
    public async Task<object> Delete([FromRoute] long id) => await service.Delete(id);
}
