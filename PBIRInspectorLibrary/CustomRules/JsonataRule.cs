using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Json.Logic;
using Json.More;
using Json.Path;
using Jsonata.Net.Native;
using System.Collections;
using System.Linq.Expressions;

namespace PBIRInspectorLibrary.CustomRules
{
    /// <summary>
    /// Handles the `path` operation.
    /// </summary>
    [Operator("jsonata")]
    [JsonConverter(typeof(JsonataRuleJsonConverter))]
    public class JsonataRule : Json.Logic.Rule
    {
        internal Json.Logic.Rule? Path { get; }
        internal Json.Logic.Rule? DefaultValue { get; }

        internal JsonataRule()
        {
        }
        internal JsonataRule(Json.Logic.Rule path)
        {
            Path = path;
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
            if (Path == null) return data;

            var path = Path.Apply(data, contextData);
            var pathString = path.Stringify()!;
            if (pathString == string.Empty) return contextData ?? data;

            var query = new JsonataQuery(pathString);
            var result = JsonNode.Parse(query.Eval((contextData ?? data).AsJsonString()));

            return result;
        }
    }

    internal class JsonataRuleJsonConverter : WeaklyTypedJsonConverter<JsonataRule>
    {
        public override JsonataRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var parameters = reader.TokenType == JsonTokenType.StartArray
           ? options.ReadArray(ref reader, PBIRInspectorSerializerContext.Default.Rule)
           : new[] { options.Read(ref reader, PBIRInspectorSerializerContext.Default.Rule)! };

            if (parameters is not ({ Length: 1 }))
                throw new JsonException("The path rule needs an array with 1 parameter.");

            return new JsonataRule(parameters[0]);
        }

        public override void Write(Utf8JsonWriter writer, JsonataRule value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();

        }
    }
}
