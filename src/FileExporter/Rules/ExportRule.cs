using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FileExporter.Dtos;
using FileExporter.Enums;
using FileExporter.Exceptions;
using FileExporter.Extensions;
using FileExporter.Helpers;

namespace FileExporter.Rules;

public class ExportRule<TModel> where TModel : class
{
    private readonly Type _modelType = typeof(TModel);

    private readonly string _fileName;
    private readonly List<IPropertyRule> _rules = [];

    protected ExportRule()
    {
        _fileName = NamingHelper.GetDisplayName<TModel>();
    }

    protected ExportRule(string fileName)
    {
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

            _rules.Add(rule);
        }

        return _rules;
    }

    public ExportFile ToXlsx(IEnumerable<TModel> data)
    {
        return data.ToXlsx(_fileName, _rules);
    }

    public ExportFile ToCsv(IEnumerable<TModel> data)
    {
        return data.ToCsv(_fileName, _rules);
    }

    public ExportFile ToPdf(IEnumerable<TModel> data)
    {
        return data.ToPdf(_fileName, _rules);
    }
}

public static class ExportRuleExtensions
{
    public static ExportFile ToFileFormat<T>(this ExportRule<T> rule, IEnumerable<T> data, ExportType type)
        where T : class
    {
        return type switch
        {
            ExportType.Xlsx => rule.ToXlsx(data),
            ExportType.Csv => rule.ToCsv(data),
            ExportType.Pdf => rule.ToPdf(data),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}