using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ShopGiay.Web.Infrastructure;
using ShopGiay.Web.Repositories;
using static ShopGiay.Web.Infrastructure.Input;

namespace ShopGiay.Web.Services;
// Snapshot chỉ tồn tại trong giao dịch kiểm giá/tồn, không tin số tiền do trình duyệt gửi.
public sealed record CheckoutSnapshot(long Cart, DbResult Items, decimal Total, Row? Promo, decimal Discount, List<Row> Eligible, Row Method, decimal Shipping, string Fingerprint);
public sealed class CheckoutService(ICheckoutRepository repository, Database db, SessionContext session, Operations operations, StoreSettings settings)
{
    // Một tài khoản có một giỏ; khóa duy nhất của database chống tạo trùng.
    private async Task<long> CartId(DbSession? c = null)
    {
        // Gọi CheckoutRepository.EnsureCart → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.EnsureCart([session.UserId], c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return (await repository.CartId([session.UserId], c)).One().L("id");
    }

    // Controller → giỏ của tài khoản hiện tại, không nhận owner từ client.
    public async Task<object> Cart() => await repository.CartItems([await CartId()]);
    // PUT đặt số lượng tuyệt đối để gửi lại không cộng hai lần; 0 là xóa dòng.
    public Task<object> UpdateCart(Row b) => db.Transaction<object>(async c =>
    {
        // Tạo biến variant để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var variant = Positive(b["variantId"]);
        // Tạo biến qty để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var qty = Integer(b["quantity"], 0, 100);
        // Tạo biến id để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var id = await CartId(c);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (qty == 0)
            // Gọi CheckoutRepository.RemoveItem → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
            await repository.RemoveItem([id, variant], c);
        else
        {
            // Tạo biến available để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var available = (await repository.AvailableVariant([variant], c)).FirstOrDefault();
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (available is null || available.N("stock") < qty)
                // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                throw new ApiException("Biến thể không đủ tồn kho.");
            // Gọi CheckoutRepository.SetQuantity → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
            await repository.SetQuantity([id, variant, qty], c);
        }

        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return Ok();
    });
    // Kiểm tra lại sản phẩm, khóa tồn theo ID tăng dần, khóa mã giảm và tính báo giá.
    private async Task<CheckoutSnapshot> Snapshot(Row b, DbSession c)
    {
        // Tạo biến cart để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var cart = await CartId(c);
        // Gọi CheckoutRepository.CheckoutItems → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        var items = await repository.CheckoutItems([cart], c);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (items.Count == 0)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Giỏ hàng đang trống.");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if ((await repository.CountItems([cart], c)).One().L("n") != items.Count)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Giỏ có sản phẩm đã ngừng kinh doanh.");
        // Tạo biến total để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        decimal total = 0;
        // Xử lý từng item trong tập dữ liệu đã lấy/kiểm tra.
        foreach (var item in items)
        {
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (!item.B("DangKinhDoanh") || item.S("TrangThai") != "DANG_BAN")
                // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                throw new ApiException("Có sản phẩm ngừng bán.");
            // Tạo biến stock để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var stock = (await repository.LockStock([item.L("BienTheID")], c)).FirstOrDefault();
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (stock is null || stock.N("stock") < item.N("SoLuong"))
                // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                throw new ApiException(item.S("TenSanPham") + " không đủ hàng.");
            // Gán kết quả vào total để bước xử lý tiếp theo dùng dữ liệu mới.
            total += item.N("price") * item.N("SoLuong");
        }

        // Tạo biến promo để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        Row? promo = null;
        // Tạo biến discount để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        decimal discount = 0;
        // Tạo biến eligible để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var eligible = items.ToList();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (b.S("promo") != "")
        {
            // Gán kết quả vào promo để bước xử lý tiếp theo dùng dữ liệu mới.
            promo = (await repository.LockPromotion([Text(b["promo"], 50)], c)).FirstOrDefault();
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (promo is null || total < promo.N("GiaTriDonToiThieu"))
                // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                throw new ApiException("Mã ưu đãi không hợp lệ hoặc đơn chưa đạt điều kiện.");
            // Tạo biến used để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var used = (await repository.PromotionUsage([session.UserId, promo.L("KhuyenMaiID")], c)).One();
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if ((promo.N("GioiHanLuotDung") > 0 && used.N("n") >= promo.N("GioiHanLuotDung")) || (promo.N("GioiHanMoiKhach") > 0 && used.N("own") >= promo.N("GioiHanMoiKhach")))
                // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                throw new ApiException("Mã ưu đãi đã hết lượt sử dụng.");
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (promo.S("PhamVi") != "TOAN_BO")
            {
                // Tạo biến map để dùng ở các bước tiếp theo trong phạm vi hiện tại.
                var map = promo.S("PhamVi") switch
                {
                    "SAN_PHAM" => new[]
                    {
                        "KhuyenMaiSanPham",
                        "SanPhamID"
                    },
                    "DANH_MUC" => ["KhuyenMaiDanhMuc", "DanhMucID"],
                    "THUONG_HIEU" => ["KhuyenMaiThuongHieu", "ThuongHieuID"],
                    _ => throw new ApiException("Phạm vi khuyến mãi không hợp lệ.")};
                // Gọi CheckoutRepository.EligibleIds → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
                var ids = await repository.EligibleIds(map, [promo.L("KhuyenMaiID")], c);
                // Gán kết quả vào eligible để bước xử lý tiếp theo dùng dữ liệu mới.
                eligible = items.Where(x => ids.Any(y => y.L("id") == x.L(map[1]))).ToList();
            }

            // Tạo biến basis để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var basis = eligible.Sum(x => x.N("price") * x.N("SoLuong"));
            // Tạo biến reduction để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var reduction = promo.S("HinhThuc") == "PHAN_TRAM" ? Math.Min(Math.Round(basis * promo.N("GiaTri") / 100, 0, MidpointRounding.AwayFromZero), promo["GiamToiDa"] is null ? decimal.MaxValue : promo.N("GiamToiDa")) : promo.N("GiaTri");
            // Gán kết quả vào discount để bước xử lý tiếp theo dùng dữ liệu mới.
            discount = Math.Min(basis, reduction);
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (discount <= 0)
                // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                throw new ApiException("Mã ưu đãi không áp dụng cho sản phẩm trong giỏ.");
        }

        // Tạo biến methodCode để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var methodCode = b.S("method", "COD");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (methodCode is not ("COD" or "BANK_TRANSFER"))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Hiện chỉ hỗ trợ COD và chuyển khoản đối soát thủ công.");
        // Tạo biến method để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var method = (await repository.PaymentMethod([methodCode], c)).One("Phương thức thanh toán không hợp lệ.");
        // Tạo biến shipping để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var shipping = total >= 1500000 ? 0m : 30000m;
        // Tạo biến data để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var data = new
        {
            items = items.Select(x => new[] { x.N("BienTheID"), x.N("SoLuong"), x.N("price") }),
            promo = promo?.L("KhuyenMaiID"),
            discount,
            shipping,
            method = method.L("id")
        };
        // Tạo biến fingerprint để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data)))).ToLowerInvariant();
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return new(cart, items, total, promo, discount, eligible, method, shipping, fingerprint);
    }

