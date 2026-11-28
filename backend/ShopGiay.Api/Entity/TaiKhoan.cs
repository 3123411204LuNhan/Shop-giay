namespace ShopGiay.Entity;

public sealed class TaiKhoan
{
    public long TaiKhoanID { get; init; }
    public required string TenDangNhap { get; init; }
    public string? SoDienThoai { get; private set; }
    public required string Email { get; init; }
    public string MatKhauHash { get; private set; } = "";
    public string HoTen { get; private set; } = "";
    public DateOnly? NgaySinh { get; init; }
    public string? GioiTinh { get; init; }
    public string? DiaChi { get; private set; }
    public string? AnhDaiDien { get; init; }
    public string TrangThai { get; private set; } = "HOAT_DONG";
    public bool EmailDaXacMinh { get; init; }
    public int SoLanDangNhapSai { get; private set; }
    public DateTime? KhoaDenLuc { get; private set; }
    public DateTime? LanDangNhapCuoi { get; init; }
    public DateTime? NgayDoiMatKhau { get; private set; }
    public DateTime NgayTao { get; init; }
    public DateTime NgayCapNhat { get; init; }

    // Tạo tài khoản với mật khẩu đã băm, dùng email làm tên đăng nhập ban đầu.
    public static TaiKhoan DangKy(string email,string hash,string name,string? phone = null) =>
        new() { Email = email, TenDangNhap = email, MatKhauHash = hash, HoTen = name, SoDienThoai = phone };

    // Từ chối đăng nhập nếu tài khoản bị khóa hoặc chưa hết thời gian khóa tạm.
    public bool CoTheDangNhap(DateTime thoiDiem) => TrangThai == "HOAT_DONG" && !(KhoaDenLuc > thoiDiem);

    // Thay bản băm và xóa số lần đăng nhập sai khi người dùng đã xác thực để đổi mật khẩu.
    public void DoiMatKhau(string hash,DateTime thoiDiem) =>
        (MatKhauHash,NgayDoiMatKhau,SoLanDangNhapSai,KhoaDenLuc) = (hash,thoiDiem,0,null);

    // Thay các thông tin cá nhân mà khách được phép chỉnh.
    public void CapNhatHoSo(string ten,string? dienThoai,string? diaChi) =>
        (HoTen,SoDienThoai,DiaChi) = (ten,dienThoai,diaChi);

    // Cập nhật tên và trạng thái quản trị sau khi Control kiểm tra quyền.
    public void CapNhatQuanTri(string ten,string trangThai) => (HoTen,TrangThai) = (ten,trangThai);
}
