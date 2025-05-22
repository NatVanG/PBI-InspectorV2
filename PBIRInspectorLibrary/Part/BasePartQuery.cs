using Json.Pointer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace PBIRInspectorLibrary.Part
{
    internal class BasePartQuery(string filePath) : IPartQuery
    {
        private const string UNIQUEPARTMETHODNAME = "UniquePart";
        private const string NAMEPOINTER = "/name";
        private const string DISPLAYNAMEPOINTER = "/displayName";

        public Part RootPart { get; set; }

        public object? Invoke(string query, Part context)
        {
            object? result = null;
            var type = this.GetType();
            System.Reflection.MethodInfo? mi = type.GetMethod(query);
            if (mi != null)
            {
                result = mi.Invoke(this, new object?[] { context });
            }
            else
            {
                mi = type.GetMethod(UNIQUEPARTMETHODNAME);
                if (mi != null)
                {
                    result = mi.Invoke(this, new object?[] { query, context });
                }
            }

            return result;
        }

        public static Part Parent(Part part)
        {
            return part.Parent;
        }

        public static Part TopParent(Part part)
        {
            if (part.Parent == null) return part;
            return TopParent(part.Parent);
        }

        public Part Platform(Part context)
        {
            IEnumerable<Part> q = from p in Part.Flatten(TopParent(context))
                                  where p.PartType == PartTypeEnum.File && p.FileSystemName.EndsWith(".platform")
                                  select p;

            return q.Single();
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

        public static string? TryGetJsonNodeStringValue(JsonNode node, string query)
        {
            JsonPointer pt = JsonPointer.Parse(query);

            if (pt.TryEvaluate(node, out var result))
            {
                if (result is JsonValue val)
                {
                    return val.ToString();
                }
            }

            return null;
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

        protected void SetParts(Part context)
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