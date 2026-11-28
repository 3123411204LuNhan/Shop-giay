using ShopGiay.Web.Domain;
using ShopGiay.Web.Infrastructure;
using System.Text.Json;

// Kiểm thử độc lập database, chạy bằng dotnet run --project tests/ShopGiay.Tests.
var passed = 0;
// Mỗi assertion báo rõ quy tắc lỗi để sinh viên lần lại lớp cần sửa.
void Check(string name, bool condition)
{
    // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
    if (!condition)
        // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
        throw new Exception("FAIL: " + name);
    // Thực hiện bước Console.WriteLine trong Program; tiếp tục theo luồng của hàm.
    Console.WriteLine("PASS: " + name);
    // Thực hiện bước xử lý hiện tại trong Program; tiếp tục theo luồng của hàm.
    passed++;
}

// Kiểm tra nghiệp vụ phải từ chối và giữ đúng HTTP status.
void Reject(string name, Action action, int status = 400)
{
    // Bắt đầu vùng xử lý có bắt lỗi/giải phóng tài nguyên ở các nhánh bên dưới.
    try
    {
        // Thực hiện bước action trong Program; tiếp tục theo luồng của hàm.
        action();
        // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
        throw new Exception("Không từ chối: " + name);
    }
    catch (ApiException e)
    {
        // Thực hiện bước Check trong Program; tiếp tục theo luồng của hàm.
        Check(name, e.Status == status);
    }
}

