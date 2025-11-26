using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EFCore.Tracker.Core.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Npgsql.EFCore.Tracker.Core.Extensions;

public static class ConfigurationExtensions
{
    public static NpgsqlDbContextOptionsBuilder EnableTableTrackingSupport(this NpgsqlDbContextOptionsBuilder optionsBuilder)
    {
        var coreOptionsBuilder = ((IRelationalDbContextOptionsBuilderInfrastructure)optionsBuilder).OptionsBuilder;
        coreOptionsBuilder.ReplaceService<IMigrationsSqlGenerator, TrackingSupportMigration>();
        return optionsBuilder;
    }
}
