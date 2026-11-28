namespace ShopGiay.Entity;

public sealed class YeuCauDoiTra
{
    public long YeuCauDoiTraID { get; init; }
    public required string MaYeuCau { get; init; }
    public Guid MaChongLap { get; init; }
    public long DonHangID { get; init; }
    public long KhachHangID { get; init; }
    public required string LoaiYeuCau { get; init; }
    public required string LyDo { get; init; }
    public string TrangThai { get; private set; } = "CHO_DUYET";
    public long? NguoiDuyetID { get; private set; }
    public DateTime NgayTao { get; init; }
    public DateTime? NgayDuyet { get; private set; }
    public string? LyDoXuLy { get; private set; }
    public DateTime? NgayHoanTat { get; private set; }
    public long? DonHangDoiMoiID { get; private set; }

    // Quyết định duyệt hoặc từ chối yêu cầu đang chờ và ghi người, giờ, lý do.
    public void QuyetDinh(bool duyet,long actor,string lyDo,DateTime thoiDiem)
    {
        if (TrangThai != "CHO_DUYET") throw new DomainRuleException("Yêu cầu đã được duyệt.");
        if (lyDo.Length == 0) throw new DomainRuleException("Vui lòng nhập lý do xử lý.");
        (TrangThai,NguoiDuyetID,LyDoXuLy,NgayDuyet) = (duyet ? "DA_DUYET" : "TU_CHOI",actor,lyDo,thoiDiem);
    }

    // Chỉ nhận hàng một lần, sau khi được duyệt, và chuyển sang xử lý.
    public void NhanHang()
    {
        if (TrangThai != "DA_DUYET") throw new DomainRuleException("Chỉ nhận hàng một lần sau khi duyệt.");
        TrangThai = "DANG_XU_LY";
    }

    // Gắn đơn vừa tạo để lần gọi sau có thể phát hiện đã đổi hàng.
    public void GanDonThayThe(long id) => DonHangDoiMoiID = id;

    // Kiểm tra đã nhận hàng và có đơn thay thế nếu là yêu cầu đổi, rồi ghi thời điểm hoàn tất.
    public void HoanTat(DateTime thoiDiem)
    {
        if (TrangThai != "DANG_XU_LY") throw new DomainRuleException("Cần nhận và kiểm tra hàng trước.");
        if (LoaiYeuCau == "DOI" && DonHangDoiMoiID is null)
            throw new DomainRuleException("Cần tạo đơn thay thế trước khi hoàn tất.");
        (TrangThai,NgayHoanTat) = ("HOAN_TAT",thoiDiem);
    }
}