// Tạo biến hash để dùng ở các bước tiếp theo trong phạm vi hiện tại.
var hash = new PasswordHasher();
// Tạo biến encoded để dùng ở các bước tiếp theo trong phạm vi hiện tại.
var encoded = hash.Hash("TiếngViệt@123");
// Thực hiện bước Check trong Program; tiếp tục theo luồng của hàm.
Check("PBKDF2 xác minh đúng", hash.Verify("TiếngViệt@123", encoded));
// Thực hiện bước Check trong Program; tiếp tục theo luồng của hàm.
Check("PBKDF2 từ chối sai mật khẩu", !hash.Verify("wrong", encoded));
// Thực hiện bước Check trong Program; tiếp tục theo luồng của hàm.
Check("Salt khác nhau mỗi lần", hash.Hash("TiếngViệt@123") != encoded);
// Thực hiện bước Check trong Program; tiếp tục theo luồng của hàm.
Check("Hash hỏng được từ chối", !hash.Verify("pw", "invalid"));
// Vector do node:crypto.scryptSync tạo; kiểm chứng tương thích tài khoản dự án Node.js.
var legacy = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "legacy-vector.txt")).Trim();
// Thực hiện bước Check trong Program; tiếp tục theo luồng của hàm.
Check("Mật khẩu scrypt Node.js cũ", hash.Verify("Legacy@123", legacy));
// Thực hiện bước Check trong Program; tiếp tục theo luồng của hàm.
Check("Scrypt từ chối sai mật khẩu", !hash.Verify("wrong", legacy));
// Thực hiện bước Check trong Program; tiếp tục theo luồng của hàm.
Check("Khách chỉ dùng khu mua sắm", SessionContext.CanUse(["CUSTOMER"], "customer"));
// Thực hiện bước Check trong Program; tiếp tục theo luồng của hàm.
Check("Quản lý không dùng phiên khách", !SessionContext.CanUse(["CUSTOMER", "SHOP_MANAGER"], "customer"));
// Thực hiện bước Check trong Program; tiếp tục theo luồng của hàm.
Check("Admin không có quyền quản lý shop", !SessionContext.CanUse(["SYSTEM_ADMIN"], "manager"));
// Thực hiện bước Check trong Program; tiếp tục theo luồng của hàm.
Check("Vai trò quản lý đúng khu vực", SessionContext.CanUse(["SHOP_MANAGER"], "manager"));
// Thực hiện bước Check trong Program; tiếp tục theo luồng của hàm.
Check("Khu vực lạ bị từ chối", !SessionContext.CanUse(["CUSTOMER"], "__proto__"));
// Thực hiện bước OrderWorkflow.AssertNext trong Program; tiếp tục theo luồng của hàm.
OrderWorkflow.AssertNext("CHO_XAC_NHAN", "DA_XAC_NHAN");
// Thực hiện bước xử lý hiện tại trong Program; tiếp tục theo luồng của hàm.
passed++;
// Thực hiện bước Reject trong Program; tiếp tục theo luồng của hàm.
Reject("Không nhảy từ chờ sang hoàn thành", () => OrderWorkflow.AssertNext("CHO_XAC_NHAN", "HOAN_THANH"));
// Thực hiện bước Reject trong Program; tiếp tục theo luồng của hàm.
Reject("Đơn đã hủy không chạy lại luồng", () => OrderWorkflow.AssertNext("DA_HUY", "CHO_XAC_NHAN"));
// Thực hiện bước Check trong Program; tiếp tục theo luồng của hàm.
Check("Đơn đang giao không được hủy trực tiếp", !OrderWorkflow.CanCancel("DANG_GIAO"));
// Tạo biến stock để dùng ở các bước tiếp theo trong phạm vi hiện tại.
var stock = new InventoryBalance(10, 2);
// Thực hiện bước stock.Apply trong Program; tiếp tục theo luồng của hàm.
stock.Apply(0, 3);
// Thực hiện bước Check trong Program; tiếp tục theo luồng của hàm.
Check("Giữ hàng chỉ giảm khả dụng", stock.OnHand == 10 && stock.Reserved == 5 && stock.Available == 5);
// Thực hiện bước stock.Apply trong Program; tiếp tục theo luồng của hàm.
stock.Apply(-3, -3);
// Thực hiện bước Check trong Program; tiếp tục theo luồng của hàm.
Check("Xuất giao giảm tồn và giữ", stock.OnHand == 7 && stock.Reserved == 2);
// Thực hiện bước Reject trong Program; tiếp tục theo luồng của hàm.
Reject("Không điều chỉnh thấp hơn phân bổ", () => stock.Apply(-6, 0));
// Thực hiện bước Check trong Program; tiếp tục theo luồng của hàm.
Check("Lỗi không đổi thực thể tồn", stock.OnHand == 7 && stock.Reserved == 2);
// Thực hiện bước Reject trong Program; tiếp tục theo luồng của hàm.
Reject("Không cho phân bổ âm", () => stock.Apply(0, -3));
// Tạo biến account để dùng ở các bước tiếp theo trong phạm vi hiện tại.
var account = new Account("BI_KHOA", ["CUSTOMER"]);
// Thực hiện bước account.AssertDeletable trong Program; tiếp tục theo luồng của hàm.
account.AssertDeletable();
// Thực hiện bước xử lý hiện tại trong Program; tiếp tục theo luồng của hàm.
passed++;
// Thực hiện bước Reject trong Program; tiếp tục theo luồng của hàm.
Reject("Khách không nâng thành quản lý", () => account.NeedsRoleChange("SHOP_MANAGER"), 403);
// Thực hiện bước Reject trong Program; tiếp tục theo luồng của hàm.
Reject("Không xóa admin", () => new Account("BI_KHOA", ["SYSTEM_ADMIN"]).AssertDeletable(), 403);
// Thực hiện bước Reject trong Program; tiếp tục theo luồng của hàm.
Reject("Khóa trước khi xóa quản lý", () => new Account("HOAT_DONG", ["SHOP_MANAGER"]).AssertDeletable(), 409);
// Thực hiện bước Check trong Program; tiếp tục theo luồng của hàm.
Check("Chuyển nhân sự hai chiều", new Account("HOAT_DONG", ["SHOP_MANAGER"]).NeedsRoleChange("SYSTEM_ADMIN"));
// Thực hiện bước Reject trong Program; tiếp tục theo luồng của hàm.
Reject("Số lượng không nhận phần lẻ", () => Input.Positive("1.5"));
// Thực hiện bước Reject trong Program; tiếp tục theo luồng của hàm.
Reject("Ảnh không nhận javascript", () => Input.Image("javascript:alert(1)"));
// Thực hiện bước Reject trong Program; tiếp tục theo luồng của hàm.
Reject("Chống lặp yêu cầu UUID", () => Input.Key("bad"));
// Tạo biến json để dùng ở các bước tiếp theo trong phạm vi hiện tại.
using var json = JsonDocument.Parse("{\"active\":true,\"items\":[{\"quantity\":2}],\"price\":123.5}");
// Tạo biến row để dùng ở các bước tiếp theo trong phạm vi hiện tại.
var row = Row.From(json.RootElement);
// Thực hiện bước Check trong Program; tiếp tục theo luồng của hàm.
Check("JSON bool và số decimal", row.B("active") && row.N("price") == 123.5m && row.Rows("items")[0].L("quantity") == 2);
// Thực hiện bước Console.WriteLine trong Program; tiếp tục theo luồng của hàm.
// Kiểm tra round-trip mật khẩu có ký tự đặc biệt và giữ các section cấu hình khác.
var connectionOptions = new MySqlConnector.MySqlConnectionStringBuilder
{
    Server = "127.0.0.1",
    UserID = "root",
    Password = "qa;\"\\special",
    Database = "qa"
};
// Tạo biến localConfig để dùng ở các bước tiếp theo trong phạm vi hiện tại.
using var localConfig = JsonDocument.Parse(DatabaseConfiguration.Merge("{\"Store\":{\"ReturnDays\":14}}", connectionOptions.ConnectionString));
// Tạo biến roundTrip để dùng ở các bước tiếp theo trong phạm vi hiện tại.
var roundTrip = new MySqlConnector.MySqlConnectionStringBuilder(localConfig.RootElement.GetProperty("ConnectionStrings").GetProperty("ShopGiay").GetString()!);
// Thực hiện bước Check trong Program; tiếp tục theo luồng của hàm.
Check("Mật khẩu đặc biệt không bị hỏng khi lưu cấu hình", roundTrip.Password == connectionOptions.Password);
// Thực hiện bước Check trong Program; tiếp tục theo luồng của hàm.
Check("Cấu hình MySQL giữ nguyên Store đang có", localConfig.RootElement.GetProperty("Store").GetProperty("ReturnDays").GetInt32() == 14);
// Báo tổng kiểm tra, không in giá trị chứa mật khẩu.
Console.WriteLine($"Đã qua {passed} kiểm tra C#.");
