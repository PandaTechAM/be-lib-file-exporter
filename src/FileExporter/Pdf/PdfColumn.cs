namespace FileExporter.Pdf;

/// <summary>A column as the PDF table sees it: the header written, its alignment, and the rule's width hint.</summary>
internal sealed record PdfColumn(string Header, bool RightAligned, int? WidthHint);
