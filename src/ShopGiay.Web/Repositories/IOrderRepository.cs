using ShopGiay.Web.Infrastructure;

namespace ShopGiay.Web.Repositories;
// Hợp đồng truy cập dữ liệu được Service sử dụng; bản triển khai nằm trong OrderRepository.Cs.
public interface IOrderRepository
{
    // Service gọi FindById qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> FindById(object? [] values);
    // Service gọi Items qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Items(object? [] values);
    // Service gọi History qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> History(object? [] values);
    // Service gọi Refunds qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Refunds(object? [] values);
    // Service gọi Payments qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Payments(object? [] values);
    // Service gọi LockOrder qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockOrder(object? [] values, DbSession? connection = null);
    // Service gọi Allocations qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Allocations(object? [] values, DbSession? connection = null);
    // Service gọi ReleaseAllocation qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> ReleaseAllocation(object? [] values, DbSession? connection = null);
    // Service gọi CancelOrder qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> CancelOrder(object? [] values, DbSession? connection = null);
    // Service gọi RecordCancellation qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> RecordCancellation(object? [] values, DbSession? connection = null);
    // Service gọi CancelPendingPayments qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> CancelPendingPayments(object? [] values, DbSession? connection = null);
    // Service gọi ExpiredOrders qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> ExpiredOrders();
    // Service gọi ConfirmedTotal qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> ConfirmedTotal(object? [] values, DbSession? connection = null);
    // Service gọi ExpirePayment qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> ExpirePayment(object? [] values, DbSession? connection = null);
    // Service gọi ForCustomer qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> ForCustomer(object? [] values);
    // Service gọi All qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> All();
    // Service gọi PaymentMethod qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> PaymentMethod(object? [] values, DbSession? connection = null);
    // Service gọi MarkShipping qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> MarkShipping(object? [] values, DbSession? connection = null);
    // Service gọi MarkDelivered qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> MarkDelivered(object? [] values, DbSession? connection = null);
    // Service gọi HeldItems qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> HeldItems(object? [] values, DbSession? connection = null);
    // Service gọi ReleaseShipped qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> ReleaseShipped(object? [] values, DbSession? connection = null);
    // Service gọi UpdateStatus qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> UpdateStatus(object? [] values, DbSession? connection = null);
    // Service gọi AppendHistory qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> AppendHistory(object? [] values, DbSession? connection = null);
    // Service gọi OrderForPayment qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> OrderForPayment(object? [] values, DbSession? connection = null);
    // Service gọi LockOrderWithMethod qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockOrderWithMethod(object? [] values, DbSession? connection = null);
    // Service gọi LockPayment qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockPayment(object? [] values, DbSession? connection = null);
    // Service gọi ConfirmPayment qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> ConfirmPayment(object? [] values, DbSession? connection = null);
    // Service gọi ClearDeadline qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> ClearDeadline(object? [] values, DbSession? connection = null);
    // Service gọi UpdateDelivery qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> UpdateDelivery(object? [] values, DbSession? connection = null);
    // Service gọi RecordDelivery qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> RecordDelivery(object? [] values, DbSession? connection = null);
    // Service gọi LockItems qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockItems(object? [] values, DbSession? connection = null);
    // Service gọi CreateReturnReceipt qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> CreateReturnReceipt(object? [] values, DbSession? connection = null);
    // Service gọi AddReceiptLine qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> AddReceiptLine(object? [] values, DbSession? connection = null);
    // Service gọi CancelReturnedOrder qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> CancelReturnedOrder(object? [] values, DbSession? connection = null);
    // Service gọi RecordReturn qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> RecordReturn(object? [] values, DbSession? connection = null);
    // Service gọi LockLateOrder qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockLateOrder(object? [] values, DbSession? connection = null);
    // Service gọi LockLatestPayment qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockLatestPayment(object? [] values, DbSession? connection = null);
    // Service gọi ConfirmLatePayment qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> ConfirmLatePayment(object? [] values, DbSession? connection = null);
}
