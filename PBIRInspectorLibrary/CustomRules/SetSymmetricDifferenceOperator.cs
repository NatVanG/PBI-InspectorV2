using System.Text.Json.Serialization;

namespace PBIRInspectorLibrary.CustomRules
{
    public class SetSymmetricDifferenceOperator : BaseJsonLogicOperator
    {
        public override string OperatorName => "symdiff";
        public override Type RuleType => typeof(SetSymmetricDifferenceRule);
    }
}