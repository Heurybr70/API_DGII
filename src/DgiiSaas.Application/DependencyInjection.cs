using DgiiSaas.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DgiiSaas.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Use MediatR for CQRS
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        // Add FluentValidation if needed:
        // services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
