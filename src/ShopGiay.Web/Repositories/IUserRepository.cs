using ShopGiay.Web.Infrastructure;

namespace ShopGiay.Web.Repositories;
// Hợp đồng truy cập dữ liệu được Service sử dụng; bản triển khai nằm trong UserRepository.Cs.
public interface IUserRepository
{
    // Service gọi List qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> List();
    // Service gọi Insert qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Insert(string email, string passwordHash, string name, DbSession? connection = null);
    // Service gọi AssignRole qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> AssignRole(long id, long actorId, string roleName, DbSession? connection = null);
    // Service gọi LockActiveAdmins qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockActiveAdmins(DbSession? connection = null);
    // Service gọi LockRoles qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockRoles(long id, DbSession? connection = null);
    // Service gọi UpdateProfile qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> UpdateProfile(string name, string status, long id, DbSession? connection = null);
    // Service gọi ChangeRole qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> ChangeRole(string roleName, long actorId, long id, DbSession? connection = null);
    // Service gọi RevokeSessions qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> RevokeSessions(long id, DbSession? connection = null);
    // Service gọi LockAccount qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> LockAccount(long id, DbSession? connection = null);
    // Service gọi Anonymize qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> Anonymize(string alias, string passwordHash, long id, DbSession? connection = null);
    // Service gọi RevokeActiveSessions qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> RevokeActiveSessions(long id, DbSession? connection = null);
    // Service gọi InvalidateResetCodes qua hợp đồng này; Repository thực thi SQL.
    Task<DbResult> InvalidateResetCodes(long id, DbSession? connection = null);
}
