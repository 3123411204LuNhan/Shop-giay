namespace ShopGiay.Infrastructure;

internal static class DatabaseSeed
{
    private sealed record Variant(long Id,string ProductCode,string Name,int Price,int Size,string Color,string Category,string Brand);

    public static async Task Run(SqlSession db)
    {
        if (await db.Value<long>("SELECT COUNT(*) FROM TaiKhoan") != 0)
            throw new InvalidOperationException("Đã có dữ liệu tài khoản. Lệnh db:seed không ghi đè.");

        foreach (var (code,name) in new[] { ("CUSTOMER","Khách hàng"),("SHOP_MANAGER","Chủ shop"),("SYSTEM_ADMIN","Nhân viên kỹ thuật") })
            await db.Execute("INSERT INTO VaiTro(MaVaiTro,TenVaiTro) VALUES(?,?)",code,name);
        var users = new List<long>();
        (string Email,string Name,string Role)[] accounts = [
            ("khach@sole.vn","Nguyễn Minh Anh","CUSTOMER"), ("shop@sole.vn","Lê Minh Sang","SHOP_MANAGER"),
            ("admin@sole.vn","Quản trị kỹ thuật","SYSTEM_ADMIN"), ("linh@sole.vn","Trần Hoàng Linh","CUSTOMER"),
            ("kiet@sole.vn","Lâm Tuấn Kiệt","CUSTOMER")
        ];
        foreach (var (email,name,role) in accounts) {
            var id = await db.Insert("""
                INSERT INTO TaiKhoan(TenDangNhap,Email,MatKhauHash,HoTen,SoDienThoai,DiaChi,EmailDaXacMinh) VALUES(?,?,?,?,?,?,1)
                """,email,email,Password.Hash("SoleDemo@123"),name,$"090123450{users.Count}","273 An Dương Vương, Phường Chợ Quán, TP. Hồ Chí Minh");
            users.Add(id);
            await db.Execute("INSERT INTO TaiKhoanVaiTro(TaiKhoanID,VaiTroID) SELECT ?,VaiTroID FROM VaiTro WHERE MaVaiTro=?",id,role);
        }

        var cod = await db.Insert("INSERT INTO PhuongThucThanhToan(MaPhuongThuc,TenPhuongThuc) VALUES('COD','Thanh toán khi nhận hàng')");
        var transfer = await db.Insert("INSERT INTO PhuongThucThanhToan(MaPhuongThuc,TenPhuongThuc) VALUES('BANK_TRANSFER','Chuyển khoản ngân hàng')");
        await db.Execute("INSERT INTO PhuongThucThanhToan(MaPhuongThuc,TenPhuongThuc,DangHoatDong) VALUES('ONLINE','Thanh toán trực tuyến (chưa tích hợp)',0)");
        var variants = await SeedCatalog(db,users[1]);

        (string Code,string Name,string Type,int Amount,int? Limit,int Minimum)[] promotions = [
            ("WELCOME10","Chào bạn mới — giảm 10%","PHAN_TRAM",10,300000,1500000),
            ("SOLE200","Bước mới, ưu đãi mới — giảm 200K","SO_TIEN",200000,null,2500000)
        ];
        foreach (var (code,name,type,amount,limit,minimum) in promotions) {
            var id = await db.Insert("""
                INSERT INTO KhuyenMai(MaKhuyenMai,TenKhuyenMai,HinhThuc,GiaTri,GiamToiDa,GiaTriDonToiThieu,PhamVi,BatDauLuc,KetThucLuc,TrangThai,NguoiTaoID)
                VALUES(?,?,?,?,?,?,'TOAN_BO',DATE_SUB(UTC_TIMESTAMP(),INTERVAL 1 DAY),DATE_ADD(UTC_TIMESTAMP(),INTERVAL 180 DAY),'DANG_AP_DUNG',?)
                """,code,name,type,amount,limit,minimum,users[1]);
            await db.Execute("INSERT INTO LichSuKhuyenMai(KhuyenMaiID,NguoiThucHienID,HanhDong,NoiDung) VALUES(?,?,'TAO','Tạo chương trình mẫu')",id,users[1]);
        }

        await SeedOrders(db,variants,users,cod,transfer);
        await db.Execute("""
            INSERT INTO NhatKyHeThong(NguoiThucHienID,HanhDong,DoiTuong,MaDoiTuong,NoiDung,KetQua)
            VALUES(?,'KHOI_TAO','HeThong','seed','Khởi tạo 37 bảng, sản phẩm và đơn mẫu','THANH_CONG')
            """,users[2]);
    }

