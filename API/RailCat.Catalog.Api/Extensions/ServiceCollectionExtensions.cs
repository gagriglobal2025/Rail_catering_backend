using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
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

        services.AddScoped<IMenuRepository, MenuRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IConcessionService, ConcessionService>();

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CreateMenuItemValidator>();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "RailCat Menu Catalog & Concession API",
                Version = "v1",
                Description = "Menu hierarchy, dishes, pricing, and Railway Employee 20% concession verification"
            });
        });

        return services;
    }
}

