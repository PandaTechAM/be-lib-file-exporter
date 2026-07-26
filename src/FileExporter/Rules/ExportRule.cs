using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using FileExporter.Enums;
using FileExporter.Helpers;

namespace FileExporter.Rules;

/// <summary>
///     Base class for defining how <typeparamref name="TModel" /> is exported. Derive from it and use
///     <see cref="RuleFor{TProperty}" /> to override the conventions inferred from the model's public properties.
/// </summary>
public abstract class ExportRule<TModel> where TModel : class
{
    private readonly List<IPropertyRule> _rules = [];
    private readonly Dictionary<string, IPropertyRule> _rulesByProperty = new(StringComparer.Ordinal);

    /// <summary>Seeds a rule for every public readable property with format inferred from its type.</summary>
    protected ExportRule()
    {
        SetNameInternal(typeof(TModel).Name);
        InitializeDefaultRules();
    }

    /// <summary>
    ///     The configured name with its <c>{DateTime}</c> placeholder still unresolved. A rule instance lives for the
    ///     process lifetime, so the timestamp is substituted per export, not here.
    /// </summary>
    internal string FileNameTemplate { get; private set; }

    /// <summary>
    ///     The configured name without its timestamp, used as the default worksheet name. Never blank — a name of
    ///     just <c>"{DateTime}"</c> falls back to the model type, because a worksheet must have a name.
    /// </summary>
    internal string DisplayName { get; private set; }

    internal IReadOnlyList<IPropertyRule> Rules =>
        _rules
            .Where(r => !r.IsIgnored)
            .OrderBy(r => r.Order ?? int.MaxValue)
            .ToList();

    /// <summary>Sets the base name used for the exported file.</summary>
    protected ExportRule<TModel> WithName(string name)
    {
        SetNameInternal(name);
        return this;
    }

    /// <summary>Returns the fluent rule for the given property, creating it if needed, so it can be configured.</summary>
    protected PropertyRule<TProperty> RuleFor<TProperty>(Expression<Func<TModel, TProperty>> navigationExpression)
    {
        if (navigationExpression.Body is not MemberExpression member)
        {
            throw new ArgumentException("Invalid property expression");
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

    [MemberNotNull(nameof(FileNameTemplate), nameof(DisplayName))]
    private void SetNameInternal(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("File name can not be null or empty.", nameof(name));
        }

        FileNameTemplate = NamingHelper.BuildTemplate(name);

        var withoutTimestamp = NamingHelper.WithoutTimestamp(FileNameTemplate);

        DisplayName = string.IsNullOrWhiteSpace(withoutTimestamp)
            ? NamingHelper.ToDisplayTitle(typeof(TModel).Name)
            : withoutTimestamp;
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

        if (type == typeof(DateTime) || type == typeof(TimeOnly))
        {
            return ColumnFormatType.DateTime;
        }

        if (type == typeof(DateOnly))
        {
            return ColumnFormatType.Date;
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
