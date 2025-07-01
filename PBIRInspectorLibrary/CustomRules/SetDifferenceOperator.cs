using System.Text.Json.Serialization;

namespace PBIRInspectorLibrary.CustomRules
{
    public class SetDifferenceOperator : BaseJsonLogicOperator
    {
        public override string OperatorName => "diff";
        public override Type RuleType => typeof(SetDifferenceRule);
    }
}