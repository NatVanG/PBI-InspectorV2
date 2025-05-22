using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBIRInspectorLibrary.Part
{
    internal class CopyJobPartQuery : BasePartQuery
    {
        public CopyJobPartQuery(string path) : base(path)
        {
            if (path == null || path.Length == 0) throw new ArgumentNullException(nameof(path));
            if (!Directory.Exists(path)) throw new ArgumentException($"Directory {path} does not exist");

            this.RootPart = new Part("root", path, null, PartTypeEnum.Folder);
            SetParts(this.RootPart);
        }

    }
}
