using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;
using Xunit;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Testcontainers_Template.Test;

public class WeatherForecastApiTests : IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer;
    private IContainer _apiContainer = null!;
    private readonly HttpClient _httpClient = new() { BaseAddress = new Uri("http://localhost:8080") };

    public WeatherForecastApiTests()
    {
        _dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2025-latest")
            .WithPassword("P@ssw0rd123!")
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        var dockerfilePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../"));

        var image = new ImageFromDockerfileBuilder()
            .WithName("testcontainers_template")
            .WithDockerfileDirectory(dockerfilePath)
            .WithDockerfile("Testcontainers_Template/Dockerfile") // <- nome relativo nel contesto
            .Build();
        try
        {
            await image.CreateAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Docker build failed:");
            Console.WriteLine(ex.ToString());
            throw;
        }
        var connStr = $"Server={_dbContainer.Hostname},{_dbContainer.GetMappedPublicPort(1433)};Database=weatherdb;User Id=sa;Password=Your_password123!;TrustServerCertificate=True;";

        _apiContainer = new ContainerBuilder()
            .WithImage(image)
            .WithPortBinding(8080, 80)
            .WithEnvironment("ConnectionStrings__DefaultConnection", connStr)
           // .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(80)) // << Cambiato
            /*
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r =>
                r.ForPort(80).ForPath("/weatherforecast").ForStatusCode(System.Net.HttpStatusCode.OK)))*/
            .Build();


        await _apiContainer.StartAsync();
        Console.WriteLine("==== CONTAINER LOGS ====");
        var logs = await _apiContainer.GetLogsAsync( );
        Console.WriteLine(logs);
    }

    public async Task DisposeAsync()
    {
        await _apiContainer.DisposeAsync();
        await _dbContainer.DisposeAsync();
    }

    [Fact]
    public async Task InsertAndVerifyForecast()
    {
        var newForecast = new
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            TemperatureC = 27,
            Summary = "Test summary"
        };
        var health = await _httpClient.GetAsync("/weatherforecast");
        Console.WriteLine($"API responded: {health.StatusCode}");
        var post = await _httpClient.PostAsJsonAsync("/weatherforecast", newForecast);
        post.EnsureSuccessStatusCode();

        var connString = _dbContainer.GetConnectionString();

        using var conn = new SqlConnection(connString);
        await conn.OpenAsync();

        using var cmd = new SqlCommand("SELECT COUNT(*) FROM WeatherForecast WHERE Summary = 'Test summary'", conn);
        var count = (int)await cmd.ExecuteScalarAsync();

        Assert.True(count > 0, "Expected data to be inserted into WeatherForecast table.");
    }
}
