using System.Globalization;
using ShopGiay.Web.Infrastructure;
using ShopGiay.Web.Repositories;
using static ShopGiay.Web.Infrastructure.Input;

namespace ShopGiay.Web.Services;
public sealed class PromotionService(IPromotionRepository repository, Database db, SessionContext session, Operations operations)
{
    // Danh sách mã đang hiệu lực và còn lượt cho trang công khai.
    public Task<DbResult> Published() => repository.Published();
    // Quản lý xem cả bản nháp, mã đang áp dụng và đã kết thúc.
    public Task<DbResult> List() => repository.List();
    // Chuẩn hóa tiền giảm; giảm phần trăm có trần, giảm số tiền không dùng trần.
    private static object? [] Fields(Row b, object? previousMaximum = null)
    {
        // Tạo biến type để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var type = b.S("type");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (type is not ("SO_TIEN" or "PHAN_TRAM"))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Hình thức giảm không hợp lệ.");
        // Tạo biến raw để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var raw = b.ContainsKey("maximum") ? b["maximum"] : previousMaximum;
        // Tạo biến maximum để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        object? maximum = type == "SO_TIEN" || Text(raw) == "" ? null : Integer(raw, 1, 9007199254740991);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return[Required(b["name"], "tên chương trình"), type, Integer(b["value"], 1, type == "PHAN_TRAM" ? 100 : 10000000000), Integer(b["minimum"] ?? 0, 0, 9007199254740991), maximum];
    }

    // Giá trị datetime-local của bản cũ được coi là UTC như backend Node.js.
    private static DateTime Date(object? value) => value is DateTime d ? DateTime.SpecifyKind(d, DateTimeKind.Utc) : DateTime.TryParse(Text(value), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var result) ? result : throw new ApiException("Ngày giờ chương trình không hợp lệ.");
    // Kiểm tra lịch trước khi kích hoạt hoặc thay đổi thời hạn.
    private static (DateTime, DateTime) Schedule(object? start, object? end, string status)
    {
        // Tạo biến a để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var a = Date(start);
        // Tạo biến b để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var b = Date(end);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (b <= a)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Ngày kết thúc phải sau ngày bắt đầu.");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (status == "DANG_AP_DUNG" && b <= DateTime.UtcNow)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Chương trình đã hết hạn. Hãy chọn ngày kết thúc mới.");
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return (a, b);
    }

    // Tạo chương trình và lịch sử; phạm vi toàn bộ giữ nguyên tính năng biểu mẫu hiện có.
    public Task<object> Create(Row b) => db.Transaction<object>(async c =>
    {
        // Tạo biến status để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var status = b.S("status", "NHAP");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (status is not ("NHAP" or "DANG_AP_DUNG"))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Trạng thái tạo không hợp lệ.");
        // Gán kết quả vào var(start,end) để bước xử lý tiếp theo dùng dữ liệu mới.
        var(start, end) = Schedule(b["start"], b["end"], status);
        // Tạo biến fields để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var fields = Fields(b);
        // Tạo biến args để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var args = new List<object?>
        {
            Required(b["code"], "mã chương trình")
        };
        // Thực hiện bước args.AddRange trong PromotionService; tiếp tục theo luồng của hàm.
        args.AddRange(fields);
        // Thực hiện bước args.AddRange trong PromotionService; tiếp tục theo luồng của hàm.
        args.AddRange([start, end, session.UserId, status]);
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        var a = await db.Query("INSERT INTO KhuyenMai(MaKhuyenMai,TenKhuyenMai,HinhThuc,GiaTri,GiaTriDonToiThieu,GiamToiDa,PhamVi,BatDauLuc,KetThucLuc,NguoiTaoID,TrangThai) VALUES(?,?,?,?,?,?,'TOAN_BO',?,?,?,?)", args.ToArray(), c);
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        await db.Query("INSERT INTO LichSuKhuyenMai(KhuyenMaiID,NguoiThucHienID,HanhDong,NoiDung) VALUES(?,?,'TAO',?)", [a.InsertId, session.UserId, fields[0]], c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return new
        {
            id = a.InsertId
        };
    });
    // Sửa mã chỉ ảnh hưởng giao dịch mới; dữ liệu giảm trên đơn cũ đã được chụp tại lúc mua.
    public Task<object> Update(long id, Row b) => db.Transaction<object>(async c =>
    {
        // Tạo biến status để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var status = b.S("status");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (status is not ("NHAP" or "DANG_AP_DUNG" or "KET_THUC"))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Trạng thái không hợp lệ.");
        // Tạo biến p để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var p = (await db.Query("SELECT * FROM KhuyenMai WHERE KhuyenMaiID=? FOR UPDATE", [id], c)).One("Không có chương trình.");
        // Gán kết quả vào var(start,end) để bước xử lý tiếp theo dùng dữ liệu mới.
        var(start, end) = Schedule(b["start"] ?? p["BatDauLuc"], b["end"] ?? p["KetThucLuc"], status);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (b.ContainsKey("name"))
        {
            // Tạo biến args để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var args = Fields(b, p.S("HinhThuc") == "PHAN_TRAM" ? p["GiamToiDa"] : null).Concat(new object? [] { id }).ToArray();
            // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            await db.Query("UPDATE KhuyenMai SET TenKhuyenMai=?,HinhThuc=?,GiaTri=?,GiaTriDonToiThieu=?,GiamToiDa=? WHERE KhuyenMaiID=?", args, c);
        }

        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        await db.Query("UPDATE KhuyenMai SET TrangThai=?,BatDauLuc=?,KetThucLuc=?,NgayCapNhat=UTC_TIMESTAMP() WHERE KhuyenMaiID=?", [status, start, end, id], c);
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        await db.Query("INSERT INTO LichSuKhuyenMai(KhuyenMaiID,NguoiThucHienID,HanhDong,NoiDung) VALUES(?,?,?,?)", [id, session.UserId, status, $"{start:O} → {end:O}"], c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    });
    // Chỉ xóa nháp chưa xuất hiện trong đơn, dọn các bảng liên kết rồi ghi nhật ký.
    public Task<object> Delete(long id) => db.Transaction<object>(async c =>
    {
        // Tạo biến p để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var p = (await db.Query("SELECT * FROM KhuyenMai WHERE KhuyenMaiID=? FOR UPDATE", [id], c)).One();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (p.S("TrangThai") != "NHAP")
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Chỉ được xóa bản nháp.", 409);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if ((await db.Query("SELECT 1 FROM KhuyenMaiDonHang WHERE KhuyenMaiID=? LIMIT 1", [id], c)).Count > 0)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Mã đã xuất hiện trong đơn hàng. Hãy kết thúc chương trình để giữ lịch sử.", 409);
        // Xử lý từng table trong tập dữ liệu đã lấy/kiểm tra.
        foreach (var table in new[]
        {
            "KhuyenMaiSanPham",
            "KhuyenMaiDanhMuc",
            "KhuyenMaiThuongHieu",
            "LichSuKhuyenMai",
            "KhuyenMai"
        }

        )
            // Database.Query → xóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            await db.Query($"DELETE FROM {table} WHERE KhuyenMaiID=?", [id], c);
        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(session.UserId, "XOA_NHAP", "KhuyenMai", id, p.S("MaKhuyenMai"), c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    });
}
