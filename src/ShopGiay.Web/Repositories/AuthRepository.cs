using ShopGiay.Web.Infrastructure;

namespace ShopGiay.Web.Repositories;
// Lớp truy cập MySQL cho Auth; Service gọi lớp này trong cùng giao dịch.
public sealed class AuthRepository(Database db) : IAuthRepository
{
    // FindLogin: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> FindLogin(object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT TaiKhoanID id,MatKhauHash hash,TrangThai status,KhoaDenLuc locked FROM TaiKhoan WHERE NgayXoa IS NULL AND (Email=? OR TenDangNhap=?)", values);
    }

    // RecordLoginFailure: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> RecordLoginFailure(object? [] values)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE TaiKhoan SET SoLanDangNhapSai=SoLanDangNhapSai+1,KhoaDenLuc=IF(SoLanDangNhapSai>=5,DATE_ADD(UTC_TIMESTAMP(),INTERVAL 15 MINUTE),KhoaDenLuc) WHERE TaiKhoanID=?", values);
    }

    // Roles: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Roles(object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT v.MaVaiTro role FROM TaiKhoanVaiTro tv JOIN VaiTro v ON v.VaiTroID=tv.VaiTroID WHERE tv.TaiKhoanID=?", values);
    }

    // RecordLogin: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> RecordLogin(object? [] values)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE TaiKhoan SET LanDangNhapCuoi=UTC_TIMESTAMP(),SoLanDangNhapSai=0 WHERE TaiKhoanID=?", values);
    }

    // CreateCustomer: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> CreateCustomer(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO TaiKhoan(TenDangNhap,Email,MatKhauHash,HoTen,SoDienThoai) VALUES(?,?,?,?,?)", values, connection);
    }

    // AssignCustomerRole: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> AssignCustomerRole(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO TaiKhoanVaiTro(TaiKhoanID,VaiTroID) SELECT ?,VaiTroID FROM VaiTro WHERE MaVaiTro='CUSTOMER'", values, connection);
    }

    // RevokeSession: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> RevokeSession(object? [] values)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE PhienDangNhap SET ThuHoiLuc=UTC_TIMESTAMP() WHERE TokenHash=?", values);
    }

    // LockActiveUser: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockActiveUser(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT TaiKhoanID FROM TaiKhoan WHERE TaiKhoanID=? AND TrangThai='HOAT_DONG' AND NgayXoa IS NULL FOR UPDATE", values, connection);
    }

    // LockSession: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockSession(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT PhienID FROM PhienDangNhap WHERE TaiKhoanID=? AND TokenHash=? AND ThuHoiLuc IS NULL AND HetHanLuc>UTC_TIMESTAMP() FOR UPDATE", values, connection);
    }

    // UpdateProfile: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> UpdateProfile(object? [] values)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE TaiKhoan SET HoTen=?,SoDienThoai=?,DiaChi=?,NgayCapNhat=UTC_TIMESTAMP() WHERE TaiKhoanID=?", values);
    }
}
