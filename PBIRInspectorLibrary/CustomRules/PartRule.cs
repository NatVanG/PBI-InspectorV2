﻿using Json.Logic;
using Json.Logic.Rules;
using Json.More;
using PBIRInspectorLibrary.Part;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;


namespace PBIRInspectorLibrary.CustomRules;

/// <summary>
/// Handles the `var` operation.
/// </summary>
[Operator("part")]
[JsonConverter(typeof(PartRuleJsonConverter))]
public class PartRule : Json.Logic.Rule
{
    internal Json.Logic.Rule Input { get; }

    internal PartRule(Json.Logic.Rule input)
    {
        Input = input;
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
		JsonNode? result = null;
        var input = Input.Apply(data, contextData);

        if (input is null) throw new ArgumentException("PartRule input cannot be null");
        var stringInput = input.Stringify();

        var contextPartQuery = ContextService.GetInstance().PartQuery;
        var contextPart = ContextService.GetInstance().Part;
		result = contextPartQuery.ToJsonNode(contextPartQuery.Invoke(stringInput, contextPart));

        return result;
    }
}

internal class PartRuleJsonConverter : WeaklyTypedJsonConverter<PartRule>
{
	public override PartRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
        var parameters = reader.TokenType == JsonTokenType.StartArray
           ? options.ReadArray(ref reader, PBIRInspectorSerializerContext.Default.Rule)
           : new[] { options.Read(ref reader, PBIRInspectorSerializerContext.Default.Rule)! };

        if (parameters is not ({ Length: 1 }))
			throw new JsonException("The part rule needs an array with 1 parameter.");

		return new PartRule(parameters[0]);
	}

	public override void Write(Utf8JsonWriter writer, PartRule value, JsonSerializerOptions options)
	{
		//throw new NotImplementedException();
	}
}