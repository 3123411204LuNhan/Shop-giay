using System.Text.Json;
using ShopGiay.Infrastructure;
using static ShopGiay.Infrastructure.ApiInput;
using Q = ShopGiay.Infrastructure.UserQueries;

namespace ShopGiay.Control;

public sealed class UserControl(Sql sql)
{
    public Task<List<dynamic>> List(CancellationToken token) => sql.Run(db => db.Rows(Q.List),token);

    private static async Task<(string Status,string[] Roles)> Lock(SqlSession db,long id)
    {
        var account = await db.One(Q.LockAccount,id) ?? throw new ApiException("Tài khoản không tồn tại.",404);
        var roles = await db.Rows(Q.LockRoles,id);
        return ((string)account.TrangThai,roles.Select(x => (string)x.role).ToArray());
    }

    public Task<object> Create(long actor,JsonElement body,CancellationToken token)
    {
        var role = body.Text("role");
        if (role is not ("CUSTOMER" or "SHOP_MANAGER")) throw new ApiException("Chỉ được tạo tài khoản khách hàng hoặc quản lý shop.");
        var email = body.Required("email","email");
        if (!Password.ValidEmail(email) || email.Length > 100) throw new ApiException("Email không hợp lệ.");
        var name = body.Required("name","họ tên",150);
        var hash = Password.Hash(Password.Validate(body.String("password")));
        return sql.Transaction<object>(async db => {
            var id = await db.Insert(Q.Insert,email,email,hash,name);
            await db.Execute(Q.AssignRole,id,actor,role);
            await db.Audit(actor,"TAO_TAI_KHOAN","TaiKhoan",id,role);
            return new { id };
        },token);
    }

    public Task<object> Update(long id,long actor,JsonElement body,CancellationToken token)
    {
        if (actor == id) throw new ApiException("Không tự sửa vai trò hoặc khóa tài khoản đang dùng.");
        var status = body.Text("status");
        if (status is not ("HOAT_DONG" or "BI_KHOA")) throw new ApiException("Trạng thái không hợp lệ.");
        var roleProvided = body.TryGetProperty("role",out _);
        var role = body.Text("role");
        return sql.Transaction(async db => {
            var admins = await db.Rows(Q.LockActiveAdmins);
            var account = await Lock(db,id);
            if (account.Roles.Length == 0) throw new ApiException("Không tìm thấy tài khoản có vai trò.",404);
            var current = account.Roles.Length == 1 ? account.Roles[0] : null;
            var change = roleProvided && role != current;
            if (change && (role is not ("SYSTEM_ADMIN" or "SHOP_MANAGER") || current is not ("SYSTEM_ADMIN" or "SHOP_MANAGER")))
                throw new ApiException("Chỉ được đổi vai trò giữa quản lý shop và nhân viên kỹ thuật.",403);
            if (admins.Count == 1 && (long)admins[0].id == id && (status != "HOAT_DONG" || (change && role != "SYSTEM_ADMIN")))
                throw new ApiException("Không thể vô hiệu hóa quản trị viên cuối cùng.");
            await db.Execute(Q.UpdateProfile,body.Required("name","họ tên",150),status,id);
            if (change) await db.Execute(Q.ChangeRole,role,actor,id);
            await db.Execute(Q.RevokeSessions,id);
            await db.Audit(actor,"SUA_PHAN_QUYEN","TaiKhoan",id,(roleProvided ? role : string.Join(", ",account.Roles)) + " / " + status);
            return Ok;
        },token);
    }

    public Task<object> Delete(long id,long actor,CancellationToken token)
    {
        if (actor == id) throw new ApiException("Bạn không thể xóa tài khoản đang đăng nhập.",403);
        return sql.Transaction(async db => {
            var account = await Lock(db,id);
            if (account.Roles.Length == 0 || account.Roles.Any(r => r is not ("CUSTOMER" or "SHOP_MANAGER")))
                throw new ApiException("Chỉ được xóa tài khoản khách hàng hoặc quản lý shop.",403);
            if (account.Status != "BI_KHOA") throw new ApiException("Hãy khóa tài khoản trước khi xóa.",409);
            var alias = "deleted-" + Guid.NewGuid();
            await db.Execute(Q.Anonymize,alias,alias,Password.Hash(Guid.NewGuid().ToString()),id);
            await db.Execute(Q.RevokeActiveSessions,id);
            await db.Execute(Q.InvalidateResetCodes,id);
            await db.Audit(actor,"XOA_TAI_KHOAN","TaiKhoan",id,string.Join(", ",account.Roles));
            return Ok;
        },token);
    }
}
