using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Testcontainers_Template.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private  string _connectionString;

    public WeatherForecastController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        EnsureDatabaseAndTableCreated();
    }

    private void EnsureDatabaseAndTableCreated()
    {
        Console.WriteLine(_connectionString);
        var masterConnStr = new SqlConnectionStringBuilder(_connectionString)
        {
            InitialCatalog = "master"
        }.ToString();
        Console.WriteLine(masterConnStr);
        using var masterConn = new SqlConnection(masterConnStr);
        masterConn.Open();

        using (var cmd = masterConn.CreateCommand())
        {
            cmd.CommandText = "IF DB_ID('TestDb') IS NULL CREATE DATABASE TestDb;";
            cmd.ExecuteNonQuery();
        }

        var appConnStr = new SqlConnectionStringBuilder(_connectionString)
        {
            InitialCatalog = "TestDb"
        }.ToString();

        using var appConn = new SqlConnection(appConnStr);
        appConn.Open();

        using (var cmd = appConn.CreateCommand())
        {
            cmd.CommandText = """
            IF OBJECT_ID('WeatherForecast') IS NULL
            CREATE TABLE WeatherForecast (
                Id INT PRIMARY KEY IDENTITY,
                Date DATE NOT NULL,
                TemperatureC INT NOT NULL,
                Summary NVARCHAR(100) NOT NULL
            );
        """;
            cmd.ExecuteNonQuery();
        }

        _connectionString = appConnStr; // aggiorna la stringa col DB corretto
    }

    [HttpPost]
    public IActionResult Post(WeatherForecast forecast)
    {
        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO WeatherForecast (Date, TemperatureC, Summary) VALUES (@date, @temp, @summary)";
        cmd.Parameters.AddWithValue("@date", forecast.Date);
        cmd.Parameters.AddWithValue("@temp", forecast.TemperatureC);
        cmd.Parameters.AddWithValue("@summary", forecast.Summary ?? (object)DBNull.Value);
        cmd.ExecuteNonQuery();

        return Ok();
    }

    [HttpGet]
    public ActionResult<IEnumerable<WeatherForecast>> Get()
    {
        var list = new List<WeatherForecast>();

        using var conn = new SqlConnection(_connectionString);
        conn.Open();

        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Date, TemperatureC, Summary FROM WeatherForecast";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new WeatherForecast
            {
                Id = reader.GetInt32(0),
                Date = DateOnly.FromDateTime(reader.GetDateTime(1)),
                TemperatureC = reader.GetInt32(2),
                Summary = reader.IsDBNull(3) ? null : reader.GetString(3)
            });
        }

        return Ok(list);
    }
}
