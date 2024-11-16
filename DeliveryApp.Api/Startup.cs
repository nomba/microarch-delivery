using System.Reflection;
using DeliveryApp.Api.Adapters.BackgroundJobs;
using DeliveryApp.Api.Adapters.Http.Contract.src.OpenApi.Formatters;
using DeliveryApp.Core.Application.UseCases.Commands.AssignOrders;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Application.UseCases.Commands.MoveCouriers;
using DeliveryApp.Core.Application.UseCases.Queries.GetBusyCouriers;
using DeliveryApp.Core.Application.UseCases.Queries.GetIncompleteOrders;
using DeliveryApp.Core.Domain.Services.DispatchService;
using DeliveryApp.Core.Ports;
using DeliveryApp.Infrastructure.Adapters.Grpc.GeoService;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
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
        services.AddTransient<IRequestHandler<AssignOrdersCommand, bool>, AssignOrdersCommandHandler>();
        
        // Queries
        services.AddTransient<IRequestHandler<GetBusyCouriersQuery, GetBusyCouriersResponse>, GetBusyCouriersQueryHandler>(_ => new GetBusyCouriersQueryHandler(connectionString));
        services.AddTransient<IRequestHandler<GetIncompleteOrdersQuery, GetIncompleteOrdersResponse>, GetIncompleteOrdersQueryHandler>(_ => new GetIncompleteOrdersQueryHandler(connectionString));
        
        // HTTP Handlers
        // or just     
        // services.AddMvc();
        services.AddControllers(options => { options.InputFormatters.Insert(0, new InputFormatterStream()); })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.Converters.Add(new StringEnumConverter
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                });
            });
        
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
        
        // Swagger
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("1.0.0", new OpenApiInfo
            {
                Title = "Delivery Service",
                Description = "Отвечает за диспетчеризацию доставки",
                Contact = new OpenApiContact
                {
                    Name = "Kirill Vetchinkin",
                    Url = new Uri("https://microarch.ru"),
                    Email = "info@microarch.ru"
                }
            });
            // TODO: Commented members does not exist in current SDK. It seems they is obsolete. Need to find out
            // options.CustomSchemaIds(type => type.FriendlyId(true));
            options.IncludeXmlComments(
                $"{AppContext.BaseDirectory}{Path.DirectorySeparatorChar}{Assembly.GetEntryAssembly()?.GetName().Name}.xml");
            // options.DocumentFilter<BasePathFilter>("");
            // options.OperationFilter<GeneratePathParamsValidationFilter>();
        });
        services.AddSwaggerGenNewtonsoftSupport();


        // gRPC
        services.AddTransient<IGeoClient, GeoClient>(_ => new GeoClient(geoServiceGrpcHost));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
            app.UseDeveloperExceptionPage();
        else
            app.UseHsts();

        app.UseHealthChecks("/health");
        app.UseRouting();
        
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseSwagger(c => { c.RouteTemplate = "openapi/{documentName}/openapi.json"; })
            .UseSwaggerUI(options =>
            {
                options.RoutePrefix = "openapi";
                options.SwaggerEndpoint("/openapi/1.0.0/openapi.json", "Swagger Delivery Service");
                options.RoutePrefix = string.Empty;
                options.SwaggerEndpoint("/openapi-original.json", "Swagger Delivery Service");
            });

        app.UseCors();
        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}