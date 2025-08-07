using PBIRInspectorLibrary;

namespace FabInspector.Operators;

public class ToStringOperator : BaseJsonLogicOperator
{
    public override string OperatorName => "tostring";
    public override Type RuleType => typeof(ToString);
}