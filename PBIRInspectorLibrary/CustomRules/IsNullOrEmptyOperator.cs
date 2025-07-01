using System.Text.Json.Serialization;

namespace PBIRInspectorLibrary.CustomRules
{
    public class IsNullOrEmptyOperator : BaseJsonLogicOperator
    {
        public override string OperatorName => "isnullorempty";
        public override Type RuleType => typeof(IsNullOrEmptyRule);
    }
}
