using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace UniRepo.Cache;

public class EntityKeyPropertyCache
{
    private static readonly ConcurrentDictionary<Type, (PropertyInfo PropertyInfo, Func<object, object> IdConverter)> Cache = new();

    public static (PropertyInfo PropertyInfo, Func<object, object> IdConverter) GetKeyPropertyAndConverter(Type entityType, DbContext context)
    {
        return Cache.GetOrAdd(entityType, type =>
        {
            var propertyInfo = context.Model.FindEntityType(type)?.FindPrimaryKey()?.Properties
                .FirstOrDefault()?.PropertyInfo;

            if (propertyInfo == null)
                throw new InvalidOperationException($"Primary key for entity type {type.Name} not found.");

            var parameter = Expression.Parameter(typeof(object), "id");

            var body = Expression.Convert(
                Expression.Convert(parameter, propertyInfo.PropertyType),
                typeof(object)
            );

            var convert = Expression.Lambda<Func<object, object>>(body, parameter).Compile();

            return (propertyInfo, convert);
        });
    }
}