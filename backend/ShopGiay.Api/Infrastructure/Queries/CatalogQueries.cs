namespace ShopGiay.Infrastructure;

internal static class CatalogQueries
{
    private const string productSelect = """
        SELECT s.SanPhamID id,s.MaSanPham code,s.TenSanPham name,s.MoTa description,s.GiaBan price,s.DanhMucID categoryId,s.ThuongHieuID brandId,s.TrangThai status,
         d.TenDanhMuc category,t.TenThuongHieu brand,(SELECT DuongDan FROM HinhAnhSanPham h WHERE h.SanPhamID=s.SanPhamID ORDER BY LaAnhChinh DESC,ThuTu,HinhAnhID LIMIT 1) image,
         COALESCE((SELECT SUM(k.SoLuongKhaDung) FROM BienTheSanPham b JOIN TonKho k ON k.BienTheID=b.BienTheID WHERE b.SanPhamID=s.SanPhamID AND b.DangKinhDoanh=1),0) stock
         FROM SanPham s JOIN DanhMuc d ON d.DanhMucID=s.DanhMucID JOIN ThuongHieu t ON t.ThuongHieuID=s.ThuongHieuID
        """;

    internal const string ProductStock = """
        SELECT k.SoLuongTon,k.SoLuongDaPhanBo FROM BienTheSanPham b JOIN TonKho k ON k.BienTheID=b.BienTheID WHERE b.SanPhamID=? FOR UPDATE
        """;

    internal const string ProductImages = """
        SELECT DuongDan url FROM HinhAnhSanPham WHERE SanPhamID=?
        """;

    internal const string DeleteImages = """
        DELETE FROM HinhAnhSanPham WHERE SanPhamID=?
        """;

    internal const string DeleteStock = """
        DELETE k FROM TonKho k JOIN BienTheSanPham b ON b.BienTheID=k.BienTheID WHERE b.SanPhamID=?
        """;

    internal const string DeleteVariants = """
        DELETE FROM BienTheSanPham WHERE SanPhamID=?
        """;

    internal const string Sizes = """
        SELECT KichCoID id,GiaTri name FROM KichCo ORDER BY ThuTu,CAST(GiaTri AS DECIMAL(8,2))
        """;

    internal const string Products = productSelect + """
         ORDER BY s.SanPhamID DESC
        """;

    internal const string AddSize = """
        INSERT INTO KichCo(HeSize,GiaTri,ThuTu) VALUES('EU',?,?) ON DUPLICATE KEY UPDATE DangHoatDong=1
        """;

    internal const string SizeByValue = """
        SELECT KichCoID id,GiaTri name FROM KichCo WHERE HeSize='EU' AND GiaTri=?
        """;

    internal const string AddColor = """
        INSERT INTO MauSac(TenMau,MaHex) VALUES(?,?) ON DUPLICATE KEY UPDATE DangHoatDong=1
        """;

    internal const string ColorByName = """
        SELECT MauSacID id,TenMau name FROM MauSac WHERE TenMau=?
        """;

    internal const string AllCategories = """
        SELECT DanhMucID id,TenDanhMuc name,DangHoatDong active,LaBanNhap draft FROM DanhMuc
        """;

    internal const string AllBrands = """
        SELECT ThuongHieuID id,TenThuongHieu name,DangHoatDong active,LaBanNhap draft FROM ThuongHieu
        """;

    internal const string AllColors = """
        SELECT MauSacID id,TenMau name FROM MauSac
        """;

    internal const string InsertProduct = """
        INSERT INTO SanPham(MaSanPham,TenSanPham,MoTa,GiaBan,DanhMucID,ThuongHieuID,TrangThai,NguoiTaoID) VALUES(?,?,?,?,?,?,?,?)
        """;

    internal const string InsertImage = """
        INSERT INTO HinhAnhSanPham(SanPhamID,DuongDan,LaAnhChinh) VALUES(?,?,1)
        """;

    internal const string LockProduct = """
        SELECT MaSanPham code,TenSanPham name,MoTa description,GiaBan price,DanhMucID categoryId,ThuongHieuID brandId,TrangThai status FROM SanPham WHERE SanPhamID=? FOR UPDATE
        """;

    internal const string LockImage = """
        SELECT HinhAnhID id,DuongDan image,BienTheID variant FROM HinhAnhSanPham WHERE SanPhamID=? ORDER BY LaAnhChinh DESC,ThuTu,HinhAnhID LIMIT 1 FOR UPDATE
        """;

    internal const string UpdateProduct = """
        UPDATE SanPham SET MaSanPham=?,TenSanPham=?,MoTa=?,GiaBan=?,DanhMucID=?,ThuongHieuID=? WHERE SanPhamID=?
        """;

    internal const string UpdateImage = """
        UPDATE HinhAnhSanPham SET DuongDan=?,LaAnhChinh=1 WHERE HinhAnhID=?
        """;

    internal const string RemoveImage = """
        DELETE FROM HinhAnhSanPham WHERE HinhAnhID=?
        """;

    internal const string ClearPrimaryImage = """
        UPDATE HinhAnhSanPham SET LaAnhChinh=0 WHERE SanPhamID=? AND LaAnhChinh=1
        """;

    internal const string SetProductStatus = """
        UPDATE SanPham SET TrangThai=?,NgayCapNhat=UTC_TIMESTAMP(),PhienBan=PhienBan+1 WHERE SanPhamID=?
        """;

    internal const string Variants = """
        SELECT b.BienTheID id,b.SanPhamID productId,b.KichCoID sizeId,b.MauSacID colorId,b.GiaBanRieng price,b.SKU sku,s.MaSanPham productCode,s.TenSanPham name,kc.GiaTri size,m.TenMau color,b.DangKinhDoanh active FROM BienTheSanPham b JOIN SanPham s ON s.SanPhamID=b.SanPhamID JOIN KichCo kc ON kc.KichCoID=b.KichCoID JOIN MauSac m ON m.MauSacID=b.MauSacID  {slot0} ORDER BY s.SanPhamID,b.BienTheID
        """;

    internal const string InsertVariant = """
        INSERT INTO BienTheSanPham(SanPhamID,SKU,KichCoID,MauSacID,GiaBanRieng) VALUES(?,?,?,?,?)
        """;

    internal const string CreateStock = """
        INSERT INTO TonKho(BienTheID) VALUES(?)
        """;

    internal const string LockVariant = """
        SELECT * FROM BienTheSanPham WHERE BienTheID=? FOR UPDATE
        """;

    internal const string CountVariantOrders = """
        SELECT COUNT(*) n FROM ChiTietDonHang WHERE BienTheID=?
        """;

    internal const string UpdateVariant = """
        UPDATE BienTheSanPham SET KichCoID=?,MauSacID=?,GiaBanRieng=? WHERE BienTheID=?
        """;

    internal const string SetVariantStatus = """
        UPDATE BienTheSanPham SET DangKinhDoanh=?,NgayCapNhat=UTC_TIMESTAMP() WHERE BienTheID=?
        """;

    internal const string LockVariantIdentity = """
        SELECT SKU,DangKinhDoanh FROM BienTheSanPham WHERE BienTheID=? FOR UPDATE
        """;

    internal const string LockStock = """
        SELECT SoLuongTon,SoLuongDaPhanBo FROM TonKho WHERE BienTheID=? FOR UPDATE
        """;

    internal const string RemoveStock = """
        DELETE FROM TonKho WHERE BienTheID=?
        """;

    internal const string RemoveVariant = """
        DELETE FROM BienTheSanPham WHERE BienTheID=?
        """;
}
