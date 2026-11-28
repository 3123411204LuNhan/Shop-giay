using System.Text.Json;
using ShopGiay.Infrastructure;
using static ShopGiay.Infrastructure.ApiInput;
using Q = ShopGiay.Infrastructure.InventoryQueries;

namespace ShopGiay.Control;

public sealed class InventoryControl(Sql sql)
{
    public Task<List<dynamic>> List(string? product,CancellationToken token) =>
        sql.Run(db => db.Rows(Q.Stock.Replace("{slot0}",product is null ? "" : "WHERE b.SanPhamID=?"),
            product is null ? [] : [Id(product)]),token);

    public Task<List<dynamic>> History(string? variant,CancellationToken token) =>
        sql.Run(db => db.Rows(Q.History.Replace("{slot0}",string.IsNullOrEmpty(variant) ? "" : "WHERE b.BienTheID=?"),
            string.IsNullOrEmpty(variant) ? [] : [Id(variant)]),token);

    public Task<List<dynamic>> Receipts(CancellationToken token) => sql.Run(db => db.Rows(Q.Receipts),token);

    public Task<object> Receipt(long id,CancellationToken token) =>
        sql.Run<object>(async db => {
            if (await db.One(Q.Receipt,id) is not IDictionary<string,object?> row) throw new ApiException("Không tìm thấy phiếu.",404);
            return new Dictionary<string,object?>(row) { ["items"] = await db.Rows(Q.Lines,id) };
        },token);

    private sealed record ReceiptLine(long Variant,int Quantity,decimal? Cost,int? Counted,int? Baseline);

    public Task<object> Save(long? id,long actor,JsonElement body,CancellationToken token) =>
        sql.Transaction<object>(async db => {
            var request = id is null ? body.RequestKey() : null;
            if (id is null) {
                await db.One(Q.LockActor,actor);
                var old = await db.One(Q.FindByKey,request);
                if (old is not null) {
                    if ((long)old.owner != actor) throw new ApiException("Mã yêu cầu đã được sử dụng.",409);
                    return new { id = (long)old.id };
                }
            } else {
                var receipt = await db.One(Q.LockReceipt,id) ?? throw new ApiException("Không tìm thấy phiếu.",404);
                if (receipt.TrangThai != "NHAP") throw new ApiException("Chỉ được sửa phiếu nháp.");
            }

            var type = body.Text("type");
            if (type is not ("NHAP" or "DIEU_CHINH")) throw new ApiException("Loại phiếu không hợp lệ.");
            var reason = body.Required("reason","lý do");
            var reference = body.Text("reference",100).NullIfEmpty();
            var supplier = body.Text("supplier",200).NullIfEmpty();
            if (type == "NHAP" && (reference is null || supplier is null))
                throw new ApiException("Phiếu nhập cần mã chứng từ và nhà cung cấp.");
            var input = body.Items();
            if (input.Length is < 1 or > 100) throw new ApiException("Phiếu cần từ 1 đến 100 dòng hàng.");
            var lines = input.Select(item => {
                var variant = Id(item.Field("variantId"));
                var cost = item.Text("cost").NullIfEmpty() is { } raw ? Number(raw) : (decimal?)null;
                if (cost is < 0 or > 10000000000m) throw new ApiException("Giá nhập không hợp lệ.");
                var counted = type == "DIEU_CHINH" && item.IsSet("counted")
                    ? (int?)Integer(item.Field("counted"),0,100000) : null;
                var baseline = counted is not null && item.IsSet("baseline")
                    ? (int?)Integer(item.Field("baseline"),0,int.MaxValue) : null;
                var quantity = counted is null ? (int)Integer(item.Field("quantity"),type == "NHAP" ? 1 : -100000,100000) : 0;
                if (counted is null && quantity == 0) throw new ApiException("Số lượng nhập hoặc chênh lệch không hợp lệ.");
                return new ReceiptLine(variant,quantity,cost,counted,baseline);
            }).OrderBy(line => line.Variant).ToArray();
            if (lines.Select(x => x.Variant).Distinct().Count() != lines.Length) throw new ApiException("Biến thể bị lặp trong phiếu.");
            if (id is null) {
                id = await db.Insert("""
                    INSERT INTO PhieuKho(MaPhieu,MaChongLap,LoaiPhieu,MaChungTuThamChieu,LyDo,NguoiThucHienID,TrangThai,NhaCungCap)
                    VALUES(?,?,?,?,?,?,'NHAP',?)
                    ""","PK-"+Guid.NewGuid(),request,type,reference,reason,actor,supplier);
            } else {
                await db.Execute("UPDATE PhieuKho SET LoaiPhieu=?,MaChungTuThamChieu=?,LyDo=?,NhaCungCap=? WHERE PhieuKhoID=?",type,reference,reason,supplier,id);
                await db.Execute("DELETE FROM ChiTietPhieuKho WHERE PhieuKhoID=?",id);
            }
            foreach (var line in lines) {
                if (await db.One("SELECT BienTheID FROM BienTheSanPham WHERE BienTheID=?",line.Variant) is null)
                    throw new ApiException("Biến thể không tồn tại.",404);
                var quantity = line.Quantity;
                int? baseline = null;
                if (line.Counted is int counted) {
                    var stock = await db.One("SELECT SoLuongTon stock,SoLuongDaPhanBo reserved FROM TonKho WHERE BienTheID=? FOR UPDATE",line.Variant)
                        ?? throw new ApiException("Không tìm thấy tồn kho.",404);
                    baseline = (int)stock.stock;
                    if (line.Baseline is int expected && expected != baseline)
                        throw new ApiException("Tồn sổ đã thay đổi. Hãy tải lại và kiểm đếm lại.",409);
                    if (counted < (int)stock.reserved) throw new ApiException("Số kiểm đếm thấp hơn lượng đang giữ.");
                    quantity = counted - baseline.Value;
                    if (quantity == 0) throw new ApiException("Số kiểm đếm bằng tồn sổ.");
                }
                await db.Execute("INSERT INTO ChiTietPhieuKho(PhieuKhoID,BienTheID,SoLuongThayDoi,DonGiaNhap,TonSoSach) VALUES(?,?,?,?,?)",
                    id,line.Variant,quantity,line.Cost,baseline);
            }
            await db.Audit(actor,"LUU_NHAP_KHO","PhieuKho",id.Value,reason);
            return new { id };
        },token);

