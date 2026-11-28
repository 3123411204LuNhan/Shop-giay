using ShopGiay.Control;

namespace ShopGiay.Infrastructure;

public sealed class OrderExpiryWorker(IServiceScopeFactory scopes,ILogger<OrderExpiryWorker> logger) : BackgroundService
{
    // Chạy định kỳ, tạo scope và gọi cùng OrderControl để hủy các đơn đủ điều kiện hết hạn.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
        do {
            try {
                await using var scope = scopes.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<OrderControl>().Expire(stoppingToken);
            } catch (Exception e) when (!stoppingToken.IsCancellationRequested) {
                logger.LogWarning("Không xử lý được đơn hết hạn ({ErrorType}).",e.GetType().Name);
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
