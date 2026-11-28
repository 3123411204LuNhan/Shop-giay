using System.Text.Json;
using ShopGiay.Entity;
using ShopGiay.Infrastructure;
using static ShopGiay.Infrastructure.ApiInput;
using Q = ShopGiay.Infrastructure.OrderQueries;

namespace ShopGiay.Control;

public sealed class OrderControl(Sql sql,StoreSettings settings)
{
    public Task<List<dynamic>> List(long? customer,CancellationToken token) =>
        sql.Run(db => customer is null ? db.Rows(Q.All) : db.Rows(Q.ForCustomer,customer),token);

    public Task<object> Details(long id,SessionUser user,bool manager,CancellationToken token) =>
        sql.Run<object>(async db => {
            if (await db.One(Q.FindById,id) is not IDictionary<string,object?> row)
                throw new ApiException("Không tìm thấy đơn.",404);
            if (!manager && Convert.ToInt64(row["customerId"]) != user.Id)
                throw new ApiException("Không có quyền xem đơn.",403);
            var result = new Dictionary<string,object?>(row) {
                ["items"] = await db.Rows(Q.Items,id), ["history"] = await db.Rows(Q.History,id),
                ["refunds"] = await db.Rows(Q.Refunds,id), ["payments"] = await db.Rows(Q.Payments,id),
                ["settings"] = settings.Public
            };
            return result;
        },token);

    private static async Task<dynamic> LockOrder(SqlSession db,long id) =>
        await db.One(Q.LockOrder,id) ?? throw new ApiException("Không tìm thấy đơn.",404);

    private static async Task CancelLocked(SqlSession db,dynamic order,long? actor,string reason)
    {
        var id = (long)order.DonHangID;
        var status = (string)order.TrangThai;
        if (status == "DA_HUY") return;
        if (!DonHang.ChoPhepHuy(status)) throw new ApiException("Đơn đang giao hoặc đã hoàn tất, không thể hủy.");
        foreach (var line in await db.Rows(Q.Allocations,id)) {
            await StockLedger.Change(db,actor,(long)line.BienTheID,"GIAI_PHONG",0,-(int)line.SoLuongGiu,(long)line.ChiTietDonHangID,null,reason);
            await db.Execute(Q.ReleaseAllocation,(long)line.ChiTietDonHangID);
        }
        await db.Execute(Q.CancelOrder,actor,reason,id);
        await db.Execute(Q.RecordCancellation,id,status,actor,reason);
        await db.Execute(Q.CancelPendingPayments,id);
        await StockLedger.ReserveRefunds(db,id,reason);
        await db.Audit(actor,"HUY_DON","DonHang",id,reason);
    }

    public Task<object> Cancel(long id,long actor,JsonElement body,CancellationToken token) =>
        sql.Transaction(async db => {
            var order = await LockOrder(db,id);
            if ((long)order.KhachHangID != actor) throw new ApiException("Không tìm thấy đơn.",404);
            await CancelLocked(db,order,actor,body.Text("reason").NullIfEmpty() ?? "Khách hủy đơn");
            return Ok;
        },token);

    public async Task<int> Expire(CancellationToken token)
    {
        var ids = await sql.Run(db => db.Rows(Q.ExpiredOrders),token);
        var count = 0;
        foreach (var item in ids) {
            var id = (long)item.id;
            count += await sql.Transaction(async db => {
                var order = await db.One(Q.LockOrder,id);
                if (order is null || !DonHang.ChoPhepHuy((string)order.TrangThai) ||
                    order.HanThanhToan is not DateTime deadline || deadline > DateTime.UtcNow ||
                    await db.Value<decimal>(Q.ConfirmedTotal,id) > 0) return 0;
                await CancelLocked(db,order,null,"Hệ thống: hết thời hạn chờ thanh toán");
                await db.Execute(Q.ExpirePayment,id);
                return 1;
            },token);
        }
        return count;
    }

