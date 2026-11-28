using ShopGiay.Web.Infrastructure;
using ShopGiay.Web.Domain;
using static ShopGiay.Web.Infrastructure.Input;

namespace ShopGiay.Web.Services;
// Các nghiệp vụ dùng chung bắt buộc nhận transaction từ Service gọi tới.
public sealed class Operations(Database db)
{
    // Service → nhật ký hệ thống; ghi cùng giao dịch để dữ liệu và lịch sử nhất quán.
    public Task<DbResult> Audit(long? actor, string action, string entity, object id, string content, DbSession? c = null) => db.Query("INSERT INTO NhatKyHeThong(NguoiThucHienID,HanhDong,DoiTuong,MaDoiTuong,NoiDung,KetQua) VALUES(?,?,?,?,?,'THANH_CONG')", [actor, action, entity, Convert.ToString(id), content], c);
    // Giữ/xuất/giải phóng/nhận hàng: khóa TonKho → cập nhật số lượng → ghi BienDongKho.
    public async Task StockLog(DbSession c, long? actor, long variant, string type, long stockChange, long reservedChange, long? detailId, long? receiptId, string reason)
    {
        // Tạo biến stock để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var stock = (await db.Query("SELECT * FROM TonKho WHERE BienTheID=? FOR UPDATE", [variant], c)).One("Không có tồn kho.");
        // Tạo biến balance để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var balance = new InventoryBalance(stock.L("SoLuongTon"), stock.L("SoLuongDaPhanBo"));
        // Thực hiện bước balance.Apply trong Operations; tiếp tục theo luồng của hàm.
        balance.Apply(stockChange, reservedChange);
        // Tạo biến total để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var total = balance.OnHand;
        // Tạo biến reserved để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var reserved = balance.Reserved;
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        await db.Query("UPDATE TonKho SET SoLuongTon=SoLuongTon+?,SoLuongDaPhanBo=SoLuongDaPhanBo+?,NgayCapNhat=UTC_TIMESTAMP(),PhienBan=PhienBan+1 WHERE BienTheID=?", [stockChange, reservedChange, variant], c);
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        await db.Query("INSERT INTO BienDongKho(BienTheID,LoaiBienDong,ThayDoiTon,ThayDoiPhanBo,TonSau,PhanBoSau,ChiTietDonHangID,ChiTietPhieuKhoID,NguoiThucHienID,MaThaoTac,LyDo) VALUES(?,?,?,?,?,?,?,?,?,?,?)", [variant, type, stockChange, reservedChange, total, reserved, detailId, receiptId, actor, Guid.NewGuid().ToString(), reason], c);
    }

