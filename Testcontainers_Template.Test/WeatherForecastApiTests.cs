using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;
using Xunit;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;

namespace Testcontainers_Template.Test;

public class WeatherForecastApiTests : IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer;
    private IContainer _apiContainer = null!;
    private readonly HttpClient _httpClient = new() { BaseAddress = new Uri("http://localhost:8080") };
    private readonly INetwork network ;

    public WeatherForecastApiTests()
    {
        network = new NetworkBuilder().WithName(Guid.NewGuid().ToString("D")).Build();
        network.CreateAsync().GetAwaiter().GetResult();

        _dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2025-latest")
            .WithPassword("P@ssw0rd123!")
            .WithCleanUp(true)
            .WithNetwork(network)
            .WithNetworkAliases("db")
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
            //.withDokercompose?? :( https://github.com/testcontainers/testcontainers-dotnet/issues/122
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
        var internalConn = "Server=db,1433;Database=master;User Id=sa;Password=P@ssw0rd123!;Encrypt=true;TrustServerCertificate=true";

        _apiContainer = new ContainerBuilder()
            .WithImage(image)
            .WithNetwork(network)
            .WithPortBinding(8080, 8080)
            .WithEnvironment("ConnectionStrings__DefaultConnection", internalConn)
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilPortIsAvailable(8080)
                    .UntilHttpRequestIsSucceeded(r =>
                        r.ForPort(8080)
                         .ForPath("/weatherforecast")
                         .ForStatusCode(System.Net.HttpStatusCode.OK)))
            .Build();


        await _apiContainer.StartAsync();
        Console.WriteLine("==== CONTAINER LOGS ====");
        var logs = await _apiContainer.GetLogsAsync();
        Console.WriteLine(logs);
    }

    public async Task DisposeAsync()
    {
        await _apiContainer.DisposeAsync();
        await _dbContainer.DisposeAsync();
    }

    [Fact]
    public async Task GetPostGet_VerifiesOneForecastInserted()
    {
        // Primo GET: nessuno insertato
        var firstGet = await _httpClient.GetFromJsonAsync<List<WeatherForecast>>("/weatherforecast");
        Assert.NotNull(firstGet);
        Assert.Empty(firstGet!);

        // POST: inserisco un forecast
        var newForecast = new
        {
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            TemperatureC = 27,
            Summary = "Test summary"
        };
        var post = await _httpClient.PostAsJsonAsync("/weatherforecast", newForecast);
        post.EnsureSuccessStatusCode();

        // Secondo GET: ci deve essere 1 elemento
        var secondGet = await _httpClient.GetFromJsonAsync<List<WeatherForecast>>("/weatherforecast");
        Assert.NotNull(secondGet);
        Assert.Single(secondGet!);
        Assert.Equal("Test summary", secondGet![0].Summary);

        // Verifica diretta dal DB
        var connString = _dbContainer.GetConnectionString(); // punta a master/TestDb via il builder
        using var conn = new SqlConnection(connString);
        await conn.OpenAsync();
        using var cmd = new SqlCommand(
            "SELECT COUNT(*) FROM TestDb.dbo.WeatherForecast WHERE Summary = @s", conn
        );
        cmd.Parameters.AddWithValue("@s", "Test summary");
        var count = (int)await cmd.ExecuteScalarAsync();

        Assert.Equal(1, count);
    }

}
