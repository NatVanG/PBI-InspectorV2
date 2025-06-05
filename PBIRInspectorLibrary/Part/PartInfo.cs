using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBIRInspectorLibrary.Part
{
    public class PartInfo
    { 
        public string FileSystemName { get; set; }
        public string FileSystemPath { get; set; }
        public PartFileSystemTypeEnum PartFileSystemType { get; private set; }
        public long? FileSize { get; private set; }
        public int? FileCount { get; private set; }

        public PartInfo(Part part)
        {
            FileSystemName = part.FileSystemName;
            FileSystemPath = part.FileSystemPath;
            PartFileSystemType = part.PartFileSystemType;

            if (File.Exists(FileSystemPath))
            {
                FileInfo fileInfo = new FileInfo(FileSystemPath);
                this.FileCount = 1;
                this.FileSize = fileInfo.Length;
            }

            if (Directory.Exists(FileSystemPath))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(FileSystemPath);
                var files = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
                this.FileCount  = files.Length;
                this.FileSize = files.Sum(f => f.Length);
            }
        }
    }
}
