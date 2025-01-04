using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Logic.Rules;

namespace PBIXInspectorLibrary;

[JsonSerializable(typeof(Json.Logic.Rule))]
[JsonSerializable(typeof(CustomRules.CountRule))]
[JsonSerializable(typeof(CustomRules.DrillVariableRule))]
[JsonSerializable(typeof(CustomRules.IsNullOrEmptyRule))]
[JsonSerializable(typeof(CustomRules.PartRule))]
[JsonSerializable(typeof(CustomRules.PathRule))]
[JsonSerializable(typeof(CustomRules.QueryRule))]
[JsonSerializable(typeof(CustomRules.RectOverlapRule))]
[JsonSerializable(typeof(CustomRules.SetDifferenceRule))]
[JsonSerializable(typeof(CustomRules.SetEqualRule))]
[JsonSerializable(typeof(CustomRules.SetIntersectionRule))]
[JsonSerializable(typeof(CustomRules.SetSymmetricDifferenceRule))]
[JsonSerializable(typeof(CustomRules.SetIntersectionRule))]
[JsonSerializable(typeof(CustomRules.SetUnionRule))]
[JsonSerializable(typeof(CustomRules.StringContains))]
[JsonSerializable(typeof(CustomRules.ToRecordRule))]
[JsonSerializable(typeof(CustomRules.ToString))]
internal partial class PBIRInspectorSerializerContext : JsonSerializerContext;
