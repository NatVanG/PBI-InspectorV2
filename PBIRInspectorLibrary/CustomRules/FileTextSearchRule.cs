using Json.Logic;
using Json.More;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.IO;
using PBIRInspectorLibrary.Exceptions;
using System.Text.RegularExpressions;

namespace PBIRInspectorLibrary.CustomRules
{
    /// <summary>
    /// Handles the `FileTextSearch` operation. 
    /// </summary>
    [Operator("filetextsearch")]
    [JsonConverter(typeof(FileTextSearchRuleJsonConverter))]
    public class FileTextSearchRule : Json.Logic.Rule
    {
        internal Json.Logic.Rule FilePath { get; }
        internal Json.Logic.Rule PatternString { get; }

        public FileTextSearchRule(Json.Logic.Rule filePath, Json.Logic.Rule patternString)
        {
            FilePath = filePath;
            PatternString = patternString;
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
            var filePath = FilePath.Apply(data, contextData);
            var patternString = PatternString.Apply(data, contextData);

            if (filePath == null) return 0;

            if (filePath is not JsonValue filePathValue || !filePathValue.TryGetValue(out string? stringFilePath))
                throw new JsonLogicException($"filetextsearch rule: filePath parameter value is not a string.");

            if (patternString is not JsonValue regexStringValue || !regexStringValue.TryGetValue(out string? stringPatternString))
                throw new JsonLogicException($"filetextsearch rule: patternString parameter value is not a string.");

            if (!File.Exists(stringFilePath))
            {
                throw new JsonLogicException($"FileTextSearchRule - file not found at \"{stringFilePath}\".");
            }

            if (string.IsNullOrEmpty(stringPatternString))
            {
                throw new JsonLogicException("FileTextSearchRule - pattern string cannot be null or empty.");
            }

            // Read the file content and count matches of the regex pattern
            string fileContent = File.ReadAllText(stringFilePath);
            if (string.IsNullOrEmpty(fileContent))
            {
                throw new JsonLogicException($"FileTextSearchRule - file at \"{stringFilePath}\" is empty.");
            }

            // Use Regex to count occurrences of the pattern in the file content
            var matches = Regex.Count(fileContent, stringPatternString);
            return matches;
        }
    }

    internal class FileTextSearchRuleJsonConverter : WeaklyTypedJsonConverter<FileTextSearchRule>
    {
        public override FileTextSearchRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var parameters = reader.TokenType == JsonTokenType.StartArray
            ? options.ReadArray(ref reader, PBIRInspectorSerializerContext.Default.Rule)
            : new[] { options.Read(ref reader, PBIRInspectorSerializerContext.Default.Rule)! };

            if (parameters is not { Length: 2 })
                throw new JsonException("The FileTextSearch rule needs an array with 2 parameters.");

            if (parameters.Length == 2) return new FileTextSearchRule(parameters[0], parameters[1]);

            return new FileTextSearchRule(parameters[0], parameters[1]);
        }

        public override void Write(Utf8JsonWriter writer, FileTextSearchRule value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
