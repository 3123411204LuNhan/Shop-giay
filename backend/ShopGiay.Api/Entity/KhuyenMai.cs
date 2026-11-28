namespace ShopGiay.Entity;

public sealed class KhuyenMai
{
    // Tính mức giảm theo loại, áp trần và giới hạn trong giá trị được áp dụng.
    public decimal TinhTienGiam(decimal giaTriApDung)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(giaTriApDung);
        var giam = HinhThuc == "PHAN_TRAM"
            ? Math.Min(Math.Round(giaTriApDung * GiaTri / 100, MidpointRounding.AwayFromZero), GiamToiDa ?? decimal.MaxValue)
            : GiaTri;
        return Math.Min(giaTriApDung, giam);
    }

    public long KhuyenMaiID { get; init; }
    public required string MaKhuyenMai { get; init; }
    public string TenKhuyenMai { get; private set; } = "";
    public string? MoTa { get; init; }
    public string HinhThuc { get; private set; } = "";
    public decimal GiaTri { get; private set; }
    public decimal? GiamToiDa { get; private set; }
    public decimal GiaTriDonToiThieu { get; private set; }
    public required string PhamVi { get; init; }
    public DateTime BatDauLuc { get; private set; }
    public DateTime KetThucLuc { get; private set; }
    public string TrangThai { get; private set; } = "NHAP";
    public bool ChoPhepKetHop { get; init; }
    public int DoUuTien { get; init; }
    public int? GioiHanLuotDung { get; init; }
    public int? GioiHanMoiKhach { get; init; }
    public long NguoiTaoID { get; init; }
    public DateTime NgayTao { get; init; }
    public DateTime NgayCapNhat { get; init; }

    // Cập nhật nội dung và điều kiện giảm đã được Control chuẩn hóa.
    public void CapNhatDieuKien(string ten,string hinhThuc,long giaTri,long minimum,long? maximum) =>
        (TenKhuyenMai,HinhThuc,GiaTri,GiaTriDonToiThieu,GiamToiDa) = (ten,hinhThuc,giaTri,minimum,maximum);

    // Kiểm tra thứ tự thời gian trước khi lưu lịch và trạng thái nháp, kích hoạt hoặc kết thúc.
    public void DatLich(string trangThai,DateTime batDau,DateTime ketThuc,DateTime hienTai)
    {
        if (ketThuc <= batDau) throw new DomainRuleException("Ngày kết thúc phải sau ngày bắt đầu.");
        if (trangThai == "DANG_AP_DUNG" && ketThuc <= hienTai)
            throw new DomainRuleException("Chương trình đã hết hạn.");
        (TrangThai,BatDauLuc,KetThucLuc) = (trangThai,batDau,ketThuc);
    }

    // Áp dụng mã chỉ khi chương trình đang hoạt động trong khoảng thời gian đã đặt.
    public bool ConHieuLuc(DateTime hienTai) =>
        TrangThai == "DANG_AP_DUNG" && BatDauLuc <= hienTai && hienTai < KetThucLuc;
}
