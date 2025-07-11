using System.Text.Json.Serialization;

namespace PBIRInspectorLibrary.CustomRules
{
    public class DrillVariableOperator : BaseJsonLogicOperator
    {
        public override string OperatorName => "drillvar";
        public override Type RuleType => typeof(DrillVariableRule);
    }
}
