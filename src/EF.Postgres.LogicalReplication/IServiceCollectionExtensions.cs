using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EF.Postgres.LogicalReplication;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddLogicalReplicationListener<TDbContext, TEntity>(this IServiceCollection services)
        where TDbContext : DbContext
        where TEntity: class, new()
    {
        services.AddHostedService<LogicalReplicationListener<TDbContext, TEntity>>();

        return services;
    }
}