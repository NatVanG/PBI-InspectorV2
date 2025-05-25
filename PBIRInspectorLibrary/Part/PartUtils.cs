using Json.Pointer;
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
            else
            {
                return ((Part)value).PartType == PartTypeEnum.File ? ToJsonNode((Part)value) : null;
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
                            //TODO: rename to reflect fabric item instead of pbiri. Note breaking changes.
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
