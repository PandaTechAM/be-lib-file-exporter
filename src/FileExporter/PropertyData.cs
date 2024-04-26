using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FileExporter
{
    internal class PropertyData
    {
        public PropertyInfo Property { get; internal set; }
        public bool HasBaseConverter { get; internal set; }
        public string Name { get; internal set; }
    }
}
