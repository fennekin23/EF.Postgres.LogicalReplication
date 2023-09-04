using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EF.Postgres.LogicalReplication;

internal class LogicalReplicationListener<TDbContext, TEntity> : IHostedService, IDisposable
    where TDbContext : DbContext
    where TEntity : class, new()
{
    private bool _disposedValue;
    private readonly Subscription<TEntity> _subscription;

    public LogicalReplicationListener(IServiceProvider serviceProvider,
        IInsertHandler<TEntity> insertHandler,
        INamingConventions? naming = null)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(insertHandler);

        IEntityType entityType;
        string connectionString;

        using (IServiceScope scope = serviceProvider.CreateScope())
        {
            TDbContext dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

            entityType = dbContext.Model.FindEntityType(typeof(TEntity))
                ?? throw new Exception("Unknown entity type");

            connectionString = dbContext.Database.GetConnectionString()
                ?? throw new Exception("Could not resolve connection string for DB context");
        }

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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _subscription.StartListening(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~LogicalReplicationListener()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
