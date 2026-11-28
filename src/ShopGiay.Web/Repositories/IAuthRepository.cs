using ShopGiay.Web.Infrastructure;

namespace ShopGiay.Web.Repositories;
// Hợp đồng truy cập dữ liệu được Service sử dụng; bản triển khai nằm trong AuthRepository.Cs.
public interface IAuthRepository
{
    // Service gọi FindLogin qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> FindLogin(object? [] values);
    // Service gọi RecordLoginFailure qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> RecordLoginFailure(object? [] values);
    // Service gọi Roles qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Roles(object? [] values);
    // Service gọi RecordLogin qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> RecordLogin(object? [] values);
    // Service gọi CreateCustomer qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> CreateCustomer(object? [] values, DbSession? connection = null);
    // Service gọi AssignCustomerRole qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> AssignCustomerRole(object? [] values, DbSession? connection = null);
    // Service gọi RevokeSession qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> RevokeSession(object? [] values);
    // Service gọi LockActiveUser qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockActiveUser(object? [] values, DbSession? connection = null);
    // Service gọi LockSession qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockSession(object? [] values, DbSession? connection = null);
    // Service gọi UpdateProfile qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> UpdateProfile(object? [] values);
}
