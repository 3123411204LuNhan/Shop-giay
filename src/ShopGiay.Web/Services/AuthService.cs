using System.Text.RegularExpressions;
using ShopGiay.Web.Infrastructure;
using ShopGiay.Web.Repositories;
using static ShopGiay.Web.Infrastructure.Input;

namespace ShopGiay.Web.Services;
public sealed class AuthService(IAuthRepository repository, Database db, SessionContext session, IPasswordHasher passwords, PasswordResetService reset, Operations operations)
{
    // AuthController → phiên được Middleware nhận diện; React đọc user và roles.
    public object Me() => new
    {
        user = session.User
    };
    // Đăng nhập: xác minh mật khẩu → kiểm tra khóa/vai trò → tạo phiên đúng khu vực.
    public async Task<LoginResult> Login(Row body)
    {
        // Tạo biến login để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var login = Text(body["login"]);
        // Tạo biến user để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var user = (await repository.FindLogin([login, login])).FirstOrDefault();
        // Tạo biến password để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var password = body.S("password");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (user is null || password.Length > 128 || !passwords.Verify(password, user.S("hash")))
        {
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (user is not null)
                // Gọi AuthRepository.RecordLoginFailure → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
                await repository.RecordLoginFailure([user.L("id")]);
            // Trả HTTP 401 bình thường để người dùng sửa và gửi lại; không ném lỗi dự kiến vào debugger.
            return await RejectLogin("Email hoặc mật khẩu không đúng.", 401);
        }

        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (user.S("status") != "HOAT_DONG" || user.D("locked") > DateTime.UtcNow)
            // Giữ nguyên quy tắc khóa tài khoản và trả thông báo trên biểu mẫu.
            return await RejectLogin("Tài khoản đang bị khóa.", 403);
        // Tạo biến roles để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var roles = (await repository.Roles([user.L("id")])).Select(x => x.S("role")).ToArray();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (!SessionContext.CanUse(roles, session.Area))
            // Từ chối sai khu vực nhưng vẫn cho phép đăng nhập lại ở /manager hoặc /admin.
            return await RejectLogin("Tài khoản không có quyền đăng nhập khu vực này. Quản lý đăng nhập tại /manager; kỹ thuật đăng nhập tại /admin.", 403);
        // SessionContext.Create xử lý phiên và tài khoản của HTTP request hiện tại.
        await session.Create(user.L("id"));
        // Gọi AuthRepository.RecordLogin → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.RecordLogin([user.L("id")]);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return LoginResult.Accepted(session.Area);
    }

    // Giữ nhật ký từ chối đăng nhập dù lỗi dự kiến được trả bằng kết quả thay vì exception.
    private async Task<LoginResult> RejectLogin(string message, int statusCode)
    {
        // Ghi metadata API; không ghi email/mật khẩu từ biểu mẫu vào log.
        await db.Query("INSERT INTO NhatKyHeThong(NguoiThucHienID,HanhDong,DoiTuong,NoiDung,KetQua) VALUES(?,'YEU_CAU_THAT_BAI','API',?,'THAT_BAI')", [session.User?["id"], "POST /api/auth/login · HTTP " + statusCode]);
        // Controller nhận HTTP 401/403 để React giữ biểu mẫu và cho phép thử lại.
        return LoginResult.Rejected(message, statusCode);
    }

    // Tạo tài khoản và vai trò CUSTOMER trong cùng giao dịch.
    public async Task<object> Register(Row body)
    {
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (session.Area != "customer")
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Vui lòng đăng ký tại khu vực mua sắm.", 403);
        // Tạo biến email để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var email = Required(body["email"], "email").ToLowerInvariant();
        // Thực hiện bước ValidateEmail trong AuthService; tiếp tục theo luồng của hàm.
        ValidateEmail(email);
        // Thực hiện bước ValidatePassword trong AuthService; tiếp tục theo luồng của hàm.
        ValidatePassword(body.S("password"));
        // Database.Transaction bao các thao tác trong COMMIT/ROLLBACK để không ghi dữ liệu dở dang.
        var id = await db.Transaction(async c =>
        {
            // Gọi AuthRepository.CreateCustomer → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
            var result = await repository.CreateCustomer([email, email, passwords.Hash(body.S("password")), Required(body["name"], "họ tên"), Text(body["phone"], 20)is { Length: > 0 } phone ? phone : null ], c);
            // Gọi AuthRepository.AssignCustomerRole → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
            await repository.AssignCustomerRole([result.InsertId], c);
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return result.InsertId;
        });
        // SessionContext.Create xử lý phiên và tài khoản của HTTP request hiện tại.
        await session.Create(id);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return new
        {
            ok = true,
            area = "customer"
        };
    }

    // Chỉ thu hồi phiên hiện tại; giữ phiên ở khu vực khác.
    public async Task<object> Logout()
    {
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (Regex.IsMatch(session.Token, "^[a-f0-9]{64}$"))
            // Gọi AuthRepository.RevokeSession → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
            await repository.RevokeSession([SessionContext.Hash(session.Token)]);
        // SessionContext.Clear xử lý phiên và tài khoản của HTTP request hiện tại.
        session.Clear();
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    }

    // Hồ sơ khách hàng → UPDATE TaiKhoan.
    public async Task<object> UpdateProfile(Row body)
    {
        // Gọi AuthRepository.UpdateProfile → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.UpdateProfile([Required(body["name"], "họ tên"), Text(body["phone"], 20), Text(body["address"], 500), session.UserId]);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    }

    // Nhân sự tự đổi mật khẩu, thu hồi toàn bộ phiên và mã khôi phục cũ.
    public async Task<object> ChangeStaffPassword(Row body)
    {
        // Thực hiện bước ValidatePassword trong AuthService; tiếp tục theo luồng của hàm.
        ValidatePassword(body.S("password"));
        // Database.Transaction bao các thao tác trong COMMIT/ROLLBACK để không ghi dữ liệu dở dang.
        await db.Transaction(async c =>
        {
            // Thực hiện bước (await repository.LockActiveUser([session.UserId],c)).One trong AuthService; tiếp tục theo luồng của hàm.
            (await repository.LockActiveUser([session.UserId], c)).One();
            // Thực hiện bước (await repository.LockSession([session.UserId,SessionContext.Hash(session.Token)],c)).One trong AuthService; tiếp tục theo luồng của hàm.
            (await repository.LockSession([session.UserId, SessionContext.Hash(session.Token)], c)).One();
            // Thực hiện bước reset.SavePassword trong AuthService; tiếp tục theo luồng của hàm.
            await reset.SavePassword(c, session.UserId, body.S("password"));
            // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
            await operations.Audit(session.UserId, "DOI_MAT_KHAU", "TaiKhoan", session.UserId, "Nhân sự tự đổi mật khẩu", c);
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return true;
        });
        // SessionContext.Clear xử lý phiên và tài khoản của HTTP request hiện tại.
        session.Clear();
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    }

    // Validation dùng chung bởi đăng ký và quản trị tài khoản.
    public static void ValidateEmail(string email)
    {
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (!Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Email không hợp lệ.");
    }

    // Mật khẩu giữ chính sách 8–128 ký tự của dự án hiện có.
    public static void ValidatePassword(string password)
    {
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (password.Length is < 8 or > 128)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Mật khẩu cần từ 8 đến 128 ký tự.");
    }
}
