using Microsoft.AspNetCore.Routing;

namespace Tracker.AspNet.Services.Contracts;

public interface IEndpointRouteResolver
{
    string ResolveRoute(RouteEndpoint endpoint);
}
