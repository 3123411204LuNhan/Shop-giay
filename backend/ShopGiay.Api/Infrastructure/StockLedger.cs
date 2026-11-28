using ShopGiay.Entity;

namespace ShopGiay.Infrastructure;

public static class StockLedger
{
    public static async Task Change(SqlSession db,long? actor,long variant,string type,int physical,int reserved,long? detail,long? receipt,string reason)
    {
        var row = await db.One("SELECT SoLuongTon,SoLuongDaPhanBo FROM TonKho WHERE BienTheID=? FOR UPDATE",variant);
        if (row is null) throw new ApiException("Không tìm thấy tồn kho.",404);
        var stock = (int)row.SoLuongTon + physical;
        var held = (int)row.SoLuongDaPhanBo + reserved;
        if (held < 0 || stock < held) throw new ApiException("Không đủ tồn khả dụng hoặc số phân bổ không hợp lệ.");
        var balance = new TonKho(stock,held);
        await db.Execute("UPDATE TonKho SET SoLuongTon=?,SoLuongDaPhanBo=?,NgayCapNhat=UTC_TIMESTAMP(),PhienBan=PhienBan+1 WHERE BienTheID=?",
            balance.SoLuongTon,balance.SoLuongDaPhanBo,variant);
        await db.Execute("""
            INSERT INTO BienDongKho(BienTheID,LoaiBienDong,ThayDoiTon,ThayDoiPhanBo,TonSau,PhanBoSau,ChiTietDonHangID,ChiTietPhieuKhoID,NguoiThucHienID,MaThaoTac,LyDo)
            VALUES(?,?,?,?,?,?,?,?,?,?,?)
            """,variant,type,physical,reserved,stock,held,detail,receipt,actor,Guid.NewGuid().ToString(),reason);
    }

    public static async Task ReserveRefunds(SqlSession db,long order,string reason,long? request = null)
    {
        var remaining = request is null ? decimal.MaxValue :
            await db.Value<decimal>("SELECT COALESCE(SUM(TienHoanDuocDuyet),0) FROM ChiTietDoiTra WHERE YeuCauDoiTraID=?",request) -
            await db.Value<decimal>("SELECT COALESCE(SUM(SoTien),0) FROM HoanTien WHERE YeuCauDoiTraID=? AND TrangThai<>'DA_HUY'",request);
        var payments = await db.Rows("""
            SELECT t.ThanhToanID,t.SoTien,COALESCE((SELECT SUM(h.SoTien) FROM HoanTien h
            WHERE h.ThanhToanID=t.ThanhToanID AND h.TrangThai<>'DA_HUY'),0) reserved
            FROM ThanhToan t WHERE t.DonHangID=? AND t.TrangThai='DA_XAC_NHAN' ORDER BY t.ThanhToanID FOR UPDATE
            """,order);
        foreach (var p in payments) {
            var amount = Math.Min(remaining,(decimal)p.SoTien - (decimal)p.reserved);
            if (amount <= 0) continue;
            await db.Execute("INSERT INTO HoanTien(DonHangID,ThanhToanID,YeuCauDoiTraID,MaYeuCau,LyDo,SoTien) VALUES(?,?,?,?,?,?)",
                order,(long)p.ThanhToanID,request,Guid.NewGuid().ToString(),reason,amount);
            remaining -= amount;
        }
    }
}
