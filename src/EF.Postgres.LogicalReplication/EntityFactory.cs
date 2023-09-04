using System.Linq.Expressions;
using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EF.Postgres.LogicalReplication;

internal class EntityFactory<TEntity>
    where TEntity : class, new()
{
    private readonly Func<TEntity> _entityCreator;
    private readonly ITableMapping _tableMapping;

    public EntityFactory(IEntityType entityType)
    {
        InstantiationBinding instantiationBinding = entityType.ConstructorBinding
            ?? throw new Exception("Could not resolve constructor for entity type");

        Expression constructorExpression = instantiationBinding.CreateConstructorExpression(new ParameterBindingInfo());
        _entityCreator = Expression.Lambda<Func<TEntity>>(constructorExpression).Compile();

        _tableMapping = entityType.GetTableMappings().First();
    }

    public TEntity CreateEntity(IDictionary<string, object> databaseValues)
    {
        TEntity entity = _entityCreator();

        foreach (IColumnMapping columnMapping in _tableMapping.ColumnMappings)
        {
            if (!databaseValues.TryGetValue(columnMapping.Column.Name, out object? sourceValue))
            {
                continue;
            }

            PropertyInfo property = columnMapping.Property.PropertyInfo!;
            var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            var targetvalue = sourceValue is IConvertible && sourceValue.GetType() != property.PropertyType
                ? Convert.ChangeType(sourceValue, targetType)
                : sourceValue;

            property.SetValue(entity, targetvalue);
        }

        return entity;
    }
}
