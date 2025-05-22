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
            switch (type.ToLower())
            {
                case "report":
                    return new PBIRPartQuery(path);
                //case "copyjob":
                //    //return new CopyJobPartQuery(path);
                //    throw new NotImplementedException("copyjob type is not currently supported.");
                //case "dataflow":
                //    throw new NotImplementedException("dataflow type is not currently supported.");
                //case "variableLibrary":
                //    throw new NotImplementedException("variableLibrary type is not currently supported.");
                default:
                    var generic = new BasePartQuery(path);
                    generic.SetParts(new Part("root", path, null, PartTypeEnum.Folder));
                    return generic;

            }
        }
    }

}
