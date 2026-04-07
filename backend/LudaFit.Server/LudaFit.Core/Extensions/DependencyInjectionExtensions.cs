using System.Reflection;
using LudaFit.SharedKernel.Interfaces;

namespace LudaFit.Core.Extensions;

internal static class DependencyInjectionExtensions
{
    public static IServiceCollection AddTypesToDi(this IServiceCollection services, Assembly[]? assembliesToScan = null)
    {
        Type[] markerInterfaces = [typeof(IScopedType), typeof(ITransientType), typeof(ISingletonType)];
        
        if (assembliesToScan is null || assembliesToScan.Length == 0)
        {
            services.ScanAssembly(Assembly.GetExecutingAssembly(), markerInterfaces);
        }
        else
        {
            foreach (Assembly assembly in assembliesToScan)
            {
                services.ScanAssembly(assembly, markerInterfaces);
            }
        }
        
        return services;
    }

    private static void ScanAssembly(this IServiceCollection services, Assembly assembly, Type[] markerInterfaces)
    {
        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo<IScopedType>())
            .As(t => t.GetInterfaces().Except(markerInterfaces))
            .AsSelf()
            .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo<ITransientType>())
            .As(t => t.GetInterfaces().Except(markerInterfaces))
            .AsSelf()
            .WithTransientLifetime()
            .AddClasses(classes => classes.AssignableTo<ISingletonType>())
            .As(t => t.GetInterfaces().Except(markerInterfaces))
            .AsSelf()
            .WithSingletonLifetime()
        );
    }
}
