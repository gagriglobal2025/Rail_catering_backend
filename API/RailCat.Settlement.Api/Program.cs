using Serilog;
using Serilog.Events;
using RailCat.Settlement.Api.Extensions;
using RailCat.Settlement.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog: Console + Rolling Daily Files in logs/ folder
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: Path.Combine(builder.Environment.ContentRootPath, "logs", "railcat-settlement-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSettlementApiServices(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSerilogRequestLogging();

// Enable Swagger in all environments (Development & Production)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("./v1/swagger.json", "RailCat Settlement & Payments API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseAuthorization();

// Root health-check endpoint
app.MapGet("/", () => Results.Ok(new
{
    service = "RailCat.Settlement.Api",
    status = "Healthy",
    version = "1.0.0",
    timestamp = DateTime.UtcNow
}));

app.MapControllers();

app.Run();
