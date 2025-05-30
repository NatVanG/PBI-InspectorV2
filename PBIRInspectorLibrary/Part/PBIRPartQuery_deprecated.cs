using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBIRInspectorLibrary.Part
{
    internal class PBIRPartQuery_deprecated : PBIRPartQuery
    {
        public PBIRPartQuery_deprecated(string fileSystemPath) : base(fileSystemPath)
        {
            SetParts(new Part("root", fileSystemPath, null, PartTypeEnum.Folder));
        }

        //TODO: deprecate this behaviour. Currently preserving the original behaviour of returning a single item instead of a list to maintain compatibility with any existing rules.
        private protected override object? SearchParts(string query, Part context)
        {
            var results = base.SearchParts(query, context);
            if (results is null) return null;
            if (results is List<Part> parts && parts.Count == 1) return parts.SingleOrDefault();
            return results;
        }
    }
}
