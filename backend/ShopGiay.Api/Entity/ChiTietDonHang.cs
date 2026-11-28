namespace ShopGiay.Entity;

public sealed class ChiTietDonHang
{
    public long ChiTietDonHangID { get; init; }
    public long DonHangID { get; init; }
    public long BienTheID { get; init; }
    public required string MaSanPhamLucMua { get; init; }
    public required string SKULucMua { get; init; }
    public required string TenSanPhamLucMua { get; init; }
    public required string KichCoLucMua { get; init; }
    public required string MauSacLucMua { get; init; }
    public required string DanhMucLucMua { get; init; }
    public required string ThuongHieuLucMua { get; init; }
    public int SoLuong { get; init; }
    public decimal DonGia { get; init; }
    public decimal TienGiamPhanBo { get; init; }
    public decimal ThanhTien { get; init; }
}
