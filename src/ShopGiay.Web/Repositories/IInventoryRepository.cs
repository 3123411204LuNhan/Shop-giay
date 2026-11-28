using ShopGiay.Web.Infrastructure;

namespace ShopGiay.Web.Repositories;
// Hợp đồng truy cập dữ liệu được Service sử dụng; bản triển khai nằm trong InventoryRepository.Cs.
public interface IInventoryRepository
{
    // Service gọi Receipts qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Receipts();
    // Service gọi Receipt qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Receipt(object? [] values);
    // Service gọi Lines qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Lines(object? [] values);
    // Service gọi LockActor qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockActor(object? [] values, DbSession? connection = null);
    // Service gọi FindByKey qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> FindByKey(object? [] values, DbSession? connection = null);
    // Service gọi LockReceipt qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockReceipt(object? [] values, DbSession? connection = null);
    // Service gọi LockLines qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockLines(object? [] values, DbSession? connection = null);
    // Service gọi ConfirmReceipt qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> ConfirmReceipt(object? [] values, DbSession? connection = null);
    // Service gọi CancelReceipt qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> CancelReceipt(object? [] values, DbSession? connection = null);
    // Service gọi SetThreshold qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> SetThreshold(object? [] values, DbSession? connection = null);
    // Service gọi History qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> History(object? [] values);
}
