using System.Text.Json;
using ShopGiay.Web.Domain;
using ShopGiay.Web.Infrastructure;
using ShopGiay.Web.Repositories;
using static ShopGiay.Web.Infrastructure.Input;

namespace ShopGiay.Web.Services;
public sealed class OrderService(IOrderRepository repository, Database db, SessionContext session, Operations operations, StoreSettings settings)
{
    // Khách chỉ xem đơn của mình; manager xem được chi tiết để xử lý nghiệp vụ.
    public async Task<object> Details(long id)
    {
        // Tạo biến o để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var o = (await repository.FindById([id])).One("Không tìm thấy đơn.");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (o.L("customerId") != session.UserId && !session.Roles.Contains("SHOP_MANAGER"))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Không có quyền xem đơn.", 403);
        // Gọi OrderRepository.Items → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        o["items"] = await repository.Items([id]);
        // Gọi OrderRepository.History → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        o["history"] = await repository.History([id]);
        // Gọi OrderRepository.Refunds → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        o["refunds"] = await repository.Refunds([id]);
        // Gọi OrderRepository.Payments → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        o["payments"] = await repository.Payments([id]);
        // Gán kết quả vào o["settings"] để bước xử lý tiếp theo dùng dữ liệu mới.
        o["settings"] = settings.Public();
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return o;
    }

    // Danh sách đơn của khách hàng hiện tại.
    public Task<DbResult> ListMine() => repository.ForCustomer([session.UserId]);
    // Danh sách toàn bộ đơn, chỉ được Controller quản lý gọi.
    public Task<DbResult> ListAll() => repository.All();
    // Quản lý xem lịch sử giao dịch của một khách hàng.
    public Task<DbResult> CustomerHistory(long id) => repository.ForCustomer([id]);
    // Khách hủy đơn phải truyền kiểm tra quyền sở hữu.
    public Task<object> Cancel(long id, Row b) => db.Transaction<object>(async c =>
    {
        // Thực hiện bước CancelInternal trong OrderService; tiếp tục theo luồng của hàm.
        await CancelInternal(c, session.UserId, id, Text(b["reason"])is { Length: > 0 } reason ? reason : "Khách hủy đơn", true);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    });
    // Dùng chung cho khách, quản lý và tác vụ hết hạn; khóa đơn trước khi giải phóng phân bổ.
    private async Task CancelInternal(DbSession c, long? actor, long id, string reason, bool owner)
    {
        // Tạo biến o để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var o = (await repository.LockOrder([id], c)).One("Không tìm thấy đơn.");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (owner && o.L("KhachHangID") != actor)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Không tìm thấy đơn.", 404);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (o.S("TrangThai") == "DA_HUY")
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return;
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (!OrderWorkflow.CanCancel(o.S("TrangThai")))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Đơn đang giao hoặc đã hoàn tất, không thể hủy.");
        // Xử lý từng l trong tập dữ liệu đã lấy/kiểm tra.
        foreach (var l in await repository.Allocations([id], c))
        {
            // Chuyển tới Operations.StockLog để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
            await operations.StockLog(c, actor, l.L("BienTheID"), "GIAI_PHONG", 0, -l.L("SoLuongGiu"), l.L("ChiTietDonHangID"), null, reason);
            // Gọi OrderRepository.ReleaseAllocation → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
            await repository.ReleaseAllocation([l.L("ChiTietDonHangID")], c);
        }

        // Gọi OrderRepository.CancelOrder → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.CancelOrder([actor, reason, id], c);
        // Gọi OrderRepository.RecordCancellation → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.RecordCancellation([id, o.S("TrangThai"), actor, reason], c);
        // Gọi OrderRepository.CancelPendingPayments → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.CancelPendingPayments([id], c);
        // Chuyển tới Operations.ReserveRefunds để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.ReserveRefunds(c, id, reason);
        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(actor, "HUY_DON", "DonHang", id, reason, c);
    }

