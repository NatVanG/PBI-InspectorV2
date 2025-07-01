using System.Text.Json.Serialization;

namespace PBIRInspectorLibrary.CustomRules
{
    public class SetUnionOperator : BaseJsonLogicOperator
    {
        public override string OperatorName => "union";
        public override Type RuleType => typeof(SetUnionRule);
    }
}