using System;
using System.ComponentModel;
using System.Reflection;

namespace FileExporter.Helpers;

internal static class NamingHelper
{
    internal static string GetDisplayName<T>()
    {
        var displayName = typeof(T).GetCustomAttribute<DisplayNameAttribute>()?.DisplayName;

        displayName ??= typeof(T).Name + " " + Constants.DateTimePlaceHolder;

        displayName = displayName.Replace(Constants.DateTimePlaceHolder, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
        return displayName;
    }
}