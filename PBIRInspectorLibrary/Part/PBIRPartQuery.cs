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
    internal class PBIRPartQuery : BasePartQuery, IPBIPartQuery
    {
        private const string NAMEPOINTER = "/name";
        private const string DISPLAYNAMEPOINTER = "/displayName";

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

        //TODO: implement for folders
        public string PartName(Part context)
        {
            string? val = null;

            if (context.PartType == PartTypeEnum.File && context.FileSystemName.EndsWith(".json"))
            {
                var node = ToJsonNode(context);
                val = TryGetJsonNodeStringValue(node, NAMEPOINTER);
            }

            return val ?? context.FileSystemName;
        }

        //TODO: implement for folders
        public string PartDisplayName(Part context)
        {
            string? val = null;

            if (context.PartType == PartTypeEnum.File && context.FileSystemName.EndsWith(".json"))
            {
                var node = ToJsonNode(context);
                val = TryGetJsonNodeStringValue(node, DISPLAYNAMEPOINTER);
            }

            return val ?? context.FileSystemName;
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

        public List<Part> Files(Part context)
        {
            IEnumerable<Part> q = from p in Part.Flatten(context.PartType == PartTypeEnum.File ? context.Parent : context)
                                  where p.PartType == PartTypeEnum.File
                                  select p;

            return q.ToList();
        }

        public Part? UniquePart(string query, Part context)
        {
            IEnumerable<Part> q = from p in Part.Flatten(TopParent(context))
                                            where p.FileSystemName.EndsWith(query, StringComparison.InvariantCultureIgnoreCase)
                                            select p;

            return q.SingleOrDefault();
        }

        public JsonNode ToJsonNode(object? value)
        {
            if (value == null) return null;

            if (value is List<Part>)
            {
                return ToJsonArray(value as List<Part>);
            }
            else
            {
                return ((Part)value).PartType == PartTypeEnum.File ? ToJsonNode((Part)value) : null;
            }
        }

        public JsonArray ToJsonArray(List<Part> parts)
        {
            if (parts == null || parts.Count == 0) return new JsonArray();
            return new JsonArray(parts.Select(_ => ToJsonNode(_)?.DeepClone()).ToArray());
        }

        //returns null if the file does not exist or is not a json file or if it's a directory
        public JsonNode? ToJsonNode(Part context)
        {
            if (context == null) return null;
            if (context.JsonContent != null) return context.JsonContent;

            JsonNode? node = null;
            
            if (File.Exists(context.FileSystemPath))
            {
                try
                {
                    node = JsonNode.Parse(File.ReadAllText(context.FileSystemPath));
                }
                catch (System.Text.Json.JsonException)
                {
                    //this is not a json file so add annotation with the file system path; this is so JsonLogic rules can still be applied
                    node = new JsonObject();

                    if (node is JsonObject jsonObject)
                    {
                        var annotations = new JsonArray
                        {
                            new JsonObject { ["name"] = "pbiri_filesystempath", ["value"] =  context.FileSystemPath },
                            new JsonObject { ["name"] = "pbiri_filesystemname", ["value"] =  context.FileSystemName }
                        };
                        jsonObject["annotations"] = annotations;
                    }
                }
                finally
                {
                    context.JsonContent = node;
                }
            }

            return node;
        }

        private void SetParts(Part context)
        {
            if (!Directory.Exists(context.FileSystemPath)) return;

            context.Parts = new List<Part>();

            foreach (string filePath in Directory.GetFiles(context.FileSystemPath))
            {
                FileInfo fileInfo = new FileInfo(filePath);
                Part filePart = new Part(fileInfo.Name, fileInfo.FullName, context, PartTypeEnum.File);
                context.Parts.Add(filePart);
            }

            foreach (string dirPath in Directory.GetDirectories(context.FileSystemPath))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
                Part dirPart = new Part(dirInfo.Name, dirInfo.FullName, context, PartTypeEnum.Folder);
                context.Parts.Add(dirPart);
                SetParts(dirPart);
            }
        }
    }
}