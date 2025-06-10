using System;
using System.Threading.Tasks;
using Testcontainers.MsSql;
using Microsoft.Data.SqlClient;
using Xunit;

public class SqlServerIntegrationTests : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlServer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2025-latest")
        .WithPassword("P@ssw0rd123!")
        .WithCleanUp(true)
        .Build();

    public async Task InitializeAsync()
    {
        await _sqlServer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _sqlServer.DisposeAsync();
    }

    [Fact]
    public async Task Should_Insert_And_Read()
    {
        var connString = _sqlServer.GetConnectionString();

        await using var conn = new SqlConnection(connString);
        await conn.OpenAsync();

        var createCmd = new SqlCommand("CREATE TABLE Test (Id INT PRIMARY KEY IDENTITY, Name NVARCHAR(100));", conn);
        await createCmd.ExecuteNonQueryAsync();

        var insertCmd = new SqlCommand("INSERT INTO Test (Name) VALUES ('Luca');", conn);
        await insertCmd.ExecuteNonQueryAsync();

        var countCmd = new SqlCommand("SELECT COUNT(*) FROM Test;", conn);
        int count = (int)(await countCmd.ExecuteScalarAsync()??-1);

        Assert.Equal(1, count);
    }
}
