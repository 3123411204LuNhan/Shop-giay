using ShopGiay.Web.Controllers;

namespace ShopGiay.Web.Infrastructure;
public sealed class ApiSecurityMiddleware(RequestDelegate next)
{
    // Bảo vệ CSRF/header, nhận diện phiên, kiểm vai trò và phát hiện tab dùng tài khoản cũ.
    public async Task InvokeAsync(HttpContext http, SessionContext session)
    {
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (!http.Request.Path.StartsWithSegments("/api"))
        {
            // Thực hiện bước next trong ApiSecurityMiddleware; tiếp tục theo luồng của hàm.
            await next(http);
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return;
        }

        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (http.Request.Method is not ("GET" or "HEAD" or "OPTIONS"))
        {
            // Tạo biến origin để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var origin = http.Request.Headers.Origin.ToString();
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (http.Request.Headers["x-sole-request"] != "1" || (origin != "" && (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) || !string.Equals(uri.Authority, http.Request.Host.Value, StringComparison.OrdinalIgnoreCase))))
                // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                throw new ApiException("Yêu cầu không hợp lệ.", 403);
        }

        // Tạo biến access để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var access = http.GetEndpoint()?.Metadata.GetMetadata<ApiAccessAttribute>();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (access is null)
        {
            // Thực hiện bước next trong ApiSecurityMiddleware; tiếp tục theo luồng của hàm.
            await next(http);
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return;
        }

        // SessionContext.Identify xử lý phiên và tài khoản của HTTP request hiện tại.
        await session.Identify();
        // Tạo biến path để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var path = http.Request.Path.Value ?? "";
        // Tạo biến publicAuth để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var publicAuth = path == "/api/me" || new[]
        {
            "/api/auth/login",
            "/api/auth/register",
            "/api/auth/forgot",
            "/api/auth/verify-otp",
            "/api/auth/reset"
        }.Contains(path);
        // Tạo biến expected để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var expected = http.Request.Headers["x-sole-account"].ToString();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (expected != "" && session.User is not null && session.UserId.ToString() != expected && !publicAuth)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Tài khoản trong trình duyệt đã thay đổi. Vui lòng kiểm tra lại giỏ hàng.", 409, "SESSION_CHANGED");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (access.Area != "public")
        {
            // Gán kết quả vào _ để bước xử lý tiếp theo dùng dữ liệu mới.
            _ = session.UserId;
            // Chọn đúng nhánh thao tác theo dữ liệu yêu cầu và trạng thái hiện hành.
            switch (access.Area)
            {
                case "manager":
                    // SessionContext.RequireRole xử lý phiên và tài khoản của HTTP request hiện tại.
                    session.RequireRole("SHOP_MANAGER");
                    // Kết thúc nhánh thao tác hiện tại, tiếp tục bước xử lý chung sau switch.
                    break;
                case "admin":
                    // SessionContext.RequireRole xử lý phiên và tài khoản của HTTP request hiện tại.
                    session.RequireRole("SYSTEM_ADMIN");
                    // Kết thúc nhánh thao tác hiện tại, tiếp tục bước xử lý chung sau switch.
                    break;
                case "staff":
                    // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
                    if (!session.Roles.Any(x => x is "SHOP_MANAGER" or "SYSTEM_ADMIN"))
                        // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                        throw new ApiException("Chỉ dành cho nhân sự.", 403);
                    // Kết thúc nhánh thao tác hiện tại, tiếp tục bước xử lý chung sau switch.
                    break;
                case "order-detail" when session.Area == "manager" && session.Roles.Contains("SHOP_MANAGER"):
                    // Kết thúc nhánh thao tác hiện tại, tiếp tục bước xử lý chung sau switch.
                    break;
                default:
                    // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
                    if (session.Area != "customer" || !SessionContext.CanUse(session.Roles, "customer"))
                        // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                        throw new ApiException("Chức năng mua sắm chỉ dành cho tài khoản khách hàng.", 403);
                    // Kết thúc nhánh thao tác hiện tại, tiếp tục bước xử lý chung sau switch.
                    break;
            }
        }

        // Thực hiện bước next trong ApiSecurityMiddleware; tiếp tục theo luồng của hàm.
        await next(http);
    }
}
