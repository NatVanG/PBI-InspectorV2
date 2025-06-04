using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace PBIRInspectorLibrary.Part
{
    public interface IPartQuery
    {
        public abstract Part RootPart { get; set; }

        public abstract object? Invoke(string query, Part context);

        public abstract string PartName(Part context);

        public abstract string PartDisplayName(Part context);
    }
}
