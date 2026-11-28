using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using ShopGiay.Infrastructure;

namespace ShopGiay.Boundary;

[ApiController]
public sealed class HealthController(Sql sql) : ControllerBase
{
    [HttpGet("/api/health")]
    public async Task<IActionResult> Health(CancellationToken token)
    {
        try {
            await sql.Run(db => db.Value<int>("SELECT 1"),token);
            return Ok(new { status = "ok", database = "connected" });
        } catch (MySqlException) {
            return StatusCode(503,new {
                status = "setup_required",
                message = "Chưa kết nối MySQL. Kiểm tra cấu hình trong .env và khởi tạo database."
            });
        }
    }
}
