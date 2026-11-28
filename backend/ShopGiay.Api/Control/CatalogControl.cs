using System.Globalization;
using MySqlConnector;
using ShopGiay.Entity;

namespace ShopGiay.Control;

public sealed class CatalogControl(MySqlDataSource database)
{
    private const string Products = """
        SELECT s.SanPhamID id,s.MaSanPham code,s.TenSanPham name,s.MoTa description,
               s.GiaBan price,s.DanhMucID categoryId,s.ThuongHieuID brandId,s.TrangThai status,
               d.TenDanhMuc category,t.TenThuongHieu brand,
               (SELECT h.DuongDan FROM HinhAnhSanPham h WHERE h.SanPhamID=s.SanPhamID
                ORDER BY h.LaAnhChinh DESC,h.ThuTu,h.HinhAnhID LIMIT 1) image,
               COALESCE((SELECT SUM(k.SoLuongKhaDung) FROM BienTheSanPham b JOIN TonKho k ON k.BienTheID=b.BienTheID
                         WHERE b.SanPhamID=s.SanPhamID AND b.DangKinhDoanh=1),0) stock
        FROM SanPham s JOIN DanhMuc d ON d.DanhMucID=s.DanhMucID JOIN ThuongHieu t ON t.ThuongHieuID=s.ThuongHieuID
        WHERE s.TrangThai='DANG_BAN' AND d.DangHoatDong=1 AND t.DangHoatDong=1
        """;

    public async Task<object> Browse(IQueryCollection filters, CancellationToken token)
    {
        var clauses = new List<string>();
        var parameters = new Dictionary<string,object>();
        void Filter(string key, string predicate, Func<string,object> convert)
        {
            var value = filters[key].ToString();
            if (value.Length == 0) return;
            clauses.Add(" AND " + predicate);
            parameters.Add(key, convert(value.Trim()));
        }

        static int Id(string value) => int.TryParse(value, out var id) && id is > 0 and <= 100000
            ? id : throw new ArgumentOutOfRangeException(nameof(value));
        Filter("q", "(s.TenSanPham LIKE @q OR s.MaSanPham LIKE @q OR t.TenThuongHieu LIKE @q)", s => "%" + s[..Math.Min(100,s.Length)] + "%");
        Filter("category","s.DanhMucID=@category", s => Id(s));
        Filter("brand","s.ThuongHieuID=@brand", s => Id(s));
        Filter("max","s.GiaBan<=@max", s => decimal.TryParse(s,NumberStyles.Float,CultureInfo.InvariantCulture,out var price)
            && price is > 0 and <= 10000000000m ? Math.Round(price,MidpointRounding.AwayFromZero) : throw new ArgumentOutOfRangeException("max"));
        Filter("size","EXISTS (SELECT 1 FROM BienTheSanPham z JOIN KichCo kc ON kc.KichCoID=z.KichCoID JOIN TonKho k ON k.BienTheID=z.BienTheID WHERE z.SanPhamID=s.SanPhamID AND kc.GiaTri=@size AND z.DangKinhDoanh=1 AND k.SoLuongKhaDung>0)", s => s[..Math.Min(20,s.Length)]);
        Filter("color","EXISTS (SELECT 1 FROM BienTheSanPham z WHERE z.SanPhamID=s.SanPhamID AND z.MauSacID=@color AND z.DangKinhDoanh=1)", s => Id(s));
        var order = filters["sort"].ToString() switch {
            "price_asc" => "s.GiaBan ASC", "price_desc" => "s.GiaBan DESC",
            "newest" => "s.SanPhamID DESC", _ => "s.SanPhamID ASC"
        };
        await using var db = await database.OpenConnectionAsync(token);
        return new {
            products = await Rows(db, Products + string.Concat(clauses) + " ORDER BY " + order, token, parameters),
            categories = await Rows(db,"SELECT DanhMucID id,TenDanhMuc name FROM DanhMuc WHERE DangHoatDong=1",token),
            brands = await Rows(db,"SELECT ThuongHieuID id,TenThuongHieu name FROM ThuongHieu WHERE DangHoatDong=1",token),
            sizes = await Rows(db,"SELECT KichCoID id,GiaTri name FROM KichCo ORDER BY ThuTu,CAST(GiaTri AS DECIMAL(8,2))",token),
            colors = await Rows(db,"SELECT MauSacID id,TenMau name FROM MauSac WHERE DangHoatDong=1",token)
        };
    }

    public async Task<object?> Details(int id, CancellationToken token)
    {
        await using var db = await database.OpenConnectionAsync(token);
        var args = new Dictionary<string,object> { ["id"] = id };
        var product = (await Rows(db, Products + " AND s.SanPhamID=@id",token,args)).SingleOrDefault();
        if (product is null) return null;
        var variants = await Rows(db,"""
            SELECT b.BienTheID id,b.SKU sku,kc.GiaTri size,m.TenMau color,m.MaHex hex,b.GiaBanRieng price,
                   COALESCE(k.SoLuongKhaDung,0) stock
            FROM BienTheSanPham b JOIN KichCo kc ON kc.KichCoID=b.KichCoID JOIN MauSac m ON m.MauSacID=b.MauSacID
            LEFT JOIN TonKho k ON k.BienTheID=b.BienTheID WHERE b.SanPhamID=@id AND b.DangKinhDoanh=1 ORDER BY kc.ThuTu
            """,token,args);
        foreach (var variant in variants)
            variant["price"] = BienTheSanPham.LayGiaHienTai((decimal?)variant["price"], (decimal)product["price"]!);
        product["variants"] = variants;
        product["reviews"] = await Rows(db,"""
            SELECT g.SoSao stars,g.NoiDung content,t.HoTen name,g.NgayTao date
            FROM DanhGia g JOIN TaiKhoan t ON t.TaiKhoanID=g.KhachHangID
            JOIN ChiTietDonHang ct ON ct.ChiTietDonHangID=g.ChiTietDonHangID JOIN BienTheSanPham b ON b.BienTheID=ct.BienTheID
            WHERE b.SanPhamID=@id AND g.TrangThai='HIEN_THI' ORDER BY g.NgayTao DESC
            """,token,args);
        return product;
    }

    private static async Task<List<Dictionary<string,object?>>> Rows(MySqlConnection db, string sql,
        CancellationToken token, Dictionary<string,object>? parameters = null)
    {
        await using var command = new MySqlCommand(sql,db);
        if (parameters != null)
            foreach (var (key,value) in parameters) command.Parameters.AddWithValue(key,value);
        await using var reader = await command.ExecuteReaderAsync(token);
        var rows = new List<Dictionary<string,object?>>();
        while (await reader.ReadAsync(token)) {
            var row = new Dictionary<string,object?>();
            for (var i=0; i<reader.FieldCount; i++)
                row.Add(reader.GetName(i),reader.IsDBNull(i) ? null : reader.GetValue(i));
            rows.Add(row);
        }
        return rows;
    }
}
