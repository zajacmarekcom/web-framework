using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace WebFramework.Host.Framework.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddControllers(this IServiceCollection services)
    {
        var controllerTypes = Assembly.GetEntryAssembly()?.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Controller)) && !t.IsAbstract);

        if (controllerTypes == null) return services;
        foreach (var controllerType in controllerTypes)
        {
            services.AddTransient(controllerType);
        }

        return services;
    }
}