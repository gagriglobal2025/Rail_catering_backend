using System.Security.Claims;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RailCat.Orders.Api.Data;
using RailCat.Orders.Api.Repositories;
using RailCat.Orders.Api.Services;
using RailCat.Orders.Api.Validators;

namespace RailCat.Orders.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrdersApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connStr = configuration.GetConnectionString("OrdersDbConnection");
        services.AddDbContext<OrdersDbContext>(options =>
            options.UseNpgsql(connStr));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IQueueTokenService, QueueTokenService>();
        services.AddScoped<IKitchenService, KitchenService>();

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CreateOrderValidator>();

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
                Title = "RailCat Orders, Queue & KDS API",
                Version = "v1",
                Description = "Order intake, daily queue sequence numbering, Kitchen Display System & thermal print spooling"
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
