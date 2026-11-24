import { query } from '../db.js';

const productSelect = `SELECT s.SanPhamID id,s.MaSanPham code,s.TenSanPham name,s.MoTa description,s.GiaBan price,s.DanhMucID categoryId,s.ThuongHieuID brandId,s.TrangThai status,
 d.TenDanhMuc category,t.TenThuongHieu brand,(SELECT DuongDan FROM HinhAnhSanPham h WHERE h.SanPhamID=s.SanPhamID ORDER BY LaAnhChinh DESC,ThuTu,HinhAnhID LIMIT 1) image,
 COALESCE((SELECT SUM(k.SoLuongKhaDung) FROM BienTheSanPham b JOIN TonKho k ON k.BienTheID=b.BienTheID WHERE b.SanPhamID=s.SanPhamID AND b.DangKinhDoanh=1),0) stock
 FROM SanPham s JOIN DanhMuc d ON d.DanhMucID=s.DanhMucID JOIN ThuongHieu t ON t.ThuongHieuID=s.ThuongHieuID`;
// CatalogService dùng các truy vấn này; connection giữ các bước ghi trong cùng giao dịch.
export class CatalogRepository {
  lockHiddenItem(flag, table, col, values, connection) {
    return query(`SELECT (${flag}) draft FROM ${table} WHERE ${col}=? FOR UPDATE`, values, connection);
  }

  productStock(values, connection) {
    return query(
      'SELECT k.SoLuongTon,k.SoLuongDaPhanBo FROM BienTheSanPham b JOIN TonKho k ON k.BienTheID=b.BienTheID WHERE b.SanPhamID=? FOR UPDATE',
      values,
      connection,
    );
  }

  productImages(values,connection) {
    return query('SELECT DuongDan url FROM HinhAnhSanPham WHERE SanPhamID=?',values,connection);
  }

  deleteImages(values, connection) {
    return query('DELETE FROM HinhAnhSanPham WHERE SanPhamID=?', values, connection);
  }

  deleteStock(values, connection) {
    return query(
      'DELETE k FROM TonKho k JOIN BienTheSanPham b ON b.BienTheID=k.BienTheID WHERE b.SanPhamID=?',
      values,
      connection,
    );
  }

  deleteVariants(values, connection) {
    return query('DELETE FROM BienTheSanPham WHERE SanPhamID=?', values, connection);
  }

  removeItem(table, col, values, connection) {
    return query(`DELETE FROM ${table} WHERE ${col}=?`, values, connection);
  }

  browse(filters, order, values) {
    return query(productSelect + ' WHERE ' + filters.join(' AND ') + ' ORDER BY ' + order, values);
  }

  categories() {
    return query('SELECT DanhMucID id,TenDanhMuc name FROM DanhMuc WHERE DangHoatDong=1');
  }

  brands() {
    return query('SELECT ThuongHieuID id,TenThuongHieu name FROM ThuongHieu WHERE DangHoatDong=1');
  }

  sizes() {
    return query(
      'SELECT KichCoID id,GiaTri name FROM KichCo ORDER BY ThuTu,CAST(GiaTri AS DECIMAL(8,2))',
    );
  }

  colors() {
    return query('SELECT MauSacID id,TenMau name FROM MauSac WHERE DangHoatDong=1');
  }

  publicProduct(values) {
    return query(
      productSelect +
        " WHERE s.SanPhamID=? AND s.TrangThai='DANG_BAN' AND d.DangHoatDong=1 AND t.DangHoatDong=1",
      values,
    );
  }

  availableVariants(values) {
    return query(
      'SELECT b.BienTheID id,b.SKU sku,kc.GiaTri size,m.TenMau color,m.MaHex hex,COALESCE(b.GiaBanRieng,?) price,COALESCE(k.SoLuongKhaDung,0) stock FROM BienTheSanPham b JOIN KichCo kc ON kc.KichCoID=b.KichCoID JOIN MauSac m ON m.MauSacID=b.MauSacID LEFT JOIN TonKho k ON k.BienTheID=b.BienTheID WHERE b.SanPhamID=? AND b.DangKinhDoanh=1 ORDER BY kc.ThuTu',
      values,
    );
  }

