using ShopGiay.Web.Infrastructure;

namespace ShopGiay.Web.Repositories;
// Hợp đồng truy cập dữ liệu được Service sử dụng; bản triển khai nằm trong CatalogRepository.Cs.
public interface ICatalogRepository
{
    // Service gọi LockHiddenItem qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockHiddenItem(string flag, string table, string col, object? [] values, DbSession? connection = null);
    // Service gọi ProductStock qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> ProductStock(object? [] values, DbSession? connection = null);
    // Service gọi DeleteImages qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> DeleteImages(object? [] values, DbSession? connection = null);
    // Service gọi DeleteStock qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> DeleteStock(object? [] values, DbSession? connection = null);
    // Service gọi DeleteVariants qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> DeleteVariants(object? [] values, DbSession? connection = null);
    // Service gọi RemoveItem qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> RemoveItem(string table, string col, object? [] values, DbSession? connection = null);
    // Service gọi Browse qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Browse(string[] filters, string order, object? [] values);
    // Service gọi Categories qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Categories();
    // Service gọi Brands qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Brands();
    // Service gọi Sizes qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Sizes();
    // Service gọi Colors qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Colors();
    // Service gọi PublicProduct qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> PublicProduct(object? [] values);
    // Service gọi AvailableVariants qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> AvailableVariants(object? [] values);
    // Service gọi Reviews qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Reviews(object? [] values);
    // Service gọi Products qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Products();
    // Service gọi AddSize qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> AddSize(object? [] values, DbSession? connection = null);
    // Service gọi SizeByValue qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> SizeByValue(object? [] values, DbSession? connection = null);
    // Service gọi AddColor qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> AddColor(object? [] values, DbSession? connection = null);
    // Service gọi ColorByName qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> ColorByName(object? [] values, DbSession? connection = null);
    // Service gọi AllCategories qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> AllCategories();
    // Service gọi AllBrands qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> AllBrands();
    // Service gọi AllColors qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> AllColors();
    // Service gọi InsertProduct qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> InsertProduct(object? [] values, DbSession? connection = null);
    // Service gọi InsertImage qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> InsertImage(object? [] values, DbSession? connection = null);
    // Service gọi LockProduct qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockProduct(object? [] values, DbSession? connection = null);
    // Service gọi LockImage qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockImage(object? [] values, DbSession? connection = null);
    // Service gọi UpdateProduct qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> UpdateProduct(object? [] values, DbSession? connection = null);
    // Service gọi UpdateImage qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> UpdateImage(object? [] values, DbSession? connection = null);
    // Service gọi RemoveImage qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> RemoveImage(object? [] values, DbSession? connection = null);
    // Service gọi ClearPrimaryImage qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> ClearPrimaryImage(object? [] values, DbSession? connection = null);
    // Service gọi SetProductStatus qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> SetProductStatus(object? [] values, DbSession? connection = null);
    // Service gọi Variants qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Variants();
    // Service gọi InsertVariant qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> InsertVariant(object? [] values, DbSession? connection = null);
    // Service gọi CreateStock qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> CreateStock(object? [] values, DbSession? connection = null);
    // Service gọi LockVariant qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockVariant(object? [] values, DbSession? connection = null);
    // Service gọi CountVariantOrders qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> CountVariantOrders(object? [] values, DbSession? connection = null);
    // Service gọi UpdateVariant qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> UpdateVariant(object? [] values, DbSession? connection = null);
    // Service gọi SetVariantStatus qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> SetVariantStatus(object? [] values, DbSession? connection = null);
    // Service gọi LockVariantIdentity qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockVariantIdentity(object? [] values, DbSession? connection = null);
    // Service gọi LockStock qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockStock(object? [] values, DbSession? connection = null);
    // Service gọi RemoveStock qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> RemoveStock(object? [] values, DbSession? connection = null);
    // Service gọi RemoveVariant qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> RemoveVariant(object? [] values, DbSession? connection = null);
    // Service gọi CreateTaxonomy qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> CreateTaxonomy(string table, object? [] values);
    // Service gọi UpdateTaxonomy qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> UpdateTaxonomy(string table, string col, object? [] values);
}
