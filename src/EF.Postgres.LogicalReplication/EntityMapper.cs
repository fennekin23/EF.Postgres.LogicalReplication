using Npgsql.Replication.PgOutput.Messages;

namespace EF.Postgres.LogicalReplication;

internal class EntityMapper<TEntity>(EntityFactory<TEntity> factory)
    where TEntity : class, new()
{
    public async ValueTask<TEntity> MapInsertEvent(InsertMessage message, CancellationToken cancellationToken)
    {
        IDictionary<string, object> databaseValues = await ProcessInsert(message, cancellationToken);
        return factory.CreateEntity(databaseValues);
    }

    private static async ValueTask<IDictionary<string, object>> ProcessInsert(InsertMessage message, CancellationToken cancellationToken)
    {
        Dictionary<string, object> result = new();
        int columnIndex = 0;

        await foreach (var value in message.NewRow)
        {
            var fieldName = message.Relation.Columns[columnIndex].ColumnName;
            var fieldValue = await value.Get<object>(cancellationToken);
            result.Add(fieldName, fieldValue);

            columnIndex++;
        }

        return result;
    }
}
