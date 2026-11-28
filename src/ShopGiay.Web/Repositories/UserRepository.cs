using ShopGiay.Web.Infrastructure;

namespace ShopGiay.Web.Repositories;
// Lớp truy cập MySQL cho User; Service gọi lớp này trong cùng giao dịch.
public sealed class UserRepository(Database db) : IUserRepository
{
    // List: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> List()
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT t.TaiKhoanID id,t.HoTen name,t.Email email,t.TenDangNhap username,t.SoDienThoai phone,t.TrangThai status,t.NgayTao date,GROUP_CONCAT(v.MaVaiTro) roles FROM TaiKhoan t LEFT JOIN TaiKhoanVaiTro tv ON tv.TaiKhoanID=t.TaiKhoanID LEFT JOIN VaiTro v ON v.VaiTroID=tv.VaiTroID WHERE t.NgayXoa IS NULL GROUP BY t.TaiKhoanID ORDER BY t.TaiKhoanID", []);
    }

    // Insert: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Insert(string email, string passwordHash, string name, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO TaiKhoan(TenDangNhap,Email,MatKhauHash,HoTen) VALUES(?,?,?,?)", new object? [] { email, email, passwordHash, name }, connection);
    }

    // AssignRole: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> AssignRole(long id, long actorId, string roleName, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO TaiKhoanVaiTro(TaiKhoanID,VaiTroID,NguoiGanID) SELECT ?,VaiTroID,? FROM VaiTro WHERE MaVaiTro=?", [id, actorId, roleName], connection);
    }

    // LockActiveAdmins: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockActiveAdmins(DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT t.TaiKhoanID id FROM TaiKhoan t JOIN TaiKhoanVaiTro tv ON tv.TaiKhoanID=t.TaiKhoanID JOIN VaiTro v ON v.VaiTroID=tv.VaiTroID WHERE v.MaVaiTro='SYSTEM_ADMIN' AND t.TrangThai='HOAT_DONG' ORDER BY t.TaiKhoanID FOR UPDATE", [], connection);
    }

    // LockRoles: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockRoles(long id, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT v.MaVaiTro role FROM TaiKhoanVaiTro tv JOIN VaiTro v ON v.VaiTroID=tv.VaiTroID WHERE tv.TaiKhoanID=? FOR UPDATE", [id], connection);
    }

    // UpdateProfile: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> UpdateProfile(string name, string status, long id, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE TaiKhoan SET HoTen=?,TrangThai=?,NgayCapNhat=UTC_TIMESTAMP() WHERE TaiKhoanID=?", [name, status, id], connection);
    }

    // ChangeRole: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> ChangeRole(string roleName, long actorId, long id, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE TaiKhoanVaiTro SET VaiTroID=(SELECT VaiTroID FROM VaiTro WHERE MaVaiTro=?),NguoiGanID=?,NgayGan=UTC_TIMESTAMP() WHERE TaiKhoanID=?", [roleName, actorId, id], connection);
    }

    // RevokeSessions: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> RevokeSessions(long id, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE PhienDangNhap SET ThuHoiLuc=UTC_TIMESTAMP() WHERE TaiKhoanID=?", [id], connection);
    }

    // LockAccount: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockAccount(long id, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT TrangThai FROM TaiKhoan WHERE TaiKhoanID=? AND NgayXoa IS NULL FOR UPDATE", [id], connection);
    }

    // Anonymize: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Anonymize(string alias, string passwordHash, long id, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE TaiKhoan SET NgayXoa=UTC_TIMESTAMP(),NgayCapNhat=UTC_TIMESTAMP(),TenDangNhap=?,Email=?,MatKhauHash=?,SoDienThoai=NULL,DiaChi=NULL,NgaySinh=NULL,GioiTinh=NULL,AnhDaiDien=NULL WHERE TaiKhoanID=?", [alias, alias, passwordHash, id], connection);
    }

    // RevokeActiveSessions: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> RevokeActiveSessions(long id, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE PhienDangNhap SET ThuHoiLuc=UTC_TIMESTAMP() WHERE TaiKhoanID=? AND ThuHoiLuc IS NULL", [id], connection);
    }

    // InvalidateResetCodes: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> InvalidateResetCodes(long id, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE DatLaiMatKhau SET DaSuDungLuc=UTC_TIMESTAMP() WHERE TaiKhoanID=? AND DaSuDungLuc IS NULL", [id], connection);
    }
}