  reviews(values) {
    return query(
      "SELECT g.SoSao stars,g.NoiDung content,t.HoTen name,g.NgayTao date FROM DanhGia g JOIN TaiKhoan t ON t.TaiKhoanID=g.KhachHangID JOIN ChiTietDonHang ct ON ct.ChiTietDonHangID=g.ChiTietDonHangID JOIN BienTheSanPham b ON b.BienTheID=ct.BienTheID WHERE b.SanPhamID=? AND g.TrangThai='HIEN_THI' ORDER BY g.NgayTao DESC",
      values,
    );
  }

  products() {
    return query(productSelect + ' ORDER BY s.SanPhamID DESC');
  }

  addSize(values, connection) {
    return query(
      "INSERT INTO KichCo(HeSize,GiaTri,ThuTu) VALUES('EU',?,?) ON DUPLICATE KEY UPDATE DangHoatDong=1",
      values,
      connection,
    );
  }

  sizeByValue(values, connection) {
    return query(
      "SELECT KichCoID id,GiaTri name FROM KichCo WHERE HeSize='EU' AND GiaTri=?",
      values,
      connection,
    );
  }

  addColor(values, connection) {
    return query(
      'INSERT INTO MauSac(TenMau,MaHex) VALUES(?,?) ON DUPLICATE KEY UPDATE DangHoatDong=1',
      values,
      connection,
    );
  }

  colorByName(values, connection) {
    return query('SELECT MauSacID id,TenMau name FROM MauSac WHERE TenMau=?', values, connection);
  }

  allCategories() {
    return query('SELECT DanhMucID id,TenDanhMuc name,DangHoatDong active,LaBanNhap draft FROM DanhMuc');
  }

  allBrands() {
    return query(
      'SELECT ThuongHieuID id,TenThuongHieu name,DangHoatDong active,LaBanNhap draft FROM ThuongHieu',
    );
  }

  allColors() {
    return query('SELECT MauSacID id,TenMau name FROM MauSac');
  }

  insertProduct(values, connection) {
    return query(
      'INSERT INTO SanPham(MaSanPham,TenSanPham,MoTa,GiaBan,DanhMucID,ThuongHieuID,TrangThai,NguoiTaoID) VALUES(?,?,?,?,?,?,?,?)',
      values,
      connection,
    );
  }

  insertImage(values, connection) {
    return query(
      'INSERT INTO HinhAnhSanPham(SanPhamID,DuongDan,LaAnhChinh) VALUES(?,?,1)',
      values,
      connection,
    );
  }

  lockProduct(values, connection) {
    return query(
      'SELECT MaSanPham code,TenSanPham name,MoTa description,GiaBan price,DanhMucID categoryId,ThuongHieuID brandId,TrangThai status FROM SanPham WHERE SanPhamID=? FOR UPDATE',
      values,
      connection,
    );
  }

  lockImage(values, connection) {
    return query(
      'SELECT HinhAnhID id,DuongDan image,BienTheID variant FROM HinhAnhSanPham WHERE SanPhamID=? ORDER BY LaAnhChinh DESC,ThuTu,HinhAnhID LIMIT 1 FOR UPDATE',
      values,
      connection,
    );
  }

  updateProduct(values, connection) {
    return query(
      'UPDATE SanPham SET MaSanPham=?,TenSanPham=?,MoTa=?,GiaBan=?,DanhMucID=?,ThuongHieuID=? WHERE SanPhamID=?',
      values,
      connection,
    );
  }

  updateImage(values, connection) {
    return query(
      'UPDATE HinhAnhSanPham SET DuongDan=?,LaAnhChinh=1 WHERE HinhAnhID=?',
      values,
      connection,
    );
  }

