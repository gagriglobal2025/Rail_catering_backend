using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RailCat.Users.Api.Data;
using RailCat.Users.Api.Helpers;
using RailCat.Users.Api.Repositories;
using RailCat.Users.Api.Repositories.Interfaces;
using RailCat.Users.Api.Services;
using RailCat.Users.Api.Services.Interfaces;
using RailCat.Users.Api.Validators;

namespace RailCat.Users.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUsersApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var connStr = configuration.GetConnectionString("UsersDbConnection");
        services.AddDbContext<UsersDbContext>(options =>
            options.UseNpgsql(connStr));

        // Repositories & Services
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddSingleton<JwtHelper>();

        // Validators
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<LoginValidator>();

        // JWT Authentication
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
                    ClockSkew = TimeSpan.Zero
                };

                // Remove requirement for "Bearer " prefix - supports tokens with OR without "Bearer "
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

        // Swagger documentation with JWT support
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "RailCat Users & IAM API",
                Version = "v1",
                Description = "Staff IAM & Authentication Microservice for RailCat"
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

