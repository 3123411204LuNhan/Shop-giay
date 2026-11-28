using Microsoft.AspNetCore.Mvc;
using ShopGiay.Control;

namespace ShopGiay.Boundary;

[ApiController]
[Route("api")]
public sealed class CatalogController(CatalogControl catalog) : ControllerBase
{
    [HttpGet("catalog")]
    public async Task<IActionResult> Browse(CancellationToken token)
    {
        try { return Ok(await catalog.Browse(Request.Query, token)); }
        catch (ArgumentOutOfRangeException) { return BadRequest(new { message = "Bộ lọc không hợp lệ." }); }
    }

    [HttpGet("products/{id}")]
    public async Task<IActionResult> Details(string id, CancellationToken token)
    {
        if (!int.TryParse(id, out var key) || key is < 1 or > 100000)
            return BadRequest(new { message = "Mã sản phẩm không hợp lệ." });
        var product = await catalog.Details(key, token);
        return product is null ? NotFound(new { message = "Không tìm thấy sản phẩm." }) : Ok(product);
    }
}
