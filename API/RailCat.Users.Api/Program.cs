using RailCat.Users.Api.Extensions;
using RailCat.Users.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add API controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add Users API service extensions (DB, Repos, Services, Auth)
builder.Services.AddUsersApiServices(builder.Configuration);

var app = builder.Build();

// Global Exception Handler
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RailCat Users & IAM API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
