using System.Text.Json.Serialization;
using PBIRInspectorLibrary;

namespace FabInspector.Operators;

public class IsNullOrEmptyOperator : BaseJsonLogicOperator
{
    public override string OperatorName => "isnullorempty";
    public override Type RuleType => typeof(IsNullOrEmptyRule);
}
