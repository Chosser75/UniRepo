// Copyright © 2023 EPAM Systems, Inc. All Rights Reserved. All information contained herein is, and remains the
// property of EPAM Systems, Inc. and/or its suppliers and is protected by international intellectual
// property law. Dissemination of this information or reproduction of this material is strictly forbidden,
// unless prior written permission is obtained from EPAM Systems, Inc

using System.Linq.Expressions;
using System.Reflection;

namespace UniRepo.Extensions;

public static class QueryableExtensions
{
    /// <summary>
    /// Selects the specified columns from the source entity and projects them to the target entity.
    /// </summary>
    /// <typeparam name="TSource">Source entity type.</typeparam>
    /// <typeparam name="TTarget">Target entity type (DTO).</typeparam>
    /// <param name="sourceQuery"><see cref="IQueryable{TSource}"/>.</param>
    /// <param name="selectionColumns">A list of columns to be selected in a resulting projection.</param>
    /// <returns><see cref="IQueryable{TTarget}"/>.</returns>
    public static IQueryable<TTarget> GetDynamicProjection<TSource, TTarget>(
        this IQueryable<TSource> sourceQuery, IEnumerable<string> selectionColumns)
    {
        var parameter = Expression.Parameter(typeof(TSource), "a");
        var bindings = new List<MemberBinding>();

        foreach (var columnName in selectionColumns)
        {
            var accountProperty = typeof(TSource).GetProperty(
                columnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (accountProperty == null) continue;

            var dtoProperty = typeof(TTarget).GetProperty(
                columnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (dtoProperty == null) continue;

            var propertyAccess = Expression.MakeMemberAccess(parameter, accountProperty);
            var binding = Expression.Bind(dtoProperty, propertyAccess);
            bindings.Add(binding);
        }

        var body = Expression.MemberInit(Expression.New(typeof(TTarget)), bindings);
        var selector = Expression.Lambda<Func<TSource, TTarget>>(body, parameter);

        return sourceQuery.Select(selector);
    }
}