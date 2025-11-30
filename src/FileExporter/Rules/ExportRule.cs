using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FileExporter.Enums;
using FileExporter.Exceptions;
using FileExporter.Helpers;

namespace FileExporter.Rules;

public abstract class ExportRule<TModel> where TModel : class
{
   private readonly List<IPropertyRule> _rules = [];
   private readonly Dictionary<string, IPropertyRule> _rulesByProperty = new(StringComparer.Ordinal);

   protected ExportRule()
      : this(NamingHelper.GetDisplayName<TModel>())
   {
   }

   protected ExportRule(string fileName)
   {
      FileName = fileName;
      InitializeDefaultRules();
   }

   public string FileName { get; }

   public IReadOnlyList<IPropertyRule> Rules =>
      _rules
         .Where(r => !r.IsIgnored)
         .OrderBy(r => r.Order ?? int.MaxValue)
         .ToList();

   protected PropertyRule<TProperty> RuleFor<TProperty>(Expression<Func<TModel, TProperty>> navigationExpression)
   {
      if (navigationExpression.Body is not MemberExpression member)
      {
         throw new InvalidPropertyNameException("Invalid property expression");
      }

      var propertyName = member.Member.Name;

      if (_rulesByProperty.TryGetValue(propertyName, out var existing)
          && existing is PropertyRule<TProperty> typedExisting)
      {
         return typedExisting;
      }

      var rule = new PropertyRule<TProperty>(member);
      _rulesByProperty[propertyName] = rule;
      _rules.Add(rule);

      return rule;
   }

   private void InitializeDefaultRules()
   {
      var props = typeof(TModel)
                  .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                  .Where(p => p.CanRead)
                  .ToArray();

      var order = 0;

      foreach (var property in props)
      {
         var memberExpression = property.GetMemberExpression();
         var ruleType = typeof(PropertyRule<>).MakeGenericType(property.PropertyType);
         var rule = (IPropertyRule)Activator.CreateInstance(ruleType, memberExpression)!;

         dynamic propertyRule = rule;

         propertyRule.HasOrder(order++);

         var format = InferFormat(property.PropertyType);
         propertyRule.HasFormat(format);

         if (format == ColumnFormatType.Decimal)
         {
            propertyRule.HasPrecision(2);
         }

         _rules.Add(rule);
         _rulesByProperty[property.Name] = rule;
      }
   }

   private static ColumnFormatType InferFormat(Type type)
   {
      type = Nullable.GetUnderlyingType(type) ?? type;

      if (type == typeof(string))
      {
         return ColumnFormatType.Text;
      }

      if (type == typeof(DateTime) || type == typeof(DateOnly) || type == typeof(TimeOnly))
      {
         return ColumnFormatType.DateTime;
      }

      if (type == typeof(bool))
      {
         return ColumnFormatType.Boolean;
      }

      if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
      {
         return ColumnFormatType.Decimal;
      }

      if (type.IsEnum)
      {
         return ColumnFormatType.Text;
      }

      return type.IsPrimitive ? ColumnFormatType.Integer : ColumnFormatType.Text;
   }
}