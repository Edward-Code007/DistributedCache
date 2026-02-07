using System.Reflection;

namespace DistributedCache.Endpoints;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        var endpointTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                        && t.GetInterfaces().Contains(typeof(IEndpoint)));

        foreach (var type in endpointTypes)
        {
            type.GetMethod(nameof(IEndpoint.MapEndpoint))!
                .Invoke(null, [app]);
        }

        return app;
    }
}
