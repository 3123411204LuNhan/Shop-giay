namespace ShopGiay.Entity;

public sealed class PhieuKho
{
    public long PhieuKhoID { get; init; }
    public required string MaPhieu { get; init; }
    public Guid MaChongLap { get; init; }
    public required string LoaiPhieu { get; init; }
    public string? MaChungTuThamChieu { get; init; }
    public required string LyDo { get; init; }
    public long NguoiThucHienID { get; init; }
    public DateTime NgayTao { get; init; }
    public string TrangThai { get; private set; } = "NHAP";
    public DateTime? NgayXacNhan { get; private set; }
    public long? NguoiXacNhanID { get; private set; }

    // Cho phép chỉnh sửa và chuyển trạng thái khi phiếu còn nháp.
    public bool LaPhieuNhap() => TrangThai == "NHAP";

    // Đánh dấu phiếu đã xác nhận và ghi người thực hiện. Trả false nếu phiếu đã xác nhận.
    public bool HoanTatPhieu(long actor,DateTime thoiDiem)
    {
        if (TrangThai == "DA_XAC_NHAN") return false;
        if (!LaPhieuNhap()) throw new DomainRuleException("Phiếu đã hủy không thể xác nhận.");
        TrangThai = "DA_XAC_NHAN";
        NguoiXacNhanID = actor;
        NgayXacNhan = thoiDiem;
        return true;
    }

    // Chỉ hủy phiếu nháp. Phiếu đã hủy trả false để không ghi nhật ký lần nữa.
    public bool Huy()
    {
        if (TrangThai == "DA_HUY") return false;
        if (!LaPhieuNhap()) throw new DomainRuleException("Không hủy phiếu đã ghi sổ.");
        TrangThai = "DA_HUY";
        return true;
    }
}
