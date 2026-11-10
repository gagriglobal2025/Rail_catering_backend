using System.Security.Claims;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RailCat.Catalog.Api.Data;
using RailCat.Catalog.Api.Repositories;
using RailCat.Catalog.Api.Services;
using RailCat.Catalog.Api.Validators;

namespace RailCat.Catalog.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCatalogApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connStr = configuration.GetConnectionString("CatalogDbConnection");
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseNpgsql(connStr));

        services.AddHttpContextAccessor();
        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IConcessionService, ConcessionService>();

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CreateMenuItemValidator>();

        // JWT Authentication Configuration
        var secret = configuration["JwtSettings:Secret"] ?? "RailCatSuperSecretStaffAuthenticationKey2026!#*SecureJwtTokenSigningPhrase";
        var issuer = configuration["JwtSettings:Issuer"] ?? "RailCat.Users.Api";
        var audience = configuration["JwtSettings:Audience"] ?? "RailCatPlatform";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role
                };

                // Remove the requirement for "Bearer " prefix - supports tokens provided with OR without "Bearer "
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(authHeader))
                        {
                            var token = authHeader.Trim();
                            while (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                            {
                                token = token.Substring("Bearer ".Length).Trim();
                            }
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        // Swagger documentation with JWT support (works with or without "Bearer " prefix)
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "RailCat Menu Catalog & Concession API",
                Version = "v1",
                Description = "Menu hierarchy, dishes, pricing, and Railway Employee 20% concession verification"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header. Paste your JWT token directly here (works with or without 'Bearer ' prefix).",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
