using EcommerceWebApi;
using EcommerceWebApi.Authentication;
using EcommerceWebApi.Filters;
using EcommerceWebApi.Notification;
using EcommerceWebApi.Services;
using Serilog;
using Serilog.Events;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;

// cz-dotnet-1005: Configure Kestrel port via environment variable for container/Kubernetes deployments.
// KESTREL_PORT env var (injected via Kubernetes ConfigMap) drives dynamic port assignment,
// preventing hardcoded port conflicts in container orchestration platforms.
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    var kestrelPort = int.TryParse(
        Environment.GetEnvironmentVariable("KESTREL_PORT"),
        out var parsedPort) ? parsedPort : 8080;
    serverOptions.ListenAnyIP(kestrelPort);
});

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

builder.Services.AddControllers();
builder.Services
    .AddControllers()
    .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

// Configure Redis-backed distributed cache for Blazor Server circuit state persistence (cz-dotnet-1004)
// Uses Amazon ElastiCache for Redis on EKS to ensure circuit/session state survives pod restarts
// and horizontal scaling. REDIS_CONNECTION_STRING env var must be set to the ElastiCache endpoint.
var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("Redis")
    ?? "localhost:6379";

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = Environment.GetEnvironmentVariable("REDIS_INSTANCE_NAME") ?? "EcommerceWebApi:";
});

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("ApplicationSettings"));

// cz-dotnet-1005: Replace hardcoded SignalR CORS origin with environment-variable-driven configuration.
// SIGNALR_CORS_ORIGINS env var (injected via Kubernetes ConfigMap) allows dynamic origin configuration
// across environments without rebuilding the container image.
var signalrCorsOrigins = (Environment.GetEnvironmentVariable("SIGNALR_CORS_ORIGINS")
    ?? builder.Configuration["SignalR:CorsOrigins"]
    ?? "http://localhost:3001")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(signalrCorsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
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
app.MapHealthChecks("/health");

app.Run();
