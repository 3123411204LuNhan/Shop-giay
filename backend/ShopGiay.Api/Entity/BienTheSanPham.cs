namespace ShopGiay.Entity;

public sealed class BienTheSanPham
{
    public long BienTheID { get; init; }
    public long SanPhamID { get; init; }
    public required string SKU { get; init; }
    public int KichCoID { get; private set; }
    public int MauSacID { get; private set; }
    public decimal? GiaBanRieng { get; private set; }
    public bool DangKinhDoanh { get; private set; }
    public DateTime NgayTao { get; init; }
    public DateTime NgayCapNhat { get; init; }

    // Chọn giá riêng nếu biến thể có giá, nếu không dùng giá sản phẩm.
    public static decimal LayGiaHienTai(decimal? giaRieng, decimal giaSanPham) => giaRieng ?? giaSanPham;

    // Chặn đổi size hoặc màu của biến thể đã giao dịch rồi cập nhật thông tin bán.
    public void CapNhatThongTin(int size,int color,decimal? gia,bool daGiaoDich)
    {
        if (daGiaoDich && (size != KichCoID || color != MauSacID))
            throw new DomainRuleException("Biến thể đã giao dịch không được đổi size/màu. Hãy tạo biến thể mới.");
        (KichCoID,MauSacID,GiaBanRieng) = (size,color,gia);
    }

    // Ẩn hoặc mở bán biến thể để các luồng tra cứu và checkout áp dụng trạng thái mới.
    public void DatKinhDoanh(bool active) => DangKinhDoanh = active;
}
