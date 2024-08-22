using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FileExporter.Dtos;
using FileExporter.Exceptions;
using FileExporter.Extensions;
using FileExporter.Helpers;

namespace FileExporter;

public class ExportRule<TModel> where TModel : class
{
    private readonly Type _modelType;
    private readonly string? _fileName;
    private readonly List<IPropertyRule> _rules = [];

    protected ExportRule()
    {
        _modelType = typeof(TModel);
        _fileName = NamingHelper.GetDisplayName<TModel>();
    }

    protected ExportRule(string fileName)
    {
        _modelType = typeof(TModel);
        _fileName = fileName;
    }

    protected PropertyRule<TProperty> RuleFor<TProperty>(Expression<Func<TModel, TProperty>> navigationExpression)
    {
        var rule = new PropertyRule<TProperty>(navigationExpression.Body as MemberExpression ??
                                               throw new InvalidPropertyNameException("Invalid property name"));

        var existingRule = _rules.FirstOrDefault(x => x.PropertyName() == rule.PropertyName());

        if (existingRule is not null)
        {
            var index = _rules.IndexOf(existingRule);
            _rules.Remove(existingRule);
            _rules.Insert(index, rule);
        }
        else
        {
            _rules.Add(rule);
        }

        return rule;
    }

    protected List<IPropertyRule> GenerateRules()
    {
        // Return properties in their order, not alphabetically ordered by default
        var modelProperties =
            _modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        foreach (var propertyInfo in modelProperties)
        {
            var memberExpression = propertyInfo.GetMemberExpression();
            var type = typeof(PropertyRule<>).MakeGenericType(propertyInfo.PropertyType);

            // Create a PropertyRule instance
            var rule = (IPropertyRule)Activator.CreateInstance(type, memberExpression)!;

            // Use reflection to create PropertyRule<TProperty> where TProperty is the property type
            // var ruleType = typeof(PropertyRule<>).MakeGenericType(type);
            // var rule = (IPropertyRule)Activator.CreateInstance(ruleType, memberExpression)!;

            _rules.Add(rule);
        }

        return _rules;
    }

    public ExportFile ToXlsx(IEnumerable<TModel> data)
    {
        return data.ToXlsx(_rules);
    }

    public ExportFile ToCsv(IEnumerable<TModel> data)
    {
        return data.ToCsv(_rules);
    }

    public ExportFile ToPdf(IEnumerable<TModel> data)
    {
        return data.ToPdf(_rules);
    }
}

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

public static class ExpressionHelper
{
    public static MemberExpression GetMemberExpression(this PropertyInfo propertyInfo)
    {
        // Validate input
        if (propertyInfo == null)
            throw new ArgumentNullException(nameof(propertyInfo));

        // Create a parameter expression for the object
        // var parameter = Expression.Parameter(propertyInfo.GetType(), "x");
        var parameter = Expression.Parameter(propertyInfo.DeclaringType!, propertyInfo.Name);

        // Create a member expression for the property
        var memberExpression = Expression.Property(parameter, propertyInfo);

        return memberExpression;
    }
}