    public Task<object> Confirm(long id,long actor,CancellationToken token) =>
        sql.Transaction(async db => {
            var receipt = await db.One(Q.LockReceipt,id) ?? throw new ApiException("Không tìm thấy phiếu.",404);
            if (receipt.TrangThai == "DA_XAC_NHAN") return Ok;
            if (receipt.TrangThai != "NHAP") throw new ApiException("Phiếu đã hủy không thể xác nhận.");
            var lines = await db.Rows(Q.LockLines,id);
            if (lines.Count == 0) throw new ApiException("Phiếu không có dòng hàng.");
            foreach (var line in lines) {
                var variant = (long)line.BienTheID;
                if (line.TonSoSach is int baseline && await db.Value<int?>(Q.LockStock,variant) != baseline)
                    throw new ApiException("Tồn sổ đã thay đổi. Hãy kiểm đếm và lưu lại phiếu nháp.",409);
                await StockLedger.Change(db,actor,variant,(string)receipt.LoaiPhieu,(int)line.SoLuongThayDoi,0,null,(long)line.ChiTietPhieuKhoID,(string)receipt.LyDo);
            }
            await db.Execute(Q.ConfirmReceipt,actor,id);
            await db.Audit(actor,"XAC_NHAN_PHIEU_KHO","PhieuKho",id,(string)receipt.LyDo);
            return Ok;
        },token);

    public Task<object> Cancel(long id,long actor,JsonElement body,CancellationToken token) =>
        sql.Transaction(async db => {
            var receipt = await db.One(Q.LockReceipt,id) ?? throw new ApiException("Không tìm thấy phiếu.",404);
            if (receipt.TrangThai == "DA_HUY") return Ok;
            if (receipt.TrangThai != "NHAP") throw new ApiException("Không hủy phiếu đã ghi sổ.");
            await db.Execute(Q.CancelReceipt,id);
            await db.Audit(actor,"HUY_NHAP_KHO","PhieuKho",id,body.Required("reason","lý do hủy"));
            return Ok;
        },token);

    public Task<object> Warning(long id,long actor,JsonElement body,CancellationToken token) =>
        sql.Transaction(async db => {
            var value = Integer(body.Field("warning"),0,100000,"Ngưỡng cảnh báo không hợp lệ.");
            if (await db.Execute(Q.SetThreshold,value,id) == 0) throw new ApiException("Không tìm thấy tồn kho.",404);
            await db.Audit(actor,"NGUONG_CANH_BAO","TonKho",id,value.ToString());
            return Ok;
        },token);
}
