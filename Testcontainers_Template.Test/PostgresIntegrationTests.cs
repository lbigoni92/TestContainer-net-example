using System.Threading.Tasks;
using Xunit;
using Testcontainers.PostgreSql;
using Npgsql;

public class PostgresIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:15-alpine")
        .WithDatabase("testdb")
        .WithUsername("postgres")
        .WithPassword("pass")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Should_Insert_And_Read()
    {
        await using var conn = new NpgsqlConnection(_postgres.GetConnectionString());
        await conn.OpenAsync();

        var createCmd = new NpgsqlCommand("CREATE TABLE test (id SERIAL PRIMARY KEY, name TEXT);", conn);
        await createCmd.ExecuteNonQueryAsync();

        var insertCmd = new NpgsqlCommand("INSERT INTO test (name) VALUES ('luca');", conn);
        await insertCmd.ExecuteNonQueryAsync();

        var selectCmd = new NpgsqlCommand("SELECT COUNT(*) FROM test;", conn);
        var count = (long)(await selectCmd.ExecuteScalarAsync()??-1);

        Assert.Equal(1, count);
    }
}
