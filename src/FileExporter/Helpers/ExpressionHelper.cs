using System;
using System.Linq.Expressions;
using System.Reflection;

namespace FileExporter.Helpers;

internal static class ExpressionHelper
{
    public static MemberExpression GetMemberExpression(this PropertyInfo propertyInfo)
    {
        // Validate input
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));

        // Create a parameter expression for the object
        var parameter = Expression.Parameter(propertyInfo.DeclaringType!, "x");

        // Create a member expression for the property
        var memberExpression = Expression.Property(parameter, propertyInfo);

        return memberExpression;
    }
}