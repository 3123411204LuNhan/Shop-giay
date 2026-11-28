using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ShopGiay.Control;
using static ShopGiay.Infrastructure.ApiInput;

namespace ShopGiay.Boundary;

[ApiController]
[Route("api")]
public sealed class PromotionController(PromotionControl promotions,SessionControl session) : ControllerBase
{
    [HttpGet("promotions")]
    public Task<List<dynamic>> Public(CancellationToken token) => promotions.List(true,token);

    [HttpGet("manage/promotions")]
    public Task<List<dynamic>> List(CancellationToken token)
    {
        session.Require("manager");
        return promotions.List(false,token);
    }

    [HttpPost("manage/promotions")]
    public Task<object> Create(JsonElement body,CancellationToken token) => promotions.Create(session.Require("manager").Id,body,token);

    [HttpPut("manage/promotions/{id}")]
    public Task<object> Update(string id,JsonElement body,CancellationToken token) => promotions.Update(Id(id),session.Require("manager").Id,body,token);

    [HttpDelete("manage/promotions/{id}")]
    public Task<object> Delete(string id,CancellationToken token) => promotions.Delete(Id(id),session.Require("manager").Id,token);
}
