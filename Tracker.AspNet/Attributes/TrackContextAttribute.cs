using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracker.AspNet.Models;
using Tracker.Core.Extensions;

namespace Tracker.AspNet.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class TrackAttribute<TContext>(
    string[]? tables = null,
    Type[]? entities = null,
    string? sourceId = null,
    string? cacheControl = null) : TrackAttributeBase where TContext : DbContext
{
    private ImmutableGlobalOptions? _actionOptions;
    private readonly Lock _lock = new();

    protected override ImmutableGlobalOptions GetOrSetOptions(ActionExecutingContext execContext)
    {
        if (_actionOptions is not null)
            return _actionOptions;

        lock (_lock)
        {
            if (_actionOptions is not null)
                return _actionOptions;

            var scopeFactory = execContext.HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();

            var uniqueTables = new HashSet<string>();
            foreach (var table in tables ?? [])
                uniqueTables.Add(table);

            var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
            var tablesNames = dbContext.GetTablesNames(entities ?? []);
            foreach (var table in tablesNames)
                uniqueTables.Add(table);

            var baseOptions = scope.ServiceProvider.GetRequiredService<ImmutableGlobalOptions>();
            _actionOptions = baseOptions with
            {
                CacheControl = cacheControl ?? baseOptions.CacheControl,
                Source = sourceId ?? typeof(TContext).GetTypeHashId(),
                Tables = [.. uniqueTables]
            };
            return _actionOptions;
        }
    }
}
