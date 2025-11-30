using System.Linq.Expressions;
using FileExporter.Enums;
using FileExporter.Exceptions;
using FileExporter.Helpers;

namespace FileExporter.Rules;

public sealed class PropertyRule<TProperty> : IPropertyRule
{
   public PropertyRule(MemberExpression navigationExpression)
   {
      PropertyName = navigationExpression.Member.Name ??
                     throw new InvalidPropertyNameException("Invalid property name");

      ColumnName = PropertyName.ToSpacedName();
   }

   public string PropertyName { get; }
   public string ColumnName { get; private set; }
   public string? DefaultValue { get; private set; }

   public int? Order { get; private set; }
   public bool IsIgnored { get; private set; }

   public ColumnFormatType FormatType { get; private set; } = ColumnFormatType.Default;
   public int? Precision { get; private set; }
   public int? ColumnWidth { get; private set; }
   public EnumFormatMode EnumFormat { get; private set; } = EnumFormatMode.MixedIntAndName;

   public PropertyRule<TProperty> WriteToColumn(string name)
   {
      if (!string.IsNullOrWhiteSpace(name))
      {
         ColumnName = name;
      }

      return this;
   }

   public PropertyRule<TProperty> WithDefaultValue(string value)
   {
      DefaultValue = value;
      return this;
   }

   public PropertyRule<TProperty> HasOrder(int order)
   {
      Order = order;
      return this;
   }

   public PropertyRule<TProperty> Ignore()
   {
      IsIgnored = true;
      return this;
   }

   public PropertyRule<TProperty> HasFormat(ColumnFormatType formatType)
   {
      FormatType = formatType;
      return this;
   }

   public PropertyRule<TProperty> HasPrecision(int precision)
   {
      Precision = precision;
      return this;
   }

   public PropertyRule<TProperty> HasWidth(int width)
   {
      ColumnWidth = width;
      return this;
   }

   public PropertyRule<TProperty> AsEnum(EnumFormatMode mode)
   {
      EnumFormat = mode;
      return this;
   }
}