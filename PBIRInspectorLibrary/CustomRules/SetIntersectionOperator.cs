using System.Text.Json.Serialization;

namespace PBIRInspectorLibrary.CustomRules
{
    public class SetIntersectionOperator : BaseJsonLogicOperator
    {
        public override string OperatorName => "intersect";
        public override Type RuleType => typeof(SetIntersectionRule);
    }
}