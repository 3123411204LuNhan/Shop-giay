namespace ShopGiay.Entity;

public sealed class KhuyenMai
{
    public decimal TinhTienGiam(decimal giaTriApDung)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(giaTriApDung);
        var giam = HinhThuc == "PHAN_TRAM"
            ? Math.Min(Math.Round(giaTriApDung * GiaTri / 100, MidpointRounding.AwayFromZero), GiamToiDa ?? decimal.MaxValue)
            : GiaTri;
        return Math.Min(giaTriApDung, giam);
    }

    public long KhuyenMaiID { get; init; }
    public required string MaKhuyenMai { get; init; }
    public required string TenKhuyenMai { get; init; }
    public string? MoTa { get; init; }
    public required string HinhThuc { get; init; }
    public decimal GiaTri { get; init; }
    public decimal? GiamToiDa { get; init; }
    public decimal GiaTriDonToiThieu { get; init; }
    public required string PhamVi { get; init; }
    public DateTime BatDauLuc { get; init; }
    public DateTime KetThucLuc { get; init; }
    public required string TrangThai { get; init; }
    public bool ChoPhepKetHop { get; init; }
    public int DoUuTien { get; init; }
    public int? GioiHanLuotDung { get; init; }
    public int? GioiHanMoiKhach { get; init; }
    public long NguoiTaoID { get; init; }
    public DateTime NgayTao { get; init; }
    public DateTime NgayCapNhat { get; init; }
}
