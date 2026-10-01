using System.Linq.Expressions;
using FileExporter.Enums;
using FileExporter.Helpers;
using FileExporter.Rules;

namespace FileExporter.Tests;

public class FormatForPdfTests
{
    [Fact]
    public void Currency_uses_group_separators_like_the_xlsx_format()
    {
        Assert.Equal("12,500.00", Format(12500m, Rule<decimal>(r => r.HasFormat(ColumnFormatType.Currency))));
        Assert.Equal("-1,234,567.89", Format(-1234567.891m, Rule<decimal>(r => r.HasFormat(ColumnFormatType.Currency))));
        Assert.Equal("12,500", Format(12500.4m, Rule<decimal>(r => r.HasFormat(ColumnFormatType.Currency).HasPrecision(0))));
        Assert.Equal("1,000.00", Format(1000, Rule<int>(r => r.HasFormat(ColumnFormatType.Currency))));
    }

    [Fact]
    public void Percentage_scales_a_fraction_without_a_space_before_the_sign()
    {
        Assert.Equal("55.32%", Format(0.5532m, Rule<decimal>(r => r.HasFormat(ColumnFormatType.Percentage))));
        Assert.Equal("55.3%", Format(0.5532m, Rule<decimal>(r => r.HasFormat(ColumnFormatType.Percentage).HasPrecision(1))));
        Assert.Equal("12.50%", Format(0.125d, Rule<double>(r => r.HasFormat(ColumnFormatType.Percentage))));
        Assert.Equal("100.00%", Format(1, Rule<int>(r => r.HasFormat(ColumnFormatType.Percentage))));
    }

    [Fact]
    public void Decimal_honours_precision()
    {
        Assert.Equal("1234.57", Format(1234.56789m, Rule<decimal>(r => r.HasFormat(ColumnFormatType.Decimal))));
        Assert.Equal("1234.5679", Format(1234.56789m, Rule<decimal>(r => r.HasFormat(ColumnFormatType.Decimal).HasPrecision(4))));
        Assert.Equal("5.00", Format(5, Rule<int>(r => r.HasFormat(ColumnFormatType.Decimal))));
    }

    [Fact]
    public void Midpoints_round_away_from_zero_as_excel_displays_them()
    {
        Assert.Equal("2.35", Format(2.345m, Rule<decimal>(r => r.HasFormat(ColumnFormatType.Decimal))));
        Assert.Equal("3", Format(2.5m, Rule<decimal>(r => r.HasFormat(ColumnFormatType.Integer))));
    }

    [Fact]
    public void Undeclared_numbers_follow_their_type()
    {
        Assert.Equal("1234.50", Format(1234.5m, Rule<decimal>()));
        Assert.Equal("0.33", Format(1d / 3, Rule<double>()));
        Assert.Equal("1234567", Format(1234567L, Rule<long>()));
    }

    [Fact]
    public void A_number_declared_as_text_is_written_as_it_is()
    {
        Assert.Equal("1234.5678", Format(1234.5678m, Rule<decimal>(r => r.HasFormat(ColumnFormatType.Text))));
    }

    [Fact]
    public void Date_format_on_a_datetime_drops_the_time()
    {
        var value = new DateTime(2026, 10, 1, 13, 45, 10);

        Assert.Equal("2026-10-01", Format(value, Rule<DateTime>(r => r.HasFormat(ColumnFormatType.Date))));
    }

    [Fact]
    public void A_datetime_keeps_its_time_by_default()
    {
        var value = new DateTime(2026, 10, 1, 13, 45, 10);

        Assert.Equal("2026-10-01 13:45:10", Format(value, Rule<DateTime>()));
        Assert.Equal("2026-10-01 13:45:10", Format(value, Rule<DateTime>(r => r.HasFormat(ColumnFormatType.DateTime))));
    }

    [Fact]
    public void DateOnly_and_TimeOnly_are_iso()
    {
        Assert.Equal("2026-10-01", Format(new DateOnly(2026, 10, 1), Rule<DateOnly>()));
        Assert.Equal("13:45:10", Format(new TimeOnly(13, 45, 10), Rule<TimeOnly>()));
    }

    [Fact]
    public void Booleans_are_yes_and_no()
    {
        Assert.Equal("Yes", Format(true, Rule<bool>()));
        Assert.Equal("No", Format(false, Rule<bool>()));
    }

    [Theory]
    [InlineData(EnumFormatMode.MixedIntAndName, "2 - Active", "2 - Ակտիվ")]
    [InlineData(EnumFormatMode.Int, "2", "2")]
    [InlineData(EnumFormatMode.Name, "Active", "Ակտիվ")]
    public void Enums_follow_their_mode_with_and_without_a_resolver(EnumFormatMode mode, string plain, string resolved)
    {
        var rule = Rule<Stage>(r => r.WithEnumFormat(mode));

        Assert.Equal(plain, Format(Stage.Active, rule));
        Assert.Equal(resolved, ValueFormatter.FormatForPdf(Stage.Active, rule, _ => "Ակտիվ"));
    }

    [Fact]
    public void An_undefined_enum_value_is_its_number()
    {
        Assert.Equal("7", Format((Stage)7, Rule<Stage>()));
    }

    [Fact]
    public void Null_uses_the_default_value()
    {
        Assert.Equal("N/A", Format(null, Rule<string?>(r => r.WithDefaultValue("N/A"))));
        Assert.Equal(string.Empty, Format(null, Rule<string?>()));
    }

    [Fact]
    public void Transform_runs_first()
    {
        var rule = Rule<decimal>(r => r.Transform(value => value * 2).HasFormat(ColumnFormatType.Currency));

        Assert.Equal("2,000.00", Format(1000m, rule));
    }

    [Theory]
    [InlineData(ColumnFormatType.Integer, typeof(string), true)]
    [InlineData(ColumnFormatType.Decimal, typeof(decimal), true)]
    [InlineData(ColumnFormatType.Currency, typeof(decimal), true)]
    [InlineData(ColumnFormatType.Percentage, typeof(decimal), true)]
    [InlineData(ColumnFormatType.Default, typeof(int?), true)]
    [InlineData(ColumnFormatType.Text, typeof(decimal), false)]
    [InlineData(ColumnFormatType.Default, typeof(string), false)]
    [InlineData(ColumnFormatType.Default, typeof(Stage), false)]
    [InlineData(ColumnFormatType.DateTime, typeof(DateTime), false)]
    public void Numeric_columns_align_right(ColumnFormatType format, Type propertyType, bool expected)
    {
        var rule = Rule<object>(r => r.HasFormat(format));

        Assert.Equal(expected, ValueFormatter.IsRightAlignedInPdf(rule, propertyType));
    }

    private static string Format(object? value, IPropertyRule rule)
    {
        return ValueFormatter.FormatForPdf(value, rule);
    }

    private static PropertyRule<T> Rule<T>(Func<PropertyRule<T>, PropertyRule<T>>? configure = null)
    {
        var member = Expression.Property(Expression.Parameter(typeof(Holder<T>)), nameof(Holder<T>.Value));
        var rule = new PropertyRule<T>(member);

        return configure?.Invoke(rule) ?? rule;
    }

    public class Holder<T>
    {
        public T Value { get; set; } = default!;
    }
}
