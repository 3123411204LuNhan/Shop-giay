using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using MySqlConnector;

namespace ShopGiay.Web.Infrastructure;
// Cấu hình cục bộ; không thay đổi tài khoản hoặc mật khẩu trên MySQL server.
public static class DatabaseConfiguration
{
    // --db-config: nhập thông tin, xác minh đăng nhập server rồi mới lưu file local.
    public static async Task Configure(Database db, IWebHostEnvironment environment)
    {
        // Cần terminal tương tác để nhập mật khẩu ẩn, tránh ghi mật khẩu vào lịch sử lệnh.
        if (Console.IsInputRedirected)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new InvalidOperationException("Hãy chạy --db-config trong CMD/PowerShell tương tác để nhập mật khẩu ẩn.");
        // Lấy mặc định host/port/user/database từ cấu hình hiện tại.
        var current = new MySqlConnectionStringBuilder(db.ConnectionString);
        // Thực hiện bước Console.WriteLine trong DatabaseConfiguration; tiếp tục theo luồng của hàm.
        Console.WriteLine("Cấu hình kết nối MySQL (không đổi mật khẩu MySQL).");
        // Gán kết quả vào current.Server để bước xử lý tiếp theo dùng dữ liệu mới.
        current.Server = Read("Máy chủ", current.Server);
        // Cổng TCP chỉ nhận số trong phạm vi hợp lệ.
        if (!uint.TryParse(Read("Cổng", current.Port.ToString()), out var port) || port is < 1 or > 65535)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new InvalidOperationException("Cổng MySQL phải từ 1 đến 65535.");
        // Gán kết quả vào current.Port để bước xử lý tiếp theo dùng dữ liệu mới.
        current.Port = port;
        // Gán kết quả vào current.UserID để bước xử lý tiếp theo dùng dữ liệu mới.
        current.UserID = Read("Tài khoản MySQL", current.UserID);
        // Giữ tên database cũ nếu muốn dùng lại dữ liệu dự án Node.js.
        current.Database = Read("Database (giữ tên cũ nếu dùng dữ liệu hiện có)", current.Database);
        // Enter trống chỉ hợp lệ khi tài khoản MySQL thực sự không có mật khẩu.
        Console.Write("Mật khẩu MySQL (gõ ẩn): ");
        // Gán kết quả vào current.Password để bước xử lý tiếp theo dùng dữ liệu mới.
        current.Password = ReadPassword();
        // Kiểm tra server trước vì database mới có thể chưa tồn tại.
        var server = new MySqlConnectionStringBuilder(current.ConnectionString)
        {
            Database = ""
        };
        // Mở tài nguyên và tự giải phóng khi ra khỏi phạm vi xử lý.
        await using (var connection = new MySqlConnection(server.ConnectionString))
            // Thực hiện bước connection.OpenAsync trong DatabaseConfiguration; tiếp tục theo luồng của hàm.
            await connection.OpenAsync();
        // Giữ nguyên Store/Mail trong cấu hình local đang có.
        var path = Path.Combine(environment.ContentRootPath, "appsettings.Local.json");
        // Tạo biến previous để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var previous = File.Exists(path) ? await File.ReadAllTextAsync(path) : null;
        // Tạo biến json để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var json = Merge(previous, current.ConnectionString);
        // Chỉ ghi sau khi đăng nhập thành công; không in connection string hoặc mật khẩu.
        await File.WriteAllTextAsync(path, json, new UTF8Encoding(false));
        // Thực hiện bước Console.WriteLine trong DatabaseConfiguration; tiếp tục theo luồng của hàm.
        Console.WriteLine("Đăng nhập MySQL thành công. Đã lưu: " + path);
        // Thực hiện bước Console.WriteLine trong DatabaseConfiguration; tiếp tục theo luồng của hàm.
        Console.WriteLine("Đây là xác minh tài khoản MySQL; database và bảng của website chưa được xác minh.");
        Console.WriteLine("Tiếp theo chạy --db-check. Nếu database chưa tồn tại, chạy --db-setup rồi --db-seed.");
        // Biến môi trường có ưu tiên cao hơn file local nên cần báo nếu đang ghi đè.
        if (Environment.GetEnvironmentVariable("ConnectionStrings__ShopGiay")is not null)
            // Thực hiện bước Console.WriteLine trong DatabaseConfiguration; tiếp tục theo luồng của hàm.
            Console.WriteLine("Lưu ý: ConnectionStrings__ShopGiay đang ghi đè file local. Hãy cập nhật hoặc bỏ biến này trước lần chạy sau.");
    }

