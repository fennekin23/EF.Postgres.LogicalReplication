using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EF.Postgres.LogicalReplication;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddLogicalReplicationListener<TDbContext, TEntity>(this IServiceCollection services)
        where TDbContext : DbContext
        where TEntity : class, new()
    {
        services.AddHostedService<LogicalReplicationListener<TDbContext, TEntity>>(sp =>
        {
            LogicalReplicationListener<TDbContext, TEntity> listener;

            using (IServiceScope scope = sp.CreateScope())
            {
                TDbContext dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
                IInsertHandler<TEntity> insertHandler = sp.GetRequiredService<IInsertHandler<TEntity>>();
                INamingConventions? namingConventions = sp.GetService<INamingConventions>();

                listener = new LogicalReplicationListener<TDbContext, TEntity>(dbContext, insertHandler, namingConventions);
            }

            return listener;
        });

        return services;
    }
}