namespace ShopGiay.Entity;

public sealed class DatLaiMatKhau
{
    public long YeuCauID { get; init; }
    public long TaiKhoanID { get; init; }
    public required byte[] TokenHash { get; init; }
    public byte[]? MaXacMinhHash { get; init; }
    public DateTime NgayTao { get; init; }
    public DateTime HetHanLuc { get; init; }
    public DateTime? DaSuDungLuc { get; private set; }
    public int SoLanThu { get; private set; }

    // Kiểm tra mã còn hạn, chưa sử dụng và chưa đạt giới hạn nhập sai.
    public bool ConHieuLuc(DateTime thoiDiem) => DaSuDungLuc is null && HetHanLuc > thoiDiem && SoLanThu < 5;

    // Ghi thêm một lần nhập sai và khóa mã ở lần thứ năm.
    public void GhiLanSai(DateTime thoiDiem)
    {
        SoLanThu++;
        if (SoLanThu >= 5) DanhDauDaSuDung(thoiDiem);
    }

    // Vô hiệu mã vừa xác minh để không đổi thành token đặt lại lần nữa.
    public void DanhDauDaSuDung(DateTime thoiDiem) => DaSuDungLuc = thoiDiem;
}
