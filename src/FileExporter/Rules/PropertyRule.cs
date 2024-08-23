using System.Linq.Expressions;
using FileExporter.Exceptions;

namespace FileExporter.Rules;

public class PropertyRule<TProperty> : IPropertyRule
{
    private readonly string _propertyName;
    private string _columnName;
    private string? _defaultValue;

    public PropertyRule(MemberExpression navigationExpression)
    {
        _propertyName = navigationExpression.Member.Name ??
                        throw new InvalidPropertyNameException($"Invalid property name {_propertyName}");
        _columnName = _propertyName;
    }

    public string PropertyName() => _propertyName;
    public string ColumnName() => _columnName;
    public string? DefaultColumnValue() => _defaultValue;

    public PropertyRule<TProperty> WriteToColumn(string name)
    {
        _columnName = name;
        return this;
    }

    public PropertyRule<TProperty> WithDefaultValue(string value)
    {
        var type = typeof(TProperty);

        if (type.IsGenericType || type.Name.Contains("String"))
        {
            _defaultValue = value;
        }

        return this;
    }
}