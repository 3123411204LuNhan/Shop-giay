using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dapper;
using MySqlConnector;
using ShopGiay.Entity;
using ShopGiay.Infrastructure;

namespace ShopGiay.Control;

public sealed record CheckoutInput(string? RequestId,string? Fingerprint,string? Name,string? Phone,string? Address,string? Note,string? Method,string? Promo);
public sealed class CheckoutException(string message,int status = 400) : Exception(message)
{
    public int Status { get; } = status;
}

public sealed class CheckoutControl(MySqlDataSource database,IConfiguration config,CartRepository carts,
    PromotionRepository promotions,OrderRepository orders)
{
    private sealed class Line
    {
        public long BienTheID { get; init; }
        public long SanPhamID { get; init; }
        public int DanhMucID { get; init; }
        public int ThuongHieuID { get; init; }
        public int SoLuong { get; init; }
        public decimal Price { get; init; }
        public required string MaSanPham { get; init; }
        public required string SKU { get; init; }
        public required string TenSanPham { get; init; }
        public required string Size { get; init; }
        public required string Color { get; init; }
        public required string TenDanhMuc { get; init; }
        public required string TenThuongHieu { get; init; }
        public bool Available { get; init; }
        public decimal Value => Price * SoLuong;
    }

    private sealed record Existing(long Id,long Owner,decimal Total);
    private static readonly JsonSerializerOptions FingerprintOptions = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    // Kiểm tra giỏ, tồn, ưu đãi và giá để báo giá hoặc tạo đơn trong một giao dịch.
    public async Task<object> Process(long account,CheckoutInput input,bool place,CancellationToken token)
    {
        if (place && !Guid.TryParseExact(input.RequestId,"D",out _)) throw new CheckoutException("Mã yêu cầu không hợp lệ.");
        await using var db = await database.OpenConnectionAsync(token);
        await using var tx = await db.BeginTransactionAsync(token);
        var session = new SqlSession(db,tx,token);
        CommandDefinition Sql(string sql,object? args = null) => new(sql,args,tx,cancellationToken:token);

        if (place) {
            await db.ExecuteScalarAsync<long>(Sql("SELECT TaiKhoanID FROM TaiKhoan WHERE TaiKhoanID=@account FOR UPDATE",new {account}));
            var previous = await db.QuerySingleOrDefaultAsync<Existing>(Sql(
                "SELECT DonHangID id,KhachHangID owner,TongThanhToan total FROM DonHang WHERE MaYeuCauTao=@RequestId",input));
            if (previous != null) {
                if (previous.Owner != account) throw new CheckoutException("Mã yêu cầu đã được sử dụng.",409);
                return new { previous.Id,previous.Total };
            }
        }

        var cart = (await carts.LayHoacTao(session,account)).GioHangID;
        var lines = (await db.QueryAsync<Line>(Sql("""
            SELECT b.BienTheID,b.SanPhamID,s.DanhMucID,s.ThuongHieuID,ct.SoLuong,COALESCE(b.GiaBanRieng,s.GiaBan) price,
                   s.MaSanPham,b.SKU,s.TenSanPham,kc.GiaTri size,m.TenMau color,d.TenDanhMuc,t.TenThuongHieu,
                   (b.DangKinhDoanh=1 AND s.TrangThai='DANG_BAN' AND d.DangHoatDong=1 AND t.DangHoatDong=1) available
            FROM ChiTietGioHang ct JOIN BienTheSanPham b ON b.BienTheID=ct.BienTheID JOIN SanPham s ON s.SanPhamID=b.SanPhamID
            JOIN DanhMuc d ON d.DanhMucID=s.DanhMucID JOIN ThuongHieu t ON t.ThuongHieuID=s.ThuongHieuID
            JOIN KichCo kc ON kc.KichCoID=b.KichCoID JOIN MauSac m ON m.MauSacID=b.MauSacID
            WHERE ct.GioHangID=@cart ORDER BY b.BienTheID
            """,new {cart}))).AsList();
        if (lines.Count == 0) throw new CheckoutException("Giỏ hàng đang trống.");
        foreach (var line in lines) {
            if (!line.Available) throw new CheckoutException("Giỏ có sản phẩm đã ngừng kinh doanh.");
            var stock = await InventoryRepository.KhoaTon(session,line.BienTheID);
            if (stock?.ConDuHang(line.SoLuong) != true) throw new CheckoutException(line.TenSanPham + " không đủ hàng.");
        }

        var subtotal = lines.Sum(x => x.Value);
        var eligible = lines;
        KhuyenMai? promo = null;
        decimal discount = 0;
        if (!string.IsNullOrWhiteSpace(input.Promo)) {
            promo = await promotions.KhoaTheoMa(session,input.Promo.Trim());
            if (promo == null || !promo.ConHieuLuc(await session.Value<DateTime>("SELECT UTC_TIMESTAMP()")) || subtotal < promo.GiaTriDonToiThieu)
                throw new CheckoutException("Mã ưu đãi không hợp lệ hoặc đơn chưa đạt điều kiện.");
            var usage = await db.QuerySingleAsync(Sql("""
                SELECT COUNT(*) uses,COALESCE(SUM(d.KhachHangID=@account),0) own FROM KhuyenMaiDonHang k
                JOIN DonHang d ON d.DonHangID=k.DonHangID WHERE k.KhuyenMaiID=@KhuyenMaiID AND d.TrangThai<>'DA_HUY'
                """,new {account,promo.KhuyenMaiID}));
            if (Convert.ToInt64(usage.uses) >= promo.GioiHanLuotDung || Convert.ToInt64(usage.own) >= promo.GioiHanMoiKhach)
                throw new CheckoutException("Mã ưu đãi đã hết lượt sử dụng.");
            if (promo.PhamVi != "TOAN_BO") {
                (string sql,Func<Line,long> key) scope = promo.PhamVi switch {
                    "SAN_PHAM" => ("SELECT SanPhamID FROM KhuyenMaiSanPham WHERE KhuyenMaiID=@KhuyenMaiID",x=>x.SanPhamID),
                    "DANH_MUC" => ("SELECT DanhMucID FROM KhuyenMaiDanhMuc WHERE KhuyenMaiID=@KhuyenMaiID",x=>x.DanhMucID),
                    "THUONG_HIEU" => ("SELECT ThuongHieuID FROM KhuyenMaiThuongHieu WHERE KhuyenMaiID=@KhuyenMaiID",x=>x.ThuongHieuID),
                    _ => throw new CheckoutException("Phạm vi khuyến mãi không hợp lệ.")
                };
                var ids = (await db.QueryAsync<long>(Sql(scope.sql,promo))).ToHashSet();
                eligible = lines.Where(line => ids.Contains(scope.key(line))).ToList();
            }
            discount = promo.TinhTienGiam(eligible.Sum(x=>x.Value));
            if (discount == 0) throw new CheckoutException("Mã ưu đãi không áp dụng cho sản phẩm trong giỏ.");
        }
        var methodCode = string.IsNullOrEmpty(input.Method) ? "COD" : input.Method;
        if (methodCode is not ("COD" or "BANK_TRANSFER")) throw new CheckoutException("Phương thức thanh toán không hợp lệ.");
        var method = await db.QuerySingleOrDefaultAsync<int?>(Sql("""
            SELECT PhuongThucID FROM PhuongThucThanhToan WHERE MaPhuongThuc=@methodCode AND DangHoatDong=1
            """,new {methodCode})) ?? throw new CheckoutException("Phương thức thanh toán không hợp lệ.");
        var shipping = subtotal >= 1500000 ? 0 : 30000;
        var don = new DonHang {
            MaDonHang = place ? "SS-" + Guid.NewGuid().ToString("N") : "",
            MaYeuCauTao = place ? Guid.Parse(input.RequestId!) : Guid.Empty, KhachHangID = account,
            HoTenNguoiNhan = input.Name?.Trim() ?? "", SoDienThoaiNhan = input.Phone?.Trim() ?? "",
            DiaChiNhan = input.Address?.Trim() ?? "", GhiChu = input.Note?.Trim() ?? "",
            PhuongThucID = method, TienHang = subtotal, TienGiam = discount, PhiVanChuyen = shipping, TienTe = "VND"
        };
        var total = don.TinhTongThanhToan();
        var signature = JsonSerializer.SerializeToUtf8Bytes(new {
            items = lines.Select(x => new object[]{x.BienTheID,x.SoLuong,(double)x.Price}),
            promo = promo?.KhuyenMaiID, discount = (double)discount,shipping,method
        },FingerprintOptions);
        var fingerprint = Convert.ToHexStringLower(SHA256.HashData(signature));
        if (!place) {
            await tx.CommitAsync(token);
            return new {items=lines.Select(x=>new {variantId=x.BienTheID,name=x.TenSanPham,x.Size,x.Color,quantity=x.SoLuong,x.Price}),
                subtotal,discount,shipping,total,fingerprint};
        }
        if (!string.IsNullOrEmpty(input.Fingerprint) && input.Fingerprint != fingerprint)
            throw new CheckoutException("Giỏ hàng, giá hoặc ưu đãi đã thay đổi.",409);
        if (string.IsNullOrWhiteSpace(input.Name) || string.IsNullOrWhiteSpace(input.Phone) || string.IsNullOrWhiteSpace(input.Address))
            throw new CheckoutException("Vui lòng nhập người nhận, số điện thoại và địa chỉ.");
        var minutes = methodCode == "BANK_TRANSFER" ? config.GetValue<int?>("PAYMENT_HOLD_MINUTES") ?? 30 : (int?)null;
        var id = await orders.ThemDon(session,don,minutes);
        var remaining = discount;
        var remainingBase = eligible.Sum(x=>x.Value);
        foreach (var line in lines) {
            decimal reduction = 0;
            if (eligible.Contains(line) && remainingBase > 0) {
                reduction = Math.Min(remaining,Math.Round(remaining*line.Value/remainingBase,MidpointRounding.AwayFromZero));
                remaining -= reduction;
                remainingBase -= line.Value;
            }
            var detail = await orders.ThemDong(session,new ChiTietDonHang {
                DonHangID = id, BienTheID = line.BienTheID, MaSanPhamLucMua = line.MaSanPham, SKULucMua = line.SKU,
                TenSanPhamLucMua = line.TenSanPham, KichCoLucMua = line.Size, MauSacLucMua = line.Color,
                DanhMucLucMua = line.TenDanhMuc, ThuongHieuLucMua = line.TenThuongHieu, SoLuong = line.SoLuong,
                DonGia = line.Price, TienGiamPhanBo = reduction
            });
            await orders.ThemPhanBo(session,new PhanBoTonKho { ChiTietDonHangID = detail, SoLuongGiu = line.SoLuong });
            await StockLedger.Change(session,account,line.BienTheID,"GIU_HANG",0,line.SoLuong,detail,null,"Phân bổ khi đặt hàng");
        }
        if (promo != null)
            await db.ExecuteAsync(Sql("""
                INSERT INTO KhuyenMaiDonHang(DonHangID,KhuyenMaiID,MaLucApDung,TenLucApDung,HinhThucLucApDung,GiaTriLucApDung,TienGiamThucTe)
                VALUES(@id,@KhuyenMaiID,@MaKhuyenMai,@TenKhuyenMai,@HinhThuc,@GiaTri,@discount)
                """,new {id,promo.KhuyenMaiID,promo.MaKhuyenMai,promo.TenKhuyenMai,promo.HinhThuc,promo.GiaTri,discount}));
        await db.ExecuteAsync(Sql("INSERT INTO LichSuDonHang(DonHangID,TrangThaiMoi,NguoiThucHienID,GhiChu) VALUES(@id,'CHO_XAC_NHAN',@account,'Khách tạo đơn')",new {id,account}));
        var thanhToan = new ThanhToan { DonHangID = id, PhuongThucID = method, MaYeuCau = Guid.NewGuid() };
        thanhToan.DatSoTienChoThu(total);
        await orders.ThemThanhToan(session,thanhToan);
        await carts.LamTrong(session,cart);
        await tx.CommitAsync(token);
        return new {id,total};
    }
}
