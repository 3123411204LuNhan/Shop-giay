using ShopGiay.Web.Infrastructure;

namespace ShopGiay.Web.Repositories;
// Hợp đồng truy cập dữ liệu được Service sử dụng; bản triển khai nằm trong ReportRepository.Cs.
public interface IReportRepository
{
    // Service gọi Customers qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Customers();
    // Service gọi Income qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Income(object? [] values);
    // Service gọi Refunded qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Refunded(object? [] values);
    // Service gọi OrderCounts qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> OrderCounts(object? [] values);
    // Service gọi DailyIncome qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> DailyIncome(object? [] values);
    // Service gọi BestSellers qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> BestSellers(object? [] values);
    // Service gọi LowStock qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LowStock();
    // Service gọi Logs qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Logs();
    // Service gọi DatabaseInfo qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> DatabaseInfo();
    // Service gọi TableCount qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> TableCount();
    // Service gọi ActiveSessions qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> ActiveSessions();
    // Service gọi AccountCount qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> AccountCount();
}
