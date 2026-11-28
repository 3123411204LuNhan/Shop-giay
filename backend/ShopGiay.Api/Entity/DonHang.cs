namespace ShopGiay.Entity;

public sealed class DonHang
{
    public static bool ChoPhepHuy(string status) => status is "CHO_XAC_NHAN" or "DA_XAC_NHAN" or "DANG_CHUAN_BI";

    public static string? TrangThaiTiepTheo(string status) => status switch {
        "CHO_XAC_NHAN" => "DA_XAC_NHAN",
        "DA_XAC_NHAN" => "DANG_CHUAN_BI",
        "DANG_CHUAN_BI" => "DANG_GIAO",
        "DANG_GIAO" => "DA_GIAO",
        "DA_GIAO" => "HOAN_THANH",
        _ => null
    };

    public long DonHangID { get; init; }
    public required string MaDonHang { get; init; }
    public Guid MaYeuCauTao { get; init; }
    public long KhachHangID { get; init; }
    public required string HoTenNguoiNhan { get; init; }
    public required string SoDienThoaiNhan { get; init; }
    public string? EmailNhan { get; init; }
    public required string DiaChiNhan { get; init; }
    public string? PhuongXa { get; init; }
    public string? TinhThanh { get; init; }
    public string? GhiChu { get; init; }
    public int PhuongThucID { get; init; }
    public required string TrangThai { get; init; }
    public decimal TienHang { get; init; }
    public decimal TienGiam { get; init; }
    public decimal PhiVanChuyen { get; init; }
    public decimal TongThanhToan { get; init; }
    public required string TienTe { get; init; }
    public DateTime NgayDat { get; init; }
    public DateTime? HanThanhToan { get; init; }
    public DateTime? NgayGiao { get; init; }
    public DateTime? NgayHoanThanh { get; init; }
    public DateTime? NgayHuy { get; init; }
    public string? LyDoHuy { get; init; }
    public long? NguoiHuyID { get; init; }
}
