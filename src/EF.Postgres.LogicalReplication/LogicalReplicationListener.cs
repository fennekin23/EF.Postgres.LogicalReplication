using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Hosting;

[assembly: InternalsVisibleTo("EF.Postgres.LogicalReplication.UnitTests")]

namespace EF.Postgres.LogicalReplication;

internal class LogicalReplicationListener<TDbContext, TEntity> : BackgroundService
    where TDbContext : DbContext
    where TEntity : class, new()
{
    private readonly Subscription<TEntity> _subscription;

    public LogicalReplicationListener(TDbContext dbContext,
        IInsertHandler<TEntity> insertHandler,
        INamingConventions naming,
        LogicalReplicationListenerOptions options)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(insertHandler);
        ArgumentNullException.ThrowIfNull(naming);

        IEntityType entityType;
        string connectionString;

        entityType = dbContext.Model.FindEntityType(typeof(TEntity))
            ?? throw new Exception("Unknown entity type");

        connectionString = options.ConnectionString
            ?? dbContext.Database.GetConnectionString()
            ?? throw new Exception("Could not resolve connection string for DB context");

        string schemaName = entityType.GetSchema()
            ?? "public";

        string tableName = entityType.GetTableName()
            ?? throw new Exception("Could not resolve table name for entity type");

        _subscription = new(
            new EntityMapper<TEntity>(
                new EntityFactory<TEntity>(entityType)),
            insertHandler,
            new SubscriptionOptions
            {
                ConnectionString = connectionString,
                CreateDatabasePublication = options.CreateDatabasePublication,
                PublicationName = naming.GetPublicationName(),
                ReplicationSlotName = naming.GetSlotName(),
                SchemaName = schemaName,
                TableName = tableName
            });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _subscription.StartListening(stoppingToken);
    }
}
