using System;

namespace FileExporter.Exceptions;

public class InvalidPropertyNameException(string message) : Exception(message);