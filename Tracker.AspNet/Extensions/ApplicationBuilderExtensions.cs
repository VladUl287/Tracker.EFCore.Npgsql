using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tracker.AspNet.Middlewares;
using Tracker.AspNet.Models;
using Tracker.AspNet.Services.Contracts;

namespace Tracker.AspNet.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseTracker(this IApplicationBuilder builder)
        => builder.UseMiddleware<TrackerMiddleware>();

    public static IApplicationBuilder UseTracker(this IApplicationBuilder builder, GlobalOptions options)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        var optionsBuilder = builder.ApplicationServices.GetRequiredService<IOptionsBuilder<GlobalOptions, ImmutableGlobalOptions>>();
        var immutableOptions = optionsBuilder.Build(options);

        return builder.UseMiddleware<TrackerMiddleware>(immutableOptions);
    }

    public static IApplicationBuilder UseTracker<TContext>(this IApplicationBuilder builder, GlobalOptions options)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        var optionsBuilder = builder.ApplicationServices.GetRequiredService<IOptionsBuilder<GlobalOptions, ImmutableGlobalOptions>>();
        var immutableOptions = optionsBuilder.Build<TContext>(options);

        return builder.UseMiddleware<TrackerMiddleware>(immutableOptions);
    }

    public static IApplicationBuilder UseTracker<TContext>(this IApplicationBuilder builder, Action<GlobalOptions> configure)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(configure, nameof(configure));

        var options = new GlobalOptions();
        configure(options);
        return builder.UseTracker<TContext>(options);
    }

    public static IApplicationBuilder UseTracker(this IApplicationBuilder builder, Action<GlobalOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure, nameof(configure));

        var options = new GlobalOptions();
        configure(options);
        return builder.UseTracker(options);
    }
}
