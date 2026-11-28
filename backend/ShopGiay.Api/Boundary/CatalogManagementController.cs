using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ShopGiay.Control;
using ShopGiay.Infrastructure;
using static ShopGiay.Infrastructure.ApiInput;

namespace ShopGiay.Boundary;

[ApiController]
[Route("api")]
public sealed class CatalogManagementController(CatalogManagement catalog,SessionControl session,ImageStore images) : ControllerBase
{
    [HttpGet("product-images/{name}")]
    public IActionResult Image(string name)
    {
        var file = images.Locate("products",name);
        return PhysicalFile(file.Path,file.ContentType);
    }

    [HttpGet("manage/products")]
    public Task<List<dynamic>> Products(CancellationToken token)
    {
        session.Require("manager");
        return catalog.Products(token);
    }

    [HttpPost("manage/products")]
    [RequestSizeLimit(5 * 1024 * 1024 + 64 * 1024)]
    public async Task<object> Create(CancellationToken token)
    {
        var user = session.Require("manager");
        var (body,image) = await ImageStore.Read(Request,token);
        return await catalog.CreateProduct(user.Id,body,image,token);
    }

    [HttpPut("manage/products/{id}")]
    [RequestSizeLimit(5 * 1024 * 1024 + 64 * 1024)]
    public async Task<object> Update(string id,CancellationToken token)
    {
        var user = session.Require("manager");
        var (body,image) = await ImageStore.Read(Request,token);
        return await catalog.UpdateProduct(Id(id),user.Id,body,image,token);
    }

    [HttpGet("manage/lookups")]
    public Task<object> Lookups(CancellationToken token)
    {
        session.Require("manager");
        return catalog.Lookups(token);
    }

    [HttpPost("manage/sizes")]
    public Task<object> Size(JsonElement body,CancellationToken token)
    {
        session.Require("manager");
        return catalog.Size(body,token);
    }

    [HttpPost("manage/colors")]
    public Task<object> Color(JsonElement body,CancellationToken token)
    {
        session.Require("manager");
        return catalog.Color(body,token);
    }

    [HttpGet("manage/variants")]
    public Task<List<dynamic>> Variants(string? productId,CancellationToken token)
    {
        session.Require("manager");
        return catalog.Variants(productId,token);
    }

    [HttpPost("manage/variants")]
    public Task<object> CreateVariant(JsonElement body,CancellationToken token) => catalog.CreateVariant(session.Require("manager").Id,body,token);

    [HttpPut("manage/variants/{id}")]
    public Task<object> UpdateVariant(string id,JsonElement body,CancellationToken token) => catalog.UpdateVariant(Id(id),session.Require("manager").Id,body,token);

    [HttpDelete("manage/variants/{id}")]
    public Task<object> DeleteVariant(string id,CancellationToken token) => catalog.DeleteVariant(Id(id),session.Require("manager").Id,token);

    [HttpPost("manage/{key:regex(^(categories|brands)$)}")]
    [HttpPut("manage/{key:regex(^(categories|brands)$)}/{id}")]
    public Task<object> SaveTaxonomy(string key,string? id,JsonElement body,CancellationToken token) =>
        catalog.SaveTaxonomy(key,id is null ? null : Id(id),session.Require("manager").Id,body,token);

    [HttpDelete("manage/{key:regex(^(products|categories|brands)$)}/{id}")]
    public Task<object> DeleteItem(string key,string id,CancellationToken token) => catalog.DeleteItem(key,Id(id),session.Require("manager").Id,token);
}
