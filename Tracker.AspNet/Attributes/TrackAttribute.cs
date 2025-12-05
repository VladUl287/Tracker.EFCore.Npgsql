using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using Tracker.AspNet.Models;

namespace Tracker.AspNet.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class TrackAttribute(
    string[]? tables = null,
    string? sourceId = null,
    string? cacheControl = null) : TrackAttributeBase
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

            var baseOptions = execContext.HttpContext.RequestServices.GetRequiredService<ImmutableGlobalOptions>();
            _actionOptions = baseOptions with
            {
                CacheControl = cacheControl ?? baseOptions.CacheControl,
                Source = sourceId ?? baseOptions.Source,
                Tables = tables?.ToImmutableArray() ?? []
            };
            return _actionOptions;
        }
    }
}