    // --db-check chỉ đọc database và đếm bảng, không tạo/sửa dữ liệu.
    public static async Task Check(Database db)
    {
        // Tạo biến options để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var options = new MySqlConnectionStringBuilder(db.ConnectionString);
        // Tạo biến tables để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var tables = (await db.Query("SELECT COUNT(*) n FROM information_schema.tables WHERE table_schema=DATABASE()")).One().L("n");
        // Kết nối được database rỗng chưa đủ để chạy các API shop giày.
        var core = (await db.Query("SELECT COUNT(*) n FROM information_schema.tables WHERE table_schema=DATABASE() AND table_name IN ('TaiKhoan','SanPham','BienTheSanPham','DonHang','TonKho','VaiTro')")).One().L("n");
        if (core != 6)
            throw new InvalidOperationException($"Đã kết nối MySQL nhưng database {options.Database} thiếu bảng của shop giày. Database rỗng: chạy --db-setup rồi --db-seed. Nếu có dữ liệu cũ, kiểm tra đúng tên database và schema trước khi migrate.");
        // Thực hiện bước Console.WriteLine trong DatabaseConfiguration; tiếp tục theo luồng của hàm.
        Console.WriteLine($"Kết nối thành công: {options.Server}:{options.Port}, database {options.Database}, {tables} bảng.");
    }

    // Serializer giữ đúng mật khẩu có dấu nháy/gạch chéo và các section cấu hình khác.
    public static string Merge(string? previous, string connectionString)
    {
        // Tạo biến root để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var root = previous is null ? new JsonObject() : JsonNode.Parse(previous) as JsonObject ?? throw new InvalidOperationException("appsettings.Local.json phải là đối tượng JSON.");
        // Chỉ thay ConnectionStrings:ShopGiay, giữ cấu hình khác hiện có.
        if (root["ConnectionStrings"] is null)
            // Gán kết quả vào root["ConnectionStrings"] để bước xử lý tiếp theo dùng dữ liệu mới.
            root["ConnectionStrings"] = new JsonObject();
        // Gán kết quả vào root["ConnectionStrings"]!["ShopGiay"] để bước xử lý tiếp theo dùng dữ liệu mới.
        root["ConnectionStrings"]!["ShopGiay"] = connectionString;
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    // Thông báo an toàn không chứa mật khẩu, SQL hoặc connection string.
    public static string Explain(MySqlException error) => error.Number switch
    {
        1045 => "MySQL từ chối đăng nhập. Kiểm tra user/mật khẩu. 'using password: NO' nghĩa là ứng dụng chưa gửi mật khẩu; 'YES' nghĩa là đã gửi nhưng đăng nhập bị từ chối.",
        1044 => "Tài khoản MySQL chưa có quyền truy cập database này. Dùng tài khoản được cấp quyền phù hợp.",
        1049 => "Database chưa tồn tại hoặc tên chưa đúng. Dùng đúng tên database cũ, hoặc chạy --db-setup để tạo database mới.",
        1146 => "Database đang thiếu bảng của shop giày. Kiểm tra đúng tên database; database mới cần --db-setup rồi --db-seed.",
        1054 => "Schema database chưa khớp phiên bản hiện tại (thiếu cột). Kiểm tra database và chạy --db-migrate sau khi sao lưu.",
        _ => $"Không hoàn tất được thao tác MySQL (mã {error.Number}). Kiểm tra dịch vụ MySQL, host/cổng và quyền truy cập."};
    // Nhập trường thông thường; Enter giữ giá trị mặc định.
    private static string Read(string label, string fallback)
    {
        // Thực hiện bước Console.Write trong DatabaseConfiguration; tiếp tục theo luồng của hàm.
        Console.Write($"{label} [{fallback}]: ");
        // Tạo biến value để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var value = Console.ReadLine()?.Trim();
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return string.IsNullOrEmpty(value) ? fallback : value;
    }

    // Đọc phím trực tiếp, không echo mật khẩu ra màn hình hoặc log.
    private static string ReadPassword()
    {
        // Tạo biến value để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var value = new StringBuilder();
        // Tiếp tục vòng xử lý cho tới khi hết dữ liệu hoặc ứng dụng yêu cầu dừng.
        while (true)
        {
            // Tạo biến key để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var key = Console.ReadKey(intercept: true);
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (key.Key == ConsoleKey.Enter)
            {
                // Thực hiện bước Console.WriteLine trong DatabaseConfiguration; tiếp tục theo luồng của hàm.
                Console.WriteLine();
                // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
                return value.ToString();
            }

            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (key.Key == ConsoleKey.Backspace)
            {
                // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
                if (value.Length > 0)
                    // Thực hiện bước xử lý hiện tại trong DatabaseConfiguration; tiếp tục theo luồng của hàm.
                    value.Length--;
            }
            else // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (!char.IsControl(key.KeyChar))
                // Thực hiện bước value.Append trong DatabaseConfiguration; tiếp tục theo luồng của hàm.
                value.Append(key.KeyChar);
        }
    }
}
