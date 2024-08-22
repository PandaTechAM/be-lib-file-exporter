using System;

namespace FileExporter.Exceptions;

public class InvalidPropertyNameException(string message, string? property) : Exception(message);
