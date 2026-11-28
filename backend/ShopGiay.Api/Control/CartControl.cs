using MySqlConnector;
using ShopGiay.Entity;

namespace ShopGiay.Control;

public sealed class CartControl(MySqlDataSource database)
{
    internal static async Task<long> CartId(MySqlConnection db, MySqlTransaction tx, long account, CancellationToken token)
    {
        await using var ensure = new MySqlCommand("""
            INSERT INTO GioHang(TaiKhoanID) VALUES(@account)
            ON DUPLICATE KEY UPDATE TaiKhoanID=VALUES(TaiKhoanID)
            """, db,tx);
        ensure.Parameters.AddWithValue("account",account);
        await ensure.ExecuteNonQueryAsync(token);
        await using var find = new MySqlCommand("SELECT GioHangID FROM GioHang WHERE TaiKhoanID=@account",db,tx);
        find.Parameters.AddWithValue("account",account);
        return Convert.ToInt64(await find.ExecuteScalarAsync(token));
    }

    public async Task<object> Read(long account,CancellationToken token)
    {
        await using var db = await database.OpenConnectionAsync(token);
        await using var tx = await db.BeginTransactionAsync(token);
        var id = await CartId(db,tx,account,token);
        await using var cmd = new MySqlCommand("""
            SELECT ct.BienTheID,ct.SoLuong,s.TenSanPham,COALESCE(b.GiaBanRieng,s.GiaBan),
                   kc.GiaTri,m.TenMau,k.SoLuongKhaDung,
                   (SELECT h.DuongDan FROM HinhAnhSanPham h WHERE h.SanPhamID=s.SanPhamID
                    ORDER BY h.LaAnhChinh DESC,h.ThuTu,h.HinhAnhID LIMIT 1)
            FROM ChiTietGioHang ct JOIN BienTheSanPham b ON b.BienTheID=ct.BienTheID
            JOIN SanPham s ON s.SanPhamID=b.SanPhamID JOIN KichCo kc ON kc.KichCoID=b.KichCoID
            JOIN MauSac m ON m.MauSacID=b.MauSacID JOIN TonKho k ON k.BienTheID=b.BienTheID
            WHERE ct.GioHangID=@id
            """,db,tx);
        cmd.Parameters.AddWithValue("id",id);
        var rows = new List<object>();
        await using (var reader = await cmd.ExecuteReaderAsync(token))
            while (await reader.ReadAsync(token)) rows.Add(new {
                variantId = reader.GetInt64(0), quantity = reader.GetInt32(1), name = reader.GetString(2),
                price = reader.GetDecimal(3), size = reader.GetString(4), color = reader.GetString(5),
                stock = reader.GetInt32(6), image = reader.IsDBNull(7) ? null : reader.GetString(7)
            });
        await tx.CommitAsync(token);
        return rows;
    }

    public async Task<bool> Update(long account,long variant,int quantity,CancellationToken token)
    {
        await using var db = await database.OpenConnectionAsync(token);
        await using var tx = await db.BeginTransactionAsync(token);
        var id = await CartId(db,tx,account,token);
        if (quantity > 0) {
            await using var check = new MySqlCommand("""
                SELECT k.SoLuongTon,k.SoLuongDaPhanBo FROM TonKho k JOIN BienTheSanPham b ON b.BienTheID=k.BienTheID
                JOIN SanPham s ON s.SanPhamID=b.SanPhamID WHERE b.BienTheID=@variant
                AND b.DangKinhDoanh=1 AND s.TrangThai='DANG_BAN'
                """,db,tx);
            check.Parameters.AddWithValue("variant",variant);
            await using var stock = await check.ExecuteReaderAsync(token);
            if (!await stock.ReadAsync(token) || !new TonKho(stock.GetInt32(0),stock.GetInt32(1)).ConDuHang(quantity)) return false;
        }
        await using var cmd = new MySqlCommand(quantity == 0
            ? "DELETE FROM ChiTietGioHang WHERE GioHangID=@cart AND BienTheID=@variant"
            : """
              INSERT INTO ChiTietGioHang(GioHangID,BienTheID,SoLuong) VALUES(@cart,@variant,@quantity)
              ON DUPLICATE KEY UPDATE SoLuong=VALUES(SoLuong),NgayCapNhat=UTC_TIMESTAMP()
              """,db,tx);
        cmd.Parameters.AddWithValue("cart",id);
        cmd.Parameters.AddWithValue("variant",variant);
        cmd.Parameters.AddWithValue("quantity",quantity);
        await cmd.ExecuteNonQueryAsync(token);
        await tx.CommitAsync(token);
        return true;
    }
}
