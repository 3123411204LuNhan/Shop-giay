using ShopGiay.Web.Infrastructure;

namespace ShopGiay.Web.Domain;
public static class OrderWorkflow
{
    public static readonly string[] Sequence = ["CHO_XAC_NHAN", "DA_XAC_NHAN", "DANG_CHUAN_BI", "DANG_GIAO", "DA_GIAO", "HOAN_THANH"];
    // OrderService dùng quy tắc này trước mọi lần chuyển trạng thái.
    public static void AssertNext(string current, string next)
    {
        // Tạo biến index để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var index = Array.IndexOf(Sequence, current);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (index < 0 || index + 1 >= Sequence.Length || Sequence[index + 1] != next)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Trạng thái chuyển tiếp không hợp lệ.");
    }

    // Hủy trước khi giao; hàng đang giao phải đi qua quy trình nhận hoàn.
    public static bool CanCancel(string status) => status is "CHO_XAC_NHAN" or "DA_XAC_NHAN" or "DANG_CHUAN_BI";
}
