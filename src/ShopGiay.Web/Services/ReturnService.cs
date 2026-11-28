using ShopGiay.Web.Infrastructure;
using ShopGiay.Web.Repositories;
using static ShopGiay.Web.Infrastructure.Input;

namespace ShopGiay.Web.Services;
public sealed class ReturnService(IReturnRepository repository, Database db, SessionContext session, Operations operations, StoreSettings settings)
{
    // Khách chỉ đánh giá dòng hàng của đơn mình đã hoàn thành; UNIQUE ngăn đánh giá trùng.
    public async Task<object> Review(Row b)
    {
        // Tạo biến stars để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var stars = Integer(b["stars"], 1, 5);
        // Tạo biến item để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var item = Positive(b["itemId"]);
        // Tạo biến line để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var line = (await repository.ReviewableLine([item, session.UserId])).One("Chỉ đánh giá sản phẩm trong đơn đã hoàn thành.");
        // Gọi ReturnRepository.InsertReview → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.InsertReview([line.L("DonHangID"), item, session.UserId, stars, Text(b["content"], 2000)]);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    }

    // Danh sách yêu cầu trả hàng thuộc tài khoản hiện tại.
    public Task<DbResult> ListMine() => repository.ForCustomer([session.UserId]);
    // Quản lý xem yêu cầu cùng ảnh minh chứng và khoản hoàn được duyệt.
    public Task<DbResult> ListAll() => repository.All();
    // Quản lý xem các khoản hoàn chờ/đã xác nhận/thất bại.
    public Task<DbResult> Refunds() => repository.AllRefunds();
    // Gửi yêu cầu: chống lặp, kiểm quyền sở hữu, hạn trả và tổng lượng yêu cầu chưa bị từ chối.
    public Task<object> RequestReturn(Row b) => db.Transaction<object>(async c =>
    {
        // Tạo biến item để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var item = Positive(b["itemId"]);
        // Tạo biến qty để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var qty = Positive(b["quantity"]);
        // Tạo biến key để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var key = Key(b.S("requestId", Guid.NewGuid().ToString()));
        // Gọi ReturnRepository.LockCustomer → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.LockCustomer([session.UserId], c);
        // Tạo biến old để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var old = (await repository.FindByKey([key], c)).FirstOrDefault();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (old is not null)
        {
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (old.L("owner") != session.UserId)
                // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                throw new ApiException("Mã yêu cầu đã sử dụng.", 409);
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return Ok();
        }

        // Tạo biến line để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var line = (await repository.LockEligibleLine([item, session.UserId], c)).One("Đơn chưa đủ điều kiện đổi/trả.");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (line.D("NgayGiao")is not { } delivered || DateTime.UtcNow - delivered > TimeSpan.FromDays(settings.ReturnDays))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException($"Đã hết thời hạn {settings.ReturnDays} ngày gửi yêu cầu trả hàng.");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if ((await repository.RequestedQuantity([item], c)).One().N("n") + qty > line.N("SoLuong"))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Số lượng vượt quá đơn.");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (b.S("type") != "TRA")
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Đổi size được xử lý bằng trả hàng, hoàn tiền và đặt đơn mới.");
        // Gọi ReturnRepository.CreateRequest → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        var a = await repository.CreateRequest([Code("DT"), key, line.L("DonHangID"), session.UserId, "TRA", Required(b["reason"], "lý do")], c);
        // Gọi ReturnRepository.AddLine → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.AddLine([a.InsertId, line.L("DonHangID"), item, qty], c);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (b.S("image") != "")
        {
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (!Uri.TryCreate(b.S("image"), UriKind.Absolute, out var url) || url.Scheme is not ("http" or "https"))
                // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                throw new ApiException("Ảnh bằng chứng phải là URL HTTP(S).");
            // Gọi ReturnRepository.AttachImage → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
            await repository.AttachImage([a.InsertId, Image(b["image"])], c);
        }

        // Gọi ReturnRepository.RecordSubmission → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.RecordSubmission([a.InsertId, session.UserId], c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    });
    // Chi tiết của khách: quyết định, hàng nhận lại, lịch sử và khoản hoàn.
    public async Task<object> Detail(long id)
    {
        // Tạo biến y để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var y = (await repository.OwnedRequest([id, session.UserId])).One("Không tìm thấy yêu cầu.");
        // Gọi ReturnRepository.Lines → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        y["items"] = await repository.Lines([id]);
        // Gọi ReturnRepository.History → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        y["history"] = await repository.History([id]);
        // Gọi ReturnRepository.Refunds → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        y["refunds"] = await repository.Refunds([id]);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return y;
    }

    // Duyệt/từ chối → nhận hàng → chuẩn bị khoản hoàn → hoàn tất khi đã hoàn đủ tiền.
    public Task<object> ProcessReturn(long id, Row b) => db.Transaction<object>(async c =>
    {
        // Tạo biến y để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var y = (await repository.LockRequest([id], c)).One("Không có yêu cầu.");
        // Gọi ReturnRepository.LockLines → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        var lines = await repository.LockLines([id], c);
        // Tạo biến action để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var action = b.S("action");
        // Tạo biến status để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        string status;
        // Chọn đúng nhánh thao tác theo dữ liệu yêu cầu và trạng thái hiện hành.
        switch (action)
        {
            case "approve":
            case "reject":
                // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
                if (y.S("TrangThai") != "CHO_DUYET")
                    // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                    throw new ApiException("Yêu cầu đã được duyệt.");
                // Gán kết quả vào status để bước xử lý tiếp theo dùng dữ liệu mới.
                status = action == "approve" ? "DA_DUYET" : "TU_CHOI";
                // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
                if (action == "approve")
                    // Xử lý từng l trong tập dữ liệu đã lấy/kiểm tra.
                    foreach (var l in lines)
                        // Gọi ReturnRepository.ApproveLine → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
                        await repository.ApproveLine([Math.Floor(l.N("paid") * l.N("SoLuongYeuCau") / l.N("bought")), l.L("ChiTietDoiTraID")], c);
                // Gọi ReturnRepository.RecordDecision → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
                await repository.RecordDecision([session.UserId, Required(b["reason"], "lý do xử lý"), id], c);
                // Kết thúc nhánh thao tác hiện tại, tiếp tục bước xử lý chung sau switch.
                break;
            case "receive":
                // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
                if (y.S("TrangThai") != "DA_DUYET")
                    // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                    throw new ApiException("Chỉ nhận hàng một lần sau khi duyệt.");
                // Gán kết quả vào status để bước xử lý tiếp theo dùng dữ liệu mới.
                status = "DANG_XU_LY";
                // Xử lý từng l trong tập dữ liệu đã lấy/kiểm tra.
                foreach (var l in lines)
                {
                    // Tạo biến sellable để dùng ở các bước tiếp theo trong phạm vi hiện tại.
                    var sellable = b.B("sellable") ? l.L("SoLuongDuyet") : 0;
                    // Gọi ReturnRepository.ReceiveLine → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
                    await repository.ReceiveLine([sellable, session.UserId, Text(b["reason"]), l.L("ChiTietDoiTraID")], c);
                    // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
                    if (sellable == 0)
                        // Bỏ qua dòng không cần thao tác và chuyển sang phần tử tiếp theo.
                        continue;
                    // Gọi ReturnRepository.CreateReceipt → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
                    var a = await repository.CreateReceipt([Code("HT"), Guid.NewGuid().ToString(), "Nhận hàng trả " + y.S("MaYeuCau"), session.UserId], c);
                    // Gọi ReturnRepository.AddReceiptLine → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
                    var p = await repository.AddReceiptLine([a.InsertId, l.L("BienTheID"), sellable, l.L("ChiTietDoiTraID")], c);
                    // Chuyển tới Operations.StockLog để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
                    await operations.StockLog(c, session.UserId, l.L("BienTheID"), "NHAN_TRA", sellable, 0, null, p.InsertId, "Hàng trả đủ điều kiện bán lại");
                }

                // Chuyển tới Operations.PrepareReturnRefunds để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
                await operations.PrepareReturnRefunds(c, id, y.L("DonHangID"), y.S("LyDo"));
                // Kết thúc nhánh thao tác hiện tại, tiếp tục bước xử lý chung sau switch.
                break;
            case "prepare-refund":
                // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
                if (y.S("TrangThai") != "DANG_XU_LY")
                    // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                    throw new ApiException("Cần nhận hàng trước khi lập khoản hoàn.");
                // Chuyển tới Operations.PrepareReturnRefunds để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
                await operations.PrepareReturnRefunds(c, id, y.L("DonHangID"), y.S("LyDo"));
                // Gán kết quả vào status để bước xử lý tiếp theo dùng dữ liệu mới.
                status = "DANG_XU_LY";
                // Kết thúc nhánh thao tác hiện tại, tiếp tục bước xử lý chung sau switch.
                break;
            case "complete":
                // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
                if (y.S("TrangThai") != "DANG_XU_LY")
                    // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                    throw new ApiException("Cần nhận và kiểm tra hàng trước.");
                // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
                if ((await repository.PendingRefunds([id], c)).One().N("n") > 0)
                    // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                    throw new ApiException("Cần xác nhận hoàn tiền trước khi hoàn tất.");
                // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
                if ((await repository.ConfirmedRefunds([id], c)).One().N("refunded") < (await repository.ApprovedTotal([id], c)).One().N("owed"))
                    // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                    throw new ApiException("Chưa hoàn đủ tiền được duyệt. Đối soát khoản thu rồi lập bổ sung khoản hoàn.");
                // Gán kết quả vào status để bước xử lý tiếp theo dùng dữ liệu mới.
                status = "HOAN_TAT";
                // Gọi ReturnRepository.MarkComplete → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
                await repository.MarkComplete([id], c);
                // Kết thúc nhánh thao tác hiện tại, tiếp tục bước xử lý chung sau switch.
                break;
            default:
                // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                throw new ApiException("Thao tác không hợp lệ.");
        }

        // Gọi ReturnRepository.SetStatus → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.SetStatus([status, id], c);
        // Gọi ReturnRepository.RecordHistory → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.RecordHistory([id, y.S("TrangThai"), status, session.UserId, b.S("reason") == "" ? action : Text(b["reason"])], c);
        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(session.UserId, "XU_LY_DOI_TRA", "YeuCauDoiTra", id, action, c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    });
    // Xác nhận hoàn tiền: khóa khoản hoàn và giao dịch, chống hoàn vượt thực thu.
    public Task<object> ConfirmRefund(long id, Row b) => db.Transaction<object>(async c =>
    {
        // Tạo biến h để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var h = (await repository.LockRefund([id], c)).One("Không có khoản hoàn.");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (h.S("TrangThai") == "DA_XAC_NHAN")
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return Ok();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (h.S("TrangThai")is not ("CHO_XU_LY" or "DANG_XU_LY"))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Khoản hoàn cần ở trạng thái chờ; hãy thử lại nếu đã thất bại.");
        // Tạo biến p để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var p = (await repository.LockPayment([h.L("ThanhToanID")], c)).One();
        // Tạo biến returned để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var returned = (await repository.RefundedForPayment([h.L("ThanhToanID")], c)).One().N("returned");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (p.S("TrangThai") != "DA_XAC_NHAN" || returned + h.N("SoTien") > p.N("SoTien"))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Tổng hoàn vượt tiền thực thu.");
        // Gọi ReturnRepository.ConfirmRefund → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.ConfirmRefund([session.UserId, Required(b["proof"], "bằng chứng hoàn tiền"), id], c);
        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(session.UserId, "XAC_NHAN_HOAN", "HoanTien", id, "Xác nhận có bằng chứng", c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    });
    // Thử lại khoản thất bại hoặc đánh dấu thất bại; không đổi tiền và không tạo khoản mới.
    public Task<object> RefundAction(long id, Row b, bool retry) => db.Transaction<object>(async c =>
    {
        // Tạo biến h để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var h = (await repository.LockRefund([id], c)).One("Không có khoản hoàn.");
        // Tạo biến current để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var current = h.S("TrangThai");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (retry)
        {
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (current == "CHO_XU_LY")
                // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
                return Ok();
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (current != "THAT_BAI")
                // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                throw new ApiException("Chỉ thử lại khoản hoàn đã thất bại.");
            // Gọi ReturnRepository.RetryRefund → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
            await repository.RetryRefund([id], c);
        }
        else
        {
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (current == "THAT_BAI")
                // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
                return Ok();
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (current is not ("CHO_XU_LY" or "DANG_XU_LY"))
                // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                throw new ApiException("Khoản hoàn không còn ở trạng thái chờ.");
            // Gọi ReturnRepository.FailRefund → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
            await repository.FailRefund([id], c);
        }

        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(session.UserId, retry ? "THU_LAI_HOAN" : "HOAN_THAT_BAI", "HoanTien", id, Required(b["reason"], "lý do"), c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    });
}
