using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FileExporter.Rules;

internal sealed class ExportRuleRegistry : IExportRuleRegistry
{
   private readonly Dictionary<Type, object> _rules = new();

   public ExportRuleRegistry(IEnumerable<Assembly> assemblies)
   {
      foreach (var assembly in assemblies.Distinct())
      {
         var types = assembly
                     .GetTypes()
                     .Where(t => t is { IsClass: true, IsAbstract: false } && InheritsExportRule(t));

         foreach (var type in types)
         {
            var baseType = GetExportRuleBase(type);
            if (baseType == null)
            {
               continue;
            }

            var modelType = baseType.GetGenericArguments()[0];
            var instance = Activator.CreateInstance(type)!;
            _rules[modelType] = instance;
         }
      }
   }

   public ExportRule<T> GetRule<T>() where T : class
   {
      var key = typeof(T);

      if (_rules.TryGetValue(key, out var rule))
      {
         return (ExportRule<T>)rule;
      }

      // fallback: convention-only rule
      return new ConventionOnlyExportRule<T>();
   }

   public bool TryGetRule<T>(out ExportRule<T>? rule) where T : class
   {
      var key = typeof(T);

      if (_rules.TryGetValue(key, out var obj))
      {
         rule = (ExportRule<T>)obj;
         return true;
      }

      rule = null;
      return false;
   }

   private static bool InheritsExportRule(Type type)
   {
      return GetExportRuleBase(type) != null;
   }

   private static Type? GetExportRuleBase(Type type)
   {
      var current = type;

      while (current != typeof(object))
      {
         if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(ExportRule<>))
         {
            return current;
         }

         current = current.BaseType!;
      }

      return null;
   }
}