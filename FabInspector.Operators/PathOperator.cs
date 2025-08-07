using System.Text.Json.Serialization;
using PBIRInspectorLibrary;

namespace FabInspector.Operators;

public class PathOperator : BaseJsonLogicOperator
{
    public override string OperatorName => "path";
    public override Type RuleType => typeof(PathRule);
}