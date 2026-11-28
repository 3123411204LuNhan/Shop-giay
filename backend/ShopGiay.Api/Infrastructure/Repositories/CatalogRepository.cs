using ShopGiay.Entity;

namespace ShopGiay.Infrastructure;

public sealed class CatalogRepository
{
    // Đọc sản phẩm có khóa để thay đổi nội dung hoặc trạng thái bán.
    public Task<SanPham?> KhoaSanPham(SqlSession db,long id) =>
        db.One<SanPham>(CatalogQueries.LockProduct,id);

    // Lưu thông tin sản phẩm mới và trả mã dùng cho ảnh và nhật ký.
    public Task<long> ThemSanPham(SqlSession db,SanPham p) =>
        db.Insert(CatalogQueries.InsertProduct,p.MaSanPham,p.TenSanPham,p.MoTa,p.GiaBan,p.DanhMucID,p.ThuongHieuID,p.TrangThai,p.NguoiTaoID);

    // Ghi nội dung, giá và phân loại đã được Entity cập nhật.
    public Task LuuThongTin(SqlSession db,SanPham p) =>
        db.Execute(CatalogQueries.UpdateProduct,p.MaSanPham,p.TenSanPham,p.MoTa,p.GiaBan,p.DanhMucID,p.ThuongHieuID,p.SanPhamID);

    // Ghi trạng thái bán của sản phẩm đã khóa.
    public Task LuuTrangThai(SqlSession db,SanPham p) =>
        db.Execute(CatalogQueries.SetProductStatus,p.TrangThai,p.SanPhamID);

    // Đọc biến thể có khóa để thay đổi trạng thái hoặc kiểm tra trước khi xóa.
    public Task<BienTheSanPham?> KhoaBienThe(SqlSession db,long id) =>
        db.One<BienTheSanPham>(CatalogQueries.LockVariant,id);

    // Tạo biến thể theo sản phẩm, SKU, size, màu và giá riêng đã kiểm tra.
    public Task<long> ThemBienThe(SqlSession db,BienTheSanPham b) =>
        db.Insert(CatalogQueries.InsertVariant,b.SanPhamID,b.SKU,b.KichCoID,b.MauSacID,b.GiaBanRieng);

    // Lưu size, màu và giá riêng từ Entity sau khi đã kiểm tra lịch sử đơn.
    public Task LuuThongTin(SqlSession db,BienTheSanPham b) =>
        db.Execute(CatalogQueries.UpdateVariant,b.KichCoID,b.MauSacID,b.GiaBanRieng,b.BienTheID);

    // Ghi trạng thái ẩn hoặc mở bán của biến thể.
    public Task LuuTrangThai(SqlSession db,BienTheSanPham b) =>
        db.Execute(CatalogQueries.SetVariantStatus,b.DangKinhDoanh,b.BienTheID);

    // Ghi thông tin và trạng thái danh mục vào bảng phân loại chuẩn.
    public Task<long> LuuDanhMuc(SqlSession db,DanhMuc d) =>
        LuuPhanLoai(db,d.DanhMucID,"DanhMuc","DanhMucID","TenDanhMuc",d.TenDanhMuc,d.MoTa,d.DangHoatDong,d.LaBanNhap);

    // Ghi thông tin và trạng thái thương hiệu vào bảng phân loại chuẩn.
    public Task<long> LuuThuongHieu(SqlSession db,ThuongHieu t) =>
        LuuPhanLoai(db,t.ThuongHieuID,"ThuongHieu","ThuongHieuID","TenThuongHieu",t.TenThuongHieu,t.MoTa,t.DangHoatDong,t.LaBanNhap);

    // Tạo hoặc cập nhật phân loại. Tên bảng và cột chỉ đến từ hai phương thức cố định ở trên.
    private static async Task<long> LuuPhanLoai(SqlSession db,int id,string table,string key,string column,
        string name,string? description,bool active,bool draft)
    {
        if (id == 0)
            return await db.Insert($"INSERT INTO {table}({column},MoTa,DangHoatDong,LaBanNhap) VALUES(?,?,?,?)",
                name,description,active,draft);
        await db.Execute($"UPDATE {table} SET {column}=?,DangHoatDong=?,LaBanNhap=?,NgayCapNhat=UTC_TIMESTAMP() WHERE {key}=?",
            name,active,draft,id);
        return id;
    }
}
