using ShopGiay.Web.Infrastructure;

namespace ShopGiay.Web.Domain;
// Thực thể tồn kho có hành vi bảo vệ bất biến: 0 <= phân bổ <= tồn vật lý.
public sealed class InventoryBalance(long onHand, long reserved)
{
    public long OnHand { get; private set; } = onHand;
    public long Reserved { get; private set; } = reserved;
    public long Available => OnHand - Reserved;

    // Operations.StockLog gọi trước khi lưu; chỉ thực thể này quyết định biến động có hợp lệ hay không.
    public void Apply(long stockChange, long reservedChange)
    {
        // Tạo biến total để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var total = OnHand + stockChange;
        // Tạo biến held để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var held = Reserved + reservedChange;
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (total < held || held < 0)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Không đủ tồn khả dụng hoặc số phân bổ không hợp lệ.");
        // Gán kết quả vào OnHand để bước xử lý tiếp theo dùng dữ liệu mới.
        OnHand = total;
        // Gán kết quả vào Reserved để bước xử lý tiếp theo dùng dữ liệu mới.
        Reserved = held;
    }
}
