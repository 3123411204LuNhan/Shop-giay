namespace ShopGiay.Infrastructure;

internal static class PromotionQueries
{
    private const string publicPromotionCondition = """
        k.TrangThai='DANG_AP_DUNG' AND k.BatDauLuc<=UTC_TIMESTAMP() AND k.KetThucLuc>UTC_TIMESTAMP() AND (k.GioiHanLuotDung IS NULL OR (SELECT COUNT(*) FROM KhuyenMaiDonHang kd JOIN DonHang d ON d.DonHangID=kd.DonHangID WHERE kd.KhuyenMaiID=k.KhuyenMaiID AND d.TrangThai<>'DA_HUY')<k.GioiHanLuotDung)
        """;

    internal const string Published = """
        SELECT k.MaKhuyenMai code,k.TenKhuyenMai name,k.HinhThuc type,k.GiaTri value,k.GiaTriDonToiThieu minimum,IF(k.HinhThuc='PHAN_TRAM',k.GiamToiDa,NULL) maximum,k.PhamVi scope,k.BatDauLuc start,k.KetThucLuc end,k.GioiHanMoiKhach perCustomer FROM KhuyenMai k WHERE 
        """ + publicPromotionCondition + """
         ORDER BY k.KetThucLuc,k.KhuyenMaiID
        """;

    internal const string List = """
        SELECT KhuyenMaiID id,MaKhuyenMai code,TenKhuyenMai name,HinhThuc type,GiaTri value,GiaTriDonToiThieu minimum,IF(HinhThuc='PHAN_TRAM',GiamToiDa,NULL) maximum,TrangThai status,BatDauLuc start,KetThucLuc end,(
        """ + publicPromotionCondition + """
        ) visible FROM KhuyenMai k ORDER BY KhuyenMaiID DESC
        """;
}
