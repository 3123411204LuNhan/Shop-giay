namespace ShopGiay.Infrastructure;

internal static class InventoryQueries
{
    internal const string Stock = """
        SELECT b.BienTheID id,b.SanPhamID productId,b.KichCoID sizeId,b.MauSacID colorId,b.SKU sku,
                s.MaSanPham productCode,s.TenSanPham name,kc.GiaTri size,m.TenMau color,b.DangKinhDoanh active,
                COALESCE(k.SoLuongTon,0) stock,COALESCE(k.SoLuongDaPhanBo,0) reserved,
                COALESCE(k.SoLuongKhaDung,0) available,COALESCE(k.MucCanhBao,0) warning
                FROM BienTheSanPham b JOIN SanPham s ON s.SanPhamID=b.SanPhamID
                JOIN KichCo kc ON kc.KichCoID=b.KichCoID JOIN MauSac m ON m.MauSacID=b.MauSacID
                LEFT JOIN TonKho k ON k.BienTheID=b.BienTheID
                {slot0} ORDER BY s.SanPhamID,b.BienTheID
        """;

    internal const string Receipts = """
        SELECT p.PhieuKhoID id,p.MaPhieu code,p.LoaiPhieu type,p.TrangThai status,p.NhaCungCap supplier,p.MaChungTuThamChieu reference,p.LyDo reason,p.NgayTao date,p.NgayXacNhan confirmedAt,t.HoTen creator,COUNT(c.ChiTietPhieuKhoID) lineCount,COALESCE(SUM(c.SoLuongThayDoi*COALESCE(c.DonGiaNhap,0)),0) cost FROM PhieuKho p JOIN TaiKhoan t ON t.TaiKhoanID=p.NguoiThucHienID LEFT JOIN ChiTietPhieuKho c ON c.PhieuKhoID=p.PhieuKhoID GROUP BY p.PhieuKhoID ORDER BY p.PhieuKhoID DESC LIMIT 500
        """;

    internal const string Receipt = """
        SELECT PhieuKhoID id,MaPhieu code,LoaiPhieu type,TrangThai status,NhaCungCap supplier,MaChungTuThamChieu reference,LyDo reason,NgayTao date,NgayXacNhan confirmedAt FROM PhieuKho WHERE PhieuKhoID=?
        """;

    internal const string Lines = """
        SELECT c.BienTheID variantId,c.SoLuongThayDoi quantity,c.TonSoSach baseline,(c.TonSoSach+c.SoLuongThayDoi) counted,c.DonGiaNhap cost,b.SKU sku,s.TenSanPham name,k.GiaTri size,m.TenMau color FROM ChiTietPhieuKho c JOIN BienTheSanPham b ON b.BienTheID=c.BienTheID JOIN SanPham s ON s.SanPhamID=b.SanPhamID JOIN KichCo k ON k.KichCoID=b.KichCoID JOIN MauSac m ON m.MauSacID=b.MauSacID WHERE c.PhieuKhoID=?
        """;

    internal const string LockActor = """
        SELECT TaiKhoanID FROM TaiKhoan WHERE TaiKhoanID=? FOR UPDATE
        """;

    internal const string FindByKey = """
        SELECT PhieuKhoID id,NguoiThucHienID owner FROM PhieuKho WHERE MaChongLap=?
        """;

    internal const string LockReceipt = """
        SELECT * FROM PhieuKho WHERE PhieuKhoID=? FOR UPDATE
        """;

    internal const string LockLines = """
        SELECT * FROM ChiTietPhieuKho WHERE PhieuKhoID=? ORDER BY BienTheID
        """;

    internal const string LockStock = """
        SELECT SoLuongTon stock FROM TonKho WHERE BienTheID=? FOR UPDATE
        """;

    internal const string SetThreshold = """
        UPDATE TonKho SET MucCanhBao=? WHERE BienTheID=?
        """;

    internal const string History = """
        SELECT b.BienDongID id,b.LoaiBienDong type,b.ThayDoiTon quantity,b.ThayDoiPhanBo reservedChange,b.TonSau stock,b.PhanBoSau reserved,b.LyDo reason,b.ThoiDiem date,v.SKU sku,t.HoTen actor FROM BienDongKho b JOIN BienTheSanPham v ON v.BienTheID=b.BienTheID LEFT JOIN TaiKhoan t ON t.TaiKhoanID=b.NguoiThucHienID {slot0} ORDER BY b.BienDongID DESC LIMIT 300
        """;
}