    // State machine → kiểm tra thanh toán → xuất kho khi giao → ghi trạng thái và lịch sử.
    public Task<object> Advance(long id, Row b) => db.Transaction<object>(async c =>
    {
        // Tạo biến next để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var next = b.S("status");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (next == "DA_HUY")
        {
            // Thực hiện bước CancelInternal trong OrderService; tiếp tục theo luồng của hàm.
            await CancelInternal(c, session.UserId, id, Required(b["reason"], "lý do hủy"), false);
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return Ok();
        }

        // Tạo biến o để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var o = (await repository.LockOrder([id], c)).One();
        // Thực hiện bước OrderWorkflow.AssertNext trong OrderService; tiếp tục theo luồng của hàm.
        OrderWorkflow.AssertNext(o.S("TrangThai"), next);
        // Tạo biến method để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var method = (await repository.PaymentMethod([o.L("PhuongThucID")], c)).One().S("code");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (method != "COD" && (await repository.ConfirmedTotal([id], c)).One().N("paid") < o.N("TongThanhToan"))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Cần đối soát thu đủ tiền chuyển khoản trước khi xác nhận/xử lý đơn.");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (next == "DA_GIAO" && o.S("TrangThaiGiaoHang")is "GIAO_THAT_BAI" or "CHO_HOAN_VE")
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Cần xử lý giao thất bại trước khi xác nhận đã giao.");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (next == "DANG_GIAO")
        {
            // Gọi OrderRepository.MarkShipping → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
            await repository.MarkShipping([id], c);
            // Xử lý từng l trong tập dữ liệu đã lấy/kiểm tra.
            foreach (var l in await repository.HeldItems([id], c))
            {
                // Chuyển tới Operations.StockLog để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
                await operations.StockLog(c, session.UserId, l.L("BienTheID"), "BAN_HANG", -l.L("SoLuongGiu"), -l.L("SoLuongGiu"), l.L("ChiTietDonHangID"), null, "Xuất giao đơn");
                // Gọi OrderRepository.ReleaseShipped → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
                await repository.ReleaseShipped([l.L("ChiTietDonHangID")], c);
            }
        }

        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (next == "DA_GIAO")
            // Gọi OrderRepository.MarkDelivered → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
            await repository.MarkDelivered([id], c);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (next == "HOAN_THANH" && (await repository.ConfirmedTotal([id], c)).One().N("paid") < o.N("TongThanhToan"))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Cần xác nhận thu đủ tiền trước khi hoàn thành đơn.");
        // Gọi OrderRepository.UpdateStatus → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.UpdateStatus([next, next, next, id], c);
        // Gọi OrderRepository.AppendHistory → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.AppendHistory([id, o.S("TrangThai"), next, session.UserId, Text(b["reason"])], c);
        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(session.UserId, "CHUYEN_TRANG_THAI", "DonHang", id, next, c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    });
    // Đối soát khoản thu: COD sau giao hàng; chuyển khoản trước hạn; bắt buộc bằng chứng.
    public Task<object> ConfirmPayment(long id, Row b) => db.Transaction<object>(async c =>
    {
        // Tạo biến lookup để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var lookup = (await repository.OrderForPayment([id], c)).One("Không có giao dịch.");
        // Tạo biến d để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var d = (await repository.LockOrderWithMethod([lookup.L("DonHangID")], c)).One();
        // Tạo biến p để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var p = (await repository.LockPayment([id], c)).One();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (p.S("TrangThai") == "DA_XAC_NHAN")
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return Ok();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (d.S("TrangThai") == "DA_HUY" || p.S("TrangThai") != "CHO_XAC_NHAN" || (d.S("MaPhuongThuc") == "COD" && d.S("TrangThai")is not ("DA_GIAO" or "HOAN_THANH")))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Chỉ xác nhận COD sau khi giao thành công; đơn hủy cần đối soát tiền đến muộn.");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (d.D("HanThanhToan") <= DateTime.UtcNow)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Đã hết hạn thanh toán. Hãy tải lại đơn và xử lý tiền đến muộn nếu đã thực nhận.");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if ((await repository.ConfirmedTotal([d.L("DonHangID")], c)).One().N("paid") + p.N("SoTien") > d.N("TongThanhToan"))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Khoản thu vượt giá trị đơn.");
        // Tạo biến proof để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var proof = Required(b["proof"], "bằng chứng thu tiền");
        // Gọi OrderRepository.ConfirmPayment → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.ConfirmPayment([session.UserId, proof, id], c);
        // Gọi OrderRepository.ClearDeadline → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.ClearDeadline([d.L("DonHangID")], c);
        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(session.UserId, "XAC_NHAN_THU", "ThanhToan", id, proof, c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    });
    // Vận chuyển: cập nhật vận đơn hoặc chuyển thất bại → giao lại/chờ hoàn về.
    public Task<object> UpdateDelivery(long id, Row b) => db.Transaction<object>(async c =>
    {
        // Tạo biến o để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var o = (await repository.LockOrder([id], c)).One();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (o.S("TrangThai") != "DANG_GIAO")
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Chỉ xử lý vận chuyển cho đơn đang giao.");
        // Tạo biến reason để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var reason = Required(b["reason"], "ghi chú vận chuyển");
        // Tạo biến status để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var status = o.S("TrangThaiGiaoHang");
        // Tạo biến action để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var action = b.S("action");
        // Gán kết quả vào status để bước xử lý tiếp theo dùng dữ liệu mới.
        status = (action, status) switch
        {
            ("failed", "DANG_GIAO" or "CHUA_GIAO") => "GIAO_THAT_BAI",
            ("retry", "GIAO_THAT_BAI") => "DANG_GIAO",
            ("await-return", "GIAO_THAT_BAI") => "CHO_HOAN_VE",
            ("tracking", _) => status,
            _ => throw new ApiException("Thao tác vận chuyển không phù hợp trạng thái.")};
        // Gọi OrderRepository.UpdateDelivery → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.UpdateDelivery([status, b.S("carrier") == "" ? null : Text(b["carrier"], 150), b.S("tracking") == "" ? null : Text(b["tracking"], 100), id], c);
        // Gọi OrderRepository.RecordDelivery → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.RecordDelivery([id, o.S("TrangThai"), o.S("TrangThai"), session.UserId, $"Vận chuyển {status}: {reason}"], c);
        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(session.UserId, "VAN_CHUYEN", "DonHang", id, reason, c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    });
    // Kiểm đếm đầy đủ kiện giao thất bại; chỉ hàng bán lại mới nhập kho và không nhận hai lần.
    public Task<object> ReceiveDeliveryReturn(long id, Row b) => db.Transaction<object>(async c =>
    {
        // Tạo biến o để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var o = (await repository.LockOrder([id], c)).One();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (o.S("TrangThaiGiaoHang") == "DA_NHAN_HOAN")
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return Ok();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (o.S("TrangThai") != "DANG_GIAO" || o.S("TrangThaiGiaoHang") != "CHO_HOAN_VE")
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Đơn phải chờ hoàn về trước khi nhận.");
        // Tạo biến reason để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var reason = Required(b["reason"], "kết quả kiểm hàng");
        // Gọi OrderRepository.LockItems → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        var lines = await repository.LockItems([id], c);
        // Tạo biến items để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var items = b.Rows("items");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (items.Count != lines.Count)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Cần kiểm đếm đầy đủ các dòng hàng hoàn về.");
        // Xử lý từng l trong tập dữ liệu đã lấy/kiểm tra.
        foreach (var l in lines)
        {
            // Tạo biến matches để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var matches = items.Where(x => x.N("itemId") == l.N("ChiTietDonHangID")).ToList();
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (matches.Count != 1 || matches[0].N("received", -1) != l.N("SoLuong"))
                // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                throw new ApiException("Chỉ xác nhận khi đã kiểm đủ toàn bộ kiện hàng.");
            // Gán kết quả vào _ để bước xử lý tiếp theo dùng dữ liệu mới.
            _ = Integer(matches[0]["sellable"], 0, l.L("SoLuong"));
        }

        // Gọi OrderRepository.CreateReturnReceipt → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        var a = await repository.CreateReturnReceipt([Code("GH"), Guid.NewGuid().ToString(), reason, session.UserId, session.UserId, id], c);
        // Xử lý từng l trong tập dữ liệu đã lấy/kiểm tra.
        foreach (var l in lines)
        {
            // Tạo biến qty để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var qty = items.Single(x => x.N("itemId") == l.N("ChiTietDonHangID")).L("sellable");
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (qty == 0)
                // Bỏ qua dòng không cần thao tác và chuyển sang phần tử tiếp theo.
                continue;
            // Gọi OrderRepository.AddReceiptLine → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
            var line = await repository.AddReceiptLine([a.InsertId, l.L("BienTheID"), qty, $"Đã nhận {l.L("SoLuong")}, bán lại {qty}. {reason}"], c);
            // Chuyển tới Operations.StockLog để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
            await operations.StockLog(c, session.UserId, l.L("BienTheID"), "NHAN_TRA", qty, 0, null, line.InsertId, reason);
        }

        // Gọi OrderRepository.CancelReturnedOrder → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.CancelReturnedOrder([session.UserId, reason, id], c);
        // Gọi OrderRepository.CancelPendingPayments → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.CancelPendingPayments([id], c);
        // Chuyển tới Operations.ReserveRefunds để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.ReserveRefunds(c, id, "Hoàn tiền đơn giao thất bại " + o.S("MaDonHang"));
        // Gọi OrderRepository.RecordReturn → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.RecordReturn([id, session.UserId, "Đã nhận hàng giao thất bại. " + reason], c);
        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(session.UserId, "NHAN_HOAN_GIAO", "DonHang", id, JsonSerializer.Serialize(b), c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    });
    // Tiền chuyển khoản đến sau khi hủy → ghi thực thu và khoản hoàn; không khôi phục đơn/kho.
    public Task<object> RecordLatePayment(long id, Row b) => db.Transaction<object>(async c =>
    {
        // Tạo biến o để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var o = (await repository.LockLateOrder([id], c)).One();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (o.S("TrangThai") != "DA_HUY" || o.S("method") != "BANK_TRANSFER")
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Chỉ đối soát chuyển khoản đến sau khi đơn bị hủy.");
        // Tạo biến p để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var p = (await repository.LockLatestPayment([id], c)).One("Không có giao dịch.");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (p.S("TrangThai") == "DA_XAC_NHAN")
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return Ok();
        // Tạo biến amount để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var amount = Integer(b["amount"], 1, (long)o.N("TongThanhToan"));
        // Gọi OrderRepository.ConfirmLatePayment → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.ConfirmLatePayment([amount, session.UserId, Required(b["proof"], "bằng chứng tiền vào"), p.L("ThanhToanID")], c);
        // Chuyển tới Operations.ReserveRefunds để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.ReserveRefunds(c, id, "Chuyển khoản đến sau khi đơn đã hủy");
        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(session.UserId, "THU_TIEN_DEN_MUON", "DonHang", id, "Đối soát và tạo khoản hoàn; không khôi phục đơn", c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    });
    // BackgroundService gọi mỗi 15 giây; khóa và kiểm tra lại tránh hủy đơn vừa được thanh toán.
    public async Task<int> ExpireUnpaidOrders()
    {
        // Tạo biến count để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var count = 0;
        // Xử lý từng row trong tập dữ liệu đã lấy/kiểm tra.
        foreach (var row in await repository.ExpiredOrders())
        {
            // Database.Transaction bao các thao tác trong COMMIT/ROLLBACK để không ghi dữ liệu dở dang.
            var expired = await db.Transaction(async c =>
            {
                // Tạo biến id để dùng ở các bước tiếp theo trong phạm vi hiện tại.
                var id = row.L("id");
                // Tạo biến o để dùng ở các bước tiếp theo trong phạm vi hiện tại.
                var o = (await repository.LockOrder([id], c)).FirstOrDefault();
                // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
                if (o is null || !OrderWorkflow.CanCancel(o.S("TrangThai")) || o.D("HanThanhToan")is not { } deadline || deadline > DateTime.UtcNow)
                    // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
                    return false;
                // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
                if ((await repository.ConfirmedTotal([id], c)).One().N("paid") > 0)
                    // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
                    return false;
                // Thực hiện bước CancelInternal trong OrderService; tiếp tục theo luồng của hàm.
                await CancelInternal(c, null, id, "Hệ thống: hết thời hạn chờ thanh toán", false);
                // Gọi OrderRepository.ExpirePayment → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
                await repository.ExpirePayment([id], c);
                // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
                return true;
            });
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (expired)
                // Thực hiện bước xử lý hiện tại trong OrderService; tiếp tục theo luồng của hàm.
                count++;
        }

        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return count;
    }
}
