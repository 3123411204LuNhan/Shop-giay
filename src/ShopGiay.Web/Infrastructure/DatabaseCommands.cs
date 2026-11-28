using System.Text.RegularExpressions;
using MySqlConnector;
using ShopGiay.Web.Services;

namespace ShopGiay.Web.Infrastructure;
// CLI tách biệt F5 để không tự thay đổi schema/dữ liệu đang dùng.
public static class DatabaseCommands
{
    // Program → lệnh C# thiết lập/migrate/seed theo tham số người chạy chủ động chọn.
    public static async Task Run(IServiceProvider services, string[] args)
    {
        // Tạo biến db để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var db = services.GetRequiredService<Database>();
        // Tạo biến env để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var env = services.GetRequiredService<IWebHostEnvironment>();
        // Cấu hình là lệnh độc lập; lần chạy sau nạp lại file local vừa lưu.
        if (args.Contains("--db-config"))
        {
            // Thực hiện bước DatabaseConfiguration.Configure trong DatabaseCommands; tiếp tục theo luồng của hàm.
            await DatabaseConfiguration.Configure(db, env);
            // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
            return;
        }

        // Chẩn đoán chỉ đọc, không tạo bảng hoặc thêm dữ liệu.
        if (args.Contains("--db-check"))
            // Thực hiện bước DatabaseConfiguration.Check trong DatabaseCommands; tiếp tục theo luồng của hàm.
            await DatabaseConfiguration.Check(db);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (args.Contains("--db-setup"))
            // Thực hiện bước Setup trong DatabaseCommands; tiếp tục theo luồng của hàm.
            await Setup(db, env);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (args.Contains("--db-migrate"))
            // Thực hiện bước Migrate trong DatabaseCommands; tiếp tục theo luồng của hàm.
            await Migrate(db);
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (args.Contains("--db-seed"))
            // Thực hiện bước Seed trong DatabaseCommands; tiếp tục theo luồng của hàm.
            await Seed(db, services.GetRequiredService<IPasswordHasher>(), services.GetRequiredService<Operations>());
    }

