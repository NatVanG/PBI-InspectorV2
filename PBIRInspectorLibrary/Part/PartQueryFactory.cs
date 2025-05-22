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
                default:
                    var generic = new BasePartQuery(path);
                    generic.SetParts(new Part("root", path, null, PartTypeEnum.Folder));
                    return generic;

            }
        }
    }
}
