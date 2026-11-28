using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ShopGiay.Web.Infrastructure;
// Dữ liệu hợp đồng JSON và kết quả SQL; tên khóa giữ đúng giao diện React hiện có.
public sealed class Row : Dictionary<string, object?>
{
    // Đọc khóa không có sẽ trả null, tương đương trường tùy chọn của API cũ.
    public new object? this[string key] { get => TryGetValue(key, out var value) ? value : null; set => base[key] = value; }

    // Chuỗi dùng cho kiểm tra dữ liệu và thông báo nghiệp vụ.
    public string S(string key, string fallback = "") => this[key] is null ? fallback : Convert.ToString(this[key], CultureInfo.InvariantCulture) ?? fallback;
    // Tiền dùng decimal để tránh sai số dấu phẩy động.
    public decimal N(string key, decimal fallback = 0) => decimal.TryParse(S(key), NumberStyles.Any, CultureInfo.InvariantCulture, out var n) ? n : fallback;
    // ID và số lượng nguyên từ MySQL hoặc JSON.
    public long L(string key) => checked((long)N(key));
    // Cờ chấp nhận bool của JSON và tinyint của MySQL.
    public bool B(string key) => this[key] is bool b ? b : N(key) != 0;
    // Ngày trong database được lưu theo UTC.
    public DateTime? D(string key) => this[key] is DateTime d ? DateTime.SpecifyKind(d, DateTimeKind.Utc) : DateTime.TryParse(S(key), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var v) ? v : null;
    // Danh sách dòng hàng trong JSON gửi lên.
    public List<Row> Rows(string key) => this[key] as List<Row> ?? [];
    // Parse JSON một lần ở Controller trước khi chuyển tới Service.
    public static Row From(JsonElement element)
    {
        // Tạo biến row để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var row = new Row();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (element.ValueKind != JsonValueKind.Object)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Nội dung yêu cầu phải là đối tượng JSON.");
        // Xử lý từng property trong tập dữ liệu đã lấy/kiểm tra.
        foreach (var property in element.EnumerateObject())
            // Gán kết quả vào row[property.Name] để bước xử lý tiếp theo dùng dữ liệu mới.
            row[property.Name] = Value(property.Value);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return row;
    }

    // Đệ quy chuyển JSON sang kiểu C# để không phụ thuộc JsonDocument đã đóng.
    private static object? Value(JsonElement e) => e.ValueKind switch
    {
        JsonValueKind.String => e.GetString(),
        JsonValueKind.Number => e.GetDecimal(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Object => From(e),
        JsonValueKind.Array => e.EnumerateArray().Select(x => x.ValueKind == JsonValueKind.Object ? From(x) : throw new ApiException("Dòng hàng không hợp lệ.")).ToList(),
        _ => null
    };
}

public sealed class ApiException(string message, int status = 400, string? code = null) : Exception(message)
{
    public int Status { get; } = status;
    public string? Code { get; } = code;
}

public static class Input
{
    // Làm sạch khoảng trắng và giới hạn độ dài trước khi lưu.
    public static string Text(object? value, int max = 1000)
    {
        // Tạo biến s để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var s = Convert.ToString(value, CultureInfo.InvariantCulture)?.Trim() ?? "";
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return s[..Math.Min(max, s.Length)];
    }

    // Kiểm tra trường bắt buộc; Service cung cấp nhãn tiếng Việt.
    public static string Required(object? value, string label) => Text(value)is { Length: > 0 } text ? text : throw new ApiException("Vui lòng nhập " + label + ".");
    // Kiểm tra ID/số lượng theo giới hạn của dự án gốc.
    public static long Positive(object? value) => Integer(value, 1, 100000);
    // Dùng chung cho số lượng, ngưỡng cảnh báo và giá trị giảm.
    public static long Integer(object? value, long min, long max)
    {
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (!decimal.TryParse(Text(value), NumberStyles.Any, CultureInfo.InvariantCulture, out var n) || n != decimal.Truncate(n) || n < min || n > max)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException($"Giá trị phải là số nguyên từ {min} đến {max}.");
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return (long)n;
    }

    // Giá VND được làm tròn tới đồng giống giao diện cũ.
    public static decimal Price(object? value)
    {
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (!decimal.TryParse(Text(value), NumberStyles.Any, CultureInfo.InvariantCulture, out var n) || n <= 0 || n > 10000000000m)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Giá tiền không hợp lệ.");
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Math.Round(n, 0, MidpointRounding.AwayFromZero);
    }

    // Không cho phép URL script trong ảnh sản phẩm/bằng chứng.
    public static string Image(object? value)
    {
        // Tạo biến s để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var s = Text(value, 1001);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (s.Length > 1000 || (s.Length > 0 && !Regex.IsMatch(s, @"^(https?://[^\s/]+|/images/[^\s]+)", RegexOptions.IgnoreCase)))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Nhập URL ảnh HTTP(S) hoặc đường dẫn /images/.");
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return s;
    }

    // Khóa chống lặp giữ nguyên khi người dùng nhấn gửi lại.
    public static string Key(object? value) => Guid.TryParse(Text(value), out _) ? Text(value) : throw new ApiException("Mã chống lặp không hợp lệ.");
    // Mã hiển thị có tiền tố nghiệp vụ và UUID để tránh trùng.
    public static string Code(string prefix) => prefix + "-" + Guid.NewGuid();
    // Giữ hợp đồng thành công của React.
    public static object Ok() => new
    {
        ok = true
    };
}
