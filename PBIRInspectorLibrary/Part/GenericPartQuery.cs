using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBIRInspectorLibrary.Part
{
    internal class GenericPartQuery : BasePartQuery
    {
        public GenericPartQuery(string fileSystemPath) : base(fileSystemPath)
        {
            SetParts(new Part("root", fileSystemPath, null, PartTypeEnum.Folder));
        }
    }
}
