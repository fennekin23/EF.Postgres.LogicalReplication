using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Hosting;

[assembly:InternalsVisibleTo("EF.Postgres.LogicalReplication.UnitTests")]

namespace EF.Postgres.LogicalReplication;

internal class LogicalReplicationListener<TDbContext, TEntity> : BackgroundService
    where TDbContext : DbContext
    where TEntity : class, new()
{
    private readonly Subscription<TEntity> _subscription;

    public LogicalReplicationListener(TDbContext dbContext,
        IInsertHandler<TEntity> insertHandler,
        INamingConventions? naming = null)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(insertHandler);

        IEntityType entityType;
        string connectionString;

        entityType = dbContext.Model.FindEntityType(typeof(TEntity))
            ?? throw new Exception("Unknown entity type");

        connectionString = dbContext.Database.GetConnectionString()
            ?? throw new Exception("Could not resolve connection string for DB context");

        string tableName = entityType.GetTableName()
            ?? throw new Exception("Could not resolve table name for entity type");

        INamingConventions namingConventions = naming ?? new DefaultNamingConventions(tableName);

        _subscription = new(
            new EntityMapper<TEntity>(
                new EntityFactory<TEntity>(entityType)),
            insertHandler,
            new SubscriptionOptions
            {
                ConnectionString = connectionString,
                CreateReplication = false,
                PublicationName = namingConventions.GetPublicationName(),
                ReplicationSlotName = namingConventions.GetSlotName(),
                TableName = tableName
            });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _subscription.StartListening(stoppingToken);
    }
}
