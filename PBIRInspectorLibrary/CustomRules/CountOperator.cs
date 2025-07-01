using System.Text.Json.Serialization;

namespace PBIRInspectorLibrary.CustomRules
{
    public class CountOperator : BaseJsonLogicOperator
    {
        public override string OperatorName => "count";
        public override Type RuleType => typeof(CountRule);
    }
}
