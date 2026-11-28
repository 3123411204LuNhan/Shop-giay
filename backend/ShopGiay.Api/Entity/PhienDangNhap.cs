namespace ShopGiay.Entity;

public sealed class PhienDangNhap
{
    public Guid PhienID { get; init; }
    public long TaiKhoanID { get; init; }
    public required byte[] TokenHash { get; init; }
    public DateTime NgayTao { get; init; }
    public DateTime HetHanLuc { get; init; }
    public DateTime? ThuHoiLuc { get; private set; }
    public string? DiaChiIP { get; init; }
    public string? UserAgent { get; init; }

    // Phiên còn dùng được khi chưa bị thu hồi và chưa hết hạn.
    public bool ConHieuLuc(DateTime thoiDiem) => ThuHoiLuc is null && HetHanLuc > thoiDiem;

    // Đánh dấu phiên đã kết thúc khi khách đăng xuất.
    public void ThuHoi(DateTime thoiDiem) => ThuHoiLuc = thoiDiem;
}
