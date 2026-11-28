namespace ShopGiay.Infrastructure;

public static class StockLedger
{
    // Khóa số dư, áp dụng quy tắc của TonKho, lưu giá trị mới rồi ghi một dòng sổ kho.
    public static async Task Change(SqlSession db,long? actor,long variant,string type,int physical,int reserved,long? detail,long? receipt,string reason)
    {
        var balance = await InventoryRepository.KhoaTon(db,variant) ?? throw new ApiException("Không tìm thấy tồn kho.",404);
        if (!balance.ApDungBienDong(physical,reserved)) throw new ApiException("Không đủ tồn khả dụng hoặc số phân bổ không hợp lệ.");
        await InventoryRepository.LuuTon(db,variant,balance);
        await db.Execute("""
            INSERT INTO BienDongKho(BienTheID,LoaiBienDong,ThayDoiTon,ThayDoiPhanBo,TonSau,PhanBoSau,ChiTietDonHangID,ChiTietPhieuKhoID,NguoiThucHienID,MaThaoTac,LyDo)
            VALUES(?,?,?,?,?,?,?,?,?,?,?)
            """,variant,type,physical,reserved,balance.SoLuongTon,balance.SoLuongDaPhanBo,detail,receipt,actor,Guid.NewGuid().ToString(),reason);
    }

    // Trừ các khoản đã dành để hoàn rồi tạo khoản hoàn còn thiếu từ giao dịch đã thu.
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