    public Task<object> Advance(long id,long actor,JsonElement body,CancellationToken token) =>
        sql.Transaction(async db => {
            var order = await LockOrder(db,id);
            var next = body.Text("status");
            if (next == "DA_HUY") {
                await CancelLocked(db,order,actor,body.Required("reason","lý do hủy"));
                return Ok;
            }
            var current = (string)order.TrangThai;
            if (DonHang.TrangThaiTiepTheo(current) != next) throw new ApiException("Trạng thái chuyển tiếp không hợp lệ.");
            var method = await db.Value<string>(Q.PaymentMethod,(int)order.PhuongThucID);
            var paid = await db.Value<decimal>(Q.ConfirmedTotal,id);
            if (method != "COD" && paid < (decimal)order.TongThanhToan)
                throw new ApiException("Cần đối soát thu đủ tiền chuyển khoản trước khi xác nhận/xử lý đơn.");
            if (next == "DA_GIAO") {
                if ((string)order.TrangThaiGiaoHang is "GIAO_THAT_BAI" or "CHO_HOAN_VE")
                    throw new ApiException("Cần xử lý giao thất bại trước khi xác nhận đã giao.");
                await db.Execute(Q.MarkDelivered,id);
            }
            if (next == "DANG_GIAO") {
                await db.Execute(Q.MarkShipping,id);
                foreach (var line in await db.Rows(Q.HeldItems,id)) {
                    var qty = -(int)line.SoLuongGiu;
                    await StockLedger.Change(db,actor,(long)line.BienTheID,"BAN_HANG",qty,qty,(long)line.ChiTietDonHangID,null,"Xuất giao đơn");
                    await db.Execute(Q.ReleaseShipped,(long)line.ChiTietDonHangID);
                }
            }
            if (next == "HOAN_THANH" && paid < (decimal)order.TongThanhToan)
                throw new ApiException("Cần xác nhận thu đủ tiền trước khi hoàn thành đơn.");
            await db.Execute(Q.UpdateStatus,next,next,next,id);
            await db.Execute(Q.AppendHistory,id,current,next,actor,body.Text("reason"));
            await db.Audit(actor,"CHUYEN_TRANG_THAI","DonHang",id,next);
            return Ok;
        },token);

    public Task<object> ConfirmPayment(long payment,long actor,JsonElement body,CancellationToken token) =>
        sql.Transaction(async db => {
            var id = await db.Value<long?>(Q.OrderForPayment,payment) ?? throw new ApiException("Không có giao dịch.");
            var order = await db.One(Q.LockOrderWithMethod,id) ?? throw new ApiException("Không tìm thấy đơn.",404);
            var row = await db.One(Q.LockPayment,payment) ?? throw new ApiException("Không có giao dịch.");
            if (row.TrangThai == "DA_XAC_NHAN") return Ok;
            if (order.TrangThai == "DA_HUY" || row.TrangThai != "CHO_XAC_NHAN" ||
                (order.MaPhuongThuc == "COD" && (string)order.TrangThai is not ("DA_GIAO" or "HOAN_THANH")))
                throw new ApiException("Chỉ xác nhận COD sau khi giao thành công. Đơn hủy cần đối soát tiền đến muộn.");
            if (order.HanThanhToan is DateTime deadline && deadline <= DateTime.UtcNow)
                throw new ApiException("Đã hết hạn thanh toán.");
            if (await db.Value<decimal>(Q.ConfirmedTotal,id) + (decimal)row.SoTien > (decimal)order.TongThanhToan)
                throw new ApiException("Khoản thu vượt giá trị đơn.");
            var proof = body.Required("proof","bằng chứng thu tiền");
            await db.Execute(Q.ConfirmPayment,actor,proof,payment);
            await db.Execute(Q.ClearDeadline,id);
            await db.Audit(actor,"XAC_NHAN_THU","ThanhToan",payment,proof);
            return Ok;
        },token);

    public Task<object> Delivery(long id,long actor,JsonElement body,CancellationToken token) =>
        sql.Transaction(async db => {
            var order = await LockOrder(db,id);
            if (order.TrangThai != "DANG_GIAO") throw new ApiException("Chỉ xử lý vận chuyển cho đơn đang giao.");
            var reason = body.Required("reason","ghi chú vận chuyển");
            var delivery = (string)order.TrangThaiGiaoHang;
            var next = body.Text("action") switch {
                "failed" when delivery is "DANG_GIAO" or "CHUA_GIAO" => "GIAO_THAT_BAI",
                "retry" when delivery == "GIAO_THAT_BAI" => "DANG_GIAO",
                "await-return" when delivery == "GIAO_THAT_BAI" => "CHO_HOAN_VE",
                "tracking" => delivery,
                _ => throw new ApiException("Thao tác vận chuyển không phù hợp trạng thái.")
            };
            await db.Execute(Q.UpdateDelivery,next,body.Text("carrier",150).NullIfEmpty(),body.Text("tracking",100).NullIfEmpty(),id);
            await db.Execute(Q.RecordDelivery,id,"DANG_GIAO","DANG_GIAO",actor,$"Vận chuyển {next}: {reason}");
            await db.Audit(actor,"VAN_CHUYEN","DonHang",id,reason);
            return Ok;
        },token);

