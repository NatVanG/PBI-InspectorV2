﻿using Json.Logic;
using Json.More;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;


namespace PBIRInspectorLibrary.CustomRules;

/// <summary>
/// Handles the `symdiff` operation.
/// </summary>
[Operator("symdiff")]
[JsonConverter(typeof(SetSymmetricDifferenceRuleJsonConverter))]
public class SetSymmetricDifferenceRule : Json.Logic.Rule
{
    internal Json.Logic.Rule Set1 { get; }
    internal Json.Logic.Rule Set2 { get; }

    public SetSymmetricDifferenceRule(Json.Logic.Rule set1, Json.Logic.Rule set2)
    {
        Set1 = set1;
        Set2 = set2;
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
        var set1 = Set1.Apply(data, contextData);
        var set2 = Set2.Apply(data, contextData);

        if (set1 is not JsonArray || set2 is not JsonArray)
            return new JsonArray();

        var arr1 = (JsonArray)set1;
        var arr2 = (JsonArray)set2;

        var symmetricDifference = new JsonArray();

        foreach (var item in arr1)
        {
            if (!arr2.Any(x => item.IsEquivalentTo(x)))
            {
                var copy = item?.DeepClone();
                if (!symmetricDifference.Any(x => x.IsEquivalentTo(copy)))
                {
                    symmetricDifference.Add(copy);
                }
            }
        }

        foreach (var item in arr2)
        {
            if (!arr1.Any(x => item.IsEquivalentTo(x)))
            {
                var copy = item?.DeepClone();
                if (!symmetricDifference.Any(x => x.IsEquivalentTo(copy)))
                {
                    symmetricDifference.Add(copy);
                }
            }
        }

        return symmetricDifference;
    }
}

internal class SetSymmetricDifferenceRuleJsonConverter : WeaklyTypedJsonConverter<SetSymmetricDifferenceRule>
{
    public override SetSymmetricDifferenceRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var parameters = reader.TokenType == JsonTokenType.StartArray
            ? options.ReadArray(ref reader, PBIRInspectorSerializerContext.Default.Rule)
            : new[] { options.Read(ref reader, PBIRInspectorSerializerContext.Default.Rule)! };

        if (parameters is not { Length: 2 })
            throw new JsonException("The symdiff rule needs an array with 2 parameters.");

        return new SetSymmetricDifferenceRule(parameters[0], parameters[1]);
    }

    public override void Write(Utf8JsonWriter writer, SetSymmetricDifferenceRule value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
        //writer.WriteStartObject();
        //writer.WritePropertyName("symdiff");
        //writer.WriteStartArray();
        //writer.WriteRule(value.Set1, options);
        //writer.WriteRule(value.Set2, options);
        //writer.WriteEndArray();
        //writer.WriteEndObject();
    }
}
