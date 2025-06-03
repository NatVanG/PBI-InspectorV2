using Json.Logic;
using Json.More;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.IO;
using PBIRInspectorLibrary.Exceptions;

namespace PBIRInspectorLibrary.CustomRules
{
    /// <summary>
    /// Handles the `filesize` operation. 
    /// </summary>
    [Operator("filesize")]
    [JsonConverter(typeof(FileSizeRuleJsonConverter))]
    public class FileSizeRule : Json.Logic.Rule
    {
        internal Json.Logic.Rule InputString { get; }

        public FileSizeRule(Json.Logic.Rule inputString)
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
            var inputString = InputString.Apply(data, contextData);
            var filePath = inputString.Stringify();
            int? fileSize = null;

            if (File.Exists(filePath))
            {
                fileSize = (int)new FileInfo(filePath).Length;
            }
            else
            {
                throw new JsonLogicException(string.Format("FileSizeRule - file not found at \"{0}\"." , filePath));
            }

            return fileSize;
        }
    }

    internal class FileSizeRuleJsonConverter : WeaklyTypedJsonConverter<FileSizeRule>
    {
        public override FileSizeRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var parameters = reader.TokenType == JsonTokenType.StartArray
           ? options.ReadArray(ref reader, PBIRInspectorSerializerContext.Default.Rule)
           : new[] { options.Read(ref reader, PBIRInspectorSerializerContext.Default.Rule)! };

            if (parameters is not { Length: 1 })
                throw new JsonException("The FileSizeRule rule needs an array with 1 parameters.");

            if (parameters.Length == 1) return new FileSizeRule(parameters[0]);

            return new FileSizeRule(parameters[0]);
        }

        public override void Write(Utf8JsonWriter writer, FileSizeRule value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