    private static async Task<List<Variant>> SeedCatalog(SqlSession db,long manager)
    {
        var categories = new Dictionary<string,long>();
        foreach (var (key,name) in new[] { ("Lifestyle","Giày Lifestyle"),("Running","Giày chạy bộ"),("Classic","Giày Classic") })
            categories[key] = await db.Insert("INSERT INTO DanhMuc(TenDanhMuc) VALUES(?)",name);
        var brands = new Dictionary<string,long>();
        foreach (var name in new[] { "Nike","Adidas","New Balance","Converse","Vans" })
            brands[name] = await db.Insert("INSERT INTO ThuongHieu(TenThuongHieu) VALUES(?)",name);
        var sizes = new List<long>();
        for (var size = 36; size <= 44; size++)
            sizes.Add(await db.Insert("INSERT INTO KichCo(HeSize,GiaTri,ThuTu) VALUES('EU',?,?)",size,size));
        var colors = new Dictionary<string,long>();
        foreach (var (key,name,hex) in new[] {
            ("red","Đỏ","#db3d32"),("white","Trắng","#f3f1eb"),("cream","Kem","#d8c7a9"),
            ("green","Xanh lá","#55796a"),("gray","Xám","#939693"),("black","Đen","#262626")
        })
            colors[key] = await db.Insert("INSERT INTO MauSac(TenMau,MaHex) VALUES(?,?)",name,hex);

        (string Brand,string Model,string Category,int Price,string Image,string Color)[] products = [
            ("Nike","Air Max Pulse","Lifestyle",3290000,"red","red"),
            ("Adidas","Samba OG","Lifestyle",2700000,"white","white"),
            ("New Balance","550 Sea Salt","Lifestyle",2890000,"cream","white"),
            ("Nike","Air Force 1 Low","Lifestyle",2490000,"white","white"),
            ("Converse","Chuck 70 Hi","Classic",1850000,"cream","cream"),
            ("Nike","Pegasus 41","Running",3590000,"red","red"),
            ("Adidas","Gazelle Indoor","Classic",2590000,"green","green"),
            ("New Balance","2002R Protection","Lifestyle",3490000,"gray","gray"),
            ("Vans","Old Skool Classic","Classic",1650000,"black","black"),
            ("Nike","Dunk Low Retro","Lifestyle",2990000,"black","black"),
            ("Adidas","Ultraboost Light","Running",3990000,"white","white"),
            ("Converse","Run Star Hike","Lifestyle",2450000,"black","black")
        ];
        const string description = "Một thiết kế dành cho nhịp sống năng động. Phom dáng dễ phối, đệm êm và cảm giác thoải mái trong từng bước chân. Ảnh minh họa phong cách sản phẩm, không dùng thay thế ảnh hàng thực tế.";
        var variants = new List<Variant>();
        for (var i = 0; i < products.Length; i++) {
            var (brand,model,category,price,image,color) = products[i];
            var name = $"{brand} {model}";
            var productCode = $"SS-{i+1:D3}";
            var product = await db.Insert("""
                INSERT INTO SanPham(MaSanPham,TenSanPham,MoTa,GiaBan,DanhMucID,ThuongHieuID,TrangThai,NguoiTaoID)
                VALUES(?,?,?,?,?,?,'DANG_BAN',?)
                """,productCode,name,description,price,categories[category],brands[brand],manager);
            await db.Execute("INSERT INTO HinhAnhSanPham(SanPhamID,DuongDan,LaAnhChinh,MoTaAnh) VALUES(?,?,1,?)",product,$"/images/shoe-{image}.jpg",name);
            for (var j = 0; j < sizes.Count; j++) {
                var size = 36 + j;
                var variant = await db.Insert("INSERT INTO BienTheSanPham(SanPhamID,SKU,KichCoID,MauSacID) VALUES(?,?,?,?)",product,$"SS-{i+1}-{size}-{color}",sizes[j],colors[color]);
                variants.Add(new Variant(variant,productCode,name,price,size,color,category,brand));
                var amount = i == 3 && j < 4 ? 3 : 8 + (i + j) % 14;
                await db.Execute("INSERT INTO TonKho(BienTheID,SoLuongTon) VALUES(?,?)",variant,amount);
                var receipt = await db.Insert("""
                    INSERT INTO PhieuKho(MaPhieu,MaChongLap,LoaiPhieu,LyDo,NguoiThucHienID) VALUES(?,?,'NHAP','Nhập hàng mẫu ban đầu',?)
                    """,$"INIT-{variant}",Guid.NewGuid().ToString(),manager);
                var line = await db.Insert("INSERT INTO ChiTietPhieuKho(PhieuKhoID,BienTheID,SoLuongThayDoi) VALUES(?,?,?)",receipt,variant,amount);
                await db.Execute("""
                    INSERT INTO BienDongKho(BienTheID,LoaiBienDong,ThayDoiTon,ThayDoiPhanBo,TonSau,PhanBoSau,ChiTietPhieuKhoID,NguoiThucHienID,MaThaoTac,LyDo)
                    VALUES(?,'NHAP',?,0,?,0,?,?,?,'Nhập hàng mẫu')
                    """,variant,amount,amount,line,manager,Guid.NewGuid().ToString());
            }
        }
        return variants;
    }