    // Hủy đơn có tiền đã thu → tạo khoản hoàn chờ xử lý, không coi là đã hoàn tiền.
    public async Task ReserveRefunds(DbSession c, long orderId, string reason)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        var payments = await db.Query("SELECT t.*,COALESCE((SELECT SUM(h.SoTien) FROM HoanTien h WHERE h.ThanhToanID=t.ThanhToanID AND h.TrangThai<>'DA_HUY'),0) reserved FROM ThanhToan t WHERE t.DonHangID=? AND t.TrangThai='DA_XAC_NHAN' ORDER BY t.ThanhToanID FOR UPDATE", [orderId], c);
        // Xử lý từng p trong tập dữ liệu đã lấy/kiểm tra.
        foreach (var p in payments)
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (p.N("SoTien") > p.N("reserved"))
                // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
                await db.Query("INSERT INTO HoanTien(DonHangID,ThanhToanID,MaYeuCau,LyDo,SoTien) VALUES(?,?,?,?,?)", [orderId, p.L("ThanhToanID"), Guid.NewGuid().ToString(), reason, p.N("SoTien") - p.N("reserved")], c);
    }

    // Trả hàng → phân bổ khoản hoàn trên từng khoản tiền thực thu, không vượt số tiền đã thu.
    public async Task PrepareReturnRefunds(DbSession c, long returnId, long orderId, string reason)
    {
        // Tạo biến owed để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var owed = (await db.Query("SELECT COALESCE(SUM(TienHoanDuocDuyet),0) owed FROM ChiTietDoiTra WHERE YeuCauDoiTraID=?", [returnId], c)).One().N("owed");
        // Tạo biến reserved để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var reserved = (await db.Query("SELECT COALESCE(SUM(SoTien),0) reserved FROM HoanTien WHERE YeuCauDoiTraID=? AND TrangThai<>'DA_HUY'", [returnId], c)).One().N("reserved");
        // Tạo biến remaining để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var remaining = owed - reserved;
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        var payments = await db.Query("SELECT t.*,COALESCE((SELECT SUM(h.SoTien) FROM HoanTien h WHERE h.ThanhToanID=t.ThanhToanID AND h.TrangThai<>'DA_HUY'),0) used FROM ThanhToan t WHERE t.DonHangID=? AND t.TrangThai='DA_XAC_NHAN' ORDER BY t.ThanhToanID FOR UPDATE", [orderId], c);
        // Xử lý từng p trong tập dữ liệu đã lấy/kiểm tra.
        foreach (var p in payments)
        {
            // Tạo biến amount để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var amount = Math.Min(remaining, p.N("SoTien") - p.N("used"));
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (amount <= 0)
                // Bỏ qua dòng không cần thao tác và chuyển sang phần tử tiếp theo.
                continue;
            // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            await db.Query("INSERT INTO HoanTien(DonHangID,ThanhToanID,YeuCauDoiTraID,MaYeuCau,LyDo,SoTien) VALUES(?,?,?,?,?,?)", [orderId, p.L("ThanhToanID"), returnId, Guid.NewGuid().ToString(), reason, amount], c);
            // Gán kết quả vào remaining để bước xử lý tiếp theo dùng dữ liệu mới.
            remaining -= amount;
        }
    }

    // Phiếu kho nháp: kiểm tra dòng, chứng từ, nhà cung cấp rồi lưu; chưa thay đổi tồn.
    public async Task<object> SaveReceipt(DbSession c, long actor, Row body, long? id = null)
    {
        // Tạo biến type để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var type = body.S("type");
        // Tạo biến reason để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var reason = Required(body["reason"], "lý do");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (type is not ("NHAP" or "DIEU_CHINH"))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Loại phiếu không hợp lệ.");
        // Tạo biến reference để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var reference = Text(body["reference"], 100);
        // Tạo biến supplier để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var supplier = Text(body["supplier"], 200);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (type == "NHAP" && (reference.Length == 0 || supplier.Length == 0))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Phiếu nhập cần chứng từ và nhà cung cấp.");
        // Tạo biến lines để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var lines = body.Rows("items").OrderBy(x => x.N("variantId")).ToList();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (lines.Count is < 1 or > 100 || lines.Select(x => x.N("variantId")).Distinct().Count() != lines.Count)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Phiếu cần 1–100 dòng, không lặp biến thể.");
        // Xử lý từng line trong tập dữ liệu đã lấy/kiểm tra.
        foreach (var line in lines)
        {
            // Tạo biến quantity để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var quantity = Integer(line["quantity"], -100000, 100000);
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (quantity == 0 || (type == "NHAP" && quantity < 0))
                // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                throw new ApiException("Số lượng không hợp lệ.");
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (line["cost"] is not null && line.S("cost") != "" && (line.N("cost", -1) < 0 || line.N("cost") > 10000000000m))
                // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                throw new ApiException("Giá nhập không hợp lệ.");
            // Thực hiện bước (await db.Query("SELECT BienTheID FROM BienTheSanPham WHERE BienTheID=?",[Positive(line["variantId"])],c)).One trong Operations; tiếp tục theo luồng của hàm.
            (await db.Query("SELECT BienTheID FROM BienTheSanPham WHERE BienTheID=?", [Positive(line["variantId"])], c)).One("Biến thể không tồn tại.");
        }

        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (id is not null)
        {
            // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            await db.Query("UPDATE PhieuKho SET LoaiPhieu=?,MaChungTuThamChieu=?,LyDo=?,NhaCungCap=? WHERE PhieuKhoID=?", [type, reference.Length > 0 ? reference : null, reason, supplier, id], c);
            // Database.Query → xóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            await db.Query("DELETE FROM ChiTietPhieuKho WHERE PhieuKhoID=?", [id], c);
        }
        else
            // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            id = (await db.Query("INSERT INTO PhieuKho(MaPhieu,MaChongLap,LoaiPhieu,MaChungTuThamChieu,LyDo,NguoiThucHienID,TrangThai,NhaCungCap) VALUES(?,?,?,?,?,?,'NHAP',?)", [Code("PK"), Key(body["requestId"]), type, reference.Length > 0 ? reference : null, reason, actor, supplier], c)).InsertId;
        // Xử lý từng line trong tập dữ liệu đã lấy/kiểm tra.
        foreach (var line in lines)
            // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            await db.Query("INSERT INTO ChiTietPhieuKho(PhieuKhoID,BienTheID,SoLuongThayDoi,DonGiaNhap) VALUES(?,?,?,?)", [id, line.L("variantId"), line.L("quantity"), line.S("cost") == "" ? null : line.N("cost")], c);
        // Thực hiện bước Audit trong Operations; tiếp tục theo luồng của hàm.
        await Audit(actor, "LUU_NHAP_KHO", "PhieuKho", id.Value, reason, c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return new
        {
            id
        };
    }
}
