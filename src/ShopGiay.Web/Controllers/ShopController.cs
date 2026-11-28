using Microsoft.AspNetCore.Mvc;
using ShopGiay.Web.Infrastructure;

namespace ShopGiay.Web.Controllers;
// Metadata được ApiSecurityMiddleware đọc trước khi Controller được gọi.
[AttributeUsage(AttributeTargets.Method)]
public sealed class ApiAccessAttribute(string area) : Attribute
{
    public string Area { get; } = area;
}

public abstract class ShopController : ControllerBase
{
    // GET query → hợp đồng Row → Service; không đọc SQL tại Controller.
    protected Row Query()
    {
        // Tạo biến q để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var q = new Row();
        // Xử lý từng pair trong tập dữ liệu đã lấy/kiểm tra.
        foreach (var pair in Request.Query)
            // Gán kết quả vào q[pair.Key] để bước xử lý tiếp theo dùng dữ liệu mới.
            q[pair.Key] = pair.Value.ToString();
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return q;
    }
}
