namespace ShopGiay.Entity;

public sealed class HoanTien
{
    public long HoanTienID { get; init; }
    public long DonHangID { get; init; }
    public long ThanhToanID { get; init; }
    public long? YeuCauDoiTraID { get; init; }
    public Guid MaYeuCau { get; init; }
    public required string LyDo { get; init; }
    public decimal SoTien { get; init; }
    public string TrangThai { get; private set; } = "CHO_XU_LY";
    public string? KenhHoanTien { get; init; }
    public string? MaGiaoDichHoan { get; init; }
    public string? BangChung { get; private set; }
    public long? NguoiXuLyID { get; private set; }
    public DateTime NgayTao { get; init; }
    public DateTime? NgayXacNhan { get; private set; }

    // Khoản hoàn chỉ có thể xử lý lần đầu khi đang chờ hoặc đang xử lý.
    public bool ConChoXuLy() => TrangThai is "CHO_XU_LY" or "DANG_XU_LY";

    // Đưa khoản hoàn thất bại về hàng chờ, bỏ qua nếu đã ở hàng chờ.
    public bool ThuLai()
    {
        if (TrangThai == "CHO_XU_LY") return false;
        if (TrangThai != "THAT_BAI") throw new DomainRuleException("Chỉ thử lại khoản hoàn đã thất bại.");
        TrangThai = "CHO_XU_LY";
        return true;
    }

    // Đánh dấu khoản hoàn đang chờ là thất bại và bỏ qua lần gọi lặp.
    public bool DanhDauThatBai()
    {
        if (TrangThai == "THAT_BAI") return false;
        if (!ConChoXuLy()) throw new DomainRuleException("Khoản hoàn không còn ở trạng thái chờ.");
        TrangThai = "THAT_BAI";
        return true;
    }

    // Ghi người xác nhận, bằng chứng và thời điểm của khoản hoàn đã đối soát.
    public void XacNhanHoanTien(long actor,string proof,DateTime thoiDiem)
    {
        if (!ConChoXuLy()) throw new DomainRuleException("Khoản hoàn cần ở trạng thái chờ.");
        (TrangThai,NguoiXuLyID,BangChung,NgayXacNhan) = ("DA_XAC_NHAN",actor,proof,thoiDiem);
    }
}
