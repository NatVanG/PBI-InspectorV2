using System.Text.Json.Serialization;
using PBIRInspectorLibrary;

namespace FabInspector.Operators;

public class SetEqualOperator : BaseJsonLogicOperator
{
    public override string OperatorName => "equalsets";
    public override Type RuleType => typeof(SetEqualRule);
}