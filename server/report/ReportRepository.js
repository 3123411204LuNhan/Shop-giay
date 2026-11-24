import { query } from '../db.js';

// Dùng cùng connection để các phần của báo cáo đọc một ảnh chụp dữ liệu nhất quán.
export class ReportRepository {
  customers() {
    return query(
      "SELECT t.TaiKhoanID id,t.HoTen name,t.Email email,t.SoDienThoai phone,COUNT(d.DonHangID) orders,COALESCE(SUM(d.TongThanhToan),0) total FROM TaiKhoan t JOIN TaiKhoanVaiTro tv ON tv.TaiKhoanID=t.TaiKhoanID JOIN VaiTro v ON v.VaiTroID=tv.VaiTroID AND v.MaVaiTro='CUSTOMER' LEFT JOIN DonHang d ON d.KhachHangID=t.TaiKhoanID WHERE t.NgayXoa IS NULL GROUP BY t.TaiKhoanID ORDER BY t.TaiKhoanID",
    );
  }

  cashByDay([start,end],connection) {
    return query(
      `SELECT DATE_FORMAT(CONVERT_TZ(confirmedAt,'+00:00','+07:00'),'%Y-%m-%d') day,
        SUM(income) income,SUM(refund) refund,SUM(income)-SUM(refund) revenue
        FROM (
          SELECT NgayXacNhan confirmedAt,SoTien income,0 refund FROM ThanhToan
            WHERE TrangThai='DA_XAC_NHAN' AND NgayXacNhan>=? AND NgayXacNhan<?
          UNION ALL
          SELECT NgayXacNhan,0,SoTien FROM HoanTien
            WHERE TrangThai='DA_XAC_NHAN' AND NgayXacNhan>=? AND NgayXacNhan<?
        ) cash GROUP BY day ORDER BY day`,
      [start,end,start,end],connection,
    );
  }

  orderCounts(values,connection) {
    return query(
      'SELECT TrangThai status,COUNT(*) count FROM DonHang WHERE NgayDat>=? AND NgayDat<? GROUP BY TrangThai',
      values,connection,
    );
  }

  bestSellers([start,end],group,rank,connection) {
    const variant=group==='variant';
    const id=variant?'b.BienTheID':'p.SanPhamID';
    const code=variant?'b.SKU':'p.MaSanPham';
    const label=variant?"CONCAT(p.TenSanPham,' · EU ',k.GiaTri,' · ',m.TenMau)":"p.TenSanPham";
    const order=rank==='amount'?'amount DESC, quantity DESC':'quantity DESC, amount DESC';
    // Mục 10.4: số bán theo đơn hoàn tất đã thu đủ, điều chỉnh theo hàng trả và khoản hoàn đến cuối kỳ.
    // Tính tiền hoàn trước khi gộp dòng để nhiều khoản hoàn không nhân số lượng hàng trả.
    return query(
      `WITH sales AS (
        SELECT ct.ChiTietDonHangID,ct.BienTheID,ct.SoLuong,ct.ThanhTien
        FROM DonHang d JOIN ChiTietDonHang ct ON ct.DonHangID=d.DonHangID
        WHERE d.TrangThai='HOAN_THANH' AND d.NgayHoanThanh>=? AND d.NgayHoanThanh<?
          AND COALESCE((SELECT SUM(t.SoTien) FROM ThanhToan t
            WHERE t.DonHangID=d.DonHangID AND t.TrangThai='DA_XAC_NHAN' AND t.NgayXacNhan<?),0)>=d.TongThanhToan
          AND NOT EXISTS (SELECT 1 FROM YeuCauDoiTra y WHERE y.DonHangDoiMoiID=d.DonHangID)
      ), returned AS (
        SELECT c.ChiTietDonHangID,c.YeuCauDoiTraID,c.TienHoanDuocDuyet,
          IF(c.NgayNhanHang<?,c.SoLuongDaNhan,0) received,
          SUM(c.TienHoanDuocDuyet) OVER (PARTITION BY c.YeuCauDoiTraID) approved,
          SUM(c.TienHoanDuocDuyet) OVER (PARTITION BY c.YeuCauDoiTraID
            ORDER BY c.ChiTietDoiTraID ROWS UNBOUNDED PRECEDING) allocated
        FROM ChiTietDoiTra c JOIN sales s ON s.ChiTietDonHangID=c.ChiTietDonHangID
        JOIN YeuCauDoiTra y ON y.YeuCauDoiTraID=c.YeuCauDoiTraID
        WHERE y.LoaiYeuCau='TRA' AND y.TrangThai IN ('DANG_XU_LY','HOAN_TAT')
      ), refunds AS (
        SELECT h.YeuCauDoiTraID,SUM(h.SoTien) amount
        FROM HoanTien h JOIN (SELECT DISTINCT YeuCauDoiTraID FROM returned) requests USING (YeuCauDoiTraID)
        WHERE h.TrangThai='DA_XAC_NHAN' AND h.NgayXacNhan<? GROUP BY h.YeuCauDoiTraID
      ), reductions AS (
        SELECT r.ChiTietDonHangID,SUM(r.received) quantity,
          SUM(COALESCE(
            ROUND(LEAST(f.amount,r.approved)*r.allocated/NULLIF(r.approved,0),2)
            - ROUND(LEAST(f.amount,r.approved)*(r.allocated-r.TienHoanDuocDuyet)/NULLIF(r.approved,0),2),0)) amount
        FROM returned r LEFT JOIN refunds f USING (YeuCauDoiTraID) GROUP BY r.ChiTietDonHangID
      )
      SELECT ${id} id,${code} code,${label} name,
        SUM(GREATEST(0,s.SoLuong-COALESCE(r.quantity,0))) quantity,
        SUM(GREATEST(0,s.ThanhTien-COALESCE(r.amount,0))) amount
      FROM sales s JOIN BienTheSanPham b ON b.BienTheID=s.BienTheID
      JOIN SanPham p ON p.SanPhamID=b.SanPhamID
      ${variant?'JOIN KichCo k ON k.KichCoID=b.KichCoID JOIN MauSac m ON m.MauSacID=b.MauSacID':''}
      LEFT JOIN reductions r ON r.ChiTietDonHangID=s.ChiTietDonHangID
      GROUP BY ${id},${code},${label}
      HAVING quantity>0 OR amount>0
      ORDER BY ${order},id`,
      [start,end,end,end,end],connection,
    );
  }

  lowStock(connection) {
    return query('SELECT COUNT(*) n FROM TonKho WHERE SoLuongKhaDung<=MucCanhBao',[],connection);
  }

  logs() {
    return query(
      'SELECT n.NhatKyID id,t.HoTen name,n.HanhDong action,n.DoiTuong object,n.MaDoiTuong objectId,n.NoiDung content,n.KetQua result,n.ThoiDiem date FROM NhatKyHeThong n LEFT JOIN TaiKhoan t ON t.TaiKhoanID=n.NguoiThucHienID ORDER BY n.NhatKyID DESC LIMIT 200',
    );
  }

  databaseInfo() {
    return query('SELECT DATABASE() name,VERSION() version');
  }

  tableCount() {
    return query('SELECT COUNT(*) n FROM information_schema.tables WHERE table_schema=DATABASE()');
  }

  activeSessions() {
    return query(
      'SELECT COUNT(*) n FROM PhienDangNhap WHERE ThuHoiLuc IS NULL AND HetHanLuc>UTC_TIMESTAMP()',
    );
  }

  accountCount() {
    return query('SELECT COUNT(*) n FROM TaiKhoan WHERE NgayXoa IS NULL');
  }
}
