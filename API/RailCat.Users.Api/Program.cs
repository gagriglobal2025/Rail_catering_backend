using Serilog;
using Serilog.Events;
using RailCat.Users.Api.Extensions;
using RailCat.Users.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog: Console + Rolling Daily Files in logs/ folder
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: Path.Combine(builder.Environment.ContentRootPath, "logs", "railcat-users-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Add API controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add Users API service extensions (DB, Repos, Services, Auth)
builder.Services.AddUsersApiServices(builder.Configuration);

var app = builder.Build();

// Global Exception Handler
app.UseMiddleware<GlobalExceptionMiddleware>();

// Serilog HTTP Request Logging
app.UseSerilogRequestLogging();

// Enable Swagger in all environments (Development & Production)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("./v1/swagger.json", "RailCat Users & IAM API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Root health-check endpoint
app.MapGet("/", () => Results.Ok(new
{
    service = "RailCat.Users.Api",
    status = "Healthy",
    version = "1.0.0",
    timestamp = DateTime.UtcNow
}));

app.MapControllers();

app.Run();
