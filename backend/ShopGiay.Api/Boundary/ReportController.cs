using Microsoft.AspNetCore.Mvc;
using ShopGiay.Control;
using ShopGiay.Infrastructure;

namespace ShopGiay.Boundary;

[ApiController]
[Route("api")]
public sealed class ReportController(ReportControl reports,StoreSettings settings,SessionControl session) : ControllerBase
{
    [HttpGet("store-settings")]
    public object Settings() => settings.Public;

    [HttpGet("manage/customers")]
    public Task<List<dynamic>> Customers(CancellationToken token)
    {
        session.Require("manager");
        return reports.Customers(token);
    }

    [HttpGet("manage/reports")]
    public Task<object> Sales(CancellationToken token)
    {
        session.Require("manager");
        return reports.Sales(Request.Query,token);
    }

    [HttpGet("admin/logs")]
    public Task<List<dynamic>> Logs(CancellationToken token)
    {
        session.Require("admin");
        return reports.Logs(token);
    }

    [HttpGet("admin/system")]
    public Task<object> SystemInfo(CancellationToken token)
    {
        session.Require("admin");
        return reports.SystemInfo(token);
    }
}
