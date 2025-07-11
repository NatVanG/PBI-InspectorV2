using System.Text.Json.Serialization;

namespace PBIRInspectorLibrary.CustomRules
{
    public class ToRecordOperator : BaseJsonLogicOperator
    {
        public override string OperatorName => "torecord";
        public override Type RuleType => typeof(ToRecordRule);
    }
}