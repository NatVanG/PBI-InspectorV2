using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBIRInspectorLibrary.Part
{
    internal class CrossItemPartQuery : BasePartQuery
    {
        public CrossItemPartQuery(string fileSystemPath) : base(fileSystemPath)
        {
            SetParts(new Part("root", fileSystemPath, null, PartFileSystemTypeEnum.Folder));
        }

        public override object? Invoke(string query, Part context)
        {
            object? result = null;
            if (string.IsNullOrEmpty(query)) return context;

            result = SearchParts(query, context);
            
            return result;
        }
    }
}
