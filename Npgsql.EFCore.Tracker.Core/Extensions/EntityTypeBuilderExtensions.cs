using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Npgsql.EFCore.Tracker.Core.Extensions;

public static class EntityTypeBuilderExtensions
{
    public static EntityTypeBuilder<TEntity> EnableTracking<TEntity>(this EntityTypeBuilder<TEntity> builder)
       where TEntity : class
    {
        var schema = builder.Metadata.GetSchema() ?? string.Empty;
        var table_name = builder.Metadata.GetTableName();

        if (!string.IsNullOrEmpty(schema))
            schema += ".";

        if (string.IsNullOrEmpty(table_name))
            return builder;

        return builder
            .HasAnnotation("EnableTracking", $"SELECT enable_table_tracking('{schema}{table_name}')");
    }
}
