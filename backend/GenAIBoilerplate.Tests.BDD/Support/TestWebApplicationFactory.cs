using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using GenAIBoilerplate.Infrastructure.Persistence;
using GenAIBoilerplate.API;

namespace GenAIBoilerplate.Tests.BDD.Support;

/// <summary>
/// Test web application factory with TestContainers for integration testing
/// </summary>
public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    private readonly PostgreSqlContainer _postgreSqlContainer;
    private readonly RedisContainer _redisContainer;
    
    public string PostgreSqlConnectionString { get; private set; } = string.Empty;
    public string RedisConnectionString { get; private set; } = string.Empty;

    public TestWebApplicationFactory()
    {
        // Configure PostgreSQL container
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("genai_test_db")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithCleanUp(true)
            .Build();

        // Configure Redis container
        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithCleanUp(true)
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            config.AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true);
        });

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add TestContainers database context
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(PostgreSqlConnectionString);
            });

            // Override configuration for test containers
            services.PostConfigure<IConfiguration>(config =>
            {
                config["ConnectionStrings:DefaultConnection"] = PostgreSqlConnectionString;
                config["RedisSettings:ConnectionString"] = RedisConnectionString;
            });

            // Configure logging for tests
            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Warning);
            });

            // Build service provider and create/migrate database
            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            try
            {
                dbContext.Database.EnsureCreated();
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<TestWebApplicationFactory<TStartup>>>();
                logger.LogError(ex, "An error occurred creating the test database");
                throw;
            }
        });

        builder.UseEnvironment("Test");
    }

    /// <summary>
    /// Start the test containers and update connection strings
    /// </summary>
    public async Task StartContainersAsync()
    {
        await _postgreSqlContainer.StartAsync();
        await _redisContainer.StartAsync();

        PostgreSqlConnectionString = _postgreSqlContainer.GetConnectionString();
        RedisConnectionString = $"{_redisContainer.Hostname}:{_redisContainer.GetMappedPublicPort(6379)}";
    }

    /// <summary>
    /// Clean up the database for the next test
    /// </summary>
    public async Task CleanupDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Clear all data but keep schema
        var entityTypes = dbContext.Model.GetEntityTypes();
        
        foreach (var entityType in entityTypes)
        {
            var tableName = entityType.GetTableName();
            if (tableName != null)
            {
                await dbContext.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE \"{tableName}\" CASCADE");
            }
        }

        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Seed test data into the database
    /// </summary>
    public async Task SeedTestDataAsync(Action<ApplicationDbContext> seedAction)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        seedAction(dbContext);
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Execute a database operation within a scope
    /// </summary>
    public async Task<T> ExecuteDatabaseOperationAsync<T>(Func<ApplicationDbContext, Task<T>> operation)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await operation(dbContext);
    }

    /// <summary>
    /// Execute a database operation within a scope (void return)
    /// </summary>
    public async Task ExecuteDatabaseOperationAsync(Func<ApplicationDbContext, Task> operation)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await operation(dbContext);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _postgreSqlContainer?.DisposeAsync().AsTask().Wait();
            _redisContainer?.DisposeAsync().AsTask().Wait();
        }
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
