using System.Text.RegularExpressions;
using MySqlConnector;

namespace ShopGiay.Web.Infrastructure;
// Một giao dịch dùng một kết nối; các Repository nhận cùng đối tượng này.
public sealed record DbSession(MySqlConnection Connection, MySqlTransaction Transaction);
// Kết quả SELECT chứa các dòng; INSERT/UPDATE có thêm ID và số dòng ảnh hưởng.
public sealed class DbResult : List<Row>
{
    public long InsertId { get; set; }
    public int AffectedRows { get; set; }

    // Service gọi One khi nghiệp vụ yêu cầu bản ghi phải tồn tại.
    public Row One(string message = "Không tìm thấy dữ liệu.") => this.FirstOrDefault() ?? throw new ApiException(message, 404);
}

public sealed class Database(IConfiguration config)
{
    // CLI và ứng dụng dùng chung cấu hình; không ghi mật khẩu ra log.
    public string ConnectionString => config.GetConnectionString("ShopGiay") ?? throw new InvalidOperationException("Thiếu ConnectionStrings:ShopGiay.");

    // Repository → Query: thay dấu ? bằng tên tham số, tuyệt đối không ghép dữ liệu người dùng vào SQL.
    public async Task<DbResult> Query(string sql, object? []? values = null, DbSession? connection = null)
    {
        // Tạo biến owned để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        await using var owned = connection is null ? new MySqlConnection(ConnectionString) : null;
        // Tạo biến c để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var c = connection?.Connection ?? owned!;
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (owned is not null)
        {
            // Thực hiện bước c.OpenAsync trong Database; tiếp tục theo luồng của hàm.
            await c.OpenAsync();
            // Tạo biến timezone để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            await using var timezone = new MySqlCommand("SET time_zone = '+00:00'", c);
            // Thực hiện bước timezone.ExecuteNonQueryAsync trong Database; tiếp tục theo luồng của hàm.
            await timezone.ExecuteNonQueryAsync();
        }

        // Tạo biến args để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var args = values ?? [];
        // Tạo biến index để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var index = 0;
        // SQL từ các Repository chỉ dùng ? ở vị trí placeholder.
        var commandText = Regex.Replace(sql, @"\?", _ => "@p" + index++);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (index != args.Length)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new InvalidOperationException("Số tham số SQL không khớp.");
        // Tạo biến command để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        await using var command = new MySqlCommand(commandText, c, connection?.Transaction);
        // Lặp theo chỉ số để xử lý đủ các phần tử, không bỏ sót dòng dữ liệu.
        for (var i = 0; i < args.Length; i++)
            // Thực hiện bước command.Parameters.AddWithValue trong Database; tiếp tục theo luồng của hàm.
            command.Parameters.AddWithValue("@p" + i, args[i] ?? DBNull.Value);
        // Tạo biến result để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var result = new DbResult();
        // Mở tài nguyên và tự giải phóng khi ra khỏi phạm vi xử lý.
        await using (var reader = await command.ExecuteReaderAsync())
        {
            // Tiếp tục vòng xử lý cho tới khi hết dữ liệu hoặc ứng dụng yêu cầu dừng.
            while (await reader.ReadAsync())
            {
                // Tạo biến row để dùng ở các bước tiếp theo trong phạm vi hiện tại.
                var row = new Row();
                // Lặp theo chỉ số để xử lý đủ các phần tử, không bỏ sót dòng dữ liệu.
                for (var i = 0; i < reader.FieldCount; i++)
                    // Gán kết quả vào row[reader.GetName(i)] để bước xử lý tiếp theo dùng dữ liệu mới.
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                // Thực hiện bước result.Add trong Database; tiếp tục theo luồng của hàm.
                result.Add(row);
            }

            // Gán kết quả vào result.AffectedRows để bước xử lý tiếp theo dùng dữ liệu mới.
            result.AffectedRows = reader.RecordsAffected;
        }

        // Gán kết quả vào result.InsertId để bước xử lý tiếp theo dùng dữ liệu mới.
        result.InsertId = command.LastInsertedId;
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return result;
    }

    // Service → Transaction: thành công thì COMMIT; mọi ngoại lệ đều ROLLBACK.
    public async Task<T> Transaction<T>(Func<DbSession, Task<T>> operation)
    {
        // Tạo biến c để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        await using var c = new MySqlConnection(ConnectionString);
        // Thực hiện bước c.OpenAsync trong Database; tiếp tục theo luồng của hàm.
        await c.OpenAsync();
        // Mở tài nguyên và tự giải phóng khi ra khỏi phạm vi xử lý.
        await using (var timezone = new MySqlCommand("SET time_zone = '+00:00'", c))
            // Thực hiện bước timezone.ExecuteNonQueryAsync trong Database; tiếp tục theo luồng của hàm.
            await timezone.ExecuteNonQueryAsync();
        // Tạo biến transaction để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        await using var transaction = await c.BeginTransactionAsync();
        // Bắt đầu vùng xử lý có bắt lỗi/giải phóng tài nguyên ở các nhánh bên dưới.
        try
        {
            // Tạo biến result để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var result = await operation(new(c, transaction));
            // Thực hiện bước transaction.CommitAsync trong Database; tiếp tục theo luồng của hàm.
            await transaction.CommitAsync();
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return result;
        }
        catch
        {
            // Thực hiện bước transaction.RollbackAsync trong Database; tiếp tục theo luồng của hàm.
            await transaction.RollbackAsync();
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw;
        }
    }
}
