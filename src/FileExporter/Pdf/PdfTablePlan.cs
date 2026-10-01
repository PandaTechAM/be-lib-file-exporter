using PdfSharp.Drawing;

namespace FileExporter.Pdf;

/// <summary>The layout a table is drawn with: page size and body font size in points, and each column's width.</summary>
internal sealed record PdfTablePlan(XSize Page, double FontSize, double[] Widths);
