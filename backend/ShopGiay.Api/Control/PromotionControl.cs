using System.Globalization;
using System.Text.Json;
using ShopGiay.Infrastructure;
using static ShopGiay.Infrastructure.ApiInput;
using Q = ShopGiay.Infrastructure.PromotionQueries;

namespace ShopGiay.Control;

public sealed class PromotionControl(Sql sql)
{
    public Task<List<dynamic>> List(bool publicOnly,CancellationToken token) =>
        sql.Run(db => db.Rows(publicOnly ? Q.Published : Q.List),token);

    private sealed record Fields(string Name,string Type,long Value,long Minimum,long? Maximum);

    private static Fields Read(JsonElement body,decimal? previousMaximum = null)
    {
        var name = body.Required("name","tên chương trình",250);
        var type = body.Text("type");
        if (type is not ("PHAN_TRAM" or "SO_TIEN")) throw new ApiException("Hình thức giảm không hợp lệ.");
        var raw = body.Field("maximum").ValueKind == JsonValueKind.Undefined ? previousMaximum?.ToString(CultureInfo.InvariantCulture) :
            body.Text("maximum").NullIfEmpty();
        const long max = 9007199254740991;
        var maximum = type == "PHAN_TRAM" && raw is not null ? (long?)Integer(raw,1,max) : null;
        return new Fields(name,type,Integer(body.Field("value"),1,type == "PHAN_TRAM" ? 100 : 10000000000),
            Integer(body.IsSet("minimum") ? body.Field("minimum") : 0,0,max),maximum);
    }

    private static DateTime Date(string raw,DateTime? fallback = null) =>
        raw == "" && fallback is DateTime old ? old :
        DateTimeOffset.TryParse(raw,CultureInfo.InvariantCulture,DateTimeStyles.AssumeUniversal,out var value)
            ? value.UtcDateTime : throw new ApiException("Ngày giờ chương trình không hợp lệ.");

    private static void Schedule(DateTime start,DateTime end,string status)
    {
        if (end <= start) throw new ApiException("Ngày kết thúc phải sau ngày bắt đầu.");
        if (status == "DANG_AP_DUNG" && end <= DateTime.UtcNow) throw new ApiException("Chương trình đã hết hạn.");
    }

    private static async Task<dynamic> Lock(SqlSession db,long id) =>
        await db.One("SELECT * FROM KhuyenMai WHERE KhuyenMaiID=? FOR UPDATE",id) ?? throw new ApiException("Không có chương trình.",404);

    public Task<object> Create(long actor,JsonElement body,CancellationToken token)
    {
        var code = body.Required("code","mã chương trình",50);
        var fields = Read(body);
        var status = body.Text("status").NullIfEmpty() ?? "NHAP";
        if (status is not ("NHAP" or "DANG_AP_DUNG")) throw new ApiException("Trạng thái tạo chương trình không hợp lệ.");
        var start = Date(body.Text("start"));
        var end = Date(body.Text("end"));
        Schedule(start,end,status);
        return sql.Transaction<object>(async db => {
            var id = await db.Insert("""
                INSERT INTO KhuyenMai(MaKhuyenMai,TenKhuyenMai,HinhThuc,GiaTri,GiaTriDonToiThieu,GiamToiDa,PhamVi,BatDauLuc,KetThucLuc,NguoiTaoID,TrangThai)
                VALUES(?,?,?,?,?,?,'TOAN_BO',?,?,?,?)
                """,code,fields.Name,fields.Type,fields.Value,fields.Minimum,fields.Maximum,start,end,actor,status);
            await db.Execute("INSERT INTO LichSuKhuyenMai(KhuyenMaiID,NguoiThucHienID,HanhDong,NoiDung) VALUES(?,?,'TAO',?)",id,actor,fields.Name);
            return new { id };
        },token);
    }

    public Task<object> Update(long id,long actor,JsonElement body,CancellationToken token)
    {
        var status = body.Text("status");
        if (status is not ("NHAP" or "DANG_AP_DUNG" or "KET_THUC")) throw new ApiException("Trạng thái không hợp lệ.");
        return sql.Transaction(async db => {
            var previous = await Lock(db,id);
            var start = Date(body.Text("start"),(DateTime)previous.BatDauLuc);
            var end = Date(body.Text("end"),(DateTime)previous.KetThucLuc);
            Schedule(start,end,status);
            if (body.TryGetProperty("name",out _)) {
                var f = Read(body,previous.HinhThuc == "PHAN_TRAM" ? (decimal?)previous.GiamToiDa : null);
                await db.Execute("UPDATE KhuyenMai SET TenKhuyenMai=?,HinhThuc=?,GiaTri=?,GiaTriDonToiThieu=?,GiamToiDa=? WHERE KhuyenMaiID=?",
                    f.Name,f.Type,f.Value,f.Minimum,f.Maximum,id);
            }
            await db.Execute("UPDATE KhuyenMai SET TrangThai=?,BatDauLuc=?,KetThucLuc=?,NgayCapNhat=UTC_TIMESTAMP() WHERE KhuyenMaiID=?",status,start,end,id);
            await db.Execute("INSERT INTO LichSuKhuyenMai(KhuyenMaiID,NguoiThucHienID,HanhDong,NoiDung) VALUES(?,?,?,?)",id,actor,status,$"{start:O} → {end:O}");
            return Ok;
        },token);
    }

    public Task<object> Delete(long id,long actor,CancellationToken token) =>
        sql.Transaction(async db => {
            var promo = await Lock(db,id);
            if (promo.TrangThai != "NHAP") throw new ApiException("Chỉ được xóa bản nháp.",409);
            if (await db.One("SELECT 1 FROM KhuyenMaiDonHang WHERE KhuyenMaiID=? LIMIT 1",id) is not null)
                throw new ApiException("Mã đã xuất hiện trong đơn hàng.",409);
            foreach (var table in new[] {"KhuyenMaiSanPham","KhuyenMaiDanhMuc","KhuyenMaiThuongHieu","LichSuKhuyenMai","KhuyenMai"})
                await db.Execute($"DELETE FROM {table} WHERE KhuyenMaiID=?",id);
            await db.Audit(actor,"XOA_NHAP","KhuyenMai",id,(string)promo.MaKhuyenMai);
            return Ok;
        },token);
}
