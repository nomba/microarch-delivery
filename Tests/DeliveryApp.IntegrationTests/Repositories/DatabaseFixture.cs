using DeliveryApp.Infrastructure.Adapters.Postgres;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace DeliveryApp.IntegrationTests.Repositories;

// ReSharper disable once ClassNeverInstantiated.Global
public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer; 
    private readonly Lazy<DbContextOptionsBuilder<ApplicationDbContext>> _dbContextOptionsBuilderLazy;
    
    public DatabaseFixture()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:14.7")
            .WithDatabase("order")
            .WithUsername("user")
            .WithPassword("secret")
            .WithCleanUp(true)
            .Build();

        // Lazy is needed because PostgreSqlContainer can not give Connection String until the container started. It's wierd
        _dbContextOptionsBuilderLazy = new Lazy<DbContextOptionsBuilder<ApplicationDbContext>>(() => new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(
            _postgresContainer.GetConnectionString(),
            sqlOptions => { sqlOptions.MigrationsAssembly("DeliveryApp.Infrastructure"); }));
    }

    internal ApplicationDbContext InstantiateDbContext()
    {
        return new ApplicationDbContext(_dbContextOptionsBuilderLazy.Value.Options);
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        
        // Ensure database created and updated
        
        await using var dbContext = InstantiateDbContext();
        await dbContext.Database.MigrateAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
    }
}