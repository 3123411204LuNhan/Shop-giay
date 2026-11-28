namespace ShopGiay.Entity;
public sealed class DonHang
{
    public long DonHangID { get; init; }
    public required string MaDonHang { get; init; }
    public Guid MaYeuCauTao { get; init; }
    public long KhachHangID { get; init; }
    public required string HoTenNguoiNhan { get; init; }
    public required string SoDienThoaiNhan { get; init; }
    public string? EmailNhan { get; init; }
    public required string DiaChiNhan { get; init; }
    public string? PhuongXa { get; init; }
    public string? TinhThanh { get; init; }
    public string? GhiChu { get; init; }
    public int PhuongThucID { get; init; }
    public string? MaPhuongThuc { get; init; }
    public string TrangThai { get; private set; } = "CHO_XAC_NHAN";
    public string TrangThaiGiaoHang { get; private set; } = "CHUA_GIAO";
    public decimal TienHang { get; init; }
    public decimal TienGiam { get; init; }
    public decimal PhiVanChuyen { get; init; }
    public decimal TongThanhToan { get; init; }
    public required string TienTe { get; init; }
    public DateTime NgayDat { get; init; }
    public DateTime? HanThanhToan { get; init; }
    public DateTime? NgayGiao { get; private set; }
    public DateTime? NgayHoanThanh { get; private set; }
    public DateTime? NgayHuy { get; private set; }
    public string? LyDoHuy { get; private set; }
    public long? NguoiHuyID { get; private set; }
    // Kiểm tra đơn còn ở trạng thái cho phép hủy trước khi giao.
    public bool ChoPhepHuy() => TrangThai is "CHO_XAC_NHAN" or "DA_XAC_NHAN" or "DANG_CHUAN_BI";

    // Kiểm tra trạng thái, ghi người hủy, lý do và thời điểm. Đơn đã hủy trả false.
    public bool HuyDon(long? nguoiHuyId,string lyDo,DateTime thoiDiem)
    {
        if (TrangThai == "DA_HUY") return false;
        if (!ChoPhepHuy()) throw new DomainRuleException("Đơn đang giao hoặc đã hoàn tất, không thể hủy.");
        GhiHuy(nguoiHuyId,lyDo,thoiDiem);
        return true;
    }
    // Xác định bước tiếp theo của đơn theo thứ tự xử lý.
    public static string? TrangThaiTiepTheo(string status) => status switch {
        "CHO_XAC_NHAN" => "DA_XAC_NHAN",
        "DA_XAC_NHAN" => "DANG_CHUAN_BI",
        "DANG_CHUAN_BI" => "DANG_GIAO",
        "DANG_GIAO" => "DA_GIAO",
        "DA_GIAO" => "HOAN_THANH",
        _ => null
    };

    // Kiểm tra đúng bước chuyển rồi ghi thời điểm giao hoặc hoàn thành tương ứng.
    public void CapNhatTrangThai(string next,DateTime thoiDiem)
    {
        if (TrangThaiTiepTheo(TrangThai) != next)
            throw new DomainRuleException("Trạng thái chuyển tiếp không hợp lệ.");
        TrangThai = next;
        if (next == "DA_GIAO") NgayGiao = thoiDiem;
        if (next == "HOAN_THANH") NgayHoanThanh = thoiDiem;
    }

    // Tính tổng báo giá từ tiền hàng, tiền giảm và phí giao trước khi lưu đơn.
    public decimal TinhTongThanhToan() => TienHang - TienGiam + PhiVanChuyen;

    // Chỉ cho đổi trạng thái giao hàng theo nhánh giao thất bại, giao lại hoặc chờ hoàn về.
    public void CapNhatVanChuyen(string action) => TrangThaiGiaoHang = action switch {
        "failed" when TrangThaiGiaoHang is "DANG_GIAO" or "CHUA_GIAO" => "GIAO_THAT_BAI",
        "retry" when TrangThaiGiaoHang == "GIAO_THAT_BAI" => "DANG_GIAO",
        "await-return" when TrangThaiGiaoHang == "GIAO_THAT_BAI" => "CHO_HOAN_VE",
        "tracking" => TrangThaiGiaoHang,
        _ => throw new DomainRuleException("Thao tác vận chuyển không phù hợp trạng thái.")
    };

    // Đánh dấu đã nhận kiện giao thất bại và hủy đơn sau khi kiện đã chờ hoàn về.
    public void NhanHoanGiao(long actor,string lyDo,DateTime thoiDiem)
    {
        if (TrangThai != "DANG_GIAO" || TrangThaiGiaoHang != "CHO_HOAN_VE")
            throw new DomainRuleException("Đơn phải được đánh dấu chờ hoàn về trước khi nhận.");
        TrangThaiGiaoHang = "DA_NHAN_HOAN";
        GhiHuy(actor,lyDo,thoiDiem);
    }

    // Ghi trạng thái hủy và thông tin truy vết chung cho hủy trước giao và hoàn giao.
    private void GhiHuy(long? actor,string lyDo,DateTime thoiDiem) =>
        (TrangThai,NgayHuy,LyDoHuy,NguoiHuyID) = ("DA_HUY",thoiDiem,lyDo,actor);
}
