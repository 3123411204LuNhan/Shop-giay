using ShopGiay.Web.Infrastructure;

namespace ShopGiay.Web.Repositories;
// Hợp đồng truy cập dữ liệu được Service sử dụng; bản triển khai nằm trong PromotionRepository.Cs.
public interface IPromotionRepository
{
    // Service gọi Published qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Published();
    // Service gọi List qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> List();
}
