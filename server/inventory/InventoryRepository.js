import { query } from '../db.js';

// InventoryService dùng các truy vấn này; connection giữ các bước ghi trong cùng giao dịch.
export class InventoryRepository {
  stock(productId) {
    return query(
      `SELECT b.BienTheID id,b.SanPhamID productId,b.KichCoID sizeId,b.MauSacID colorId,b.SKU sku,
        s.MaSanPham productCode,s.TenSanPham name,kc.GiaTri size,m.TenMau color,b.DangKinhDoanh active,
        COALESCE(k.SoLuongTon,0) stock,COALESCE(k.SoLuongDaPhanBo,0) reserved,
        COALESCE(k.SoLuongKhaDung,0) available,COALESCE(k.MucCanhBao,0) warning
        FROM BienTheSanPham b JOIN SanPham s ON s.SanPhamID=b.SanPhamID
        JOIN KichCo kc ON kc.KichCoID=b.KichCoID JOIN MauSac m ON m.MauSacID=b.MauSacID
        LEFT JOIN TonKho k ON k.BienTheID=b.BienTheID
        ${productId == null ? '' : 'WHERE b.SanPhamID=?'} ORDER BY s.SanPhamID,b.BienTheID`,
      productId==null?[]:[productId],
    );
  }

  receipts() {
    return query(
      `SELECT p.PhieuKhoID id,p.MaPhieu code,p.LoaiPhieu type,p.TrangThai status,p.NhaCungCap supplier,p.MaChungTuThamChieu reference,p.LyDo reason,p.NgayTao date,p.NgayXacNhan confirmedAt,t.HoTen creator,COUNT(c.ChiTietPhieuKhoID) lineCount,COALESCE(SUM(c.SoLuongThayDoi*COALESCE(c.DonGiaNhap,0)),0) cost FROM PhieuKho p JOIN TaiKhoan t ON t.TaiKhoanID=p.NguoiThucHienID LEFT JOIN ChiTietPhieuKho c ON c.PhieuKhoID=p.PhieuKhoID GROUP BY p.PhieuKhoID ORDER BY p.PhieuKhoID DESC LIMIT 500`,
    );
  }

  receipt(values) {
    return query(
      'SELECT PhieuKhoID id,MaPhieu code,LoaiPhieu type,TrangThai status,NhaCungCap supplier,MaChungTuThamChieu reference,LyDo reason,NgayTao date,NgayXacNhan confirmedAt FROM PhieuKho WHERE PhieuKhoID=?',
      values,
    );
  }

  lines(values) {
    return query(
      'SELECT c.BienTheID variantId,c.SoLuongThayDoi quantity,c.TonSoSach baseline,(c.TonSoSach+c.SoLuongThayDoi) counted,c.DonGiaNhap cost,b.SKU sku,s.TenSanPham name,k.GiaTri size,m.TenMau color FROM ChiTietPhieuKho c JOIN BienTheSanPham b ON b.BienTheID=c.BienTheID JOIN SanPham s ON s.SanPhamID=b.SanPhamID JOIN KichCo k ON k.KichCoID=b.KichCoID JOIN MauSac m ON m.MauSacID=b.MauSacID WHERE c.PhieuKhoID=?',
      values,
    );
  }

  lockActor(values, connection) {
    return query('SELECT TaiKhoanID FROM TaiKhoan WHERE TaiKhoanID=? FOR UPDATE', values, connection);
  }

  findByKey(values, connection) {
    return query(
      'SELECT PhieuKhoID id,NguoiThucHienID owner FROM PhieuKho WHERE MaChongLap=?',
      values,
      connection,
    );
  }

  lockReceipt(values, connection) {
    return query('SELECT * FROM PhieuKho WHERE PhieuKhoID=? FOR UPDATE', values, connection);
  }

  lockLines(values, connection) {
    return query(
      'SELECT * FROM ChiTietPhieuKho WHERE PhieuKhoID=? ORDER BY BienTheID',
      values,
      connection,
    );
  }

  lockStock(values,connection) {
    return query('SELECT SoLuongTon stock FROM TonKho WHERE BienTheID=? FOR UPDATE',values,connection);
  }

  confirmReceipt(values, connection) {
    return query(
      "UPDATE PhieuKho SET TrangThai='DA_XAC_NHAN',NgayXacNhan=UTC_TIMESTAMP(),NguoiXacNhanID=? WHERE PhieuKhoID=?",
      values,
      connection,
    );
  }

  cancelReceipt(values, connection) {
    return query("UPDATE PhieuKho SET TrangThai='DA_HUY' WHERE PhieuKhoID=?", values, connection);
  }

  setThreshold(values, connection) {
    return query('UPDATE TonKho SET MucCanhBao=? WHERE BienTheID=?', values, connection);
  }

  history(values) {
    return query(
      `SELECT b.BienDongID id,b.LoaiBienDong type,b.ThayDoiTon quantity,b.ThayDoiPhanBo reservedChange,b.TonSau stock,b.PhanBoSau reserved,b.LyDo reason,b.ThoiDiem date,v.SKU sku,t.HoTen actor FROM BienDongKho b JOIN BienTheSanPham v ON v.BienTheID=b.BienTheID LEFT JOIN TaiKhoan t ON t.TaiKhoanID=b.NguoiThucHienID ${values.length ? 'WHERE b.BienTheID=?' : ''} ORDER BY b.BienDongID DESC LIMIT 300`,
      values,
    );
  }
}
