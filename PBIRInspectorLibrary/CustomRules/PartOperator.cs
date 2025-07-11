using System.Text.Json.Serialization;

namespace PBIRInspectorLibrary.CustomRules
{
    public class PartOperator : BaseJsonLogicOperator
    {
        public override string OperatorName => "part";
        public override Type RuleType => typeof(PartRule);
    }
}