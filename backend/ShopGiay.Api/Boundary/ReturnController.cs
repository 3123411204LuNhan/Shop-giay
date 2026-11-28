using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ShopGiay.Control;
using ShopGiay.Infrastructure;
using static ShopGiay.Infrastructure.ApiInput;

namespace ShopGiay.Boundary;

[ApiController]
[Route("api")]
public sealed class ReturnController(ReturnControl returns,SessionControl session,ImageStore images) : ControllerBase
{
    [HttpPost("reviews")]
    public Task<object> Review(JsonElement body,CancellationToken token) => returns.Review(session.Require("customer").Id,body,token);

    [HttpGet("returns")]
    public Task<List<dynamic>> Mine(CancellationToken token) => returns.List(session.Require("customer").Id,token);

    [HttpPost("returns")]
    [RequestSizeLimit(5 * 1024 * 1024 + 64 * 1024)]
    public async Task<object> Create(CancellationToken token)
    {
        var customer = session.Require("customer").Id;
        var (body,image) = await ImageStore.Read(Request,token);
        return await returns.Create(customer,body,image,token);
    }

    [HttpGet("returns/options/{itemId}")]
    public Task<object> Options(string itemId,CancellationToken token) => returns.Options(Id(itemId),session.Require("customer").Id,token);

    [HttpGet("returns/{id}")]
    public Task<object> Detail(string id,CancellationToken token) => returns.Details(Id(id),session.Require("customer").Id,token);

    [HttpGet("manage/returns")]
    public Task<List<dynamic>> All(CancellationToken token)
    {
        session.Require("manager");
        return returns.List(null,token);
    }

    [HttpGet("manage/returns/{id}")]
    public Task<object> Managed(string id,CancellationToken token)
    {
        session.Require("manager");
        return returns.Details(Id(id),null,token);
    }

    [HttpGet("returns/{id}/images/{name}")]
    public Task<IActionResult> CustomerEvidence(string id,string name,CancellationToken token) =>
        Evidence(Id(id),session.Require("customer").Id,name,token);

    [HttpGet("manage/returns/{id}/images/{name}")]
    public Task<IActionResult> StaffEvidence(string id,string name,CancellationToken token)
    {
        session.Require("manager");
        return Evidence(Id(id),null,name,token);
    }

    private async Task<IActionResult> Evidence(long id,long? customer,string name,CancellationToken token)
    {
        await returns.Evidence(id,customer,name,token);
        var file = images.Locate("returns",name);
        return PhysicalFile(file.Path,file.ContentType);
    }

    [HttpPost("manage/returns/{id}/action")]
    public Task<object> Process(string id,JsonElement body,CancellationToken token) => returns.Process(Id(id),session.Require("manager").Id,body,token);

    [HttpGet("manage/refunds")]
    public Task<List<dynamic>> Refunds(CancellationToken token)
    {
        session.Require("manager");
        return returns.Refunds(token);
    }

    [HttpPost("manage/refunds/{id}/{operation:regex(^(confirm|retry|failure)$)}")]
    public Task<object> Refund(string id,string operation,JsonElement body,CancellationToken token) =>
        returns.Refund(Id(id),session.Require("manager").Id,operation,body,token);
}
