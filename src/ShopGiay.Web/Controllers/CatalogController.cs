using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ShopGiay.Web.Infrastructure;
using ShopGiay.Web.Services;

namespace ShopGiay.Web.Controllers;
[ApiController]
[Route("api")]
public sealed class CatalogController(CatalogService service) : ShopController
{
    // Tìm kiếm và lọc: React GET /api/catalog → service.Browse → Repository → MySQL.
    [HttpGet("catalog")]
    [ApiAccess("public")]
    public async Task<object> Browse() => await service.Browse(Query());
    // Chi tiết sản phẩm: React GET /api/products/{id} → service.Details → Repository → MySQL.
    [HttpGet("products/{id}")]
    [ApiAccess("public")]
    public async Task<object> Details([FromRoute] long id) => await service.Details(id);
    // Quản lý sản phẩm và biến thể: React GET /api/manage/products → service.ListProducts → Repository → MySQL.
    [HttpGet("manage/products")]
    [ApiAccess("manager")]
    public async Task<object> ListProducts() => await service.ListProducts();
    // Quản lý sản phẩm và biến thể: React GET /api/manage/variants → service.ListVariants → Repository → MySQL.
    [HttpGet("manage/variants")]
    [ApiAccess("manager")]
    public async Task<object> ListVariants() => await service.ListVariants();
    // Quản lý sản phẩm và biến thể: React GET /api/manage/lookups → service.Lookups → Repository → MySQL.
    [HttpGet("manage/lookups")]
    [ApiAccess("manager")]
    public async Task<object> Lookups() => await service.Lookups();
    // Quản lý sản phẩm và biến thể: React POST /api/manage/sizes → service.CreateSize → Repository → MySQL.
    [HttpPost("manage/sizes")]
    [ApiAccess("manager")]
    public async Task<object> CreateSize([FromBody] JsonElement body) => await service.CreateSize(Row.From(body));
    // Quản lý sản phẩm và biến thể: React POST /api/manage/colors → service.CreateColor → Repository → MySQL.
    [HttpPost("manage/colors")]
    [ApiAccess("manager")]
    public async Task<object> CreateColor([FromBody] JsonElement body) => await service.CreateColor(Row.From(body));
    // Quản lý sản phẩm và biến thể: React POST /api/manage/products → service.CreateProduct → Repository → MySQL.
    [HttpPost("manage/products")]
    [ApiAccess("manager")]
    public async Task<object> CreateProduct([FromBody] JsonElement body) => await service.CreateProduct(Row.From(body));
    // Quản lý sản phẩm và biến thể: React PUT /api/manage/products/{id} → service.UpdateProduct → Repository → MySQL.
    [HttpPut("manage/products/{id}")]
    [ApiAccess("manager")]
    public async Task<object> UpdateProduct([FromRoute] long id, [FromBody] JsonElement body) => await service.UpdateProduct(id, Row.From(body));
    // Quản lý sản phẩm và biến thể: React DELETE /api/manage/products/{id} → service.DeleteItem → Repository → MySQL.
    [HttpDelete("manage/products/{id}")]
    [ApiAccess("manager")]
    public async Task<object> DeleteProduct([FromRoute] long id) => await service.DeleteItem(id, "SanPham", "SanPhamID");
    // Quản lý sản phẩm và biến thể: React POST /api/manage/variants → service.CreateVariant → Repository → MySQL.
    [HttpPost("manage/variants")]
    [ApiAccess("manager")]
    public async Task<object> CreateVariant([FromBody] JsonElement body) => await service.CreateVariant(Row.From(body));
    // Quản lý sản phẩm và biến thể: React PUT /api/manage/variants/{id} → service.UpdateVariant → Repository → MySQL.
    [HttpPut("manage/variants/{id}")]
    [ApiAccess("manager")]
    public async Task<object> UpdateVariant([FromRoute] long id, [FromBody] JsonElement body) => await service.UpdateVariant(id, Row.From(body));
    // Quản lý sản phẩm và biến thể: React DELETE /api/manage/variants/{id} → service.DeleteVariant → Repository → MySQL.
    [HttpDelete("manage/variants/{id}")]
    [ApiAccess("manager")]
    public async Task<object> DeleteVariant([FromRoute] long id) => await service.DeleteVariant(id);
    // Tạo danh mục hoặc thương hiệu: React POST /api/manage/categories → service.CreateTaxonomy → Repository → MySQL.
    [HttpPost("manage/categories")]
    [ApiAccess("manager")]
    public async Task<object> CreateCategory([FromBody] JsonElement body) => await service.CreateTaxonomy(Row.From(body), "DanhMuc");
    // Sửa danh mục hoặc thương hiệu: React PUT /api/manage/categories/{id} → service.UpdateTaxonomy → Repository → MySQL.
    [HttpPut("manage/categories/{id}")]
    [ApiAccess("manager")]
    public async Task<object> UpdateCategory([FromRoute] long id, [FromBody] JsonElement body) => await service.UpdateTaxonomy(id, Row.From(body), "DanhMuc", "DanhMucID");
    // Xóa mục đã ẩn: React DELETE /api/manage/categories/{id} → service.DeleteItem → Repository → MySQL.
    [HttpDelete("manage/categories/{id}")]
    [ApiAccess("manager")]
    public async Task<object> DeleteCategory([FromRoute] long id) => await service.DeleteItem(id, "DanhMuc", "DanhMucID");
    // Tạo danh mục hoặc thương hiệu: React POST /api/manage/brands → service.CreateTaxonomy → Repository → MySQL.
    [HttpPost("manage/brands")]
    [ApiAccess("manager")]
    public async Task<object> CreateBrand([FromBody] JsonElement body) => await service.CreateTaxonomy(Row.From(body), "ThuongHieu");
    // Sửa danh mục hoặc thương hiệu: React PUT /api/manage/brands/{id} → service.UpdateTaxonomy → Repository → MySQL.
    [HttpPut("manage/brands/{id}")]
    [ApiAccess("manager")]
    public async Task<object> UpdateBrand([FromRoute] long id, [FromBody] JsonElement body) => await service.UpdateTaxonomy(id, Row.From(body), "ThuongHieu", "ThuongHieuID");
    // Xóa mục đã ẩn: React DELETE /api/manage/brands/{id} → service.DeleteItem → Repository → MySQL.
    [HttpDelete("manage/brands/{id}")]
    [ApiAccess("manager")]
    public async Task<object> DeleteBrand([FromRoute] long id) => await service.DeleteItem(id, "ThuongHieu", "ThuongHieuID");
}
