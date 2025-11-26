using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

namespace Npgsql.EFCore.Tracker.Core.Migrations;

public class TrackingSupportMigration(MigrationsSqlGeneratorDependencies dependencies, INpgsqlSingletonOptions npgsqlOptions)
    : NpgsqlMigrationsSqlGenerator(dependencies, npgsqlOptions)
{
    protected override void Generate(CreateTableOperation operation, IModel? model, MigrationCommandListBuilder builder, bool terminate = true)
    {
        base.Generate(operation, model, builder, terminate);

        var enableTrackingSql = operation.FindAnnotation("EnableTracking")?.Value?.ToString();
        if (!string.IsNullOrEmpty(enableTrackingSql))
        {
            builder.AppendLine();
            builder.Append(enableTrackingSql);
            builder.AppendLine();
            builder.EndCommand();
        }
    }
}
