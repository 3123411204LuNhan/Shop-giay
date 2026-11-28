using ShopGiay.Entity;

namespace ShopGiay.Infrastructure;

public sealed class InventoryRepository
{
    // Khóa phiếu trước khi kiểm tra trạng thái hoặc ghi sổ.
    public Task<PhieuKho?> KhoaPhieu(SqlSession db,long id) =>
        db.One<PhieuKho>(InventoryQueries.LockReceipt,id);

    // Đọc các dòng theo mã biến thể để các giao dịch khóa tồn cùng thứ tự.
    public Task<List<ChiTietPhieuKho>> TaiDongPhieu(SqlSession db,long id) =>
        db.Rows<ChiTietPhieuKho>(InventoryQueries.LockLines,id);

    // Lưu trạng thái cùng dấu thời gian và người xác nhận từ Entity.
    public Task LuuTrangThai(SqlSession db,PhieuKho phieu) =>
        db.Execute("UPDATE PhieuKho SET TrangThai=?,NgayXacNhan=?,NguoiXacNhanID=? WHERE PhieuKhoID=?",
            phieu.TrangThai,phieu.NgayXacNhan,phieu.NguoiXacNhanID,phieu.PhieuKhoID);

    // Khóa số dư biến thể rồi dựng đối tượng để tính thay đổi tồn.
    public static Task<TonKho?> KhoaTon(SqlSession db,long variant) =>
        db.One<TonKho>("SELECT SoLuongTon ton,SoLuongDaPhanBo daPhanBo FROM TonKho WHERE BienTheID=? FOR UPDATE",variant);

    // Ghi lại tồn, lượng giữ và phiên bản sau khi Entity chấp nhận thay đổi.
    public static Task LuuTon(SqlSession db,long variant,TonKho ton) =>
        db.Execute("UPDATE TonKho SET SoLuongTon=?,SoLuongDaPhanBo=?,NgayCapNhat=UTC_TIMESTAMP(),PhienBan=PhienBan+1 WHERE BienTheID=?",
            ton.SoLuongTon,ton.SoLuongDaPhanBo,variant);
}
