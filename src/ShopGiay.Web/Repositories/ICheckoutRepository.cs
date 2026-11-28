using ShopGiay.Web.Infrastructure;

namespace ShopGiay.Web.Repositories;
// Hợp đồng truy cập dữ liệu được Service sử dụng; bản triển khai nằm trong CheckoutRepository.Cs.
public interface ICheckoutRepository
{
    // Service gọi EnsureCart qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> EnsureCart(object? [] values, DbSession? connection = null);
    // Service gọi CartId qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> CartId(object? [] values, DbSession? connection = null);
    // Service gọi CheckoutItems qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> CheckoutItems(object? [] values, DbSession? connection = null);
    // Service gọi CountItems qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> CountItems(object? [] values, DbSession? connection = null);
    // Service gọi LockStock qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockStock(object? [] values, DbSession? connection = null);
    // Service gọi LockPromotion qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockPromotion(object? [] values, DbSession? connection = null);
    // Service gọi PromotionUsage qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> PromotionUsage(object? [] values, DbSession? connection = null);
    // Service gọi EligibleIds qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> EligibleIds(string[] map, object? [] values, DbSession? connection = null);
    // Service gọi PaymentMethod qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> PaymentMethod(object? [] values, DbSession? connection = null);
    // Service gọi CartItems qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> CartItems(object? [] values);
    // Service gọi RemoveItem qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> RemoveItem(object? [] values, DbSession? connection = null);
    // Service gọi AvailableVariant qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> AvailableVariant(object? [] values, DbSession? connection = null);
    // Service gọi SetQuantity qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> SetQuantity(object? [] values, DbSession? connection = null);
    // Service gọi LockCustomer qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockCustomer(object? [] values, DbSession? connection = null);
    // Service gọi FindOrderByKey qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> FindOrderByKey(object? [] values, DbSession? connection = null);
    // Service gọi OrderTotal qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> OrderTotal(object? [] values, DbSession? connection = null);
    // Service gọi CreateOrder qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> CreateOrder(object? [] values, DbSession? connection = null);
    // Service gọi SetDeadline qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> SetDeadline(object? [] values, DbSession? connection = null);
    // Service gọi AddOrderLine qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> AddOrderLine(object? [] values, DbSession? connection = null);
    // Service gọi AllocateStock qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> AllocateStock(object? [] values, DbSession? connection = null);
    // Service gọi RecordPromotion qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> RecordPromotion(object? [] values, DbSession? connection = null);
    // Service gọi RecordHistory qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> RecordHistory(object? [] values, DbSession? connection = null);
    // Service gọi CreatePayment qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> CreatePayment(object? [] values, DbSession? connection = null);
    // Service gọi EmptyCart qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> EmptyCart(object? [] values, DbSession? connection = null);
}
