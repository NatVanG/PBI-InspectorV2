namespace PBIRInspectorLibrary.CustomRules
{
    public class FromYamlOperator : BaseJsonLogicOperator
    {
        public override string OperatorName => "fromyaml";
        public override Type RuleType => typeof(FromYamlRule);
    }
}
