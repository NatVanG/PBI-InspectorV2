using System.Text.Json.Serialization;
using PBIRInspectorLibrary;

namespace FabInspector.Operators
{
    public class DrillVariableOperator : BaseJsonLogicOperator
    {
        public override string OperatorName => "drillvar";
        public override Type RuleType => typeof(DrillVariableRule);
    }
}
