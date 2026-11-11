-- MySQL 8.0.16+ conversion of all 37 core tables from 01_QuanLyBanGiay.sql.
-- No DROP statements. Use a new dedicated schema. Store dates as UTC.

CREATE TABLE IF NOT EXISTS VaiTro (
  VaiTroID int NOT NULL AUTO_INCREMENT PRIMARY KEY,
  MaVaiTro varchar(30) NOT NULL UNIQUE,
  TenVaiTro varchar(100) NOT NULL,
  MoTa varchar(500) NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS TaiKhoan (
  TaiKhoanID bigint NOT NULL AUTO_INCREMENT PRIMARY KEY,
  TenDangNhap varchar(100) NOT NULL UNIQUE,
  Email varchar(254) NOT NULL UNIQUE,
  SoDienThoai varchar(20) NULL,
  MatKhauHash varchar(512) NOT NULL,
  HoTen varchar(150) NOT NULL,
  NgaySinh date NULL,
  GioiTinh varchar(20) NULL,
  DiaChi varchar(500) NULL,
  AnhDaiDien varchar(1000) NULL,
  TrangThai varchar(20) NOT NULL DEFAULT 'HOAT_DONG' CHECK (TrangThai IN ('HOAT_DONG','BI_KHOA','VO_HIEU_HOA')),
  EmailDaXacMinh tinyint NOT NULL DEFAULT 0,
  SoLanDangNhapSai int NOT NULL DEFAULT 0 CHECK (SoLanDangNhapSai >= 0),
  KhoaDenLuc datetime NULL,
  LanDangNhapCuoi datetime NULL,
  NgayDoiMatKhau datetime NULL,
  NgayTao datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  NgayCapNhat datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PhienBan bigint NOT NULL DEFAULT 1
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS TaiKhoanVaiTro (
  TaiKhoanID bigint NOT NULL,
  VaiTroID int NOT NULL,
  NguoiGanID bigint NULL,
  NgayGan datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (TaiKhoanID,VaiTroID),
  FOREIGN KEY (TaiKhoanID) REFERENCES TaiKhoan(TaiKhoanID),
  FOREIGN KEY (VaiTroID) REFERENCES VaiTro(VaiTroID),
  FOREIGN KEY (NguoiGanID) REFERENCES TaiKhoan(TaiKhoanID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS PhienDangNhap (
  PhienID char(36) NOT NULL DEFAULT (UUID()) PRIMARY KEY,
  TaiKhoanID bigint NOT NULL,
  TokenHash binary(32) NOT NULL UNIQUE,
  NgayTao datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  HetHanLuc datetime NOT NULL,
  ThuHoiLuc datetime NULL,
  DiaChiIP varchar(45) NULL,
  UserAgent varchar(500) NULL,
  CHECK (HetHanLuc > NgayTao),
  FOREIGN KEY (TaiKhoanID) REFERENCES TaiKhoan(TaiKhoanID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS DatLaiMatKhau (
  YeuCauID bigint NOT NULL AUTO_INCREMENT PRIMARY KEY,
  TaiKhoanID bigint NOT NULL,
  TokenHash binary(32) NOT NULL UNIQUE,
  NgayTao datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  HetHanLuc datetime NOT NULL,
  DaSuDungLuc datetime NULL,
  SoLanThu int NOT NULL DEFAULT 0 CHECK (SoLanThu >= 0),
  CHECK (HetHanLuc > NgayTao),
  FOREIGN KEY (TaiKhoanID) REFERENCES TaiKhoan(TaiKhoanID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS DanhMuc (
  DanhMucID int NOT NULL AUTO_INCREMENT PRIMARY KEY,
  TenDanhMuc varchar(150) NOT NULL UNIQUE,
  MoTa varchar(1000) NULL,
  HinhAnh varchar(1000) NULL,
  DangHoatDong tinyint NOT NULL DEFAULT 1,
  NgayTao datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  NgayCapNhat datetime NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ThuongHieu (
  ThuongHieuID int NOT NULL AUTO_INCREMENT PRIMARY KEY,
  TenThuongHieu varchar(150) NOT NULL UNIQUE,
  MoTa varchar(1000) NULL,
  HinhAnh varchar(1000) NULL,
  DangHoatDong tinyint NOT NULL DEFAULT 1,
  NgayTao datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  NgayCapNhat datetime NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS SanPham (
  SanPhamID bigint NOT NULL AUTO_INCREMENT PRIMARY KEY,
  MaSanPham varchar(50) NOT NULL UNIQUE,
  TenSanPham varchar(250) NOT NULL,
  MoTa LONGTEXT NULL,
  GiaBan decimal(18,2) NOT NULL CHECK (GiaBan > 0),
  DanhMucID int NOT NULL,
  ThuongHieuID int NOT NULL,
  TrangThai varchar(20) NOT NULL DEFAULT 'NHAP' CHECK (TrangThai IN ('NHAP','DANG_BAN','AN','NGUNG_BAN')),
  NguoiTaoID bigint NULL,
  NgayTao datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  NgayCapNhat datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PhienBan bigint NOT NULL DEFAULT 1,
  FOREIGN KEY (DanhMucID) REFERENCES DanhMuc(DanhMucID),
  FOREIGN KEY (ThuongHieuID) REFERENCES ThuongHieu(ThuongHieuID),
  FOREIGN KEY (NguoiTaoID) REFERENCES TaiKhoan(TaiKhoanID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS KichCo (
  KichCoID int NOT NULL AUTO_INCREMENT PRIMARY KEY,
  HeSize varchar(10) NOT NULL DEFAULT 'EU',
  GiaTri varchar(20) NOT NULL,
  ThuTu int NOT NULL DEFAULT 0,
  DangHoatDong tinyint NOT NULL DEFAULT 1,
  UNIQUE (HeSize,GiaTri)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS MauSac (
  MauSacID int NOT NULL AUTO_INCREMENT PRIMARY KEY,
  TenMau varchar(100) NOT NULL UNIQUE,
  MaHex char(7) NULL,
  DangHoatDong tinyint NOT NULL DEFAULT 1
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS BienTheSanPham (
  BienTheID bigint NOT NULL AUTO_INCREMENT PRIMARY KEY,
  SanPhamID bigint NOT NULL,
  SKU varchar(80) NOT NULL UNIQUE,
  KichCoID int NOT NULL,
  MauSacID int NOT NULL,
  GiaBanRieng decimal(18,2) NULL CHECK (GiaBanRieng > 0),
  DangKinhDoanh tinyint NOT NULL DEFAULT 1,
  NgayTao datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  NgayCapNhat datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PhienBan bigint NOT NULL DEFAULT 1,
  UNIQUE (SanPhamID,KichCoID,MauSacID),
  UNIQUE (SanPhamID,BienTheID),
  FOREIGN KEY (SanPhamID) REFERENCES SanPham(SanPhamID),
  FOREIGN KEY (KichCoID) REFERENCES KichCo(KichCoID),
  FOREIGN KEY (MauSacID) REFERENCES MauSac(MauSacID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS HinhAnhSanPham (
  HinhAnhID bigint NOT NULL AUTO_INCREMENT PRIMARY KEY,
  SanPhamID bigint NOT NULL,
  BienTheID bigint NULL,
  DuongDan varchar(1000) NOT NULL,
  MoTaAnh varchar(250) NULL,
  LaAnhChinh tinyint NOT NULL DEFAULT 0,
  ThuTu int NOT NULL DEFAULT 0,
  FOREIGN KEY (SanPhamID,BienTheID) REFERENCES BienTheSanPham(SanPhamID,BienTheID),
  FOREIGN KEY (SanPhamID) REFERENCES SanPham(SanPhamID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS TonKho (
  BienTheID bigint NOT NULL PRIMARY KEY,
  SoLuongTon int NOT NULL DEFAULT 0,
  SoLuongDaPhanBo int NOT NULL DEFAULT 0,
  SoLuongKhaDung int GENERATED ALWAYS AS (SoLuongTon - SoLuongDaPhanBo) STORED,
  MucCanhBao int NOT NULL DEFAULT 5 CHECK (MucCanhBao >= 0),
  NgayCapNhat datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PhienBan bigint NOT NULL DEFAULT 1,
  CHECK (SoLuongTon >= 0 AND SoLuongDaPhanBo >= 0 AND SoLuongDaPhanBo <= SoLuongTon),
  FOREIGN KEY (BienTheID) REFERENCES BienTheSanPham(BienTheID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS GioHang (
  GioHangID bigint NOT NULL AUTO_INCREMENT PRIMARY KEY,
  TaiKhoanID bigint NULL,
  MaPhienHash binary(32) NULL,
  NgayTao datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  NgayCapNhat datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  HetHanLuc datetime NULL,
  CHECK ((TaiKhoanID IS NOT NULL AND MaPhienHash IS NULL) OR (TaiKhoanID IS NULL AND MaPhienHash IS NOT NULL)),
  FOREIGN KEY (TaiKhoanID) REFERENCES TaiKhoan(TaiKhoanID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ChiTietGioHang (
  GioHangID bigint NOT NULL,
  BienTheID bigint NOT NULL,
  SoLuong int NOT NULL CHECK (SoLuong > 0),
  NgayThem datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  NgayCapNhat datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (GioHangID,BienTheID),
  FOREIGN KEY (GioHangID) REFERENCES GioHang(GioHangID),
  FOREIGN KEY (BienTheID) REFERENCES BienTheSanPham(BienTheID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS PhuongThucThanhToan (
  PhuongThucID int NOT NULL AUTO_INCREMENT PRIMARY KEY,
  MaPhuongThuc varchar(30) NOT NULL UNIQUE,
  TenPhuongThuc varchar(100) NOT NULL,
  DangHoatDong tinyint NOT NULL DEFAULT 1
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS DonHang (
  DonHangID bigint NOT NULL AUTO_INCREMENT PRIMARY KEY,
  MaDonHang varchar(50) NOT NULL UNIQUE,
  MaYeuCauTao char(36) NOT NULL UNIQUE,
  KhachHangID bigint NOT NULL,
  HoTenNguoiNhan varchar(150) NOT NULL,
  SoDienThoaiNhan varchar(20) NOT NULL,
  EmailNhan varchar(254) NULL,
  DiaChiNhan varchar(500) NOT NULL,
  PhuongXa varchar(150) NULL,
  TinhThanh varchar(150) NULL,
  GhiChu varchar(1000) NULL,
  PhuongThucID int NOT NULL,
  TrangThai varchar(25) NOT NULL DEFAULT 'CHO_XAC_NHAN' CHECK (TrangThai IN ('CHO_XAC_NHAN','DA_XAC_NHAN','DANG_CHUAN_BI','DANG_GIAO','DA_GIAO','HOAN_THANH','DA_HUY')),
  TienHang decimal(18,2) NOT NULL CHECK (TienHang >= 0),
  TienGiam decimal(18,2) NOT NULL DEFAULT 0,
  PhiVanChuyen decimal(18,2) NOT NULL DEFAULT 0 CHECK (PhiVanChuyen >= 0),
  TongThanhToan decimal(20,2) GENERATED ALWAYS AS (TienHang - TienGiam + PhiVanChuyen) STORED,
  TienTe char(3) NOT NULL DEFAULT 'VND' CHECK (TienTe = 'VND'),
  NgayDat datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  HanThanhToan datetime NULL,
  NgayGiao datetime NULL,
  NgayHoanThanh datetime NULL,
  NgayHuy datetime NULL,
  LyDoHuy varchar(1000) NULL,
  NguoiHuyID bigint NULL,
  PhienBan bigint NOT NULL DEFAULT 1,
  CHECK (TienGiam >= 0 AND TienGiam <= TienHang),
  UNIQUE (DonHangID,KhachHangID),
  CHECK (TrangThai <> 'DA_HUY' OR NgayHuy IS NOT NULL),
  FOREIGN KEY (KhachHangID) REFERENCES TaiKhoan(TaiKhoanID),
  FOREIGN KEY (PhuongThucID) REFERENCES PhuongThucThanhToan(PhuongThucID),
  FOREIGN KEY (NguoiHuyID) REFERENCES TaiKhoan(TaiKhoanID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ChiTietDonHang (
  ChiTietDonHangID bigint NOT NULL AUTO_INCREMENT PRIMARY KEY,
  DonHangID bigint NOT NULL,
  BienTheID bigint NOT NULL,
  MaSanPhamLucMua varchar(50) NOT NULL,
  SKULucMua varchar(80) NOT NULL,
  TenSanPhamLucMua varchar(250) NOT NULL,
  KichCoLucMua varchar(30) NOT NULL,
  MauSacLucMua varchar(100) NOT NULL,
  DanhMucLucMua varchar(150) NOT NULL,
  ThuongHieuLucMua varchar(150) NOT NULL,
  SoLuong int NOT NULL CHECK (SoLuong > 0),
  DonGia decimal(18,2) NOT NULL CHECK (DonGia > 0),
  TienGiamPhanBo decimal(18,2) NOT NULL DEFAULT 0,
  ThanhTien decimal(18,2) GENERATED ALWAYS AS (CAST(SoLuong * DonGia - TienGiamPhanBo AS decimal(18,2))) STORED,
  UNIQUE (DonHangID,BienTheID),
  UNIQUE (DonHangID,ChiTietDonHangID),
  CHECK (TienGiamPhanBo >= 0 AND TienGiamPhanBo <= SoLuong * DonGia),
  FOREIGN KEY (DonHangID) REFERENCES DonHang(DonHangID),
  FOREIGN KEY (BienTheID) REFERENCES BienTheSanPham(BienTheID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS LichSuDonHang (
  LichSuID bigint NOT NULL AUTO_INCREMENT PRIMARY KEY,
  DonHangID bigint NOT NULL,
  TrangThaiCu varchar(25) NULL,
  TrangThaiMoi varchar(25) NOT NULL,
  NguoiThucHienID bigint NULL,
  ThoiDiem datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  GhiChu varchar(1000) NULL,
  FOREIGN KEY (DonHangID) REFERENCES DonHang(DonHangID),
  FOREIGN KEY (NguoiThucHienID) REFERENCES TaiKhoan(TaiKhoanID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS KhuyenMai (
  KhuyenMaiID bigint NOT NULL AUTO_INCREMENT PRIMARY KEY,
  MaKhuyenMai varchar(50) NOT NULL UNIQUE,
  TenKhuyenMai varchar(200) NOT NULL,
  MoTa varchar(1000) NULL,
  HinhThuc varchar(15) NOT NULL CHECK (HinhThuc IN ('PHAN_TRAM','SO_TIEN')),
  GiaTri decimal(18,2) NOT NULL,
  GiamToiDa decimal(18,2) NULL CHECK (GiamToiDa >= 0),
  GiaTriDonToiThieu decimal(18,2) NOT NULL DEFAULT 0 CHECK (GiaTriDonToiThieu >= 0),
  PhamVi varchar(20) NOT NULL CHECK (PhamVi IN ('TOAN_BO','SAN_PHAM','DANH_MUC','THUONG_HIEU')),
  BatDauLuc datetime NOT NULL,
  KetThucLuc datetime NOT NULL,
  TrangThai varchar(20) NOT NULL DEFAULT 'NHAP' CHECK (TrangThai IN ('NHAP','DANG_AP_DUNG','KET_THUC')),
  ChoPhepKetHop tinyint NOT NULL DEFAULT 0,
  DoUuTien int NOT NULL DEFAULT 0,
  GioiHanLuotDung int NULL CHECK (GioiHanLuotDung > 0),
  GioiHanMoiKhach int NULL CHECK (GioiHanMoiKhach > 0),
  NguoiTaoID bigint NOT NULL,
  NgayTao datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  NgayCapNhat datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PhienBan bigint NOT NULL DEFAULT 1,
  CHECK (KetThucLuc > BatDauLuc),
  CHECK (GiaTri > 0 AND (HinhThuc <> 'PHAN_TRAM' OR GiaTri <= 100)),
  FOREIGN KEY (NguoiTaoID) REFERENCES TaiKhoan(TaiKhoanID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS KhuyenMaiSanPham (
  KhuyenMaiID bigint NOT NULL,
  SanPhamID bigint NOT NULL,
  PRIMARY KEY (KhuyenMaiID,SanPhamID),
  FOREIGN KEY (KhuyenMaiID) REFERENCES KhuyenMai(KhuyenMaiID),
  FOREIGN KEY (SanPhamID) REFERENCES SanPham(SanPhamID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS KhuyenMaiDanhMuc (
  KhuyenMaiID bigint NOT NULL,
  DanhMucID int NOT NULL,
  PRIMARY KEY (KhuyenMaiID,DanhMucID),
  FOREIGN KEY (KhuyenMaiID) REFERENCES KhuyenMai(KhuyenMaiID),
  FOREIGN KEY (DanhMucID) REFERENCES DanhMuc(DanhMucID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS KhuyenMaiThuongHieu (
  KhuyenMaiID bigint NOT NULL,
  ThuongHieuID int NOT NULL,
  PRIMARY KEY (KhuyenMaiID,ThuongHieuID),
  FOREIGN KEY (KhuyenMaiID) REFERENCES KhuyenMai(KhuyenMaiID),
  FOREIGN KEY (ThuongHieuID) REFERENCES ThuongHieu(ThuongHieuID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS KhuyenMaiDonHang (
  DonHangID bigint NOT NULL,
  KhuyenMaiID bigint NOT NULL,
  MaLucApDung varchar(50) NOT NULL,
  TenLucApDung varchar(200) NOT NULL,
  HinhThucLucApDung varchar(15) NOT NULL,
  GiaTriLucApDung decimal(18,2) NOT NULL,
  TienGiamThucTe decimal(18,2) NOT NULL CHECK (TienGiamThucTe >= 0),
  NgayApDung datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (DonHangID,KhuyenMaiID),
  FOREIGN KEY (DonHangID) REFERENCES DonHang(DonHangID),
  FOREIGN KEY (KhuyenMaiID) REFERENCES KhuyenMai(KhuyenMaiID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS LichSuKhuyenMai (
  LichSuID bigint NOT NULL AUTO_INCREMENT PRIMARY KEY,
  KhuyenMaiID bigint NOT NULL,
  NguoiThucHienID bigint NOT NULL,
  HanhDong varchar(30) NOT NULL,
  NoiDung LONGTEXT NOT NULL,
  ThoiDiem datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (KhuyenMaiID) REFERENCES KhuyenMai(KhuyenMaiID),
  FOREIGN KEY (NguoiThucHienID) REFERENCES TaiKhoan(TaiKhoanID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ThanhToan (
  ThanhToanID bigint NOT NULL AUTO_INCREMENT PRIMARY KEY,
  DonHangID bigint NOT NULL,
  PhuongThucID int NOT NULL,
  MaYeuCau char(36) NOT NULL UNIQUE,
  CongThanhToan varchar(50) NULL,
  MaGiaoDich varchar(150) NULL,
  SoTien decimal(18,2) NOT NULL CHECK (SoTien > 0),
  TrangThai varchar(20) NOT NULL DEFAULT 'CHO_XAC_NHAN' CHECK (TrangThai IN ('CHO_XAC_NHAN','DA_XAC_NHAN','THAT_BAI','DA_HUY','HET_HAN')),
  NgayTao datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  NgayXacNhan datetime NULL,
  NguoiXacNhanID bigint NULL,
  BangChung varchar(1000) NULL,
  ThongBaoLoi varchar(1000) NULL,
  PhienBan bigint NOT NULL DEFAULT 1,
  UNIQUE (DonHangID,ThanhToanID),
  CHECK (TrangThai <> 'DA_XAC_NHAN' OR NgayXacNhan IS NOT NULL),
  CHECK (MaGiaoDich IS NULL OR CongThanhToan IS NOT NULL),
  FOREIGN KEY (DonHangID) REFERENCES DonHang(DonHangID),
  FOREIGN KEY (PhuongThucID) REFERENCES PhuongThucThanhToan(PhuongThucID),
  FOREIGN KEY (NguoiXacNhanID) REFERENCES TaiKhoan(TaiKhoanID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS YeuCauDoiTra (
  YeuCauDoiTraID bigint NOT NULL AUTO_INCREMENT PRIMARY KEY,
  MaYeuCau varchar(50) NOT NULL UNIQUE,
  MaChongLap char(36) NOT NULL UNIQUE,
  DonHangID bigint NOT NULL,
  KhachHangID bigint NOT NULL,
  LoaiYeuCau varchar(10) NOT NULL CHECK (LoaiYeuCau IN ('DOI','TRA')),
  LyDo varchar(1000) NOT NULL,
  TrangThai varchar(20) NOT NULL DEFAULT 'CHO_DUYET' CHECK (TrangThai IN ('CHO_DUYET','DA_DUYET','TU_CHOI','DANG_XU_LY','HOAN_TAT','DA_HUY')),
  NguoiDuyetID bigint NULL,
  NgayTao datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  NgayDuyet datetime NULL,
  LyDoXuLy varchar(1000) NULL,
  NgayHoanTat datetime NULL,
  DonHangDoiMoiID bigint NULL,
  PhienBan bigint NOT NULL DEFAULT 1,
  FOREIGN KEY (DonHangID,KhachHangID) REFERENCES DonHang(DonHangID,KhachHangID),
  UNIQUE (DonHangID,YeuCauDoiTraID),
  CHECK (DonHangDoiMoiID IS NULL OR DonHangDoiMoiID <> DonHangID),
  FOREIGN KEY (NguoiDuyetID) REFERENCES TaiKhoan(TaiKhoanID),
  FOREIGN KEY (DonHangDoiMoiID) REFERENCES DonHang(DonHangID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ChiTietDoiTra (
  ChiTietDoiTraID bigint NOT NULL AUTO_INCREMENT PRIMARY KEY,
  YeuCauDoiTraID bigint NOT NULL,
  DonHangID bigint NOT NULL,
  ChiTietDonHangID bigint NOT NULL,
  SoLuongYeuCau int NOT NULL CHECK (SoLuongYeuCau > 0),
  SoLuongDuyet int NOT NULL DEFAULT 0,
  SoLuongDaNhan int NOT NULL DEFAULT 0,
  SoLuongBanLai int NOT NULL DEFAULT 0,
  TienHoanDuocDuyet decimal(18,2) NOT NULL DEFAULT 0 CHECK (TienHoanDuocDuyet >= 0),
  BienTheDoiSangID bigint NULL,
  TinhTrangHang varchar(1000) NULL,
  NguoiNhanHangID bigint NULL,
  NgayNhanHang datetime NULL,
  FOREIGN KEY (DonHangID,YeuCauDoiTraID) REFERENCES YeuCauDoiTra(DonHangID,YeuCauDoiTraID),
  FOREIGN KEY (DonHangID,ChiTietDonHangID) REFERENCES ChiTietDonHang(DonHangID,ChiTietDonHangID),
  UNIQUE (YeuCauDoiTraID,ChiTietDonHangID),
  CHECK (SoLuongDuyet BETWEEN 0 AND SoLuongYeuCau AND SoLuongDaNhan BETWEEN 0 AND SoLuongDuyet AND SoLuongBanLai BETWEEN 0 AND SoLuongDaNhan),
  FOREIGN KEY (BienTheDoiSangID) REFERENCES BienTheSanPham(BienTheID),
  FOREIGN KEY (NguoiNhanHangID) REFERENCES TaiKhoan(TaiKhoanID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS HinhAnhDoiTra (
  HinhAnhID bigint NOT NULL AUTO_INCREMENT PRIMARY KEY,
  YeuCauDoiTraID bigint NOT NULL,
  DuongDan varchar(1000) NOT NULL,
  MoTa varchar(500) NULL,
  NgayTao datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (YeuCauDoiTraID) REFERENCES YeuCauDoiTra(YeuCauDoiTraID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS LichSuDoiTra (
  LichSuID bigint NOT NULL AUTO_INCREMENT PRIMARY KEY,
  YeuCauDoiTraID bigint NOT NULL,
  TrangThaiCu varchar(20) NULL,
  TrangThaiMoi varchar(20) NOT NULL,
  NguoiThucHienID bigint NOT NULL,
  NoiDung varchar(1000) NOT NULL,
  ThoiDiem datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (YeuCauDoiTraID) REFERENCES YeuCauDoiTra(YeuCauDoiTraID),
  FOREIGN KEY (NguoiThucHienID) REFERENCES TaiKhoan(TaiKhoanID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS HoanTien (
  HoanTienID bigint NOT NULL AUTO_INCREMENT PRIMARY KEY,
  DonHangID bigint NOT NULL,
  ThanhToanID bigint NOT NULL,
  YeuCauDoiTraID bigint NULL,
  MaYeuCau char(36) NOT NULL UNIQUE,
  LyDo varchar(1000) NOT NULL,
  SoTien decimal(18,2) NOT NULL CHECK (SoTien > 0),
  TrangThai varchar(20) NOT NULL DEFAULT 'CHO_XU_LY' CHECK (TrangThai IN ('CHO_XU_LY','DANG_XU_LY','DA_XAC_NHAN','THAT_BAI','DA_HUY')),
  KenhHoanTien varchar(50) NULL,
  MaGiaoDichHoan varchar(150) NULL,
  BangChung varchar(1000) NULL,
  NguoiXuLyID bigint NULL,
  NgayTao datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  NgayXacNhan datetime NULL,
  PhienBan bigint NOT NULL DEFAULT 1,
  FOREIGN KEY (DonHangID,ThanhToanID) REFERENCES ThanhToan(DonHangID,ThanhToanID),
  FOREIGN KEY (DonHangID,YeuCauDoiTraID) REFERENCES YeuCauDoiTra(DonHangID,YeuCauDoiTraID),
  CHECK (TrangThai <> 'DA_XAC_NHAN' OR (NgayXacNhan IS NOT NULL AND BangChung IS NOT NULL AND NguoiXuLyID IS NOT NULL)),
  CHECK (MaGiaoDichHoan IS NULL OR KenhHoanTien IS NOT NULL),
  FOREIGN KEY (NguoiXuLyID) REFERENCES TaiKhoan(TaiKhoanID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS PhieuKho (
  PhieuKhoID bigint NOT NULL AUTO_INCREMENT PRIMARY KEY,
  MaPhieu varchar(50) NOT NULL UNIQUE,
  MaChongLap char(36) NOT NULL UNIQUE,
  LoaiPhieu varchar(20) NOT NULL CHECK (LoaiPhieu IN ('NHAP','DIEU_CHINH','NHAN_TRA')),
  MaChungTuThamChieu varchar(100) NULL,
  LyDo varchar(1000) NOT NULL,
  NguoiThucHienID bigint NOT NULL,
  NgayTao datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (NguoiThucHienID) REFERENCES TaiKhoan(TaiKhoanID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ChiTietPhieuKho (
  ChiTietPhieuKhoID bigint NOT NULL AUTO_INCREMENT PRIMARY KEY,
  PhieuKhoID bigint NOT NULL,
  BienTheID bigint NOT NULL,
  SoLuongThayDoi int NOT NULL CHECK (SoLuongThayDoi <> 0),
  DonGiaNhap decimal(18,2) NULL CHECK (DonGiaNhap >= 0),
  ChiTietDoiTraID bigint NULL,
  GhiChu varchar(1000) NULL,
  UNIQUE (PhieuKhoID,BienTheID),
  FOREIGN KEY (PhieuKhoID) REFERENCES PhieuKho(PhieuKhoID),
  FOREIGN KEY (BienTheID) REFERENCES BienTheSanPham(BienTheID),
  FOREIGN KEY (ChiTietDoiTraID) REFERENCES ChiTietDoiTra(ChiTietDoiTraID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS PhanBoTonKho (
  ChiTietDonHangID bigint NOT NULL PRIMARY KEY,
  SoLuongGiu int NOT NULL CHECK (SoLuongGiu > 0),
  TrangThai varchar(20) NOT NULL DEFAULT 'DANG_GIU' CHECK (TrangThai IN ('DANG_GIU','DA_XUAT','DA_GIAI_PHONG')),
  NgayPhanBo datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  HanGiuLuc datetime NULL,
  NgayKetThuc datetime NULL,
  PhienBan bigint NOT NULL DEFAULT 1,
  CHECK (TrangThai = 'DANG_GIU' OR NgayKetThuc IS NOT NULL),
  FOREIGN KEY (ChiTietDonHangID) REFERENCES ChiTietDonHang(ChiTietDonHangID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS BienDongKho (
  BienDongID bigint NOT NULL AUTO_INCREMENT PRIMARY KEY,
  BienTheID bigint NOT NULL,
  LoaiBienDong varchar(20) NOT NULL CHECK (LoaiBienDong IN ('NHAP','DIEU_CHINH','GIU_HANG','GIAI_PHONG','BAN_HANG','NHAN_TRA')),
  ThayDoiTon int NOT NULL,
  ThayDoiPhanBo int NOT NULL,
  TonSau int NOT NULL CHECK (TonSau >= 0),
  PhanBoSau int NOT NULL CHECK (PhanBoSau >= 0),
  ChiTietPhieuKhoID bigint NULL,
  ChiTietDonHangID bigint NULL,
  NguoiThucHienID bigint NOT NULL,
  MaThaoTac char(36) NOT NULL UNIQUE,
  LyDo varchar(1000) NOT NULL,
  ThoiDiem datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CHECK (ThayDoiTon <> 0 OR ThayDoiPhanBo <> 0),
  CHECK (PhanBoSau <= TonSau),
  CHECK ((ChiTietPhieuKhoID IS NOT NULL AND ChiTietDonHangID IS NULL) OR (ChiTietPhieuKhoID IS NULL AND ChiTietDonHangID IS NOT NULL)),
  FOREIGN KEY (BienTheID) REFERENCES BienTheSanPham(BienTheID),
  FOREIGN KEY (ChiTietPhieuKhoID) REFERENCES ChiTietPhieuKho(ChiTietPhieuKhoID),
  FOREIGN KEY (ChiTietDonHangID) REFERENCES ChiTietDonHang(ChiTietDonHangID),
  FOREIGN KEY (NguoiThucHienID) REFERENCES TaiKhoan(TaiKhoanID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS DanhGia (
  DanhGiaID bigint NOT NULL AUTO_INCREMENT PRIMARY KEY,
  DonHangID bigint NOT NULL,
  ChiTietDonHangID bigint NOT NULL UNIQUE,
  KhachHangID bigint NOT NULL,
  SoSao tinyint NOT NULL CHECK (SoSao BETWEEN 1 AND 5),
  NoiDung varchar(2000) NULL,
  TrangThai varchar(15) NOT NULL DEFAULT 'HIEN_THI' CHECK (TrangThai IN ('HIEN_THI','AN')),
  NgayTao datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  NgayCapNhat datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (DonHangID,ChiTietDonHangID) REFERENCES ChiTietDonHang(DonHangID,ChiTietDonHangID),
  FOREIGN KEY (DonHangID,KhachHangID) REFERENCES DonHang(DonHangID,KhachHangID)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS NhatKyHeThong (
  NhatKyID bigint NOT NULL AUTO_INCREMENT PRIMARY KEY,
  NguoiThucHienID bigint NULL,
  HanhDong varchar(100) NOT NULL,
  DoiTuong varchar(100) NOT NULL,
  MaDoiTuong varchar(100) NULL,
  NoiDung LONGTEXT NOT NULL,
  KetQua varchar(15) NOT NULL CHECK (KetQua IN ('THANH_CONG','THAT_BAI')),
  DiaChiIP varchar(45) NULL,
  MaYeuCau char(36) NULL,
  ThoiDiem datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  FOREIGN KEY (NguoiThucHienID) REFERENCES TaiKhoan(TaiKhoanID)
) ENGINE=InnoDB;

CREATE UNIQUE INDEX UX_TaiKhoan_SDT ON TaiKhoan(SoDienThoai);
CREATE INDEX IX_Phien_TaiKhoan ON PhienDangNhap(TaiKhoanID,HetHanLuc);
CREATE INDEX IX_SanPham_Loc ON SanPham(TrangThai,DanhMucID,ThuongHieuID,GiaBan);
ALTER TABLE HinhAnhSanPham ADD COLUMN AnhChinhSanPhamID bigint GENERATED ALWAYS AS (CASE WHEN LaAnhChinh=1 THEN SanPhamID ELSE NULL END) STORED;
CREATE UNIQUE INDEX UX_AnhChinh ON HinhAnhSanPham(AnhChinhSanPhamID);
CREATE UNIQUE INDEX UX_Gio_TaiKhoan ON GioHang(TaiKhoanID);
CREATE UNIQUE INDEX UX_Gio_Phien ON GioHang(MaPhienHash);
CREATE INDEX IX_Don_Khach ON DonHang(KhachHangID,NgayDat DESC);
CREATE INDEX IX_Don_TrangThai ON DonHang(TrangThai,NgayDat);
CREATE INDEX IX_LichSuDon ON LichSuDonHang(DonHangID,ThoiDiem);
CREATE UNIQUE INDEX UX_ThanhToan_GiaoDich ON ThanhToan(CongThanhToan,MaGiaoDich);
CREATE INDEX IX_ThanhToan_Don ON ThanhToan(DonHangID,TrangThai);
CREATE INDEX IX_ThanhToan_BaoCao ON ThanhToan(TrangThai,NgayXacNhan);
CREATE UNIQUE INDEX UX_HoanTien_GiaoDich ON HoanTien(KenhHoanTien,MaGiaoDichHoan);
CREATE INDEX IX_HoanTien_BaoCao ON HoanTien(TrangThai,NgayXacNhan);
CREATE UNIQUE INDEX UX_PhieuKho_ChungTu ON PhieuKho(LoaiPhieu,MaChungTuThamChieu);
CREATE UNIQUE INDEX UX_NhanTra_MotLan ON ChiTietPhieuKho(ChiTietDoiTraID);
CREATE INDEX IX_BienDong_BienThe ON BienDongKho(BienTheID,BienDongID);
CREATE UNIQUE INDEX UX_BienDong_Phieu ON BienDongKho(ChiTietPhieuKhoID);
CREATE UNIQUE INDEX UX_BienDong_Don_Loai ON BienDongKho(ChiTietDonHangID,LoaiBienDong);
CREATE INDEX IX_NhatKy_ThoiDiem ON NhatKyHeThong(ThoiDiem,NguoiThucHienID);