    public Task<object> Receive(long id,long actor,JsonElement body,CancellationToken token) =>
        sql.Transaction(async db => {
            var order = await LockOrder(db,id);
            if (order.TrangThaiGiaoHang == "DA_NHAN_HOAN") return Ok;
            if (order.TrangThai != "DANG_GIAO" || order.TrangThaiGiaoHang != "CHO_HOAN_VE")
                throw new ApiException("Đơn phải được đánh dấu chờ hoàn về trước khi nhận.");
            var reason = body.Required("reason","kết quả kiểm hàng");
            var lines = await db.Rows(Q.LockItems,id);
            var items = body.Items();
            if (items.Length != lines.Count) throw new ApiException("Cần kiểm đếm đầy đủ các dòng hàng hoàn về.");
            var stock = new Dictionary<long,int>();
            foreach (var line in lines) {
                var detail = (long)line.ChiTietDonHangID;
                var qty = (int)line.SoLuong;
                var matches = items.Where(x => Integer(x.Field("itemId"),1,long.MaxValue) == detail).ToArray();
                if (matches.Length != 1 || Integer(matches[0].Field("received"),0,qty) != qty)
                    throw new ApiException("Chỉ xác nhận khi đã kiểm đủ toàn bộ kiện hàng.");
                stock.Add(detail,(int)Integer(matches[0].Field("sellable"),0,qty));
            }
            var receipt = await db.Insert(Q.CreateReturnReceipt,"GH-"+Guid.NewGuid(),Guid.NewGuid().ToString(),reason,actor,actor,id);
            foreach (var line in lines) {
                var qty = stock[(long)line.ChiTietDonHangID];
                if (qty == 0) continue;
                var variant = (long)line.BienTheID;
                var receiptLine = await db.Insert(Q.AddReceiptLine,receipt,variant,qty,$"Đã nhận {line.SoLuong}, bán lại {qty}. {reason}");
                await StockLedger.Change(db,actor,variant,"NHAN_TRA",qty,0,null,receiptLine,reason);
            }
            await db.Execute(Q.CancelReturnedOrder,actor,reason,id);
            await db.Execute(Q.CancelPendingPayments,id);
            await StockLedger.ReserveRefunds(db,id,"Hoàn tiền đơn giao thất bại " + (string)order.MaDonHang);
            await db.Execute(Q.RecordReturn,id,actor,"Đã nhận hàng giao thất bại. " + reason);
            await db.Audit(actor,"NHAN_HOAN_GIAO","DonHang",id,body.GetRawText());
            return Ok;
        },token);

    public Task<object> LatePayment(long id,long actor,JsonElement body,CancellationToken token) =>
        sql.Transaction(async db => {
            var order = await db.One(Q.LockLateOrder,id);
            if (order is null) throw new ApiException("Không tìm thấy đơn.",404);
            if (order.TrangThai != "DA_HUY" || order.method != "BANK_TRANSFER")
                throw new ApiException("Chỉ đối soát chuyển khoản đến sau khi đơn bị hủy.");
            var pay = await db.One(Q.LockLatestPayment,id) ?? throw new ApiException("Không có giao dịch.");
            if (pay.TrangThai == "DA_XAC_NHAN") return Ok;
            var amount = Integer(body.Field("amount"),1,(long)order.TongThanhToan,"Số tiền nhận phải dương và không vượt tổng đơn.");
            await db.Execute(Q.ConfirmLatePayment,amount,actor,body.Required("proof","bằng chứng tiền vào"),(long)pay.ThanhToanID);
            await StockLedger.ReserveRefunds(db,id,"Chuyển khoản đến sau khi đơn đã hủy");
            await db.Audit(actor,"THU_TIEN_DEN_MUON","DonHang",id,"Đối soát và tạo khoản hoàn");
            return Ok;
        },token);
}
