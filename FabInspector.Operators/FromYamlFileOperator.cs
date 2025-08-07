using PBIRInspectorLibrary;

namespace FabInspector.Operators;

public class FromYamlFileOperator : BaseJsonLogicOperator
{
    public override string OperatorName => "fromyamlfile";
    public override Type RuleType => typeof(FromYamlFileRule);
}
