using ShopGiay.Web.Infrastructure;
using ShopGiay.Web.Repositories;
using static ShopGiay.Web.Infrastructure.Input;

namespace ShopGiay.Web.Services;
public sealed class InventoryService(IInventoryRepository repository, Database db, SessionContext session, Operations operations)
{
    // Danh sách chứng từ kho cho quản lý.
    public Task<DbResult> Receipts() => repository.Receipts();
    // Chi tiết phiếu và các biến thể trong phiếu.
    public async Task<object> Receipt(long id)
    {
        // Tạo biến p để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var p = (await repository.Receipt([id])).One("Không tìm thấy phiếu.");
        // Gọi InventoryRepository.Lines → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        p["items"] = await repository.Lines([id]);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return p;
    }

    // Tạo nháp chống gửi trùng nhờ khóa tài khoản và mã yêu cầu duy nhất.
    public Task<object> CreateReceipt(Row b) => db.Transaction<object>(async c =>
    {
        // Gọi InventoryRepository.LockActor → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.LockActor([session.UserId], c);
        // Tạo biến old để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var old = (await repository.FindByKey([Key(b["requestId"])], c)).FirstOrDefault();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (old is not null)
        {
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (old.L("owner") != session.UserId)
                // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                throw new ApiException("Mã yêu cầu đã được sử dụng.", 409);
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return new
            {
                id = old.L("id")
            };
        }

        // Chuyển tới Operations.SaveReceipt để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        return await operations.SaveReceipt(c, session.UserId, b);
    });
    // Phiếu đã ghi sổ phải điều chỉnh bằng phiếu mới, không sửa lịch sử.
    public Task<object> UpdateReceipt(long id, Row b) => db.Transaction<object>(async c =>
    {
        // Tạo biến p để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var p = (await repository.LockReceipt([id], c)).One();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (p.S("TrangThai") != "NHAP")
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Chỉ được sửa phiếu nháp. Phiếu đã xác nhận cần lập phiếu điều chỉnh.");
        // Chuyển tới Operations.SaveReceipt để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        return await operations.SaveReceipt(c, session.UserId, b, id);
    });
    // Xác nhận phiếu → cập nhật tồn và sổ kho đúng một lần.
    public Task<object> ConfirmReceipt(long id) => db.Transaction<object>(async c =>
    {
        // Tạo biến p để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var p = (await repository.LockReceipt([id], c)).One();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (p.S("TrangThai") == "DA_XAC_NHAN")
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return Ok();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (p.S("TrangThai") != "NHAP")
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Phiếu đã hủy không thể xác nhận.");
        // Gọi InventoryRepository.LockLines → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        var lines = await repository.LockLines([id], c);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (lines.Count == 0)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Phiếu không có dòng hàng.");
        // Xử lý từng l trong tập dữ liệu đã lấy/kiểm tra.
        foreach (var l in lines)
            // Chuyển tới Operations.StockLog để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
            await operations.StockLog(c, session.UserId, l.L("BienTheID"), p.S("LoaiPhieu"), l.L("SoLuongThayDoi"), 0, null, l.L("ChiTietPhieuKhoID"), p.S("LyDo"));
        // Gọi InventoryRepository.ConfirmReceipt → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.ConfirmReceipt([session.UserId, id], c);
        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(session.UserId, "XAC_NHAN_PHIEU_KHO", "PhieuKho", id, p.S("LyDo"), c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    });
    // Chỉ hủy nháp; xác nhận lặp trả lại thành công an toàn.
    public Task<object> CancelReceipt(long id, Row b) => db.Transaction<object>(async c =>
    {
        // Tạo biến p để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var p = (await repository.LockReceipt([id], c)).One();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (p.S("TrangThai") == "DA_HUY")
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return Ok();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (p.S("TrangThai") != "NHAP")
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Không hủy phiếu đã ghi sổ; hãy lập phiếu điều chỉnh.");
        // Gọi InventoryRepository.CancelReceipt → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.CancelReceipt([id], c);
        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(session.UserId, "HUY_NHAP_KHO", "PhieuKho", id, Required(b["reason"], "lý do hủy"), c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    });
    // Thay ngưỡng cảnh báo, không thay đổi số lượng tồn.
    public Task<object> SetWarning(long id, Row b) => db.Transaction<object>(async c =>
    {
        // Tạo biến value để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var value = Integer(b["warning"], 0, 100000);
        // Gọi InventoryRepository.SetThreshold → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        var a = await repository.SetThreshold([value, id], c);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (a.AffectedRows == 0)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Không tìm thấy tồn kho.", 404);
        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(session.UserId, "NGUONG_CANH_BAO", "TonKho", id, value.ToString(), c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    });
    // Lịch sử có thể lọc theo biến thể.
    public Task<DbResult> StockHistory(Row q) => repository.History(q.S("variantId") == "" ? [] : [Positive(q["variantId"])]);
}
