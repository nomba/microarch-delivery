using System.Reflection;
using DeliveryApp.Api.Adapters.BackgroundJobs;
using DeliveryApp.Core.Application.UseCases.Commands.AssignOrder;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Application.UseCases.Commands.MoveCouriers;
using DeliveryApp.Core.Domain.Services.DispatchService;
using DeliveryApp.Core.Ports;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Primitives;
using Quartz;

namespace DeliveryApp.Api;

public class Startup
{
    public Startup()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables();
        var configuration = builder.Build();
        Configuration = configuration;
    }

    /// <summary>
    ///     Конфигурация
    /// </summary>
    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // Health Checks
        services.AddHealthChecks();

        // Cors
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(
                policy =>
                {
                    policy.AllowAnyOrigin(); // Не делайте так в проде!
                });
        });

        // Configuration
        services.Configure<Settings>(options => Configuration.Bind(options));
        var connectionString = Configuration["CONNECTION_STRING"];
        var geoServiceGrpcHost = Configuration["GEO_SERVICE_GRPC_HOST"];
        var messageBrokerHost = Configuration["MESSAGE_BROKER_HOST"];
        
        // Domain Services
        services.AddTransient<IDispatchService, DispatchService>();
        
        // EF config
        services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(connectionString,
                    sqlOptions => { sqlOptions.MigrationsAssembly("DeliveryApp.Infrastructure"); });
                options.EnableSensitiveDataLogging();
            }
        );
        
        // Add register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        // Commands
        services.AddTransient<IRequestHandler<CreateOrderCommand, bool>, CreateOrderCommandHandler>();
        services.AddTransient<IRequestHandler<MoveCouriersCommand, bool>, MoveCouriersCommandHandler>();
        services.AddTransient<IRequestHandler<AssignOrderCommand, bool>, AssignOrderCommandHandler>();
        
        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Port & Adapters
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICourierRepository, CourierRepository>();
        
        // CRON Jobs
        services.AddQuartz(configure =>
        {
            var assignOrdersJobKey = new JobKey(nameof(AssignOrdersJob));
            var moveCouriersJobKey = new JobKey(nameof(MoveCouriersJob));
            configure
                .AddJob<AssignOrdersJob>(assignOrdersJobKey)
                .AddTrigger(
                    trigger => trigger.ForJob(assignOrdersJobKey)
                        .WithSimpleSchedule(
                            schedule => schedule.WithIntervalInSeconds(1)
                                .RepeatForever()))
                .AddJob<MoveCouriersJob>(moveCouriersJobKey)
                .AddTrigger(
                    trigger => trigger.ForJob(moveCouriersJobKey)
                        .WithSimpleSchedule(
                            schedule => schedule.WithIntervalInSeconds(2)
                                .RepeatForever()));
            configure.UseMicrosoftDependencyInjectionJobFactory();
        });
        services.AddQuartzHostedService();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
            app.UseDeveloperExceptionPage();
        else
            app.UseHsts();

        app.UseHealthChecks("/health");
        app.UseRouting();
    }
}