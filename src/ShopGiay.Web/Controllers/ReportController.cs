using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ShopGiay.Web.Infrastructure;
using ShopGiay.Web.Services;

namespace ShopGiay.Web.Controllers;
[ApiController]
[Route("api")]
public sealed class ReportController(ReportService service) : ShopController
{
    // Cấu hình, khách hàng, báo cáo hoặc giám sát: React GET /api/store-settings → service.Settings → Repository → MySQL.
    [HttpGet("store-settings")]
    [ApiAccess("public")]
    public object Settings() => service.Settings();
    // Cấu hình, khách hàng, báo cáo hoặc giám sát: React GET /api/manage/customers → service.Customers → Repository → MySQL.
    [HttpGet("manage/customers")]
    [ApiAccess("manager")]
    public async Task<object> Customers() => await service.Customers();
    // Cấu hình, khách hàng, báo cáo hoặc giám sát: React GET /api/manage/reports → service.Sales → Repository → MySQL.
    [HttpGet("manage/reports")]
    [ApiAccess("manager")]
    public async Task<object> Sales() => await service.Sales(Query());
    // Cấu hình, khách hàng, báo cáo hoặc giám sát: React GET /api/admin/logs → service.Logs → Repository → MySQL.
    [HttpGet("admin/logs")]
    [ApiAccess("admin")]
    public async Task<object> Logs() => await service.Logs();
    // Cấu hình, khách hàng, báo cáo hoặc giám sát: React GET /api/admin/system → service.SystemInfo → Repository → MySQL.
    [HttpGet("admin/system")]
    [ApiAccess("admin")]
    public async Task<object> SystemInfo() => await service.SystemInfo();
}
