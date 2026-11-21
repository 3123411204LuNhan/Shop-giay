import {query} from './db.js';

// Additive and repeatable: existing orders, stock movements and confirmed receipts are preserved.
export const workflowColumns = {
  DanhMuc: { LaBanNhap: 'tinyint NOT NULL DEFAULT 0' },
  ThuongHieu: { LaBanNhap: 'tinyint NOT NULL DEFAULT 0' },
  TaiKhoan: { NgayXoa: 'datetime NULL' },
  DonHang: {
    DonViVanChuyen: 'varchar(150) NULL', MaVanDon: 'varchar(100) NULL',
    TrangThaiGiaoHang: "varchar(20) NOT NULL DEFAULT 'CHUA_GIAO'",
  },
  PhieuKho: {
    TrangThai: "varchar(20) NOT NULL DEFAULT 'DA_XAC_NHAN'",
    NhaCungCap: 'varchar(200) NULL', NgayXacNhan: 'datetime NULL',
    NguoiXacNhanID: 'bigint NULL', DonHangID: 'bigint NULL',
  },
};
export async function migrateWorkflows() {
  // Automated expiry has no human actor; foreign keys still apply to non-null IDs.
  for(const table of ['LichSuDonHang','BienDongKho']){
    const [column]=await query("SELECT IS_NULLABLE nullable FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name=? AND column_name='NguoiThucHienID'",[table]);
    if(column?.nullable==='NO')await query(`ALTER TABLE ${table} MODIFY COLUMN NguoiThucHienID bigint NULL`);
  }

  for (const [table,columns] of Object.entries(workflowColumns)) {
    for (const [column,definition] of Object.entries(columns)) {
      const [{n}] = await query('SELECT COUNT(*) n FROM information_schema.columns WHERE table_schema=DATABASE() AND table_name=? AND column_name=?',[table,column]);
      if (!n) await query(`ALTER TABLE ${table} ADD COLUMN ${column} ${definition}`);
    }
  }
  await query("UPDATE TaiKhoan SET TrangThai='BI_KHOA' WHERE TrangThai='VO_HIEU_HOA'");
  for(const [table,name,clause] of [
    ['TaiKhoan','CK_TaiKhoan_TrangThai',"CHECK (TrangThai IN ('HOAT_DONG','BI_KHOA'))"],
    ['PhieuKho','CK_PhieuKho_TrangThai',"CHECK (TrangThai IN ('NHAP','DA_XAC_NHAN','DA_HUY'))"],
    ['DonHang','CK_DonHang_GiaoHang',"CHECK (TrangThaiGiaoHang IN ('CHUA_GIAO','DANG_GIAO','GIAO_THAT_BAI','CHO_HOAN_VE','DA_NHAN_HOAN','DA_GIAO'))"],
    ['PhieuKho','FK_PhieuKho_XacNhan','FOREIGN KEY (NguoiXacNhanID) REFERENCES TaiKhoan(TaiKhoanID)'],
    ['PhieuKho','FK_PhieuKho_DonHang','FOREIGN KEY (DonHangID) REFERENCES DonHang(DonHangID)'],
  ]){
    const [{n}]=await query('SELECT COUNT(*) n FROM information_schema.table_constraints WHERE constraint_schema=DATABASE() AND table_name=? AND constraint_name=?',[table,name]);
    if(!n)await query(`ALTER TABLE ${table} ADD CONSTRAINT ${name} ${clause}`);
  }
}