    // Chỉ khởi tạo database trống; không DROP, không ghi đè database đã có bảng.
    private static async Task Setup(Database db, IWebHostEnvironment env)
    {
        // Tạo biến options để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var options = new MySqlConnectionStringBuilder(db.ConnectionString);
        // Tạo biến name để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var name = options.Database;
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (!Regex.IsMatch(name, "^[a-zA-Z0-9_]+$"))
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new InvalidOperationException("Tên database chỉ chứa chữ, số và gạch dưới.");
        // Gán kết quả vào options.Database để bước xử lý tiếp theo dùng dữ liệu mới.
        options.Database = "";
        // Tạo biến c để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        await using var c = new MySqlConnection(options.ConnectionString);
        // Thực hiện bước c.OpenAsync trong DatabaseCommands; tiếp tục theo luồng của hàm.
        await c.OpenAsync();
        // Mở tài nguyên và tự giải phóng khi ra khỏi phạm vi xử lý.
        await using (var create = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{name}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci", c))
            // Thực hiện bước create.ExecuteNonQueryAsync trong DatabaseCommands; tiếp tục theo luồng của hàm.
            await create.ExecuteNonQueryAsync();
        // Thực hiện bước c.ChangeDatabaseAsync trong DatabaseCommands; tiếp tục theo luồng của hàm.
        await c.ChangeDatabaseAsync(name);
        // Mở tài nguyên và tự giải phóng khi ra khỏi phạm vi xử lý.
        await using (var count = new MySqlCommand("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema=DATABASE()", c))
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (Convert.ToInt64(await count.ExecuteScalarAsync()) > 0)
                // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
                throw new InvalidOperationException("Database đã có bảng. Chọn tên mới hoặc chạy --db-migrate.");
        // Tạo biến path để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var path = Path.Combine(AppContext.BaseDirectory, "database", "schema.sql");
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if (!File.Exists(path))
            // Gán kết quả vào path để bước xử lý tiếp theo dùng dữ liệu mới.
            path = Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "..", "database", "schema.sql"));
        // Tạo biến command để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        await using var command = new MySqlCommand(await File.ReadAllTextAsync(path), c);
        // Thực hiện bước command.ExecuteNonQueryAsync trong DatabaseCommands; tiếp tục theo luồng của hàm.
        await command.ExecuteNonQueryAsync();
        // Thực hiện bước Console.WriteLine trong DatabaseCommands; tiếp tục theo luồng của hàm.
        Console.WriteLine("Đã tạo 37 bảng trong " + name);
    }

    // Bổ sung cột quy trình theo schema gốc; không xóa dữ liệu hiện có.
    private static async Task Migrate(Database db)
    {
        // Tạo biến columns để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var columns = new Dictionary<string, Dictionary<string, string>>
        {
            ["DanhMuc"] = new()
            {
                ["LaBanNhap"] = "tinyint NOT NULL DEFAULT 0"
            },
            ["ThuongHieu"] = new()
            {
                ["LaBanNhap"] = "tinyint NOT NULL DEFAULT 0"
            },
            ["TaiKhoan"] = new()
            {
                ["NgayXoa"] = "datetime NULL"
            },
            ["DonHang"] = new()
            {
                ["DonViVanChuyen"] = "varchar(150) NULL",
                ["MaVanDon"] = "varchar(100) NULL",
                ["TrangThaiGiaoHang"] = "varchar(20) NOT NULL DEFAULT 'CHUA_GIAO'"
            },
            ["PhieuKho"] = new()
            {
                ["TrangThai"] = "varchar(20) NOT NULL DEFAULT 'DA_XAC_NHAN'",
                ["NhaCungCap"] = "varchar(200) NULL",
                ["NgayXacNhan"] = "datetime NULL",
                ["NguoiXacNhanID"] = "bigint NULL",
                ["DonHangID"] = "bigint NULL"
            }
        };
        // Xử lý từng table trong tập dữ liệu đã lấy/kiểm tra.
        foreach (var table in new[]
        {
            "LichSuDonHang",
            "BienDongKho"
        }

        )
        {
            // Tạo biến column để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var column = (await db.Query("SELECT IS_NULLABLE nullable FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name=? AND column_name='NguoiThucHienID'", [table])).FirstOrDefault();
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (column?.S("nullable") == "NO")
                // Database.Query → thực thi câu lệnh trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
                await db.Query($"ALTER TABLE {table} MODIFY COLUMN NguoiThucHienID bigint NULL");
        }

        // Thực hiện bước (await db.Query("SELECT COUNT(*) n FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name=? AND column_name=?",[table,column])).One().L trong DatabaseCommands; tiếp tục theo luồng của hàm.
        foreach (var(table, definitions)in columns)
            // Thực hiện bước (await db.Query("SELECT COUNT(*) n FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name=? AND column_name=?",[table,column])).One().L trong DatabaseCommands; tiếp tục theo luồng của hàm.
            foreach (var(column, definition)in definitions)
            {
                // Tạo biến count để dùng ở các bước tiếp theo trong phạm vi hiện tại.
                var count = (await db.Query("SELECT COUNT(*) n FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name=? AND column_name=?", [table, column])).One().L("n");
                // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
                if (count == 0)
                    // Database.Query → thực thi câu lệnh trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
                    await db.Query($"ALTER TABLE {table} ADD COLUMN {column} {definition}");
            }

        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        await db.Query("UPDATE TaiKhoan SET TrangThai='BI_KHOA' WHERE TrangThai='VO_HIEU_HOA'");
        // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        await db.Query("UPDATE SanPham SET TrangThai='AN' WHERE TrangThai='NHAP'");
        // Xử lý từng table trong tập dữ liệu đã lấy/kiểm tra.
        foreach (var table in new[]
        {
            "DanhMuc",
            "ThuongHieu"
        }

        )
            // Database.Query → cập nhật bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            await db.Query($"UPDATE {table} SET LaBanNhap=1,DangHoatDong=0 WHERE (DangHoatDong=0 AND LaBanNhap=0) OR (LaBanNhap=1 AND DangHoatDong=1)");
        // Database.Query → thực thi câu lệnh trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        await db.Query("ALTER TABLE SanPham ALTER COLUMN TrangThai SET DEFAULT 'AN'");
        // Thực hiện bước (await db.Query("SELECT COUNT(*) n FROM information_schema.table_constraints WHERE constraint_schema=DATABASE() AND table_name=? AND constraint_name=?",[table,name])).One().L trong DatabaseCommands; tiếp tục theo luồng của hàm.
        foreach (var(table, name, clause)in new[]
        {
            ("SanPham", "CK_SanPham_TrangThai", "CHECK (TrangThai IN ('DANG_BAN','AN','NGUNG_BAN'))"),
            ("TaiKhoan", "CK_TaiKhoan_TrangThai", "CHECK (TrangThai IN ('HOAT_DONG','BI_KHOA'))"),
            ("PhieuKho", "CK_PhieuKho_TrangThai", "CHECK (TrangThai IN ('NHAP','DA_XAC_NHAN','DA_HUY'))"),
            ("DonHang", "CK_DonHang_GiaoHang", "CHECK (TrangThaiGiaoHang IN ('CHUA_GIAO','DANG_GIAO','GIAO_THAT_BAI','CHO_HOAN_VE','DA_NHAN_HOAN','DA_GIAO'))"),
            ("PhieuKho", "FK_PhieuKho_XacNhan", "FOREIGN KEY (NguoiXacNhanID) REFERENCES TaiKhoan(TaiKhoanID)"),
            ("PhieuKho", "FK_PhieuKho_DonHang", "FOREIGN KEY (DonHangID) REFERENCES DonHang(DonHangID)")
        }

        )
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if ((await db.Query("SELECT COUNT(*) n FROM information_schema.table_constraints WHERE constraint_schema=DATABASE() AND table_name=? AND constraint_name=?", [table, name])).One().L("n") == 0)
                // Database.Query → thực thi câu lệnh trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
                await db.Query($"ALTER TABLE {table} ADD CONSTRAINT {name} {clause}");
        // Thực hiện bước Console.WriteLine trong DatabaseCommands; tiếp tục theo luồng của hàm.
        Console.WriteLine("Đã cập nhật schema quy trình.");
    }

    // Dữ liệu demo mới chỉ được thêm khi chưa có tài khoản; database đang sử dụng được giữ nguyên.
    private static Task<bool> Seed(Database db, IPasswordHasher passwords, Operations operations) => db.Transaction(async c =>
    {
        // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
        if ((await db.Query("SELECT COUNT(*) n FROM TaiKhoan", [], c)).One().L("n") > 0)
            // Dừng luồng với lỗi; nếu đang ở Database.Transaction thì các bước ghi sẽ được hoàn tác.
            throw new InvalidOperationException("Đã có tài khoản; seed không ghi đè dữ liệu.");
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        foreach (var(code, name)in new[]
        {
            ("CUSTOMER", "Khách hàng"),
            ("SHOP_MANAGER", "Chủ shop"),
            ("SYSTEM_ADMIN", "Nhân viên kỹ thuật")
        }

        )
            // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            await db.Query("INSERT INTO VaiTro(MaVaiTro,TenVaiTro) VALUES(?,?)", [code, name], c);
        // Tạo biến users để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var users = new List<long>();
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        foreach (var(email, name, role)in new[]
        {
            ("khach@sole.vn", "Nguyễn Minh Anh", "CUSTOMER"),
            ("shop@sole.vn", "Lê Minh Sang", "SHOP_MANAGER"),
            ("admin@sole.vn", "Quản trị kỹ thuật", "SYSTEM_ADMIN"),
            ("linh@sole.vn", "Trần Hoàng Linh", "CUSTOMER"),
            ("kiet@sole.vn", "Lâm Tuấn Kiệt", "CUSTOMER")
        }

        )
        {
            // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            var user = await db.Query("INSERT INTO TaiKhoan(TenDangNhap,Email,MatKhauHash,HoTen,SoDienThoai,DiaChi,EmailDaXacMinh) VALUES(?,?,?,?,?,?,1)", [email, email, passwords.Hash("SoleDemo@123"), name, "090123450" + users.Count, "273 An Dương Vương, TP. Hồ Chí Minh"], c);
            // Thực hiện bước users.Add trong DatabaseCommands; tiếp tục theo luồng của hàm.
            users.Add(user.InsertId);
            // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            await db.Query("INSERT INTO TaiKhoanVaiTro(TaiKhoanID,VaiTroID) SELECT ?,VaiTroID FROM VaiTro WHERE MaVaiTro=?", [user.InsertId, role], c);
        }

        // Tạo biến categories để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var categories = new Dictionary<string, long>();
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        foreach (var(name, label)in new[]
        {
            ("Lifestyle", "Giày Lifestyle"),
            ("Running", "Giày chạy bộ"),
            ("Classic", "Giày Classic")
        }

        )
            // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            categories[name] = (await db.Query("INSERT INTO DanhMuc(TenDanhMuc) VALUES(?)", [label], c)).InsertId;
        // Tạo biến brands để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var brands = new Dictionary<string, long>();
        // Xử lý từng name trong tập dữ liệu đã lấy/kiểm tra.
        foreach (var name in new[]
        {
            "Nike",
            "Adidas",
            "New Balance",
            "Converse",
            "Vans"
        }

        )
            // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            brands[name] = (await db.Query("INSERT INTO ThuongHieu(TenThuongHieu) VALUES(?)", [name], c)).InsertId;
        // Tạo biến sizes để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var sizes = new List<long>();
        // Lặp theo chỉ số để xử lý đủ các phần tử, không bỏ sót dòng dữ liệu.
        for (var i = 36; i <= 44; i++)
            // Thực hiện bước sizes.Add trong DatabaseCommands; tiếp tục theo luồng của hàm.
            sizes.Add((await db.Query("INSERT INTO KichCo(HeSize,GiaTri,ThuTu) VALUES('EU',?,?)", [i.ToString(), i], c)).InsertId);
        // Tạo biến colors để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var colors = new Dictionary<string, long>();
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        foreach (var(code, name, hex)in new[]
        {
            ("red", "Đỏ", "#db3d32"),
            ("white", "Trắng", "#f3f1eb"),
            ("cream", "Kem", "#d8c7a9"),
            ("green", "Xanh lá", "#55796a"),
            ("gray", "Xám", "#939693"),
            ("black", "Đen", "#262626")
        }

        )
            // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            colors[code] = (await db.Query("INSERT INTO MauSac(TenMau,MaHex) VALUES(?,?)", [name, hex], c)).InsertId;
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        foreach (var(code, name)in new[]
        {
            ("COD", "Thanh toán khi nhận hàng"),
            ("BANK_TRANSFER", "Chuyển khoản ngân hàng"),
            ("ONLINE", "Thanh toán trực tuyến (chưa tích hợp)")
        }

        )
            // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            await db.Query("INSERT INTO PhuongThucThanhToan(MaPhuongThuc,TenPhuongThuc,DangHoatDong) VALUES(?,?,?)", [code, name, code == "ONLINE" ? 0 : 1], c);
        // Tạo biến products để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var products = new[]
        {
            ("Nike", "Air Max Pulse", "Lifestyle", 3290000, "red", "red"),
            ("Adidas", "Samba OG", "Lifestyle", 2700000, "white", "white"),
            ("New Balance", "550 Sea Salt", "Lifestyle", 2890000, "cream", "white"),
            ("Nike", "Air Force 1 Low", "Lifestyle", 2490000, "white", "white"),
            ("Converse", "Chuck 70 Hi", "Classic", 1850000, "cream", "cream"),
            ("Nike", "Pegasus 41", "Running", 3590000, "red", "red"),
            ("Adidas", "Gazelle Indoor", "Classic", 2590000, "green", "green"),
            ("New Balance", "2002R Protection", "Lifestyle", 3490000, "gray", "gray"),
            ("Vans", "Old Skool Classic", "Classic", 1650000, "black", "black"),
            ("Nike", "Dunk Low Retro", "Lifestyle", 2990000, "black", "black"),
            ("Adidas", "Ultraboost Light", "Running", 3990000, "white", "white"),
            ("Converse", "Run Star Hike", "Lifestyle", 2450000, "black", "black")
        };
        // Lặp theo chỉ số để xử lý đủ các phần tử, không bỏ sót dòng dữ liệu.
        for (var i = 0; i < products.Length; i++)
        {
            // Gán kết quả vào var(brand,name,category,price,image,color) để bước xử lý tiếp theo dùng dữ liệu mới.
            var(brand, name, category, price, image, color) = products[i];
            // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            var p = await db.Query("INSERT INTO SanPham(MaSanPham,TenSanPham,MoTa,GiaBan,DanhMucID,ThuongHieuID,TrangThai,NguoiTaoID) VALUES(?,?,?,?,?,?,'DANG_BAN',?)", [$"SS-{i + 1:000}", brand + " " + name, "Một thiết kế dành cho nhịp sống năng động. Phom dáng dễ phối, đệm êm và cảm giác thoải mái trong từng bước chân. Ảnh minh họa phong cách sản phẩm, không dùng thay thế ảnh hàng thực tế.", price, categories[category], brands[brand], users[1]], c);
            // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            await db.Query("INSERT INTO HinhAnhSanPham(SanPhamID,DuongDan,LaAnhChinh,MoTaAnh) VALUES(?,?,1,?)", [p.InsertId, $"/images/shoe-{image}.jpg", brand + " " + name], c);
            // Lặp theo chỉ số để xử lý đủ các phần tử, không bỏ sót dòng dữ liệu.
            for (var j = 0; j < sizes.Count; j++)
            {
                // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
                var v = await db.Query("INSERT INTO BienTheSanPham(SanPhamID,SKU,KichCoID,MauSacID) VALUES(?,?,?,?)", [p.InsertId, $"SS-{i + 1}-{36 + j}-{color}", sizes[j], colors[color]], c);
                // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
                await db.Query("INSERT INTO TonKho(BienTheID) VALUES(?)", [v.InsertId], c);
                // Tạo biến quantity để dùng ở các bước tiếp theo trong phạm vi hiện tại.
                var quantity = i == 3 && j < 4 ? 3 : 8 + (i + j) % 14;
                // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
                var receipt = await db.Query("INSERT INTO PhieuKho(MaPhieu,MaChongLap,LoaiPhieu,LyDo,NguoiThucHienID) VALUES(?,?,'NHAP','Nhập hàng mẫu ban đầu',?)", ["INIT-" + v.InsertId, Guid.NewGuid().ToString(), users[1]], c);
                // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
                var line = await db.Query("INSERT INTO ChiTietPhieuKho(PhieuKhoID,BienTheID,SoLuongThayDoi) VALUES(?,?,?)", [receipt.InsertId, v.InsertId, quantity], c);
                // Chuyển tới Operations.StockLog để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
                await operations.StockLog(c, users[1], v.InsertId, "NHAP", quantity, 0, null, line.InsertId, "Nhập hàng mẫu");
            }
        }

        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        foreach (var(code, name, type, value, max, min)in new (string, string, string, int, int? , int)[]
        {
            ("WELCOME10", "Chào bạn mới — giảm 10%", "PHAN_TRAM", 10, 300000, 1500000),
            ("SOLE200", "Bước mới, ưu đãi mới — giảm 200K", "SO_TIEN", 200000, null, 2500000)
        }

        )
        {
            // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            var a = await db.Query("INSERT INTO KhuyenMai(MaKhuyenMai,TenKhuyenMai,HinhThuc,GiaTri,GiamToiDa,GiaTriDonToiThieu,PhamVi,BatDauLuc,KetThucLuc,TrangThai,NguoiTaoID) VALUES(?,?,?,?,?,?,'TOAN_BO',DATE_SUB(UTC_TIMESTAMP(),INTERVAL 1 DAY),DATE_ADD(UTC_TIMESTAMP(),INTERVAL 180 DAY),'DANG_AP_DUNG',?)", [code, name, type, value, max, min, users[1]], c);
            // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            await db.Query("INSERT INTO LichSuKhuyenMai(KhuyenMaiID,NguoiThucHienID,HanhDong,NoiDung) VALUES(?,?,'TAO','Tạo chương trình mẫu')", [a.InsertId, users[1]], c);
        }

        // Bổ sung cùng 12 đơn và yêu cầu trả minh họa như bản Node.js để màn hình báo cáo có dữ liệu.
        await SeedOrders(db, operations, c, users);
        // Chuyển tới Operations.Audit để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
        await operations.Audit(users[2], "KHOI_TAO", "HeThong", "seed", "Khởi tạo 12 sản phẩm, 108 biến thể và ưu đãi mẫu", c);
        // Thực hiện bước Console.WriteLine trong DatabaseCommands; tiếp tục theo luồng của hàm.
        Console.WriteLine("Đã seed. Tài khoản: khach@sole.vn / shop@sole.vn / admin@sole.vn. Mật khẩu demo: SoleDemo@123");
        // Trả kết quả về hàm gọi; các lệnh sau return trong nhánh này không chạy.
        return true;
    });
    // Dữ liệu demo đơn hàng được ghi qua StockLog để sổ kho luôn đối chiếu được.
    private static async Task SeedOrders(Database db, Operations operations, DbSession c, List<long> users)
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        var variants = await db.Query("SELECT b.BienTheID id,b.SKU sku,s.MaSanPham code,s.TenSanPham name,s.GiaBan price,k.GiaTri size,m.TenMau color,d.TenDanhMuc category,t.TenThuongHieu brand FROM BienTheSanPham b JOIN SanPham s ON s.SanPhamID=b.SanPhamID JOIN KichCo k ON k.KichCoID=b.KichCoID JOIN MauSac m ON m.MauSacID=b.MauSacID JOIN DanhMuc d ON d.DanhMucID=s.DanhMucID JOIN ThuongHieu t ON t.ThuongHieuID=s.ThuongHieuID ORDER BY b.BienTheID", [], c);
        // Tạo biến statuses để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        string[] statuses = ["CHO_XAC_NHAN", "DA_XAC_NHAN", "DANG_CHUAN_BI", "DANG_GIAO", "DA_GIAO", "HOAN_THANH", "HOAN_THANH", "HOAN_THANH", "HOAN_THANH", "HOAN_THANH", "DA_HUY", "CHO_XAC_NHAN"];
        // Tạo biến orderIds để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var orderIds = new List<long>();
        // Lặp theo chỉ số để xử lý đủ các phần tử, không bỏ sót dòng dữ liệu.
        for (var i = 0; i < statuses.Length; i++)
        {
            // Tạo biến v để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var v = variants[i * 5];
            // Tạo biến status để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var status = statuses[i];
            // Tạo biến customer để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var customer = users[new[] { 0, 3, 4 }[i % 3]];
            // Tạo biến method để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var method = i % 2 == 0 ? 1 : 2;
            // Tạo biến shipped để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var shipped = status is "DANG_GIAO" or "DA_GIAO" or "HOAN_THANH";
            // Tạo biến paid để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var paid = status is "DA_GIAO" or "HOAN_THANH";
            // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            var order = await db.Query("INSERT INTO DonHang(MaDonHang,MaYeuCauTao,KhachHangID,HoTenNguoiNhan,SoDienThoaiNhan,DiaChiNhan,PhuongThucID,TrangThai,TienHang,NgayDat,NgayGiao,NgayHoanThanh,NgayHuy) VALUES(?,?,?,?,?,?,?,?,?,DATE_SUB(UTC_TIMESTAMP(),INTERVAL ? DAY),?,?,?)", [$"SS-2026-{1001 + i}", Guid.NewGuid().ToString(), customer, new[] { "Nguyễn Minh Anh", "Trần Hoàng Linh", "Lâm Tuấn Kiệt" }[i % 3], "0901234567", "273 An Dương Vương, TP. Hồ Chí Minh", method, status, v.N("price"), i % 10, paid ? DateTime.UtcNow : null, status == "HOAN_THANH" ? DateTime.UtcNow : null, status == "DA_HUY" ? DateTime.UtcNow : null ], c);
            // Thực hiện bước orderIds.Add trong DatabaseCommands; tiếp tục theo luồng của hàm.
            orderIds.Add(order.InsertId);
            // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            var line = await db.Query("INSERT INTO ChiTietDonHang(DonHangID,BienTheID,MaSanPhamLucMua,SKULucMua,TenSanPhamLucMua,KichCoLucMua,MauSacLucMua,DanhMucLucMua,ThuongHieuLucMua,SoLuong,DonGia) VALUES(?,?,?,?,?,?,?,?,?,1,?)", [order.InsertId, v.L("id"), v.S("code"), v.S("sku"), v.S("name"), v.S("size"), v.S("color"), v.S("category"), v.S("brand"), v.N("price")], c);
            // Tạo biến allocation để dùng ở các bước tiếp theo trong phạm vi hiện tại.
            var allocation = status == "DA_HUY" ? "DA_GIAI_PHONG" : shipped ? "DA_XUAT" : "DANG_GIU";
            // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            await db.Query("INSERT INTO PhanBoTonKho(ChiTietDonHangID,SoLuongGiu,TrangThai,NgayKetThuc) VALUES(?,1,?,?)", [line.InsertId, allocation, allocation == "DANG_GIU" ? null : DateTime.UtcNow], c);
            // Chuyển tới Operations.StockLog để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
            await operations.StockLog(c, customer, v.L("id"), "GIU_HANG", 0, 1, line.InsertId, null, "Đặt hàng mẫu");
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (shipped || status == "DA_HUY")
                // Chuyển tới Operations.StockLog để cập nhật dữ liệu và lịch sử cùng giao dịch hiện tại.
                await operations.StockLog(c, users[1], v.L("id"), shipped ? "BAN_HANG" : "GIAI_PHONG", shipped ? -1 : 0, -1, line.InsertId, null, shipped ? "Xuất đơn mẫu" : "Hủy đơn mẫu");
            // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            await db.Query("INSERT INTO LichSuDonHang(DonHangID,TrangThaiMoi,NguoiThucHienID,GhiChu) VALUES(?,?,?,'Dữ liệu đơn mẫu')", [order.InsertId, status, users[1]], c);
            // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
            await db.Query("INSERT INTO ThanhToan(DonHangID,PhuongThucID,MaYeuCau,SoTien,TrangThai,NgayXacNhan,NguoiXacNhanID,BangChung) VALUES(?,?,?,?,?,?,?,?)", [order.InsertId, method, Guid.NewGuid().ToString(), v.N("price"), paid ? "DA_XAC_NHAN" : "CHO_XAC_NHAN", paid ? DateTime.UtcNow.AddDays(-(i % 7)) : null, paid ? users[1] : null, paid ? "Giao dịch mẫu, không phải giao dịch thực" : null ], c);
            // Chọn nhánh xử lý theo điều kiện hiện tại; đọc các lệnh trong nhánh để biết kết quả.
            if (status == "HOAN_THANH")
                // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
                await db.Query("INSERT INTO DanhGia(DonHangID,ChiTietDonHangID,KhachHangID,SoSao,NoiDung) VALUES(?,?,?,?,?)", [order.InsertId, line.InsertId, customer, 5, "Giày êm, form vừa chân, đóng gói rất cẩn thận. Sẽ quay lại!"], c);
        }

        // Tạo biến completed để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var completed = orderIds[6];
        // Tạo biến item để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var item = (await db.Query("SELECT * FROM ChiTietDonHang WHERE DonHangID=?", [completed], c)).One();
        // Tạo biến owner để dùng ở các bước tiếp theo trong phạm vi hiện tại.
        var owner = (await db.Query("SELECT KhachHangID FROM DonHang WHERE DonHangID=?", [completed], c)).One().L("KhachHangID");
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        var ret = await db.Query("INSERT INTO YeuCauDoiTra(MaYeuCau,MaChongLap,DonHangID,KhachHangID,LoaiYeuCau,LyDo) VALUES(?,?,?,?,'TRA','Muốn đổi sang size rộng hơn, đề nghị trả và đặt lại')", ["DT-MAU-001", Guid.NewGuid().ToString(), completed, owner], c);
        // Database.Query → thêm bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        await db.Query("INSERT INTO ChiTietDoiTra(YeuCauDoiTraID,DonHangID,ChiTietDonHangID,SoLuongYeuCau) VALUES(?,?,?,1)", [ret.InsertId, completed, item.L("ChiTietDonHangID")], c);
    }
}
