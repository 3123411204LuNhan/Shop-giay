using System.Globalization;
using ShopGiay.Web.Infrastructure;
using ShopGiay.Web.Repositories;

namespace ShopGiay.Web.Services;
public sealed class ReportService(IReportRepository repository, StoreSettings settings)
{
    // Cấu hình ngân hàng, hạn giữ hàng và chính sách trả hàng cho khách.
    public object Settings() => settings.Public();
    // Khách hàng và lịch sử giá trị đơn cho quản lý.
    public Task<DbResult> Customers() => repository.Customers();
    // Nhật ký chỉ dành cho quản trị kỹ thuật.
    public Task<DbResult> Logs() => repository.Logs();
    // Chỉ số hệ thống không chứa bí mật kết nối.
    public async Task<object> SystemInfo() => new
    {
        database = (await repository.DatabaseInfo()).One(),
        tables = (await repository.TableCount()).One().L("n"),
        sessions = (await repository.ActiveSessions()).One().L("n"),
        accounts = (await repository.AccountCount()).One().L("n")
    };
    // Khoảng ngày theo UTC+7 → đổi UTC khi truy vấn; doanh thu là tiền thực thu trừ hoàn đã xác nhận.
    public async Task<object> Sales(Row q)
    {
        // Tạo biến from để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var from = q.S("from", DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd"));
        // Tạo biến to để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var to = q.S("to", DateTime.UtcNow.ToString("yyyy-MM-dd"));
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (!DateTime.TryParseExact(from, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var a) || !DateTime.TryParseExact(to, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var b) || a > b)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Khoảng ngày không hợp lệ.");
        // Tạo biến args để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        object? [] args = [DateTime.SpecifyKind(a.AddHours(-7), DateTimeKind.Utc), DateTime.SpecifyKind(b.AddDays(1).AddHours(-7), DateTimeKind.Utc)];
        // Tạo biến income để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var income = (await repository.Income(args)).One().N("value");
        // Tạo biến refund để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var refund = (await repository.Refunded(args)).One().N("value");
        // Gọi ReportRepository.OrderCounts → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        return new
        {
            from,
            to,
            income,
            refund,
            revenue = income - refund,
            orders = await repository.OrderCounts(args),
            daily = await repository.DailyIncome(args),
            top = await repository.BestSellers(args),
            lowStock = (await repository.LowStock()).One().L("n")
        };
    }
}
