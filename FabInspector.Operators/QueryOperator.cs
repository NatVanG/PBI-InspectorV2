using PBIRInspectorLibrary;

namespace FabInspector.Operators;

public class QueryOperator : BaseJsonLogicOperator
{
    public override string OperatorName => "query";
    public override Type RuleType => typeof(QueryRule);
}