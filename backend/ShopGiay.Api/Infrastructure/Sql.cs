using System.Data.Common;
using System.Globalization;
using System.Text.RegularExpressions;
using Dapper;
using MySqlConnector;

namespace ShopGiay.Infrastructure;

public sealed class Sql(MySqlDataSource source)
{
    public async Task<T> Run<T>(Func<SqlSession,Task<T>> work,CancellationToken token = default)
    {
        await using var connection = await source.OpenConnectionAsync(token);
        return await work(new SqlSession(connection,null,token));
    }

    public async Task<T> Transaction<T>(Func<SqlSession,Task<T>> work,CancellationToken token = default)
    {
        await using var connection = await source.OpenConnectionAsync(token);
        await using var transaction = await connection.BeginTransactionAsync(token);
        var result = await work(new SqlSession(connection,transaction,token));
        await transaction.CommitAsync(token);
        return result;
    }
}

public sealed class SqlSession(DbConnection connection,DbTransaction? transaction,CancellationToken token)
{
    private CommandDefinition Command(string sql,object?[] values)
    {
        var parameters = new DynamicParameters();
        var index = 0;
        var command = Regex.Replace(sql,@"'(''|[^'])*'|[?]",match => {
            if (match.Value.Length > 1) return match.Value;
            var name = "p" + index;
            parameters.Add(name,values[index++]);
            return "@" + name;
        });
        if (index != values.Length) throw new InvalidOperationException("SQL parameter count mismatch.");
        return new CommandDefinition(command,parameters,transaction,cancellationToken:token);
    }

    public async Task<List<dynamic>> Rows(string sql,params object?[] values) =>
        (await connection.QueryAsync(Command(sql,values))).AsList();

    public async Task<dynamic?> One(string sql,params object?[] values) =>
        await connection.QueryFirstOrDefaultAsync(Command(sql,values));

    public async Task<T?> Value<T>(string sql,params object?[] values) =>
        await connection.ExecuteScalarAsync<T>(Command(sql,values));

    public Task<int> Execute(string sql,params object?[] values) =>
        connection.ExecuteAsync(Command(sql,values));

    public async Task<long> Insert(string sql,params object?[] values)
    {
        await Execute(sql,values);
        return await Value<long>("SELECT LAST_INSERT_ID()");
    }

    public async Task Audit(long? actor,string action,string type,long id,string detail) =>
        await Execute("INSERT INTO NhatKyHeThong(NguoiThucHienID,HanhDong,DoiTuong,MaDoiTuong,NoiDung,KetQua) VALUES(?,?,?,?,?,'THANH_CONG')",
            actor,action,type,id.ToString(CultureInfo.InvariantCulture),detail);
}
