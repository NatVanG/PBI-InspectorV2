using System.Text.Json.Serialization;

namespace PBIRInspectorLibrary.CustomRules
{
    public class RectangleOverlapOperator : BaseJsonLogicOperator
    {
        public override string OperatorName => "rectoverlap";
        public override Type RuleType => typeof(RectOverlapRule);
    }
}