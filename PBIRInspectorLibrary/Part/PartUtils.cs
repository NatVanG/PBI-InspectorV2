using Json.Path;
using Json.Pointer;
using Jsonata.Net.Native.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace PBIRInspectorLibrary.Part
{
    public class PartUtils
    {
        public static JsonNode ToJsonNode(object? value)
        {
            if (value == null) return null;

            if (value is List<Part>)
            {
                return ToJsonArray(value as List<Part>);
            }
            else if (value is Part)
            {
                return ((Part)value).PartType == PartTypeEnum.File ? ToJsonNode((Part)value) : null;
            }
            else
            {
                //if the value is not a Part or List<Part>, we return it as a JsonValue
                return JsonValue.Create(value);
            }
        }

        public static JsonArray ToJsonArray(List<Part> parts)
        {
            if (parts == null || parts.Count == 0) return new JsonArray();
            return new JsonArray(parts.Select(_ => ToJsonNode(_)?.DeepClone()).ToArray());
        }

        //returns null if the file does not exist or is not a json file or if it's a directory
        public static JsonNode? ToJsonNode(Part context)
        {
            if (context == null) return null;
            if (context.JsonContent != null) return context.JsonContent;

            JsonNode? node = null;


            try
            {
                if (File.Exists(context.FileSystemPath))
                {
                    node = JsonNode.Parse(File.ReadAllText(context.FileSystemPath));
                }

                if (Directory.Exists(context.FileSystemPath))
                {
                    //if the path is a directory, we cannot parse it as JSON, so we return an annotation with the file system path
                    node = FileSystemPathAnnotations(context.FileSystemPath, context.FileSystemName);
                }
            }
            catch (System.Text.Json.JsonException)
            {
                //this is not a json file or not a valid json file so add annotation with the file system path; this is so JsonLogic rules can still be applied
                node = FileSystemPathAnnotations(context.FileSystemPath, context.FileSystemName);
            }
            finally
            {
                context.JsonContent = node;
            }
            

            return node;
        }

        private static JsonNode FileSystemPathAnnotations(string fileSystemPath, string fileSystemName)
        {
            var node = new JsonObject();

            if (node is JsonObject jsonObject)
            {
                var annotations = new JsonArray
                        {
                            //TODO: Legacy, deprecate.
                            new JsonObject { ["name"] = "pbiri_filesystempath", ["value"] =  fileSystemPath },
                            new JsonObject { ["name"] = "pbiri_filesystemname", ["value"] =  fileSystemName }
                };
                jsonObject["annotations"] = annotations;
            }

            return node;
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
    }
}
