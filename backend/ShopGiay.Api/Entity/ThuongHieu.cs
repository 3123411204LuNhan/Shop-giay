namespace ShopGiay.Entity;

public sealed class ThuongHieu
{
    public int ThuongHieuID { get; init; }
    public string TenThuongHieu { get; private set; } = "";
    public string? MoTa { get; init; }
    public string? HinhAnh { get; init; }
    public bool DangHoatDong { get; private set; }
    public bool LaBanNhap { get; private set; }
    public DateTime NgayTao { get; init; }
    public DateTime NgayCapNhat { get; init; }

    // Đổi tên phân loại và đồng bộ cờ nháp khi bật hoặc ẩn.
    public void CapNhat(string ten,bool active) =>
        (TenThuongHieu,DangHoatDong,LaBanNhap) = (ten,active,!active);
}
