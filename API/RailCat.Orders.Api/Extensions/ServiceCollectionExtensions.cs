using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
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

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "RailCat Orders, Queue & KDS API",
                Version = "v1",
                Description = "Order intake, daily queue sequence numbering, Kitchen Display System & thermal print spooling"
            });
        });

        return services;
    }
}

