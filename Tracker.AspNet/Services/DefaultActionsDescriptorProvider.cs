using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Tracker.AspNet.Attributes;
using Tracker.AspNet.Extensions;
using Tracker.AspNet.Models;
using Tracker.AspNet.Services.Contracts;

namespace Tracker.AspNet.Services;

public class DefaultActionsDescriptorProvider<TContext>(
    EndpointDataSource endpDataSrc, ILogger<DefaultActionsDescriptorProvider<TContext>> logger) : IActionsDescriptorProvider
    where TContext : DbContext
{
    public virtual IEnumerable<ActionDescriptor> GetActionsDescriptors(params Assembly[] assemblies)
    {
        foreach (var endpoint in endpDataSrc.Endpoints)
        {
            if (endpoint is not RouteEndpoint routeEndpoint)
                continue;

            var methods = routeEndpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods ?? [];
            if (!methods.Any(c => c.Equals("GET", StringComparison.OrdinalIgnoreCase)))
                continue;

            var controllerTracking = routeEndpoint.Metadata.GetMetadata<TrackAttribute>();
            if (controllerTracking is not null)
            {
                var route = routeEndpoint.RoutePattern.RawText ?? controllerTracking.Route ??
                    throw new NullReferenceException($"Route for '{routeEndpoint.DisplayName}' not found.");
                
                yield return new ActionDescriptor
                {
                    Route = route,
                    Tables = controllerTracking.Tables ?? []
                };
            }

            var minimalApiTracking = routeEndpoint.Metadata.GetMetadata<TrackRouteMetadata>();
            if (minimalApiTracking is not null && routeEndpoint is { RoutePattern.RawText: not null })
            {
                yield return new ActionDescriptor
                {
                    Route = routeEndpoint.RoutePattern.RawText,
                    Tables = minimalApiTracking.Tables ?? []
                };
            }
        }
    }
}
