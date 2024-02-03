using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace UniRepo.Cache;

public class EntityPrimaryKeyCache
{
    private static readonly ConcurrentDictionary<Type,
        (PropertyInfo PropertyInfo, Func<object, object> IdConverter, IKey PrimaryKey)> Cache = new();

    public static (PropertyInfo PropertyInfo, Func<object, object> IdConverter, IKey PrimaryKey) GetPrimaryKeyProperties
        (Type entityType, DbContext context)
    {
        return Cache.GetOrAdd(entityType, type =>
        {
            var primaryKey = context.Model.FindEntityType(type)?.FindPrimaryKey();
            var propertyInfo = primaryKey?.Properties
                .FirstOrDefault()?.PropertyInfo;

            if (propertyInfo == null)
                throw new InvalidOperationException($"Primary key for entity type {type.Name} not found.");

            var parameter = Expression.Parameter(typeof(object), "id");

            var body = Expression.Convert(
                Expression.Convert(parameter, propertyInfo.PropertyType),
                typeof(object)
            );

            var convert = Expression.Lambda<Func<object, object>>(body, parameter).Compile();

            return (propertyInfo, convert, primaryKey!);
        });
    }
}