using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace PBIRInspectorLibrary.Part
{
    internal static class PartQueryFactory
    {
        internal static IPartQuery CreatePartQuery(string type, string path)
        {
            switch (type.ToLowerInvariant())
            {
                case "report":
                    return new PBIRPartQuery(path);
                case "report_deprecated":
                    return new PBIRPartQuery_deprecated(path);
                default:
                    return new GenericPartQuery(path);
            }
        }
    }
}
