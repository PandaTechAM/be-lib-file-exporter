using System;
using System.Linq.Expressions;
using FileExporter.Enums;
using FileExporter.Helpers;

namespace FileExporter.Rules;

public class PropertyRule<TProperty> : IPropertyRule
{
   private readonly string _propertyName;
   private string _columnName;
   private string? _defaultValue;

   private int? _order;
   private bool _isIgnored;
   private ColumnFormatType _formatType;
   private int? _precision;
   private int? _columnWidth;
   private EnumFormatMode _enumFormat;
   private Func<object?, object?>? _customTransform;

   public PropertyRule(MemberExpression navigationExpression)
   {
      _propertyName = navigationExpression.Member.Name;
      _columnName = NamingHelper.ToDisplayTitle(_propertyName);
      _formatType = ColumnFormatType.Default;
      _enumFormat = EnumFormatMode.MixedIntAndName;
      _precision = GuessDefaultPrecision(typeof(TProperty));
   }

   // ---- IPropertyRule explicit implementations ----

   string IPropertyRule.PropertyName => _propertyName;
   string IPropertyRule.ColumnName => _columnName;
   string? IPropertyRule.DefaultValue => _defaultValue;

   int? IPropertyRule.Order => _order;
   bool IPropertyRule.IsIgnored => _isIgnored;

   ColumnFormatType IPropertyRule.FormatType => _formatType;
   int? IPropertyRule.Precision => _precision;
   int? IPropertyRule.ColumnWidth => _columnWidth;
   EnumFormatMode IPropertyRule.EnumFormat => _enumFormat;

   Func<object?, object?>? IPropertyRule.CustomTransform => _customTransform;

   // ---- Fluent configuration API ----

   public PropertyRule<TProperty> WriteToColumn(string name)
   {
      _columnName = name;
      return this;
   }

   public PropertyRule<TProperty> WithDefaultValue(string value)
   {
      var type = typeof(TProperty);

      if (!type.IsValueType || Nullable.GetUnderlyingType(type) is not null)
      {
         _defaultValue = value;
      }

      return this;
   }

   public PropertyRule<TProperty> HasOrder(int order)
   {
      _order = order;
      return this;
   }

   public PropertyRule<TProperty> Ignore()
   {
      _isIgnored = true;
      return this;
   }

   public PropertyRule<TProperty> HasWidth(int width)
   {
      _columnWidth = width;
      return this;
   }

   public PropertyRule<TProperty> HasFormat(ColumnFormatType formatType)
   {
      _formatType = formatType;
      return this;
   }

   public PropertyRule<TProperty> HasPrecision(int precision)
   {
      if (precision < 0)
      {
         throw new ArgumentOutOfRangeException(nameof(precision), precision, "Precision must be non-negative.");
      }

      _precision = precision;
      return this;
   }

   public PropertyRule<TProperty> WithEnumFormat(EnumFormatMode mode)
   {
      _enumFormat = mode;
      return this;
   }

   public PropertyRule<TProperty> Transform(Func<TProperty?, object?> transform)
   {
      ArgumentNullException.ThrowIfNull(transform);

      _customTransform = raw => raw is null ? transform(default) : transform((TProperty?)raw);

      return this;
   }

   private static int? GuessDefaultPrecision(Type type)
   {
      if (type == typeof(decimal) || type == typeof(decimal?) ||
          type == typeof(double) || type == typeof(double?) ||
          type == typeof(float) || type == typeof(float?))
      {
         return 2;
      }

      return null;
   }
}