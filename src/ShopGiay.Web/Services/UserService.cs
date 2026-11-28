using ShopGiay.Web.Domain;
using ShopGiay.Web.Infrastructure;
using ShopGiay.Web.Repositories;
using static ShopGiay.Web.Infrastructure.Input;

namespace ShopGiay.Web.Services;
public sealed class UserService(IUserRepository repository, Database db, SessionContext session, IPasswordHasher passwords, Operations operations)
{
    // Admin xem tài khoản chưa xóa mềm cùng vai trò hiện hành.
    public Task<DbResult> List() => repository.List();
    // Khóa tài khoản trước khi tạo thực thể chứa quy tắc vai trò/xóa.
    private async Task<Account> Load(long id, DbSession c)
    {
        // Tạo biến account để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var account = (await repository.LockAccount(id, c)).One("Tài khoản không tồn tại.");
        // Gọi UserRepository.LockRoles → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        var roles = await repository.LockRoles(id, c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return new(account.S("TrangThai"), roles.Select(x => x.S("role")).ToArray());
    }

    // Tạo khách/quản lý, hash mật khẩu, gán vai trò và ghi nhật ký nguyên tử.
    public Task<object> Create(Row b) => db.Transaction<object>(async c =>
    {
        // Thực hiện bước AuthService.ValidatePassword trong UserService; tiếp tục theo luồng của hàm.
        AuthService.ValidatePassword(b.S("password"));
        // Tạo biến role để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var role = b.S("role");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (role is not ("CUSTOMER" or "SHOP_MANAGER"))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Chỉ được tạo khách hàng hoặc quản lý shop.");
        // Tạo biến email để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var email = Required(b["email"], "email");
        // Thực hiện bước AuthService.ValidateEmail trong UserService; tiếp tục theo luồng của hàm.
        AuthService.ValidateEmail(email);
        // Gọi UserRepository.Insert → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        var a = await repository.Insert(email, passwords.Hash(b.S("password")), Required(b["name"], "họ tên"), c);
        // Gọi UserRepository.AssignRole → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.AssignRole(a.InsertId, session.UserId, role, c);
        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(session.UserId, "TAO_TAI_KHOAN", "TaiKhoan", a.InsertId, role, c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return new
        {
            id = a.InsertId
        };
    });
    // Khóa nhóm admin trước tài khoản đích để hai thao tác đồng thời không loại bỏ admin cuối.
    public Task<object> Update(long id, Row b) => db.Transaction<object>(async c =>
    {
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (id == session.UserId)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Không tự sửa vai trò hoặc khóa tài khoản đang dùng.");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (b.S("status")is not ("HOAT_DONG" or "BI_KHOA"))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Trạng thái không hợp lệ.");
        // Gọi UserRepository.LockActiveAdmins → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        var admins = await repository.LockActiveAdmins(c);
        // Tạo biến user để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var user = await Load(id, c);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (user.Roles.Length == 0)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Không tìm thấy tài khoản có vai trò.", 404);
        // Tạo biến role để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var role = b.ContainsKey("role") ? b.S("role") : null;
        // Tạo biến change để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var change = user.NeedsRoleChange(role);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (admins.Count == 1 && admins[0].L("id") == id && (b.S("status") != "HOAT_DONG" || (change && role != "SYSTEM_ADMIN")))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Không thể vô hiệu hóa quản trị viên cuối cùng.");
        // Gọi UserRepository.UpdateProfile → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.UpdateProfile(Required(b["name"], "họ tên"), b.S("status"), id, c);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (change)
            // Gọi UserRepository.ChangeRole → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
            await repository.ChangeRole(role!, session.UserId, id, c);
        // Gọi UserRepository.RevokeSessions → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.RevokeSessions(id, c);
        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(session.UserId, "SUA_PHAN_QUYEN", "TaiKhoan", id, (role ?? string.Join(", ", user.Roles)) + " / " + b.S("status"), c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    });
    // Xóa mềm giữ ID trong đơn; vô hiệu toàn bộ phiên và mã khôi phục.
    public Task<object> Remove(long id) => db.Transaction<object>(async c =>
    {
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (id == session.UserId)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Không thể xóa tài khoản đang đăng nhập.", 403);
        // Tạo biến user để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var user = await Load(id, c);
        // Thực hiện bước user.AssertDeletable trong UserService; tiếp tục theo luồng của hàm.
        user.AssertDeletable();
        // Gọi UserRepository.Anonymize → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.Anonymize("deleted-" + Guid.NewGuid(), passwords.Hash(Guid.NewGuid().ToString()), id, c);
        // Gọi UserRepository.RevokeActiveSessions → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.RevokeActiveSessions(id, c);
        // Gọi UserRepository.InvalidateResetCodes → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.InvalidateResetCodes(id, c);
        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(session.UserId, "XOA_TAI_KHOAN", "TaiKhoan", id, string.Join(", ", user.Roles), c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    });
}
