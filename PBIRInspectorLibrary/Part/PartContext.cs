using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBIRInspectorLibrary.Part
{
    internal class PartContext
    {
        public IPartQuery PartQuery { get; set; }
        public Part Part { get; set; }
    }
}