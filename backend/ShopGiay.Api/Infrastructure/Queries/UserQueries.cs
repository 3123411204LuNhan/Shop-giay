namespace ShopGiay.Infrastructure;

internal static class UserQueries
{
    internal const string List = """
        SELECT t.TaiKhoanID id,t.HoTen name,t.Email email,t.TenDangNhap username,t.SoDienThoai phone,t.TrangThai status,t.NgayTao date,GROUP_CONCAT(v.MaVaiTro) roles FROM TaiKhoan t LEFT JOIN TaiKhoanVaiTro tv ON tv.TaiKhoanID=t.TaiKhoanID LEFT JOIN VaiTro v ON v.VaiTroID=tv.VaiTroID WHERE t.NgayXoa IS NULL GROUP BY t.TaiKhoanID ORDER BY t.TaiKhoanID
        """;

    internal const string Insert = """
        INSERT INTO TaiKhoan(TenDangNhap,Email,MatKhauHash,HoTen) VALUES(?,?,?,?)
        """;

    internal const string AssignRole = """
        INSERT INTO TaiKhoanVaiTro(TaiKhoanID,VaiTroID,NguoiGanID) SELECT ?,VaiTroID,? FROM VaiTro WHERE MaVaiTro=?
        """;

    internal const string LockActiveAdmins = """
        SELECT t.TaiKhoanID id FROM TaiKhoan t JOIN TaiKhoanVaiTro tv ON tv.TaiKhoanID=t.TaiKhoanID JOIN VaiTro v ON v.VaiTroID=tv.VaiTroID WHERE v.MaVaiTro='SYSTEM_ADMIN' AND t.TrangThai='HOAT_DONG' ORDER BY t.TaiKhoanID FOR UPDATE
        """;

    internal const string LockRoles = """
        SELECT v.MaVaiTro role FROM TaiKhoanVaiTro tv JOIN VaiTro v ON v.VaiTroID=tv.VaiTroID WHERE tv.TaiKhoanID=? FOR UPDATE
        """;

    internal const string UpdateProfile = """
        UPDATE TaiKhoan SET HoTen=?,TrangThai=?,NgayCapNhat=UTC_TIMESTAMP() WHERE TaiKhoanID=?
        """;

    internal const string ChangeRole = """
        UPDATE TaiKhoanVaiTro SET VaiTroID=(SELECT VaiTroID FROM VaiTro WHERE MaVaiTro=?),NguoiGanID=?,NgayGan=UTC_TIMESTAMP() WHERE TaiKhoanID=?
        """;

    internal const string RevokeSessions = """
        UPDATE PhienDangNhap SET ThuHoiLuc=UTC_TIMESTAMP() WHERE TaiKhoanID=?
        """;

    internal const string LockAccount = """
        SELECT TrangThai FROM TaiKhoan WHERE TaiKhoanID=? AND NgayXoa IS NULL FOR UPDATE
        """;

    internal const string Anonymize = """
        UPDATE TaiKhoan SET NgayXoa=UTC_TIMESTAMP(),NgayCapNhat=UTC_TIMESTAMP(),TenDangNhap=?,Email=?,MatKhauHash=?,SoDienThoai=NULL,DiaChi=NULL,NgaySinh=NULL,GioiTinh=NULL,AnhDaiDien=NULL WHERE TaiKhoanID=?
        """;

    internal const string RevokeActiveSessions = """
        UPDATE PhienDangNhap SET ThuHoiLuc=UTC_TIMESTAMP() WHERE TaiKhoanID=? AND ThuHoiLuc IS NULL
        """;

    internal const string InvalidateResetCodes = """
        UPDATE DatLaiMatKhau SET DaSuDungLuc=UTC_TIMESTAMP() WHERE TaiKhoanID=? AND DaSuDungLuc IS NULL
        """;
}
