namespace ShopGiay.Entity;

public sealed class ThanhToan
{
    public long ThanhToanID { get; init; }
    public long DonHangID { get; init; }
    public int PhuongThucID { get; init; }
    public Guid MaYeuCau { get; init; }
    public string? CongThanhToan { get; init; }
    public string? MaGiaoDich { get; init; }
    public decimal SoTien { get; private set; }
    public string TrangThai { get; private set; } = "CHO_XAC_NHAN";
    public DateTime NgayTao { get; init; }
    public DateTime? NgayXacNhan { get; private set; }
    public long? NguoiXacNhanID { get; private set; }
    public string? BangChung { get; private set; }
    public string? ThongBaoLoi { get; init; }

    // Khởi tạo số tiền chờ thu từ tổng tiền đã chốt của đơn.
    public void DatSoTienChoThu(decimal soTien) => SoTien = soTien;

    // Ghi nhận thời điểm, người xác nhận và bằng chứng đối soát. Tiền đến muộn có thể ghi số thực thu.
    public void XacNhanThanhToan(long actor,string proof,DateTime thoiDiem,decimal? tienThucThu = null)
    {
        if (tienThucThu is decimal amount) SoTien = amount;
        (TrangThai,NgayXacNhan,NguoiXacNhanID,BangChung) = ("DA_XAC_NHAN",thoiDiem,actor,proof);
    }
}
