using System.Text.Json;
using ShopGiay.Entity;
using ShopGiay.Infrastructure;
using static ShopGiay.Infrastructure.ApiInput;
using Q = ShopGiay.Infrastructure.ReturnQueries;
using O = ShopGiay.Infrastructure.OrderQueries;

namespace ShopGiay.Control;

public sealed class ReturnControl(Sql sql,StoreSettings settings,ImageStore images,ReturnRepository returns,OrderRepository orders)
{
    public Task<List<dynamic>> List(long? customer,CancellationToken token) =>
        sql.Run(db => customer is null ? db.Rows(Q.All) : db.Rows(Q.ForCustomer,customer),token);

    public Task<List<dynamic>> Refunds(CancellationToken token) => sql.Run(db => db.Rows(Q.AllRefunds),token);

    // Kiểm tra quyền đánh giá và trạng thái đơn trước khi lưu nội dung cùng số sao.
    public Task<object> Review(long customer,JsonElement body,CancellationToken token) =>
        sql.Transaction(async db => {
            var item = Id(body.Field("itemId"));
            var stars = Integer(body.Field("stars"),1,5,"Số sao từ 1 đến 5.");
            var line = await db.One(Q.ReviewableLine,item,customer) ?? throw new ApiException("Chỉ đánh giá sản phẩm trong đơn đã hoàn thành.");
            await returns.ThemDanhGia(db,new DanhGia {
                DonHangID = (long)line.DonHangID, ChiTietDonHangID = item, KhachHangID = customer,
                SoSao = (byte)stars, NoiDung = body.Text("content",2000), TrangThai = "HIEN_THI"
            });
            return Ok;
        },token);

    // Khóa dòng đơn của khách, kiểm tra hạn đổi trả rồi trừ số lượng đã yêu cầu trước đó.
    private async Task<(dynamic Line,int Remaining)> Eligible(SqlSession db,long item,long customer)
    {
        var line = await db.One(Q.LockEligibleLine,item,customer) ?? throw new ApiException("Đơn chưa đủ điều kiện đổi/trả.");
        if (line.NgayGiao is not DateTime delivered || DateTime.UtcNow - delivered > TimeSpan.FromDays(settings.ReturnDays))
            throw new ApiException($"Đã hết thời hạn {settings.ReturnDays} ngày gửi yêu cầu đổi/trả.");
        var requested = await db.Value<int>(Q.RequestedQuantity,item);
        return (line,(int)line.SoLuong-requested);
    }

    // Tìm biến thể cùng giá hoặc khóa đúng biến thể đã chọn trước khi tạo đơn đổi.
    private static Task<List<dynamic>> Options(SqlSession db,long original,decimal price,long? target) =>
        db.Rows(Q.ExchangeOptions.Replace("{slot0}",target is null ? "AND t.SoLuongKhaDung>0 ORDER BY k.ThuTu,b.BienTheID" : "AND b.BienTheID=? FOR UPDATE"),
            target is null ? [original,price] : [original,price,target]);

    // Trả lượng còn được đổi và các biến thể đáp ứng điều kiện của dòng đơn.
    public Task<object> Options(long item,long customer,CancellationToken token) =>
        sql.Transaction<object>(async db => {
            var (line,remaining) = await Eligible(db,item,customer);
            var variants = remaining > 0 ? await Options(db,(long)line.BienTheID,(decimal)line.DonGia,null) : [];
            return new { remaining,variants };
        },token);