    // Báo giá trả fingerprint; đặt hàng sẽ tính lại để phát hiện giá/giỏ/ưu đãi thay đổi.
    public Task<object> Quote(Row b) => db.Transaction<object>(async c =>
    {
        // Tạo biến s để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var s = await Snapshot(b, c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return new
        {
            items = s.Items.Select(x => new { variantId = x.L("BienTheID"), name = x.S("TenSanPham"), size = x.S("size"), color = x.S("color"), quantity = x.L("SoLuong"), price = x.N("price") }),
            subtotal = s.Total,
            discount = s.Discount,
            shipping = s.Shipping,
            total = s.Total - s.Discount + s.Shipping,
            fingerprint = s.Fingerprint
        };
    });
    // Một transaction bao trọn đơn, dòng hàng, giữ kho, khuyến mãi, thanh toán và dọn giỏ.
    public Task<object> PlaceOrder(Row b) => db.Transaction<object>(async c =>
    {
        // Tạo biến key để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var key = Key(b["requestId"]);
        // Gọi CheckoutRepository.LockCustomer → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.LockCustomer([session.UserId], c);
        // Tạo biến existing để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var existing = (await repository.FindOrderByKey([key], c)).FirstOrDefault();
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (existing is not null)
        {
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (existing.L("owner") != session.UserId)
                // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                throw new ApiException("Mã yêu cầu đã được sử dụng.", 409);
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return new
            {
                id = existing.L("id"),
                total = (await repository.OrderTotal([existing.L("id")], c)).One().N("total")
            };
        }

        // Tạo biến s để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var s = await Snapshot(b, c);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (b.S("fingerprint") != "" && b.S("fingerprint") != s.Fingerprint)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new ApiException("Giỏ hàng, giá hoặc ưu đãi đã thay đổi. Vui lòng xem lại tổng tiền trước khi đặt.", 409);
        // Gọi CheckoutRepository.CreateOrder → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        var result = await repository.CreateOrder([Code("SS"), key, session.UserId, Required(b["name"], "người nhận"), Required(b["phone"], "số điện thoại"), Required(b["address"], "địa chỉ"), Text(b["note"]), s.Method.L("id"), s.Total, s.Discount, s.Shipping], c);
        // Tạo biến id để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var id = result.InsertId;
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (b.S("method", "COD") == "BANK_TRANSFER")
            // Gọi CheckoutRepository.SetDeadline → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
            await repository.SetDeadline([settings.HoldMinutes, id], c);
        // Tạo biến remaining để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var remaining = s.Discount;
        // Tạo biến remainingBase để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var remainingBase = s.Eligible.Sum(x => x.N("price") * x.N("SoLuong"));
        // Xử lý từng item trong tập dữ liệu đã lấy/kiểm tra.
        foreach (var item in s.Items)
        {
            // Tạo biến reduction để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            decimal reduction = 0;
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (s.Eligible.Contains(item) && remainingBase > 0)
            {
                // Gán kết quả vào reduction để bước xử lý tiếp theo dùng dữ liệu mới.
                reduction = Math.Min(remaining, Math.Round(remaining * item.N("price") * item.N("SoLuong") / remainingBase, 0, MidpointRounding.AwayFromZero));
                // Gán kết quả vào remaining để bước xử lý tiếp theo dùng dữ liệu mới.
                remaining -= reduction;
                // Gán kết quả vào remainingBase để bước xử lý tiếp theo dùng dữ liệu mới.
                remainingBase -= item.N("price") * item.N("SoLuong");
            }

            // Gọi CheckoutRepository.AddOrderLine → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
            var line = await repository.AddOrderLine([id, item.L("BienTheID"), item.S("MaSanPham"), item.S("SKU"), item.S("TenSanPham"), item.S("size"), item.S("color"), item.S("TenDanhMuc"), item.S("TenThuongHieu"), item.L("SoLuong"), item.N("price"), reduction], c);
            // Gọi CheckoutRepository.AllocateStock → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
            await repository.AllocateStock([line.InsertId, item.L("SoLuong")], c);
            // Chuyển tới Operations.StockLog để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
            await operations.StockLog(c, session.UserId, item.L("BienTheID"), "GIU_HANG", 0, item.L("SoLuong"), line.InsertId, null, "Phân bổ khi đặt hàng");
        }

        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (s.Promo is { } p)
            // Gọi CheckoutRepository.RecordPromotion → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
            await repository.RecordPromotion([id, p.L("KhuyenMaiID"), p.S("MaKhuyenMai"), p.S("TenKhuyenMai"), p.S("HinhThuc"), p.N("GiaTri"), s.Discount], c);
        // Gọi CheckoutRepository.RecordHistory → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.RecordHistory([id, session.UserId], c);
        // Gọi CheckoutRepository.CreatePayment → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.CreatePayment([id, s.Method.L("id"), Guid.NewGuid().ToString(), s.Total - s.Discount + s.Shipping], c);
        // Gọi CheckoutRepository.EmptyCart → SQL có tham số → MySQL; await chờ kết quả trước bước sau.
        await repository.EmptyCart([s.Cart], c);
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return new
        {
            id,
            total = s.Total - s.Discount + s.Shipping
        };
    });
}
