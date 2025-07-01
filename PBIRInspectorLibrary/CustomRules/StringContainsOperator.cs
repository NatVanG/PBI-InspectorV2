using PBIRInspectorLibrary;
using PBIRInspectorLibrary.CustomRules;
using System.Text.Json.Serialization;

public class StringContainsOperator : BaseJsonLogicOperator
{
    public override string OperatorName => "strcontains";
    public override Type RuleType => typeof(StringContains);

    //public void Register(JsonSerializerContext context)
    //{
    //    if (context == null) throw new ArgumentNullException(nameof(context));
    //    if (Json.Logic.RuleRegistry.GetRule(OperatorName) == null)
    //    {
    //        Json.Logic.RuleRegistry.AddRule<StringContains>(context);
    //    }
    //    else
    //    {
    //        //TODO: Handle the case where the operator is already registered.
    //        //throw new InvalidOperationException($"Operator '{OperatorName}' is already registered.");
    //    }

    //}
}