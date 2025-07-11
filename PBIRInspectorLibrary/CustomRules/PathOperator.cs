using System.Text.Json.Serialization;

namespace PBIRInspectorLibrary.CustomRules
{
    public class PathOperator : BaseJsonLogicOperator
    {
        public override string OperatorName => "path";
        public override Type RuleType => typeof(PathRule);
    }
}