    private static async Task SeedOrders(SqlSession db,List<Variant> variants,List<long> users,long cod,long transfer)
    {
        string[] statuses = ["CHO_XAC_NHAN","DA_XAC_NHAN","DANG_CHUAN_BI","DANG_GIAO","DA_GIAO","HOAN_THANH",
            "HOAN_THANH","HOAN_THANH","HOAN_THANH","HOAN_THANH","DA_HUY","CHO_XAC_NHAN"];
        (long Id,string Name)[] customers = [(users[0],"Nguyễn Minh Anh"),(users[3],"Trần Hoàng Linh"),(users[4],"Lâm Tuấn Kiệt")];
        var now = DateTime.UtcNow;
        for (var i = 0; i < statuses.Length; i++) {
            var v = variants[i * 5];
            var status = statuses[i];
            var (customer,recipient) = customers[i % customers.Length];
            var method = i % 2 == 0 ? cod : transfer;
            var paid = status is "DA_GIAO" or "HOAN_THANH";
            var shipped = paid || status == "DANG_GIAO";
            var canceled = status == "DA_HUY";
            var order = await db.Insert("""
                INSERT INTO DonHang(MaDonHang,MaYeuCauTao,KhachHangID,HoTenNguoiNhan,SoDienThoaiNhan,DiaChiNhan,PhuongThucID,TrangThai,TienHang,NgayDat,NgayGiao,NgayHoanThanh,NgayHuy)
                VALUES(?,?,?,?,'0901234567','273 An Dương Vương, TP. Hồ Chí Minh',?,?,?,DATE_SUB(UTC_TIMESTAMP(),INTERVAL ? DAY),?,?,?)
                """,$"SS-2026-{1001+i}",Guid.NewGuid().ToString(),customer,recipient,method,status,v.Price,i%10,
                paid ? (DateTime?)now : null,status == "HOAN_THANH" ? (DateTime?)now : null,canceled ? (DateTime?)now : null);
            var line = await db.Insert("""
                INSERT INTO ChiTietDonHang(DonHangID,BienTheID,MaSanPhamLucMua,SKULucMua,TenSanPhamLucMua,KichCoLucMua,MauSacLucMua,DanhMucLucMua,ThuongHieuLucMua,SoLuong,DonGia)
                VALUES(?,?,?,?,?,?,?,?,?,1,?)
                """,order,v.Id,v.ProductCode,$"SS-{v.Id}",v.Name,v.Size,v.Color,v.Category,v.Brand,v.Price);
            var allocation = canceled ? "DA_GIAI_PHONG" : shipped ? "DA_XUAT" : "DANG_GIU";
            await db.Execute("INSERT INTO PhanBoTonKho(ChiTietDonHangID,SoLuongGiu,TrangThai,NgayKetThuc) VALUES(?,1,?,?)",line,allocation,
                shipped || canceled ? (DateTime?)now : null);

            var stock = await db.Value<int>("SELECT SoLuongTon FROM TonKho WHERE BienTheID=?",v.Id);
            await db.Execute("UPDATE TonKho SET SoLuongDaPhanBo=SoLuongDaPhanBo+1 WHERE BienTheID=?",v.Id);
            await db.Execute("""
                INSERT INTO BienDongKho(BienTheID,LoaiBienDong,ThayDoiTon,ThayDoiPhanBo,TonSau,PhanBoSau,ChiTietDonHangID,NguoiThucHienID,MaThaoTac,LyDo)
                VALUES(?,'GIU_HANG',0,1,?,1,?,?,?,'Đặt hàng mẫu')
                """,v.Id,stock,line,customer,Guid.NewGuid().ToString());
            if (shipped || canceled) {
                var change = shipped ? -1 : 0;
                await db.Execute("UPDATE TonKho SET SoLuongTon=SoLuongTon+?,SoLuongDaPhanBo=0 WHERE BienTheID=?",change,v.Id);
                await db.Execute("""
                    INSERT INTO BienDongKho(BienTheID,LoaiBienDong,ThayDoiTon,ThayDoiPhanBo,TonSau,PhanBoSau,ChiTietDonHangID,NguoiThucHienID,MaThaoTac,LyDo)
                    VALUES(?,?,?,-1,?,0,?,?,?,?)
                    """,v.Id,shipped ? "BAN_HANG" : "GIAI_PHONG",change,stock+change,line,users[1],
                    Guid.NewGuid().ToString(),shipped ? "Xuất đơn mẫu" : "Hủy đơn mẫu");
            }

            await db.Execute("INSERT INTO LichSuDonHang(DonHangID,TrangThaiMoi,NguoiThucHienID,GhiChu) VALUES(?,?,?,'Dữ liệu đơn mẫu')",order,status,users[1]);
            await db.Execute("""
                INSERT INTO ThanhToan(DonHangID,PhuongThucID,MaYeuCau,SoTien,TrangThai,NgayXacNhan,NguoiXacNhanID,BangChung)
                VALUES(?,?,?,?,?,?,?,?)
                """,order,method,Guid.NewGuid().ToString(),v.Price,paid ? "DA_XAC_NHAN" : "CHO_XAC_NHAN",
                paid ? (DateTime?)now.AddDays(-(i%7)) : null,paid ? (long?)users[1] : null,paid ? "Giao dịch mẫu, không phải giao dịch thực" : null);
            if (status == "HOAN_THANH")
                await db.Execute("""
                    INSERT INTO DanhGia(DonHangID,ChiTietDonHangID,KhachHangID,SoSao,NoiDung)
                    VALUES(?,?,?,5,'Giày êm, form vừa chân, đóng gói rất cẩn thận. Sẽ quay lại!')
                    """,order,line,customer);
            if (i == 6) {
                var request = await db.Insert("""
                    INSERT INTO YeuCauDoiTra(MaYeuCau,MaChongLap,DonHangID,KhachHangID,LoaiYeuCau,LyDo)
                    VALUES('DT-MAU-001',?,?,?,'TRA','Muốn đổi sang size rộng hơn, đề nghị trả và đặt lại')
                    """,Guid.NewGuid().ToString(),order,customer);
                await db.Execute("INSERT INTO ChiTietDoiTra(YeuCauDoiTraID,DonHangID,ChiTietDonHangID,SoLuongYeuCau) VALUES(?,?,?,1)",request,order,line);
            }
        }
    }
}
