using Json.Logic;
using Json.More;
using PBIRInspectorLibrary.Exceptions;
using PBIRInspectorLibrary.Part;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Xml;
using Yaml2JsonNode;
using YamlDotNet.RepresentationModel;

namespace PBIRInspectorLibrary.CustomRules
{
    /// <summary>
    /// Handles the `fromyaml` operation. 
    /// </summary>
    [Operator("fromyaml")]
    [JsonConverter(typeof(FromYamlRuleJsonConverter))]
    public class FromYamlRule : Json.Logic.Rule
    {
        internal Json.Logic.Rule InputString { get; }

        public FromYamlRule(Json.Logic.Rule inputString)
        {
            InputString = inputString;
        }

        /// <summary>
        /// Applies the rule to the input data.
        /// </summary>
        /// <param name="data">The input data.</param>
        /// <param name="contextData">
        ///     Optional secondary data.  Used by a few operators to pass a secondary
        ///     data context to inner operators.
        /// </param>
        /// <returns>The result of the rule.</returns>
        public override JsonNode? Apply(JsonNode? data, JsonNode? contextData = null)
        {
            var node = InputString.Apply(data, contextData);

            if (node is JsonArray) throw new JsonException("The FromYamlRule filePath parameter cannot be an array.");

            var partInfo = PartUtils.TryGetPartInfo(node, setAdvancedProperties: true);

            if (partInfo == null || !partInfo.Exists || partInfo.PartFileSystemType != PartFileSystemTypeEnum.File) { throw new JsonLogicException($"FromYamlRule - file not found. Try using in conjunction with partinfo rule, instead of the part rule."); }


            // Load the YAML file into the YamlStream
            var stream = new YamlStream();
            try
            {
                //stream.Load(new StringReader(File.ReadAllText(partInfo.FileSystemPath)));
               
                using var fileStream = new FileStream(partInfo.FileSystemPath, FileMode.Open, FileAccess.Read);
                using var yamlReader = new StreamReader(fileStream);
                stream.Load(yamlReader);
            }
            catch (YamlDotNet.Core.YamlException ex)
            {
                throw new JsonLogicException($"FromYamlRule - error loading YAML file: {ex.Message}");
            }

            //iterate through the stream.Documents and append each document's JsonNode to a JsonArray

            if (stream.Documents.Count == 0)
            {
                return null;
            }
            if (stream.Documents.Count == 1)
            {
                return stream.Documents[0].ToJsonNode();
            }
            else
            {
                var jsonArray = new JsonArray();
                foreach (var document in stream.Documents)
                {
                    var jsonNode = document.ToJsonNode();
                    jsonArray.Add(jsonNode);
                }
                return jsonArray;
            }
        }
    }

    internal class FromYamlRuleJsonConverter : WeaklyTypedJsonConverter<FromYamlRule>
    {
        public override FromYamlRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var parameters = reader.TokenType == JsonTokenType.StartArray
           ? options.ReadArray(ref reader, PBIRInspectorSerializerContext.Default.Rule)
           : new[] { options.Read(ref reader, PBIRInspectorSerializerContext.Default.Rule)! };

            if (parameters is not { Length: 1 })
                throw new JsonException("The FromYamlRule rule needs an array with 1 parameters.");

            if (parameters.Length == 1) return new FromYamlRule(parameters[0]);

            return new FromYamlRule(parameters[0]);
        }

        public override void Write(Utf8JsonWriter writer, FromYamlRule value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}