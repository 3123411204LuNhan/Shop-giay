using System.Globalization;
using ShopGiay.Infrastructure;
using Q = ShopGiay.Infrastructure.ReportQueries;

namespace ShopGiay.Control;

public sealed class ReportControl(Sql sql)
{
    private static readonly string[] Statuses = ["CHO_XAC_NHAN","DA_XAC_NHAN","DANG_CHUAN_BI","DANG_GIAO","DA_GIAO","HOAN_THANH","DA_HUY"];

    public Task<List<dynamic>> Customers(CancellationToken token) => sql.Run(db => db.Rows(Q.Customers),token);
    public Task<List<dynamic>> Logs(CancellationToken token) => sql.Run(db => db.Rows(Q.Logs),token);
    public async Task<object> SystemInfo(CancellationToken token) => await sql.Run(async db => new {
        database = await db.One(Q.DatabaseInfo), tables = await db.Value<long>(Q.TableCount),
        sessions = await db.Value<long>(Q.ActiveSessions), accounts = await db.Value<long>(Q.AccountCount)
    },token);

    private static DateTime Start(string value)
    {
        if (value.Length != 10 || value[0] == '0' || !DateOnly.TryParseExact(value,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out var day))
            throw new ApiException("Khoảng ngày không hợp lệ.");
        return day.ToDateTime(TimeOnly.MinValue,DateTimeKind.Utc).AddHours(-7);
    }

    public Task<object> Sales(IQueryCollection query,CancellationToken token)
    {
        var now = DateTime.UtcNow.AddHours(7);
        string Get(string key,string fallback) => query.TryGetValue(key,out var raw) ? raw.ToString() : fallback;
        var from = Get("from",now.AddDays(-30).ToString("yyyy-MM-dd",CultureInfo.InvariantCulture));
        var to = Get("to",now.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture));
        var type = Get("type","summary");
        var group = Get("group","product");
        var rank = Get("rank","quantity");
        var start = Start(from);
        var end = Start(to);
        if (start > end || end.Year >= 9999) throw new ApiException("Khoảng ngày không hợp lệ.");
        end = end.AddDays(1);
        if (type is not ("summary" or "revenue" or "orders" or "best-sellers") || group is not ("product" or "variant") || rank is not ("quantity" or "amount"))
            throw new ApiException("Loại hoặc tiêu chí báo cáo không hợp lệ.");
        return sql.Transaction<object>(async db => {
            var result = new Dictionary<string,object?> {
                ["from"] = from, ["to"] = to, ["type"] = type, ["group"] = group, ["rank"] = rank,
                ["currency"] = "VND", ["generatedAt"] = DateTime.UtcNow
            };
            if (type is "summary" or "revenue") {
                var daily = await db.Rows(Q.CashByDay,start,end,start,end);
                var income = daily.Sum(r => (decimal)r.income);
                var refund = daily.Sum(r => (decimal)r.refund);
                result["daily"] = daily;
                result["income"] = income;
                result["refund"] = refund;
                result["revenue"] = income-refund;
            }
            if (type is "summary" or "orders") {
                var rows = await db.Rows(Q.OrderCounts,start,end);
                var counts = rows.ToDictionary(r => (string)r.status,r => Convert.ToInt64((object)r.count));
                result["orders"] = Statuses.Select(status => new { status, count = counts.GetValueOrDefault(status) }).ToArray();
            }
            if (type is "summary" or "best-sellers") {
                var variant = group == "variant";
                var statement = Q.BestSellers.Replace("{slot0}",variant ? "b.BienTheID" : "p.SanPhamID")
                    .Replace("{slot1}",variant ? "b.SKU" : "p.MaSanPham")
                    .Replace("{slot2}",variant ? "CONCAT(p.TenSanPham,' · EU ',k.GiaTri,' · ',m.TenMau)" : "p.TenSanPham")
                    .Replace("{slot3}",variant ? "JOIN KichCo k ON k.KichCoID=b.KichCoID JOIN MauSac m ON m.MauSacID=b.MauSacID" : "")
                    .Replace("{slot4}",rank == "amount" ? "amount DESC, quantity DESC" : "quantity DESC, amount DESC");
                result["top"] = await db.Rows(statement,start,end,end,end,end);
            }
            if (type == "summary") result["lowStock"] = await db.Value<long>(Q.LowStock);
            return result;
        },token);
    }
}
