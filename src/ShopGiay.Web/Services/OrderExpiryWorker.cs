namespace ShopGiay.Web.Services;
// Bộ nền ASP.NET Core tự dừng theo ứng dụng, không tạo timer rời bị chồng lần chạy.
public sealed class OrderExpiryWorker(IServiceScopeFactory scopes, ILogger<OrderExpiryWorker> logger) : BackgroundService
{
    // Mỗi lần tạo scope mới để Database/Repository/Service được giải phóng đúng vòng đời.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Tạo biến timer để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
        // Tiếp tục vòng xử lý cho tới khi hết dữ liệu hoặc ứng dụng yêu cầu dừng.
        do
        {
            // Bắt đầu vùng xử lý có bắt lỗi/giải phóng tài nguyên ở các nhánh bên dưới.
            try
            {
                // Tạo biến scope để dùng ở các bước tiếp theo trong phạm vi hiện tại.
                using var scope = scopes.CreateScope();
                // Thực hiện bước scope.ServiceProvider.GetRequiredService<OrderService>().ExpireUnpaidOrders trong OrderExpiryWorker; tiếp tục theo luồng của hàm.
                await scope.ServiceProvider.GetRequiredService<OrderService>().ExpireUnpaidOrders();
            }
            catch (Exception e)when (!stoppingToken.IsCancellationRequested)
            {
                // Thực hiện bước logger.LogWarning trong OrderExpiryWorker; tiếp tục theo luồng của hàm.
                logger.LogWarning("Chưa xử lý hết hạn đơn: {ErrorType}", e.GetType().Name);
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
