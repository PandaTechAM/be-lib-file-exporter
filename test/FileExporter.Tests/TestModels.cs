using FileExporter.Enums;
using FileExporter.Rules;

namespace FileExporter.Tests;

// Models and rules are public with public property types: ExportRule seeds its default rules through `dynamic`, which
// binds with the library's accessibility, so an internal type fails at rule construction.

public enum Stage
{
    Draft = 1,
    Active = 2
}

public class Order
{
    public int Id { get; set; }
    public string Customer { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public Stage Stage { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Paid { get; set; }
    public string? Note { get; set; }

    public static List<Order> Sample(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => new Order
            {
                Id = i,
                Customer = $"Customer {i}",
                Amount = 1000.5m * i,
                Stage = i % 2 == 0 ? Stage.Active : Stage.Draft,
                CreatedAt = new DateTime(2026, 10, 1, 12, 0, 0).AddMinutes(i),
                Paid = i % 3 == 0,
                Note = i % 4 == 0 ? null : $"Note {i}"
            })
            .ToList();
    }
}

public class OrderExportRule : ExportRule<Order>
{
    public OrderExportRule()
    {
        WithName("Orders");

        RuleFor(x => x.Customer)
            .WriteToColumn("Client")
            .HasOrder(0);

        RuleFor(x => x.Id)
            .HasOrder(1);

        RuleFor(x => x.Amount)
            .HasFormat(ColumnFormatType.Currency)
            .HasOrder(2);

        RuleFor(x => x.Note)
            .WithDefaultValue("None");

        RuleFor(x => x.Paid)
            .Ignore();
    }
}

public class Line
{
    public string Text { get; set; } = string.Empty;
}

public class Contact
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class Amount
{
    public decimal Value { get; set; }
}

public class LineExportRule : ExportRule<Line>
{
    public LineExportRule()
    {
        WithName("Lines");
    }
}

/// <summary>27 columns, shaped like a payment register: the widest table the exports produce.</summary>
public class Wide
{
    public long Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Condominium { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Estate { get; set; } = string.Empty;
    public string EstateType { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public string Document { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Stage Stage { get; set; }
    public string PaymentType { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Commission { get; set; }
    public decimal Total { get; set; }
    public decimal Rate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime EffectiveAt { get; set; }
    public DateTime? ReversedAt { get; set; }
    public bool Reversible { get; set; }
    public string Narrative { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    public static List<Wide> Sample(int count)
    {
        return Enumerable.Range(1, count)
            .Select(i => new Wide
            {
                Id = i,
                Reference = $"REF-{i:000000}-{i * 7919 % 100000:00000}",
                Condominium = "Kentron Condominium Association",
                Building = $"Building {i % 300}",
                Address = $"Yerevan, Komitas avenue {i % 120 + 1}",
                Estate = $"{i % 60 + 1}",
                EstateType = i % 4 == 0 ? "Parking" : "Apartment",
                Owner = i % 2 == 0 ? "Anna Hakobyan" : "Иван Петров",
                Document = $"{1000000000 + i}",
                Phone = $"+374 91 {i % 1000000:000000}",
                Email = $"owner{i}@example.am",
                Stage = i % 2 == 0 ? Stage.Active : Stage.Draft,
                PaymentType = i % 2 == 0 ? "Card" : "Bank transfer",
                Institution = i % 3 == 0 ? "Ameriabank" : "IDBank",
                Amount = 12500 + i % 1000,
                Commission = 125.5m,
                Total = 12625.5m + i % 1000,
                Rate = 0.01m,
                CreatedAt = new DateTime(2026, 9, 1).AddMinutes(i),
                EffectiveAt = new DateTime(2026, 9, 1).AddMinutes(i),
                ReversedAt = i % 10 == 0 ? new DateTime(2026, 9, 2) : null,
                Reversible = i % 2 == 0,
                Narrative = $"Payment {i} for the monthly maintenance fee",
                Comment = i % 17 == 0 ? "A longer comment that wraps over more than one line in its column." : "",
                CreatedBy = "Super Admin",
                District = "Kentron",
                City = "Yerevan"
            })
            .ToList();
    }
}

public class WideExportRule : ExportRule<Wide>
{
    public WideExportRule()
    {
        WithName("Payments");

        RuleFor(x => x.Amount).HasFormat(ColumnFormatType.Currency);
        RuleFor(x => x.Commission).HasFormat(ColumnFormatType.Currency);
        RuleFor(x => x.Total).HasFormat(ColumnFormatType.Currency);
        RuleFor(x => x.Rate).HasFormat(ColumnFormatType.Percentage);
    }
}
