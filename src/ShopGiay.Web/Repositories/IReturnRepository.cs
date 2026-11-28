using ShopGiay.Web.Infrastructure;

namespace ShopGiay.Web.Repositories;
// Hợp đồng truy cập dữ liệu được Service sử dụng; bản triển khai nằm trong ReturnRepository.Cs.
public interface IReturnRepository
{
    // Service gọi ReviewableLine qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> ReviewableLine(object? [] values);
    // Service gọi InsertReview qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> InsertReview(object? [] values);
    // Service gọi ForCustomer qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> ForCustomer(object? [] values);
    // Service gọi LockCustomer qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockCustomer(object? [] values, DbSession? connection = null);
    // Service gọi FindByKey qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> FindByKey(object? [] values, DbSession? connection = null);
    // Service gọi LockEligibleLine qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockEligibleLine(object? [] values, DbSession? connection = null);
    // Service gọi RequestedQuantity qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> RequestedQuantity(object? [] values, DbSession? connection = null);
    // Service gọi CreateRequest qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> CreateRequest(object? [] values, DbSession? connection = null);
    // Service gọi AddLine qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> AddLine(object? [] values, DbSession? connection = null);
    // Service gọi AttachImage qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> AttachImage(object? [] values, DbSession? connection = null);
    // Service gọi RecordSubmission qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> RecordSubmission(object? [] values, DbSession? connection = null);
    // Service gọi OwnedRequest qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> OwnedRequest(object? [] values);
    // Service gọi Lines qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Lines(object? [] values);
    // Service gọi History qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> History(object? [] values);
    // Service gọi Refunds qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Refunds(object? [] values);
    // Service gọi All qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> All();
    // Service gọi LockRequest qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockRequest(object? [] values, DbSession? connection = null);
    // Service gọi LockLines qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockLines(object? [] values, DbSession? connection = null);
    // Service gọi ApproveLine qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> ApproveLine(object? [] values, DbSession? connection = null);
    // Service gọi RecordDecision qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> RecordDecision(object? [] values, DbSession? connection = null);
    // Service gọi ReceiveLine qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> ReceiveLine(object? [] values, DbSession? connection = null);
    // Service gọi CreateReceipt qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> CreateReceipt(object? [] values, DbSession? connection = null);
    // Service gọi AddReceiptLine qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> AddReceiptLine(object? [] values, DbSession? connection = null);
    // Service gọi PendingRefunds qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> PendingRefunds(object? [] values, DbSession? connection = null);
    // Service gọi ApprovedTotal qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> ApprovedTotal(object? [] values, DbSession? connection = null);
    // Service gọi ConfirmedRefunds qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> ConfirmedRefunds(object? [] values, DbSession? connection = null);
    // Service gọi MarkComplete qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> MarkComplete(object? [] values, DbSession? connection = null);
    // Service gọi SetStatus qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> SetStatus(object? [] values, DbSession? connection = null);
    // Service gọi RecordHistory qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> RecordHistory(object? [] values, DbSession? connection = null);
    // Service gọi AllRefunds qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> AllRefunds();
    // Service gọi LockRefund qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockRefund(object? [] values, DbSession? connection = null);
    // Service gọi LockPayment qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockPayment(object? [] values, DbSession? connection = null);
    // Service gọi RefundedForPayment qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> RefundedForPayment(object? [] values, DbSession? connection = null);
    // Service gọi ConfirmRefund qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> ConfirmRefund(object? [] values, DbSession? connection = null);
    // Service gọi RetryRefund qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> RetryRefund(object? [] values, DbSession? connection = null);
    // Service gọi FailRefund qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> FailRefund(object? [] values, DbSession? connection = null);
}
