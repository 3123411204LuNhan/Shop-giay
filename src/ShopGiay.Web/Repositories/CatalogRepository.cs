using ShopGiay.Web.Infrastructure;

namespace ShopGiay.Web.Repositories;
// Lớp truy cập MySQL cho Catalog; Service gọi lớp này trong cùng giao dịch.
public sealed class CatalogRepository(Database db) : ICatalogRepository
{
    private const string productSelect = $@"SELECT s.SanPhamID id,s.MaSanPham code,s.TenSanPham name,s.MoTa description,s.GiaBan price,s.DanhMucID categoryId,s.ThuongHieuID brandId,s.TrangThai status,
 d.TenDanhMuc category,t.TenThuongHieu brand,(SELECT DuongDan FROM HinhAnhSanPham h WHERE h.SanPhamID=s.SanPhamID ORDER BY LaAnhChinh DESC,ThuTu,HinhAnhID LIMIT 1) image,
 COALESCE((SELECT SUM(k.SoLuongKhaDung) FROM BienTheSanPham b JOIN TonKho k ON k.BienTheID=b.BienTheID WHERE b.SanPhamID=s.SanPhamID AND b.DangKinhDoanh=1),0) stock
 FROM SanPham s JOIN DanhMuc d ON d.DanhMucID=s.DanhMucID JOIN ThuongHieu t ON t.ThuongHieuID=s.ThuongHieuID";
    // LockHiddenItem: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockHiddenItem(string flag, string table, string col, object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query($@"SELECT ({flag}) draft FROM {table} WHERE {col}=? FOR UPDATE", values, connection);
    }

    // ProductStock: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> ProductStock(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT k.SoLuongTon,k.SoLuongDaPhanBo FROM BienTheSanPham b JOIN TonKho k ON k.BienTheID=b.BienTheID WHERE b.SanPhamID=? FOR UPDATE", values, connection);
    }

    // DeleteImages: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> DeleteImages(object? [] values, DbSession? connection = null)
    {
        // Database.Query → xóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"DELETE FROM HinhAnhSanPham WHERE SanPhamID=?", values, connection);
    }

    // DeleteStock: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> DeleteStock(object? [] values, DbSession? connection = null)
    {
        // Database.Query → xóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"DELETE k FROM TonKho k JOIN BienTheSanPham b ON b.BienTheID=k.BienTheID WHERE b.SanPhamID=?", values, connection);
    }

    // DeleteVariants: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> DeleteVariants(object? [] values, DbSession? connection = null)
    {
        // Database.Query → xóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"DELETE FROM BienTheSanPham WHERE SanPhamID=?", values, connection);
    }

    // RemoveItem: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> RemoveItem(string table, string col, object? [] values, DbSession? connection = null)
    {
        // Database.Query → xóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query($@"DELETE FROM {table} WHERE {col}=?", values, connection);
    }

    // Browse: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Browse(string[] filters, string order, object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(productSelect + @" WHERE " + string.Join(" AND ", filters) + @" ORDER BY " + order, values);
    }

    // Categories: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Categories()
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT DanhMucID id,TenDanhMuc name FROM DanhMuc WHERE DangHoatDong=1");
    }

    // Brands: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Brands()
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT ThuongHieuID id,TenThuongHieu name FROM ThuongHieu WHERE DangHoatDong=1");
    }

    // Sizes: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Sizes()
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT KichCoID id,GiaTri name FROM KichCo ORDER BY ThuTu,CAST(GiaTri AS DECIMAL(8,2))");
    }

    // Colors: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Colors()
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT MauSacID id,TenMau name FROM MauSac WHERE DangHoatDong=1");
    }

    // PublicProduct: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> PublicProduct(object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(productSelect + @" WHERE s.SanPhamID=? AND s.TrangThai='DANG_BAN' AND d.DangHoatDong=1 AND t.DangHoatDong=1", values);
    }

    // AvailableVariants: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> AvailableVariants(object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT b.BienTheID id,b.SKU sku,kc.GiaTri size,m.TenMau color,m.MaHex hex,COALESCE(b.GiaBanRieng,?) price,COALESCE(k.SoLuongKhaDung,0) stock FROM BienTheSanPham b JOIN KichCo kc ON kc.KichCoID=b.KichCoID JOIN MauSac m ON m.MauSacID=b.MauSacID LEFT JOIN TonKho k ON k.BienTheID=b.BienTheID WHERE b.SanPhamID=? AND b.DangKinhDoanh=1 ORDER BY kc.ThuTu", values);
    }

    // Reviews: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Reviews(object? [] values)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT g.SoSao stars,g.NoiDung content,t.HoTen name,g.NgayTao date FROM DanhGia g JOIN TaiKhoan t ON t.TaiKhoanID=g.KhachHangID JOIN ChiTietDonHang ct ON ct.ChiTietDonHangID=g.ChiTietDonHangID JOIN BienTheSanPham b ON b.BienTheID=ct.BienTheID WHERE b.SanPhamID=? AND g.TrangThai='HIEN_THI' ORDER BY g.NgayTao DESC", values);
    }

    // Products: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Products()
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(productSelect + @" ORDER BY s.SanPhamID DESC");
    }

    // AddSize: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> AddSize(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO KichCo(HeSize,GiaTri,ThuTu) VALUES('EU',?,?) ON DUPLICATE KEY UPDATE DangHoatDong=1", values, connection);
    }

    // SizeByValue: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> SizeByValue(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT KichCoID id,GiaTri name FROM KichCo WHERE HeSize='EU' AND GiaTri=?", values, connection);
    }

    // AddColor: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> AddColor(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO MauSac(TenMau,MaHex) VALUES(?,?) ON DUPLICATE KEY UPDATE DangHoatDong=1", values, connection);
    }

    // ColorByName: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> ColorByName(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT MauSacID id,TenMau name FROM MauSac WHERE TenMau=?", values, connection);
    }

    // AllCategories: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> AllCategories()
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT DanhMucID id,TenDanhMuc name,DangHoatDong active,LaBanNhap draft FROM DanhMuc");
    }

    // AllBrands: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> AllBrands()
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT ThuongHieuID id,TenThuongHieu name,DangHoatDong active,LaBanNhap draft FROM ThuongHieu");
    }

    // AllColors: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> AllColors()
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT MauSacID id,TenMau name FROM MauSac");
    }

    // InsertProduct: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> InsertProduct(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO SanPham(MaSanPham,TenSanPham,MoTa,GiaBan,DanhMucID,ThuongHieuID,TrangThai,NguoiTaoID) VALUES(?,?,?,?,?,?,?,?)", values, connection);
    }

    // InsertImage: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> InsertImage(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO HinhAnhSanPham(SanPhamID,DuongDan,LaAnhChinh) VALUES(?,?,1)", values, connection);
    }

    // LockProduct: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockProduct(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT MaSanPham code,TenSanPham name,MoTa description,GiaBan price,DanhMucID categoryId,ThuongHieuID brandId,TrangThai status FROM SanPham WHERE SanPhamID=? FOR UPDATE", values, connection);
    }

    // LockImage: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockImage(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT HinhAnhID id,DuongDan image,BienTheID variant FROM HinhAnhSanPham WHERE SanPhamID=? ORDER BY LaAnhChinh DESC,ThuTu,HinhAnhID LIMIT 1 FOR UPDATE", values, connection);
    }

    // UpdateProduct: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> UpdateProduct(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE SanPham SET MaSanPham=?,TenSanPham=?,MoTa=?,GiaBan=?,DanhMucID=?,ThuongHieuID=? WHERE SanPhamID=?", values, connection);
    }

    // UpdateImage: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> UpdateImage(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE HinhAnhSanPham SET DuongDan=?,LaAnhChinh=1 WHERE HinhAnhID=?", values, connection);
    }

    // RemoveImage: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> RemoveImage(object? [] values, DbSession? connection = null)
    {
        // Database.Query → xóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"DELETE FROM HinhAnhSanPham WHERE HinhAnhID=?", values, connection);
    }

    // ClearPrimaryImage: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> ClearPrimaryImage(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE HinhAnhSanPham SET LaAnhChinh=0 WHERE SanPhamID=? AND LaAnhChinh=1", values, connection);
    }

    // SetProductStatus: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> SetProductStatus(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE SanPham SET TrangThai=?,NgayCapNhat=UTC_TIMESTAMP(),PhienBan=PhienBan+1 WHERE SanPhamID=?", values, connection);
    }

    // Variants: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Variants()
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query($@"SELECT b.BienTheID id,b.SanPhamID productId,b.KichCoID sizeId,b.MauSacID colorId,b.GiaBanRieng price,b.SKU sku,s.MaSanPham productCode,s.TenSanPham name,kc.GiaTri size,m.TenMau color,b.DangKinhDoanh active,k.SoLuongTon stock,k.SoLuongDaPhanBo reserved,k.SoLuongKhaDung available,k.MucCanhBao warning FROM BienTheSanPham b JOIN SanPham s ON s.SanPhamID=b.SanPhamID JOIN KichCo kc ON kc.KichCoID=b.KichCoID JOIN MauSac m ON m.MauSacID=b.MauSacID JOIN TonKho k ON k.BienTheID=b.BienTheID ORDER BY s.SanPhamID,b.BienTheID");
    }

    // InsertVariant: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> InsertVariant(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO BienTheSanPham(SanPhamID,SKU,KichCoID,MauSacID,GiaBanRieng) VALUES(?,?,?,?,?)", values, connection);
    }

    // CreateStock: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> CreateStock(object? [] values, DbSession? connection = null)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"INSERT INTO TonKho(BienTheID) VALUES(?)", values, connection);
    }

    // LockVariant: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockVariant(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT * FROM BienTheSanPham WHERE BienTheID=? FOR UPDATE", values, connection);
    }

    // CountVariantOrders: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> CountVariantOrders(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT COUNT(*) n FROM ChiTietDonHang WHERE BienTheID=?", values, connection);
    }

    // UpdateVariant: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> UpdateVariant(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE BienTheSanPham SET KichCoID=?,MauSacID=?,GiaBanRieng=? WHERE BienTheID=?", values, connection);
    }

    // SetVariantStatus: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> SetVariantStatus(object? [] values, DbSession? connection = null)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"UPDATE BienTheSanPham SET DangKinhDoanh=?,NgayCapNhat=UTC_TIMESTAMP() WHERE BienTheID=?", values, connection);
    }

    // LockVariantIdentity: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockVariantIdentity(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT SKU,DangKinhDoanh FROM BienTheSanPham WHERE BienTheID=? FOR UPDATE", values, connection);
    }

    // LockStock: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> LockStock(object? [] values, DbSession? connection = null)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT SoLuongTon,SoLuongDaPhanBo FROM TonKho WHERE BienTheID=? FOR UPDATE", values, connection);
    }

    // RemoveStock: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> RemoveStock(object? [] values, DbSession? connection = null)
    {
        // Database.Query → xóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"DELETE FROM TonKho WHERE BienTheID=?", values, connection);
    }

    // RemoveVariant: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> RemoveVariant(object? [] values, DbSession? connection = null)
    {
        // Database.Query → xóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"DELETE FROM BienTheSanPham WHERE BienTheID=?", values, connection);
    }

    // CreateTaxonomy: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> CreateTaxonomy(string table, object? [] values)
    {
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query($@"INSERT INTO {table}(Ten{table},MoTa,DangHoatDong,LaBanNhap) VALUES(?,?,?,?)", values);
    }

    // UpdateTaxonomy: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> UpdateTaxonomy(string table, string col, object? [] values)
    {
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query($@"UPDATE {table} SET Ten{table}=?,DangHoatDong=?,LaBanNhap=?,NgayCapNhat=UTC_TIMESTAMP() WHERE {col}=?", values);
    }
}
