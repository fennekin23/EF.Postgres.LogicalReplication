using Npgsql.Replication;
using Npgsql.Replication.PgOutput;
using Npgsql.Replication.PgOutput.Messages;

namespace EF.Postgres.LogicalReplication;

internal class Subscription<TEntity>(EntityMapper<TEntity> eventMapper, IInsertHandler<TEntity> insertHandler, SubscriptionOptions options)
    where TEntity : class, new()
{
    public async Task StartListening(CancellationToken cancellationToken = default)
    {
        await ConfigureDatabase(options, cancellationToken);

        await using var connection = new LogicalReplicationConnection(options.ConnectionString);
        await connection.Open(cancellationToken);

        PgOutputReplicationSlot replicationSlot = new(options.ReplicationSlotName);
        PgOutputReplicationOptions replicationOptions = new(options.PublicationName, 1);

        await foreach (var replicationMessage in connection.StartReplication(replicationSlot,
                                                                             replicationOptions,
                                                                             cancellationToken))
        {
            if (replicationMessage is InsertMessage insertMessage)
            {
                TEntity entity = await eventMapper.MapInsertEvent(insertMessage, cancellationToken);
                await insertHandler.Handle(entity, cancellationToken);
            }

            // Always call SetReplicationStatus() or assign LastAppliedLsn and LastFlushedLsn individually
            // so that Npgsql can inform the server which WAL files can be removed/recycled.
            connection.SetReplicationStatus(replicationMessage.WalEnd);
            // send the ACK to Postgres that we processed message
            await connection.SendStatusUpdate(cancellationToken);
        }
    }

    private static async Task ConfigureDatabase(SubscriptionOptions options, CancellationToken cancellationToken)
    {
        Management management = new(options.ConnectionString);

        if (!await management.PublicationExists(options.PublicationName, cancellationToken))
        {
            if (options.CreateDatabasePublication)
            {
                await management.CreatePublication(options.PublicationName, options.SchemaName, options.TableName, cancellationToken);
            }
            else
            {
                throw new Exception($"Publication \"{options.PublicationName}\" does not exists. Create publication first, or set \"{nameof(SubscriptionOptions.CreateDatabasePublication)}\" to \"true\"");
            }
        }

        if (!await management.ReplicationSlotExists(options.ReplicationSlotName, cancellationToken))
        {
            if (options.CreateDatabasePublication)
            {
                await management.CreateReplicationSlot(options.ReplicationSlotName, cancellationToken);
            }
            else
            {
                throw new Exception($"Replication slot \"{options.ReplicationSlotName}\" does not exists. Create publication first, or set \"{nameof(SubscriptionOptions.CreateDatabasePublication)}\" to \"true\"");
            }
        }
    }
}

public record SubscriptionOptions
{
    public string ConnectionString { get; set; } = null!;

    public bool CreateDatabasePublication { get; set; }

    public string PublicationName { get; set; } = null!;

    public string ReplicationSlotName { get; set; } = null!;

    public string SchemaName { get; set; } = null!;

    public string TableName { get; set; } = null!;
}
