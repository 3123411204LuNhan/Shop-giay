namespace ShopGiay.Entity;

public sealed class BienTheSanPham
{
    public static decimal LayGiaHienTai(decimal? giaRieng, decimal giaSanPham) => giaRieng ?? giaSanPham;

    public long BienTheID { get; init; }
    public long SanPhamID { get; init; }
    public required string SKU { get; init; }
    public int KichCoID { get; init; }
    public int MauSacID { get; init; }
    public decimal? GiaBanRieng { get; init; }
    public bool DangKinhDoanh { get; init; }
    public DateTime NgayTao { get; init; }
    public DateTime NgayCapNhat { get; init; }
}
