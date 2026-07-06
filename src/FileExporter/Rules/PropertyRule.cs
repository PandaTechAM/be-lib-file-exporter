using System.Linq.Expressions;
using FileExporter.Enums;
using FileExporter.Helpers;

namespace FileExporter.Rules;

/// <summary>Fluent configuration for a single exported column, backing <see cref="IPropertyRule" />.</summary>
public class PropertyRule<TProperty> : IPropertyRule
{
    private readonly string _propertyName;
    private string _columnName;
    private int? _columnWidth;
    private Func<object?, object?>? _customTransform;
    private string? _defaultValue;
    private EnumFormatMode _enumFormat;
    private ColumnFormatType _formatType;
    private bool _isIgnored;

    private int? _order;
    private int? _precision;

    /// <summary>Creates a rule for the property referenced by the given member expression, applying convention defaults.</summary>
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

    /// <summary>Sets the column header text.</summary>
    public PropertyRule<TProperty> WriteToColumn(string name)
    {
        _columnName = name;
        return this;
    }

    /// <summary>Sets the value written when the property is null. Ignored for non-nullable value types.</summary>
    public PropertyRule<TProperty> WithDefaultValue(string value)
    {
        var type = typeof(TProperty);

        if (!type.IsValueType || Nullable.GetUnderlyingType(type) is not null)
        {
            _defaultValue = value;
        }

        return this;
    }

    /// <summary>Sets the zero-based column order.</summary>
    public PropertyRule<TProperty> HasOrder(int order)
    {
        _order = order;
        return this;
    }

    /// <summary>Excludes this column from the export.</summary>
    public PropertyRule<TProperty> Ignore()
    {
        _isIgnored = true;
        return this;
    }

    /// <summary>Sets the column width in the XLSX output.</summary>
    public PropertyRule<TProperty> HasWidth(int width)
    {
        _columnWidth = width;
        return this;
    }

    /// <summary>Sets how the value is formatted.</summary>
    public PropertyRule<TProperty> HasFormat(ColumnFormatType formatType)
    {
        _formatType = formatType;
        return this;
    }

    /// <summary>Sets the number of decimal places for numeric formats. Must be non-negative.</summary>
    public PropertyRule<TProperty> HasPrecision(int precision)
    {
        if (precision < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(precision), precision, "Precision must be non-negative.");
        }

        _precision = precision;
        return this;
    }

    /// <summary>Sets how enum values are rendered.</summary>
    public PropertyRule<TProperty> WithEnumFormat(EnumFormatMode mode)
    {
        _enumFormat = mode;
        return this;
    }

    /// <summary>Sets a custom transform applied to the value before formatting.</summary>
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
