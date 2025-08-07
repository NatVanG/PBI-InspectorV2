using PBIRInspectorLibrary;

namespace FabInspector.Operators;

public class ToRecordOperator : BaseJsonLogicOperator
{
    public override string OperatorName => "torecord";
    public override Type RuleType => typeof(ToRecordRule);
}