    // Kiểm tra thời hạn, hàng còn được đổi trả và mã chống lặp rồi lưu yêu cầu, dòng hàng và ảnh.
    public Task<object> Create(long customer,JsonElement body,Upload? image,CancellationToken token) =>
        images.WithUpload("returns",image,save => sql.Transaction<object>(async db => {
            var item = Id(body.Field("itemId"));
            var quantity = Id(body.Field("quantity"));
            var key = body.Text("requestId").Length > 0 ? body.RequestKey() : Guid.NewGuid().ToString();
            await db.One(Q.LockCustomer,customer);
            var old = await db.One(Q.FindByKey,key);
            if (old is not null) {
                if ((long)old.owner != customer) throw new ApiException("Mã yêu cầu đã sử dụng.",409);
                return new { ok = true,id = (long)old.id,code = (string)old.code,status = (string)old.status };
            }
            var (line,remaining) = await Eligible(db,item,customer);
            if (quantity > remaining) throw new ApiException("Số lượng vượt phần còn được đổi/trả.");
            var type = body.Text("type");
            if (type is not ("TRA" or "DOI")) throw new ApiException("Loại yêu cầu không hợp lệ.");
            long? target = null;
            if (type == "DOI") {
                target = Id(body.Field("targetVariantId"));
                var other = (await Options(db,(long)line.BienTheID,(decimal)line.DonGia,target)).FirstOrDefault();
                if (other is null) throw new ApiException("Chọn biến thể khác cùng sản phẩm, cùng đơn giá lúc mua.");
                if ((int)other.stock < quantity) throw new ApiException("Biến thể đổi không đủ hàng.",409);
            }
            var order = (long)line.DonHangID;
            var code = "DT-" + Guid.NewGuid();
            var request = new YeuCauDoiTra {
                MaYeuCau = code, MaChongLap = Guid.Parse(key), DonHangID = order, KhachHangID = customer,
                LoaiYeuCau = type, LyDo = body.Required("reason","lý do")
            };
            var id = await returns.Them(db,request);
            await db.Execute(Q.AddLine,id,order,item,quantity,target);
            var url = ImageStore.Url(body.Text("image",1001),false);
            if (image is not null) url = $"/api/returns/{id}/images/" + await save();
            if (url.Length > 0) await db.Execute(Q.AttachImage,id,url);
            await db.Execute(Q.RecordSubmission,id,customer);
            return new { ok = true,id,code,status = request.TrangThai };
        },token),token);

    public Task<object> Details(long id,long? customer,CancellationToken token) =>
        sql.Run<object>(async db => {
            var filter = customer is null ? "" : " AND y.KhachHangID=?";
            if (await db.One(Q.RequestDetails.Replace("{slot0}",filter),customer is null ? [id] : [id,customer])
                is not IDictionary<string,object?> row) throw new ApiException("Không tìm thấy yêu cầu.",404);
            var photos = await db.Rows(Q.Images,id);
            if (customer is null) {
                foreach (var photo in photos)
                    if (photo.url is string url && url.StartsWith("/api/returns/")) photo.url = "/api/manage" + url[4..];
            }
            return new Dictionary<string,object?>(row) {
                ["items"] = await db.Rows(Q.Lines,id), ["history"] = await db.Rows(Q.History,id),
                ["refunds"] = await db.Rows(Q.Refunds,id), ["images"] = photos
            };
        },token);

    public async Task Evidence(long id,long? customer,string name,CancellationToken token)
    {
        var url = $"/api/returns/{id}/images/{name}";
        var found = await sql.Run(db => db.Value<int?>(Q.RequestImage + (customer is null ? "" : " AND y.KhachHangID=?"),
            customer is null ? [id,url] : [id,url,customer]),token);
        if (found is null) throw new ApiException("Không tìm thấy ảnh.",404);
    }

    // Đối soát đủ tiền đơn gốc, kiểm tra biến thể cùng giá và tồn rồi tạo đơn đổi trong cùng giao dịch.
    private async Task<string?> Replacement(SqlSession db,long actor,YeuCauDoiTra request,DonHang original,ChiTietDoiTra line)
    {
        var order = request.DonHangID;
        if (await db.Value<decimal>(O.ConfirmedTotal,order) < original.TongThanhToan)
            return "Chờ đối soát đủ tiền đơn gốc trước khi tạo đơn thay thế.";
        if (line.BienTheDoiSangID is not long targetId) throw new ApiException("Thiếu biến thể thay thế.");
        var target = (await Options(db,line.BienTheID,line.DonGia,targetId)).FirstOrDefault();
        var qty = line.SoLuongDuyet;
        if (target is null || (int)target.stock < qty)
            return "Biến thể thay thế thiếu tồn hoặc không còn đủ điều kiện cùng giá. Thử lại khi đủ điều kiện.";
        var value = qty * line.DonGia;
        var note = "Đơn thay thế cho " + request.MaYeuCau;
        var replacement = await orders.ThemDon(db,new DonHang {
            MaDonHang = "DOI-" + Guid.NewGuid(), MaYeuCauTao = Guid.NewGuid(), KhachHangID = original.KhachHangID,
            HoTenNguoiNhan = original.HoTenNguoiNhan, SoDienThoaiNhan = original.SoDienThoaiNhan,
            DiaChiNhan = original.DiaChiNhan, GhiChu = note, PhuongThucID = original.PhuongThucID,
            TienHang = value, TienGiam = value, TienTe = "VND"
        },null);
        var item = await orders.ThemDong(db,new ChiTietDonHang {
            DonHangID = replacement, BienTheID = targetId, MaSanPhamLucMua = line.MaSanPhamLucMua,
            SKULucMua = (string)target.sku, TenSanPhamLucMua = line.TenSanPhamLucMua,
            KichCoLucMua = (string)target.size, MauSacLucMua = (string)target.color, DanhMucLucMua = line.DanhMucLucMua,
            ThuongHieuLucMua = line.ThuongHieuLucMua, SoLuong = qty, DonGia = line.DonGia, TienGiamPhanBo = value
        });
        await orders.ThemPhanBo(db,new PhanBoTonKho { ChiTietDonHangID = item, SoLuongGiu = qty });
        await StockLedger.Change(db,actor,targetId,"GIU_HANG",0,qty,item,null,"Giữ hàng đổi " + request.MaYeuCau);
        await db.Execute(O.AppendHistory,replacement,null,"CHO_XAC_NHAN",actor,note);
        request.GanDonThayThe(replacement);
        return null;
    }

