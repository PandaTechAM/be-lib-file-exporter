using System;
using System.Linq.Expressions;
using System.Reflection;

namespace FileExporter.Helpers;

internal static class ExpressionHelper
{
   public static MemberExpression GetMemberExpression(this PropertyInfo propertyInfo)
   {
      ArgumentNullException.ThrowIfNull(propertyInfo);

      var parameter = Expression.Parameter(propertyInfo.DeclaringType!, "x");
      var memberExpression = Expression.Property(parameter, propertyInfo);
      return memberExpression;
   }
}