  removeImage(values, connection) {
    return query('DELETE FROM HinhAnhSanPham WHERE HinhAnhID=?', values, connection);
  }

  clearPrimaryImage(values, connection) {
    return query(
      'UPDATE HinhAnhSanPham SET LaAnhChinh=0 WHERE SanPhamID=? AND LaAnhChinh=1',
      values,
      connection,
    );
  }

  setProductStatus(values, connection) {
    return query(
      'UPDATE SanPham SET TrangThai=?,NgayCapNhat=UTC_TIMESTAMP(),PhienBan=PhienBan+1 WHERE SanPhamID=?',
      values,
      connection,
    );
  }

  variants(productId) {
    return query(
      `SELECT b.BienTheID id,b.SanPhamID productId,b.KichCoID sizeId,b.MauSacID colorId,b.GiaBanRieng price,b.SKU sku,s.MaSanPham productCode,s.TenSanPham name,kc.GiaTri size,m.TenMau color,b.DangKinhDoanh active FROM BienTheSanPham b JOIN SanPham s ON s.SanPhamID=b.SanPhamID JOIN KichCo kc ON kc.KichCoID=b.KichCoID JOIN MauSac m ON m.MauSacID=b.MauSacID  ${productId == null ? '' : 'WHERE b.SanPhamID=?'} ORDER BY s.SanPhamID,b.BienTheID`,
      productId == null ? [] : [productId],
    );
  }

  insertVariant(values, connection) {
    return query(
      'INSERT INTO BienTheSanPham(SanPhamID,SKU,KichCoID,MauSacID,GiaBanRieng) VALUES(?,?,?,?,?)',
      values,
      connection,
    );
  }

  createStock(values, connection) {
    return query('INSERT INTO TonKho(BienTheID) VALUES(?)', values, connection);
  }

  lockVariant(values, connection) {
    return query('SELECT * FROM BienTheSanPham WHERE BienTheID=? FOR UPDATE', values, connection);
  }

  countVariantOrders(values, connection) {
    return query('SELECT COUNT(*) n FROM ChiTietDonHang WHERE BienTheID=?', values, connection);
  }

  updateVariant(values, connection) {
    return query(
      'UPDATE BienTheSanPham SET KichCoID=?,MauSacID=?,GiaBanRieng=? WHERE BienTheID=?',
      values,
      connection,
    );
  }

  setVariantStatus(values, connection) {
    return query(
      'UPDATE BienTheSanPham SET DangKinhDoanh=?,NgayCapNhat=UTC_TIMESTAMP() WHERE BienTheID=?',
      values,
      connection,
    );
  }

  lockVariantIdentity(values, connection) {
    return query(
      'SELECT SKU,DangKinhDoanh FROM BienTheSanPham WHERE BienTheID=? FOR UPDATE',
      values,
      connection,
    );
  }

  lockStock(values, connection) {
    return query(
      'SELECT SoLuongTon,SoLuongDaPhanBo FROM TonKho WHERE BienTheID=? FOR UPDATE',
      values,
      connection,
    );
  }

  removeStock(values, connection) {
    return query('DELETE FROM TonKho WHERE BienTheID=?', values, connection);
  }

  removeVariant(values, connection) {
    return query('DELETE FROM BienTheSanPham WHERE BienTheID=?', values, connection);
  }

  createTaxonomy(table, values) {
    return query(
      `INSERT INTO ${table}(Ten${table},MoTa,DangHoatDong,LaBanNhap) VALUES(?,?,?,?)`,
      values,
    );
  }

  updateTaxonomy(table, col, values) {
    return query(
      `UPDATE ${table} SET Ten${table}=?,DangHoatDong=?,LaBanNhap=?,NgayCapNhat=UTC_TIMESTAMP() WHERE ${col}=?`,
      values,
    );
  }
}
