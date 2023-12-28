# PandaFileExporter

The `PandaFileExporter` is a C# library that provides functionalities to export data into various file formats such as CSV, XLSX, and PDF. It is designed to work with data in the form of `IQueryable<T>`, `IEnumerable<T>`, or `List<T>`.

## Classes

### DataTable

The `DataTable` class represents a table of data with headers and rows. It provides functionalities to convert an `IQueryable<T>` or `IEnumerable<T>` to a `DataTable` and to export the `DataTable` to CSV, XLSX, or PDF format.

## Usage

To use the `PandaFileExporter` library, you can follow these steps:

1. Convert your data to a `DataTable` using the `ToDataTable` method of the `DataTable` class or the `FromQueryable` method of the `DataTable` class.

```csharp
    var data = new List<YourClass> { /* your data */ };
    var dataTable = data.ToDataTable("YourDataName");
```

```csharp
    var data = new List<YourClass> { /* your data */ };
    var dataTable = DataTable.FromQueryable(data.AsQueryable(), "YourDataName");
```

2. Call the appropriate method of the `FileExporter` class to export the `DataTable` to the desired file format. The methods include `ToCsvArray`, `ToExcelArray`, and `ToPdfArray`.

```csharp
    var csvData = dataTable.ToCsv();
    var xlsxData = dataTable.ToXlsx();
    var pdfData = dataTable.ToPdf();
```

3. If the exported data is larger than a certain size, you can compress it into a ZIP file using the `ToZip` method of the `ZipExtensions` class.

```csharp
    var zipData = csvData.ToZip("YourDataName.csv");
```