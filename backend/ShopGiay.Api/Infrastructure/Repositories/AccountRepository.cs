using ShopGiay.Entity;

namespace ShopGiay.Infrastructure;

public sealed class AccountRepository
{
    private const string AccountFields = """
        SELECT TaiKhoanID,TenDangNhap,Email,MatKhauHash,HoTen,SoDienThoai,DiaChi,TrangThai,
            SoLanDangNhapSai,KhoaDenLuc,LanDangNhapCuoi,NgayDoiMatKhau FROM TaiKhoan
        """;
    public sealed record DongNhanDien(long TaiKhoanID,string HoTen,string Email,string? SoDienThoai,string? DiaChi,string TenDangNhap,string MaVaiTro);

    // Tìm tài khoản còn tồn tại theo email hoặc tên đăng nhập.
    public Task<TaiKhoan?> TimDangNhap(SqlSession db,string login) =>
        db.One<TaiKhoan>(AccountFields + " WHERE NgayXoa IS NULL AND (Email=? OR TenDangNhap=?)",login,login);

    // Khóa tài khoản hoạt động trước khi đặt lại mật khẩu hoặc sửa hồ sơ.
    public Task<TaiKhoan?> KhoaHoatDong(SqlSession db,long id) =>
        db.One<TaiKhoan>(AccountFields + " WHERE TaiKhoanID=? AND TrangThai='HOAT_DONG' AND NgayXoa IS NULL FOR UPDATE",id);

    // Đọc tài khoản quản trị đang cần cập nhật hoặc xóa mềm.
    public Task<TaiKhoan?> KhoaTaiKhoan(SqlSession db,long id) =>
        db.One<TaiKhoan>(AccountFields + " WHERE TaiKhoanID=? AND NgayXoa IS NULL FOR UPDATE",id);

    // Lưu tài khoản đã chuẩn hóa và băm mật khẩu trước khi cấp vai trò.
    public Task<long> Them(SqlSession db,TaiKhoan taiKhoan) =>
        db.Insert("INSERT INTO TaiKhoan(TenDangNhap,Email,MatKhauHash,HoTen,SoDienThoai) VALUES(?,?,?,?,?)",
            taiKhoan.TenDangNhap,taiKhoan.Email,taiKhoan.MatKhauHash,taiKhoan.HoTen,taiKhoan.SoDienThoai);

    // Ghi các trường hồ sơ mà khách hàng được thay đổi.
    public Task LuuHoSo(SqlSession db,TaiKhoan taiKhoan) =>
        db.Execute("UPDATE TaiKhoan SET HoTen=?,SoDienThoai=?,DiaChi=?,NgayCapNhat=UTC_TIMESTAMP() WHERE TaiKhoanID=?",
            taiKhoan.HoTen,taiKhoan.SoDienThoai,taiKhoan.DiaChi,taiKhoan.TaiKhoanID);

    // Lưu tên và trạng thái tài khoản sau khi Admin đã kiểm tra quy tắc phân quyền.
    public Task LuuQuanTri(SqlSession db,TaiKhoan taiKhoan) =>
        db.Execute(UserQueries.UpdateProfile,taiKhoan.HoTen,taiKhoan.TrangThai,taiKhoan.TaiKhoanID);

    // Ghi bản băm và trạng thái khóa sau khi Entity đổi mật khẩu.
    public Task LuuMatKhau(SqlSession db,TaiKhoan taiKhoan) =>
        db.Execute("UPDATE TaiKhoan SET MatKhauHash=?,NgayDoiMatKhau=?,SoLanDangNhapSai=?,KhoaDenLuc=? WHERE TaiKhoanID=?",
            taiKhoan.MatKhauHash,taiKhoan.NgayDoiMatKhau,taiKhoan.SoLanDangNhapSai,taiKhoan.KhoaDenLuc,taiKhoan.TaiKhoanID);

    // Đọc hồ sơ và vai trò trong một truy vấn để xác định phiên còn hiệu lực nhất quán.
    public Task<List<DongNhanDien>> NhanDien(SqlSession db,byte[] hash) =>
        db.Rows<DongNhanDien>("""
            SELECT t.TaiKhoanID,t.HoTen,t.Email,t.SoDienThoai,t.DiaChi,t.TenDangNhap,v.MaVaiTro
            FROM PhienDangNhap p JOIN TaiKhoan t ON t.TaiKhoanID=p.TaiKhoanID
            JOIN TaiKhoanVaiTro tv ON tv.TaiKhoanID=t.TaiKhoanID JOIN VaiTro v ON v.VaiTroID=tv.VaiTroID
            WHERE p.TokenHash=? AND p.ThuHoiLuc IS NULL AND p.HetHanLuc>UTC_TIMESTAMP()
            AND t.TrangThai='HOAT_DONG' AND t.NgayXoa IS NULL
            """,hash);

    // Tạo phiên với bản băm token và thời hạn do luồng đăng nhập thiết lập.
    public Task ThemPhien(SqlSession db,PhienDangNhap phien) =>
        db.Execute("INSERT INTO PhienDangNhap(TaiKhoanID,TokenHash,HetHanLuc) VALUES(?,?,?)",
            phien.TaiKhoanID,phien.TokenHash,phien.HetHanLuc);

    // Khóa phiên theo băm token để đăng xuất hoặc xác thực lần cuối trước khi đổi mật khẩu.
    public Task<PhienDangNhap?> KhoaPhien(SqlSession db,byte[] hash) =>
        db.One<PhienDangNhap>("SELECT * FROM PhienDangNhap WHERE TokenHash=? FOR UPDATE",hash);

    // Lưu thời điểm thu hồi của phiên đã bị kết thúc.
    public Task LuuPhien(SqlSession db,PhienDangNhap phien) =>
        db.Execute("UPDATE PhienDangNhap SET ThuHoiLuc=? WHERE PhienID=?",phien.ThuHoiLuc,phien.PhienID);

    // Khóa mã còn hạn sau khi Control đã khóa tài khoản sở hữu.
    public Task<DatLaiMatKhau?> KhoaMa(SqlSession db,byte[] hash) =>
        db.One<DatLaiMatKhau>("SELECT * FROM DatLaiMatKhau WHERE TokenHash=? AND DaSuDungLuc IS NULL AND HetHanLuc>UTC_TIMESTAMP() FOR UPDATE",hash);

    // Tạo yêu cầu xác minh hoặc token đổi mật khẩu với bản băm và thời hạn 10 phút.
    public Task ThemMa(SqlSession db,DatLaiMatKhau reset) =>
        db.Execute("INSERT INTO DatLaiMatKhau(TaiKhoanID,TokenHash,MaXacMinhHash,HetHanLuc) VALUES(?,?,?,DATE_ADD(UTC_TIMESTAMP(),INTERVAL 10 MINUTE))",
            reset.TaiKhoanID,reset.TokenHash,reset.MaXacMinhHash);

    // Lưu số lần thử và trạng thái sử dụng để yêu cầu khác không thể dùng lại mã.
    public Task LuuMa(SqlSession db,DatLaiMatKhau reset) =>
        db.Execute("UPDATE DatLaiMatKhau SET SoLanThu=?,DaSuDungLuc=? WHERE YeuCauID=?",
            reset.SoLanThu,reset.DaSuDungLuc,reset.YeuCauID);
}
