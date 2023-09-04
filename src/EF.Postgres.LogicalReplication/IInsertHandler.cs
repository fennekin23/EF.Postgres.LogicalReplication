namespace EF.Postgres.LogicalReplication;

public interface IInsertHandler<TEntity>
{
    Task<bool> Handle(TEntity entity, CancellationToken cancellationToken = default);
}