    // Đối chiếu từng dòng với lượng đã duyệt rồi trả số đôi được phép nhập lại.
    private static int CountReceived(int lineCount,JsonElement body,long detail,int approved)
    {
        if (body.Field("items").ValueKind == JsonValueKind.Undefined && body.Field("sellable").ValueKind is JsonValueKind.True or JsonValueKind.False)
            return body.Field("sellable").GetBoolean() ? approved : 0;
        var items = body.Items();
        if (items.Length != lineCount) throw new ApiException("Hãy nhập kết quả kiểm hàng cho từng dòng.");
        var matching = items.Where(item=>Integer(item.Field("id"),1,long.MaxValue) == detail).ToArray();
        if (matching.Length != 1 || Integer(matching[0].Field("received"),0,approved) != approved)
            throw new ApiException("Chỉ xác nhận khi đã nhận đủ hàng được duyệt.");
        return (int)Integer(matching[0].Field("sellable"),0,approved,"Số đôi bán lại phải từ 0 đến số đã nhận.");
    }

    // Khóa yêu cầu, áp dụng quyết định hoặc nhận hàng rồi lưu tồn, đơn đổi hoặc khoản hoàn và lịch sử.
    public Task<object> Process(long id,long actor,JsonElement body,CancellationToken token) =>
        sql.Transaction<object>(async db => {
            var request = await returns.Khoa(db,id) ?? throw new ApiException("Không tìm thấy yêu cầu.",404);
            var orderId = request.DonHangID;
            var exchange = request.LoaiYeuCau == "DOI";
            var original = exchange ? await orders.TaiDeCapNhat(db,orderId) ?? throw new ApiException("Không tìm thấy đơn.",404) : null;
            var lines = await returns.KhoaDong(db,id);
            if (lines.Count == 0 || exchange && lines.Count != 1) throw new ApiException("Yêu cầu có dòng hàng không hợp lệ.",409);
            var action = body.Text("action");
            var status = request.TrangThai;
            if (exchange && request.DonHangDoiMoiID is not null && action is "receive" or "create-exchange") return Ok;
            string? pending = null;
            switch (action) {
                case "approve":
                case "reject":
                    request.QuyetDinh(action == "approve",actor,body.Text("reason"),DateTime.UtcNow);
                    if (action == "approve") {
                        foreach (var line in lines) {
                            var refund = 0m;
                            if (!exchange) {
                                var shares = await db.One(Q.ApprovedReturnShares,line.ChiTietDonHangID,id);
                                var quantity = Convert.ToInt32(shares!.quantity) + line.SoLuongYeuCau;
                                var bought = line.SoLuongLucMua;
                                var paid = line.ThanhTienLucMua;
                                var allocated = quantity == bought ? paid : decimal.Floor(paid * quantity / bought);
                                refund = Math.Max(0,Math.Round(allocated - (decimal)shares.amount,2,MidpointRounding.AwayFromZero));
                            }
                            line.Duyet(refund);
                            await returns.LuuDuyet(db,line);
                        }
                    }
                    break;
                case "receive":
                    request.NhanHang();
                    foreach (var line in lines) {
                        var detail = line.ChiTietDoiTraID;
                        var sellable = CountReceived(lines.Count,body,detail,line.SoLuongDuyet);
                        line.GhiNhanNhanHang(sellable,actor,body.Text("reason"),DateTime.UtcNow);
                        await returns.LuuNhanHang(db,line);
                        if (sellable == 0) continue;
                        var reason = "Nhận hàng trả " + request.MaYeuCau;
                        var receipt = await db.Insert(Q.CreateReceipt,"HT-"+Guid.NewGuid(),Guid.NewGuid().ToString(),reason,actor,actor);
                        var variant = line.BienTheID;
                        var receiptLine = await db.Insert(Q.AddReceiptLine,receipt,variant,sellable,detail);
                        await StockLedger.Change(db,actor,variant,"NHAN_TRA",sellable,0,null,receiptLine,"Hàng trả đủ điều kiện bán lại");
                    }
                    if (exchange) pending = await Replacement(db,actor,request,original!,lines[0]);
                    else await StockLedger.ReserveRefunds(db,orderId,request.LyDo,id);
                    break;
                case "create-exchange":
                    if (!exchange || status != "DANG_XU_LY") throw new ApiException("Cần nhận đủ hàng đổi trước khi tạo đơn thay thế.");
                    pending = await Replacement(db,actor,request,original!,lines[0]);
                    if (pending is not null) throw new ApiException(pending,409);
                    break;
                case "prepare-refund":
                    if (exchange || status != "DANG_XU_LY") throw new ApiException("Cần nhận hàng trả trước khi lập khoản hoàn.");
                    await StockLedger.ReserveRefunds(db,orderId,request.LyDo,id);
                    break;
                case "complete":
                    request.HoanTat(DateTime.UtcNow);
                    if (await db.Value<int>(Q.PendingRefunds,id) > 0) throw new ApiException("Cần xác nhận hoàn tiền trước khi hoàn tất.");
                    if (await db.Value<decimal>(Q.ConfirmedRefunds,id) < await db.Value<decimal>(Q.ApprovedTotal,id))
                        throw new ApiException("Chưa hoàn đủ tiền được duyệt. Đối soát khoản thu rồi lập bổ sung khoản hoàn.");
                    break;
                default: throw new ApiException("Thao tác không hợp lệ.");
            }
            await returns.Luu(db,request);
            await db.Execute(Q.RecordHistory,id,status,request.TrangThai,actor,pending ?? (action == "create-exchange" ? "Đã tạo đơn thay thế" : body.Text("reason").NullIfEmpty() ?? action));
            await db.Audit(actor,"XU_LY_DOI_TRA","YeuCauDoiTra",id,action);
            return pending is null ? Ok : new { ok = true,message = pending };
        },token);

