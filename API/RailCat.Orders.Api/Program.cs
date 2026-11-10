using Serilog;
using Serilog.Events;
using RailCat.Orders.Api.Extensions;
using RailCat.Orders.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog: Console + Rolling Daily Files in logs/ folder
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: Path.Combine(builder.Environment.ContentRootPath, "logs", "railcat-orders-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOrdersApiServices(builder.Configuration);

// Configure CORS for Admin Web and Customer Mobile Web frontends
var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:3000", "http://localhost:3001", "https://mobilecatering.gagriglobal.com", "https://catering.gagriglobal.com" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("RailCatCorsPolicy", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
              {
                  if (string.IsNullOrEmpty(origin)) return false;
                  if (allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase)) return true;
                  try
                  {
                      var uri = new Uri(origin);
                      return uri.Host == "localhost" 
                          || uri.Host == "127.0.0.1" 
                          || uri.Host.EndsWith("gagriglobal.com", StringComparison.OrdinalIgnoreCase);
                  }
                  catch
                  {
                      return false;
                  }
              })
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSerilogRequestLogging();

// Enable Swagger in all environments (Development & Production)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("./v1/swagger.json", "RailCat Orders, Queue & KDS API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

app.UseCors("RailCatCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

// Root health-check endpoint
app.MapGet("/", () => Results.Ok(new
{
    service = "RailCat.Orders.Api",
    status = "Healthy",
    version = "1.0.0",
    timestamp = DateTime.UtcNow
}));

app.MapControllers();

app.Run();
