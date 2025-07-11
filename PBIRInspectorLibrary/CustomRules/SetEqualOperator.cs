using System.Text.Json.Serialization;

namespace PBIRInspectorLibrary.CustomRules
{
    public class SetEqualOperator : BaseJsonLogicOperator
    {
        public override string OperatorName => "equalsets";
        public override Type RuleType => typeof(SetEqualRule);
    }
}