    // Khóa khoản hoàn, đối soát tiền đã thu và ghi xác nhận, thất bại hoặc thử lại.
    public Task<object> Refund(long id,long actor,string action,JsonElement body,CancellationToken token) =>
        sql.Transaction(async db => {
            var row = await returns.KhoaKhoanHoan(db,id) ?? throw new ApiException("Không có khoản hoàn.",404);
            if (action == "retry") {
                if (!row.ThuLai()) return Ok;
                await returns.LuuKhoanHoan(db,row);
                await db.Audit(actor,"THU_LAI_HOAN","HoanTien",id,body.Required("reason","lý do thử lại"));
                return Ok;
            }
            if (action == "failure") {
                if (!row.DanhDauThatBai()) return Ok;
                await returns.LuuKhoanHoan(db,row);
                await db.Audit(actor,"HOAN_THAT_BAI","HoanTien",id,body.Required("reason","lý do thất bại"));
                return Ok;
            }
            if (row.TrangThai == "DA_XAC_NHAN") return Ok;
            if (!row.ConChoXuLy()) throw new ApiException("Khoản hoàn cần ở trạng thái chờ.");
            var paymentId = row.ThanhToanID;
            var payment = await orders.KhoaThanhToan(db,paymentId);
            var returned = await db.Value<decimal>(Q.RefundedForPayment,paymentId);
            if (payment is null) throw new ApiException("Không có giao dịch thanh toán.");
            if (payment.TrangThai != "DA_XAC_NHAN" || returned+row.SoTien > payment.SoTien)
                throw new ApiException("Tổng hoàn vượt tiền thực thu.");
            row.XacNhanHoanTien(actor,body.Required("proof","bằng chứng hoàn tiền"),DateTime.UtcNow);
            await returns.LuuKhoanHoan(db,row);
            await db.Audit(actor,"XAC_NHAN_HOAN","HoanTien",id,"Xác nhận có bằng chứng");
            return Ok;
        },token);
}
