using EcommerceWebApi;
using EcommerceWebApi.Authentication;
using EcommerceWebApi.Filters;
using EcommerceWebApi.Notification;
using EcommerceWebApi.Services;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;

var logger = new LoggerConfiguration().MinimumLevel
    .Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.File(
        "Logs/log-.txt",
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(5),
        rollingInterval: RollingInterval.Day
    )
    .CreateLogger();
builder.Logging.AddSerilog(logger);

// Configure Kestrel to use environment-variable-driven port assignment for
// container orchestration platforms (Kubernetes). KESTREL_HTTP_PORT allows
// dynamic port mapping via Kubernetes ConfigMap / Pod environment fields,
// replacing any hardcoded port that would break in containerized deployments.
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    var httpPort = int.TryParse(
        Environment.GetEnvironmentVariable("KESTREL_HTTP_PORT")
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORT"),
        out var parsedPort) ? parsedPort : 80;

    serverOptions.ListenAnyIP(httpPort);
});

builder.Services.AddControllers();
builder.Services
    .AddControllers()
    .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("ApplicationSettings"));

// Resolve the allowed SignalR/CORS origin from an environment variable so that
// the hardcoded port (3001) is no longer embedded in the binary.
// Inject SIGNALR_CORS_ORIGIN via a Kubernetes ConfigMap or Pod environment field.
// Example: SIGNALR_CORS_ORIGIN=http://frontend-service:3001
var signalRCorsOrigin = Environment.GetEnvironmentVariable("SIGNALR_CORS_ORIGIN")
    ?? builder.Configuration["SignalR:CorsOrigin"]
    ?? "http://localhost:3001";

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(signalRCorsOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Redis-backed distributed cache for Blazor Server circuit state persistence.
// Replaces in-memory state to enable circuit survival across Kubernetes pod restarts
// and horizontal scaling on EKS with Amazon ElastiCache for Redis.
// Configure REDIS_CONNECTION_STRING environment variable to point to ElastiCache endpoint.
var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("Redis")
    ?? "localhost:6379";

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnectionString)
);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = Environment.GetEnvironmentVariable("REDIS_INSTANCE_NAME") ?? "EcommerceWebApi:";
});

builder.Services.AddScoped<UnitOfWork>();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OrderService>();

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<TotpService>();
builder.Services.AddScoped<AuthorizeFilter>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddSignalR();

builder.Services.AddSingleton<NotificationSubject>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");

app.Run();
