using ShopGiay.Web.Infrastructure;

namespace ShopGiay.Web.Repositories;
// Lớp truy cập MySQL cho Promotion; Service gọi lớp này trong cùng giao dịch.
public sealed class PromotionRepository(Database db) : IPromotionRepository
{
    private const string publicPromotionCondition = @"k.TrangThai='DANG_AP_DUNG' AND k.BatDauLuc<=UTC_TIMESTAMP() AND k.KetThucLuc>UTC_TIMESTAMP() AND (k.GioiHanLuotDung IS NULL OR (SELECT COUNT(*) FROM KhuyenMaiDonHang kd JOIN DonHang d ON d.DonHangID=kd.DonHangID WHERE kd.KhuyenMaiID=k.KhuyenMaiID AND d.TrangThai<>'DA_HUY')<k.GioiHanLuotDung)";
    // Published: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> Published()
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT k.MaKhuyenMai code,k.TenKhuyenMai name,k.HinhThuc type,k.GiaTri value,k.GiaTriDonToiThieu minimum,IF(k.HinhThuc='PHAN_TRAM',k.GiamToiDa,NULL) maximum,k.PhamVi scope,k.BatDauLuc start,k.KetThucLuc end,k.GioiHanMoiKhach perCustomer FROM KhuyenMai k WHERE " + publicPromotionCondition + @" ORDER BY k.KetThucLuc,k.KhuyenMaiID");
    }

    // List: Service → câu SQL có tham số → MySQL; connection giữ transaction hiện hành.
    public Task<DbResult> List()
    {
        // Database.Query → đọc/khóa bản ghi trong MySQL bằng tham số, dùng kết nối giao dịch nếu có.
        return db.Query(@"SELECT KhuyenMaiID id,MaKhuyenMai code,TenKhuyenMai name,HinhThuc type,GiaTri value,GiaTriDonToiThieu minimum,IF(HinhThuc='PHAN_TRAM',GiamToiDa,NULL) maximum,TrangThai status,BatDauLuc start,KetThucLuc end,(" + publicPromotionCondition + @") visible FROM KhuyenMai k ORDER BY KhuyenMaiID DESC");
    }
}
