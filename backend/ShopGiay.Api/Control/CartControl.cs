using ShopGiay.Infrastructure;

namespace ShopGiay.Control;

public sealed class CartControl(Sql sql,CartRepository carts)
{
    // Tìm giỏ của tài khoản rồi trả danh sách sản phẩm để giao diện hiển thị.
    public Task<List<dynamic>> Read(long account,CancellationToken token) =>
        sql.Transaction(async db => await carts.DocHienThi(db,(await carts.LayHoacTao(db,account)).GioHangID),token);

    // Kiểm tra tồn, nạp giỏ và áp dụng số lượng mới. Số lượng 0 xóa dòng, cập nhật giỏ không giữ kho.
    public Task<bool> Update(long account,long variant,int quantity,CancellationToken token) =>
        sql.Transaction(async db => {
            if (quantity > 0 && (await carts.TonDangBan(db,variant))?.ConDuHang(quantity) != true) return false;
            var cart = await carts.LayHoacTao(db,account);
            await carts.NapChiTiet(db,cart);
            if (quantity > 0) await carts.LuuDong(db,cart.DatSoLuong(variant,quantity));
            else if (cart.XoaSanPham(variant)) await carts.XoaDong(db,cart.GioHangID,variant);
            return true;
        },token);
}
