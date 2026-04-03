using System.Reflection;
using LudaFit.Core.Features.Common.Endpoints;

namespace LudaFit.Core.Extensions;

internal static class AppExtensions
{
    public static void UseConfiguration(this WebApplication app)
    {
        app.UseStaticFiles();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.MapGet("/", context =>
            {
                context.Response.Redirect("/swagger/index.html");
                return Task.CompletedTask;
            });
        }

        app.UseHttpsRedirection();
        app.MapStaticAssets();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseResponseCompression();

        app.UseCors("AllowReactDevClient");

        var apiGroup = app.MapGroup("api");
        app.MapEndpoints(apiGroup);
    }
    
    private static WebApplication MapEndpoints(this WebApplication app, IEndpointRouteBuilder? routeBuilder = null, Assembly? assemblyToScan = null)
    {
        IEndpointRouteBuilder endpoints = routeBuilder ?? app;

        Assembly assembly = assemblyToScan ?? Assembly.GetExecutingAssembly();
        
        var endpointTypes = assembly.GetTypes()
            .Where(t => typeof(IEndpoint).IsAssignableFrom(t)
                        && t is { IsInterface: false, IsAbstract: false, IsNested: true });

        foreach (Type type in endpointTypes)
        {
            if (Activator.CreateInstance(type) is IEndpoint endpoint)
            {
                endpoint.MapEndpoint(endpoints);
            }
        }

        return app;
    }
}
