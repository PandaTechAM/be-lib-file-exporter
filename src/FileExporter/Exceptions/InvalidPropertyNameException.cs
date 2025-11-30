using System;

namespace FileExporter.Exceptions;

public sealed class InvalidPropertyNameException(string message) : Exception(message);