using System.Text.RegularExpressions;
using MySqlConnector;

namespace ShopGiay.Infrastructure;

internal static class DatabaseMaintenance
{
    public static async Task Run(string command,MySqlConnectionStringBuilder settings)
    {
        var database = settings.Database;
        if (!Regex.IsMatch(database,@"\A[a-zA-Z0-9_]{1,64}\z"))
            throw new InvalidOperationException("DB_NAME phải có từ 1 đến 64 ký tự chữ, số hoặc dấu gạch dưới.");

        // Closing this dedicated connection also releases its MySQL advisory lock.
        settings.Database = "";
        settings.Pooling = false;
        await using var connection = new MySqlConnection(settings.ConnectionString);
        await connection.OpenAsync();
        var db = new SqlSession(connection,null,default);
        await db.Execute("SET time_zone = '+00:00'");
        if (await db.Value<long?>("SELECT GET_LOCK(SHA2(CONCAT('shop-giay:db:',LOWER(?)),256),10)",database) != 1)
            throw new InvalidOperationException("Database đang chạy một lệnh bảo trì khác. Hãy thử lại sau.");

        if (command == "--db-setup")
            await db.Execute($"CREATE DATABASE IF NOT EXISTS `{database}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci");
        await connection.ChangeDatabaseAsync(database);

        switch (command) {
            case "--db-setup":
                if (await db.Value<long>("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema=DATABASE()") != 0)
                    throw new InvalidOperationException("Database đã có bảng. Lệnh db:setup không ghi đè.");
                using (var schema = typeof(DatabaseMaintenance).Assembly.GetManifestResourceStream("ShopGiay.Schema.sql")
                    ?? throw new InvalidOperationException("Không tìm thấy database/schema.sql."))
                using (var reader = new StreamReader(schema))
                await db.Execute(await reader.ReadToEndAsync());
                Console.WriteLine($"Đã tạo đủ 37 bảng MySQL trong {database}.");
                break;
            case "--db-migrate":
                await Migrate(db);
                Console.WriteLine("Schema kho, vận chuyển và OTP đã sẵn sàng. Dữ liệu hiện có được giữ lại.");
                break;
            case "--db-seed":
                await using (var transaction = await connection.BeginTransactionAsync()) {
                    await DatabaseSeed.Run(new SqlSession(connection,transaction,default));
                    await transaction.CommitAsync();
                }
                Console.WriteLine("Đã tạo 12 sản phẩm, 108 biến thể, 12 đơn cùng dữ liệu mẫu.");
                Console.WriteLine("Tài khoản local: khach@sole.vn / shop@sole.vn / admin@sole.vn. Mật khẩu mẫu: SoleDemo@123");
                break;
        }
    }

    private static Task<string?> NullableColumn(SqlSession db,string table,string column) => db.Value<string>(
        "SELECT IS_NULLABLE FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name=? AND column_name=?",table,column);

    private static async Task Migrate(SqlSession db)
    {
        foreach (var table in new[] { "LichSuDonHang","BienDongKho" })
            if (await NullableColumn(db,table,"NguoiThucHienID") == "NO")
                await db.Execute($"ALTER TABLE {table} MODIFY COLUMN NguoiThucHienID bigint NULL");

        foreach (var table in new[] { "DanhMuc","ThuongHieu" }) {
            var nullable = await NullableColumn(db,table,"LaBanNhap");
            if (nullable == "NO") continue;
            // NULL identifies rows awaiting backfill if the command stops after ADD COLUMN.
            if (nullable is null) await db.Execute($"ALTER TABLE {table} ADD COLUMN LaBanNhap tinyint NULL");
            await db.Execute($"UPDATE {table} SET LaBanNhap=(DangHoatDong=0) WHERE LaBanNhap IS NULL");
            await db.Execute($"ALTER TABLE {table} MODIFY COLUMN LaBanNhap tinyint NOT NULL DEFAULT 0");
        }

        (string Table,string Column,string Definition)[] columns = [
            ("TaiKhoan","NgayXoa","datetime NULL"),
            ("DonHang","DonViVanChuyen","varchar(150) NULL"),
            ("DonHang","MaVanDon","varchar(100) NULL"),
            ("DonHang","TrangThaiGiaoHang","varchar(20) NOT NULL DEFAULT 'CHUA_GIAO'"),
            ("ChiTietPhieuKho","TonSoSach","int NULL"),
            ("PhieuKho","TrangThai","varchar(20) NOT NULL DEFAULT 'DA_XAC_NHAN'"),
            ("PhieuKho","NhaCungCap","varchar(200) NULL"),
            ("PhieuKho","NgayXacNhan","datetime NULL"),
            ("PhieuKho","NguoiXacNhanID","bigint NULL"),
            ("PhieuKho","DonHangID","bigint NULL"),
            ("DatLaiMatKhau","MaXacMinhHash","binary(32) NULL AFTER TokenHash")
        ];
        foreach (var (table,column,definition) in columns)
            if (await NullableColumn(db,table,column) is null)
                await db.Execute($"ALTER TABLE {table} ADD COLUMN {column} {definition}");

        await db.Execute("UPDATE TaiKhoan SET TrangThai='BI_KHOA' WHERE TrangThai='VO_HIEU_HOA'");
        await db.Execute("UPDATE SanPham SET TrangThai='AN' WHERE TrangThai='NHAP'");
        if (await db.Value<string>("SELECT COLUMN_DEFAULT FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name='SanPham' AND column_name='TrangThai'") != "AN")
            await db.Execute("ALTER TABLE SanPham ALTER COLUMN TrangThai SET DEFAULT 'AN'");

        (string Table,string Name,string Clause)[] constraints = [
            ("SanPham","CK_SanPham_TrangThai","CHECK (TrangThai IN ('DANG_BAN','AN','NGUNG_BAN'))"),
            ("TaiKhoan","CK_TaiKhoan_TrangThai","CHECK (TrangThai IN ('HOAT_DONG','BI_KHOA'))"),
            ("PhieuKho","CK_PhieuKho_TrangThai","CHECK (TrangThai IN ('NHAP','DA_XAC_NHAN','DA_HUY'))"),
            ("DonHang","CK_DonHang_GiaoHang","CHECK (TrangThaiGiaoHang IN ('CHUA_GIAO','DANG_GIAO','GIAO_THAT_BAI','CHO_HOAN_VE','DA_NHAN_HOAN','DA_GIAO'))"),
            ("PhieuKho","FK_PhieuKho_XacNhan","FOREIGN KEY (NguoiXacNhanID) REFERENCES TaiKhoan(TaiKhoanID)"),
            ("PhieuKho","FK_PhieuKho_DonHang","FOREIGN KEY (DonHangID) REFERENCES DonHang(DonHangID)")
        ];
        foreach (var (table,name,clause) in constraints)
            if (await db.Value<long>("SELECT COUNT(*) FROM information_schema.table_constraints WHERE constraint_schema=DATABASE() AND table_name=? AND constraint_name=?",table,name) == 0)
                await db.Execute($"ALTER TABLE {table} ADD CONSTRAINT {name} {clause}");
    }
}
