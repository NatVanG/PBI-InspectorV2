using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBIRInspectorLibrary.Part
{
    internal static class ContextService
    {
        private static readonly ThreadLocal<PartContext> _current = new();

        public static PartContext Current
        {
            get => _current.Value;
            set => _current.Value = value;
        }
    }
}
