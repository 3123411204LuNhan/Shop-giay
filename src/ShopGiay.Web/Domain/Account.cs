using ShopGiay.Web.Infrastructure;

namespace ShopGiay.Web.Domain;
// Thực thể tài khoản đóng gói quy tắc vai trò và xóa mềm trong tài liệu chương 3.
public sealed record Account(string Status, string[] Roles)
{
    // UserService hỏi thực thể trước khi sửa quyền; khách hàng không được chuyển thành nhân sự.
    public bool NeedsRoleChange(string? requested)
    {
        // Tạo biến current để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var current = Roles.Length == 1 ? Roles[0] : null;
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (requested is null || requested == current)
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return false;
        // Tạo biến staff để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        string[] staff = ["SYSTEM_ADMIN", "SHOP_MANAGER"];
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (!staff.Contains(current) || !staff.Contains(requested))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Chỉ đổi vai trò giữa quản lý shop và nhân viên kỹ thuật.", 403);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return true;
    }

    // Bảo toàn lịch sử giao dịch: chỉ tài khoản khách/quản lý đã khóa được xóa mềm.
    public void AssertDeletable()
    {
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (Roles.Length == 0 || Roles.Any(x => x is not ("CUSTOMER" or "SHOP_MANAGER")))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Chỉ xóa tài khoản khách hàng hoặc quản lý shop.", 403);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (Status != "BI_KHOA")
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Hãy khóa tài khoản trước khi xóa.", 409);
    }
}
