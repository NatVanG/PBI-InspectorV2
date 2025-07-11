using System.Text.Json.Serialization;

namespace PBIRInspectorLibrary.CustomRules
{
    public class QueryOperator : BaseJsonLogicOperator
    {
        public override string OperatorName => "query";
        public override Type RuleType => typeof(QueryRule);
    }
}