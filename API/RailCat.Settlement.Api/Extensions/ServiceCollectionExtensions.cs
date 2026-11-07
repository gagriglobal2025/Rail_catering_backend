using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RailCat.Settlement.Api.Data;
using RailCat.Settlement.Api.Repositories;
using RailCat.Settlement.Api.Services;
using RailCat.Settlement.Api.Validators;

namespace RailCat.Settlement.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSettlementApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connStr = configuration.GetConnectionString("SettlementDbConnection");
        services.AddDbContext<SettlementDbContext>(options =>
            options.UseNpgsql(connStr));

        services.AddScoped<IShiftRepository, ShiftRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();

        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IReportService, ReportService>();

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<OpenShiftValidator>();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "RailCat Settlement & Payments API",
                Version = "v1",
                Description = "Cashier shift drawer management, multi-mode payment tracking & daily commercial sales reports"
            });
        });

        return services;
    }
}
