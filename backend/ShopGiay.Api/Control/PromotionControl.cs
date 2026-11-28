using System.Globalization;
using System.Text.Json;
using ShopGiay.Entity;
using ShopGiay.Infrastructure;
using static ShopGiay.Infrastructure.ApiInput;
using Q = ShopGiay.Infrastructure.PromotionQueries;

namespace ShopGiay.Control;

public sealed class PromotionControl(Sql sql,PromotionRepository promotions)
{
    public Task<List<dynamic>> List(bool publicOnly,CancellationToken token) =>
        sql.Run(db => db.Rows(publicOnly ? Q.Published : Q.List),token);

    private sealed record Fields(string Name,string Type,long Value,long Minimum,long? Maximum);

    // Chuẩn hóa hình thức giảm, giá trị và các giới hạn từ yêu cầu HTTP.
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

    // Đọc thời gian UTC và giữ mốc cũ nếu yêu cầu cập nhật bỏ trống.
    private static DateTime Date(string raw,DateTime? fallback = null) =>
        raw == "" && fallback is DateTime old ? old :
        DateTimeOffset.TryParse(raw,CultureInfo.InvariantCulture,DateTimeStyles.AssumeUniversal,out var value)
            ? value.UtcDateTime : throw new ApiException("Ngày giờ chương trình không hợp lệ.");

    // Lấy Entity có khóa ghi hoặc trả lỗi không tìm thấy chương trình.
    private async Task<KhuyenMai> Lock(SqlSession db,long id) =>
        await promotions.Khoa(db,id) ?? throw new ApiException("Không có chương trình.",404);

    // Dựng chương trình, kiểm tra lịch rồi lưu chương trình và lịch sử tạo cùng lúc.
    public Task<object> Create(long actor,JsonElement body,CancellationToken token)
    {
        var code = body.Required("code","mã chương trình",50);
        var fields = Read(body);
        var status = body.Text("status").NullIfEmpty() ?? "NHAP";
        if (status is not ("NHAP" or "DANG_AP_DUNG")) throw new ApiException("Trạng thái tạo chương trình không hợp lệ.");
        var start = Date(body.Text("start"));
        var end = Date(body.Text("end"));
        var promo = new KhuyenMai { MaKhuyenMai = code, PhamVi = "TOAN_BO", NguoiTaoID = actor };
        promo.CapNhatDieuKien(fields.Name,fields.Type,fields.Value,fields.Minimum,fields.Maximum);
        promo.DatLich(status,start,end,DateTime.UtcNow);
        return sql.Transaction<object>(async db => {
            var id = await promotions.Them(db,promo);
            await db.Execute("INSERT INTO LichSuKhuyenMai(KhuyenMaiID,NguoiThucHienID,HanhDong,NoiDung) VALUES(?,?,'TAO',?)",id,actor,fields.Name);
            return new { id };
        },token);
    }

    // Khóa chương trình, áp dụng lịch và điều kiện mới rồi lưu cùng lịch sử thao tác.
    public Task<object> Update(long id,long actor,JsonElement body,CancellationToken token)
    {
        var status = body.Text("status");
        if (status is not ("NHAP" or "DANG_AP_DUNG" or "KET_THUC")) throw new ApiException("Trạng thái không hợp lệ.");
        return sql.Transaction(async db => {
            var previous = await Lock(db,id);
            var start = Date(body.Text("start"),previous.BatDauLuc);
            var end = Date(body.Text("end"),previous.KetThucLuc);
            previous.DatLich(status,start,end,DateTime.UtcNow);
            if (body.TryGetProperty("name",out _)) {
                var f = Read(body,previous.HinhThuc == "PHAN_TRAM" ? previous.GiamToiDa : null);
                previous.CapNhatDieuKien(f.Name,f.Type,f.Value,f.Minimum,f.Maximum);
            }
            await promotions.Luu(db,previous);
            await db.Execute("INSERT INTO LichSuKhuyenMai(KhuyenMaiID,NguoiThucHienID,HanhDong,NoiDung) VALUES(?,?,?,?)",id,actor,status,$"{start:O} → {end:O}");
            return Ok;
        },token);
    }

    // Chỉ xóa bản nháp chưa gắn với đơn, sau đó ghi nhật ký quản lý.
    public Task<object> Delete(long id,long actor,CancellationToken token) =>
        sql.Transaction(async db => {
            var promo = await Lock(db,id);
            if (promo.TrangThai != "NHAP") throw new ApiException("Chỉ được xóa bản nháp.",409);
            if (await db.One("SELECT 1 FROM KhuyenMaiDonHang WHERE KhuyenMaiID=? LIMIT 1",id) is not null)
                throw new ApiException("Mã đã xuất hiện trong đơn hàng.",409);
            foreach (var table in new[] {"KhuyenMaiSanPham","KhuyenMaiDanhMuc","KhuyenMaiThuongHieu","LichSuKhuyenMai","KhuyenMai"})
                await db.Execute($"DELETE FROM {table} WHERE KhuyenMaiID=?",id);
            await db.Audit(actor,"XOA_NHAP","KhuyenMai",id,promo.MaKhuyenMai);
            return Ok;
        },token);
}
