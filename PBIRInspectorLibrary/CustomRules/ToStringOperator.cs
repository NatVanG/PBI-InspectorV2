using System.Text.Json.Serialization;

namespace PBIRInspectorLibrary.CustomRules
{
    public class ToStringOperator : BaseJsonLogicOperator
    {
        public override string OperatorName => "tostring";
        public override Type RuleType => typeof(ToString);
    }
}