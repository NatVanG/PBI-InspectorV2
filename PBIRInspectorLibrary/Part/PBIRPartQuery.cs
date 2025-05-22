using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json.Nodes;
using System.ComponentModel;
using PBIRInspectorLibrary.Part;
using System.Reflection;
using System.IO;
using System.Runtime.CompilerServices;
using Json.More;

namespace PBIRInspectorLibrary.Part
{
    /// <summary>
    /// https://learn.microsoft.com/en-us/power-bi/developer/projects/projects-report#pbir-format
    /// </summary>
    internal class PBIRPartQuery : BasePartQuery
    {
        //TODO: harden logic to extract path value here.
        private const string REPORTFOLDERPOINTER = "/artifacts/0/report/path";
        private const string PBIPEXT = ".pbip";
        private const string PBIXEXT = ".pbix";

        public PBIRPartQuery(string path) : base(path)
        {
            if (path == null || path.Length == 0) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path) && path.EndsWith(PBIPEXT)) throw new ArgumentException($"PBI Desktop file {path} does not exist");
            if (path.ToLower().EndsWith(PBIXEXT)) throw new ArgumentException($"PBIX files are not currently supported, please specify a PBIP");
            if (File.Exists(path) && !path.ToLower().EndsWith(PBIPEXT)) throw new ArgumentException($"PBI Desktop file {path} must have .pbip extension");
            if (!File.Exists(path) && !Directory.Exists(path)) throw new ArgumentException($"{path} does not exist");

            string? reportFolderPath = null;

            if (File.Exists(path) && path.EndsWith(PBIPEXT))
            {
                var pbip = new Part("pbip", path);
                reportFolderPath = ReportPath(pbip);
            }
            else if (Directory.Exists(path))
            {
                reportFolderPath = path;
            }

            this.RootPart = new Part("root", reportFolderPath!, null, PartTypeEnum.Folder);
            SetParts(this.RootPart);
        }

        //TODO: add support for pbir or folder.
        private string ReportPath(Part context)
        {
            var node = ToJsonNode(context);
            var val = TryGetJsonNodeStringValue(node, REPORTFOLDERPOINTER);

            val = Path.Combine(Path.GetDirectoryName(context.FileSystemPath), val);

            return val;
        }

        public Part Report(Part context)
        {
            IEnumerable<Part> q = from p in Part.Flatten(TopParent(context))
                                          where p.PartType == PartTypeEnum.File && p.FileSystemName.EndsWith("report.json")
                                          select p;

            return q.Single();
        }


        public Part? ReportExtensions(Part context)
        {
            IEnumerable<Part> q = from p in Part.Flatten(TopParent(context))
                                  where p.PartType == PartTypeEnum.File && p.FileSystemName.EndsWith("reportExtensions.json")
                                  select p;

            return q.SingleOrDefault();
        }

        public Part Version(Part context)
        {
            IEnumerable<Part> q = from p in Part.Flatten(TopParent(context))
                                          where p.PartType == PartTypeEnum.File && p.FileSystemName.EndsWith("version.json")
                                          select p;

            return q.Single();
        }

        public Part PagesHeader(Part context)
        {
            IEnumerable<Part> q = from p in Part.Flatten(TopParent(context))
                                          where p.PartType == PartTypeEnum.File && p.FileSystemName.EndsWith("pages.json")
                                          select p;

            return q.Single();
        }

        public List<Part> Pages(Part context)
        {
            IEnumerable<Part> q = from p in Part.Flatten(context.PartType == PartTypeEnum.File ? context.Parent : context)
                                          where p.PartType == PartTypeEnum.File && p.FileSystemName.EndsWith("page.json")
                                          select p;

            return q.ToList();
        }

        public List<Part> AllVisuals(Part context)
        {
            return Visuals(TopParent(context));
        }

        public List<Part> Visuals(Part context)
        {
            IEnumerable<Part> q = from p in Part.Flatten(context.PartType == PartTypeEnum.File ? context.Parent : context)
                                            where p.PartType == PartTypeEnum.File && p.FileSystemName.EndsWith("visual.json")
                                            select p;

            return q.ToList();
        }

        public List<Part> AllMobileVisuals(Part context)
        {
            return MobileVisuals(TopParent(context));
        }

        public List<Part> MobileVisuals(Part context)
        {
            IEnumerable<Part> q = from p in Part.Flatten(context.PartType == PartTypeEnum.File ? context.Parent : context)
                                            where p.PartType == PartTypeEnum.File && p.FileSystemName.EndsWith("mobile.json")
                                            select p;

            return q.ToList();
        }

        public Part BookmarksHeader(Part context)
        {
            IEnumerable<Part> q = from p in Part.Flatten(TopParent(context))
                                            where p.PartType == PartTypeEnum.File && p.FileSystemName.EndsWith("bookmarks.json")
                                            select p;

            return q.Single();
        }


        public List<Part> AllBookmarks(Part context)
        {
            return Bookmarks(TopParent(context));
        }

        public List<Part> Bookmarks(Part context)
        {
            IEnumerable<Part> q = from p in Part.Flatten(context.PartType == PartTypeEnum.File ? context.Parent : context)
                                            where p.PartType == PartTypeEnum.File && p.FileSystemName.EndsWith("bookmark.json")
                                            select p;

            return q.ToList();
        }

        
    }
}