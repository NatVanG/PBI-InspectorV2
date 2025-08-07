using System.Text.Json.Serialization;
using PBIRInspectorLibrary;

namespace FabInspector.Operators;

public class SetUnionOperator : BaseJsonLogicOperator
{
    public override string OperatorName => "union";
    public override Type RuleType => typeof(SetUnionRule);
}