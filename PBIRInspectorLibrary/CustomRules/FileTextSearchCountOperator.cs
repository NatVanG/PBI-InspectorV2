using System.Text.Json.Serialization;

namespace PBIRInspectorLibrary.CustomRules
{
    public class FileTextSearchCountOperator : BaseJsonLogicOperator
    {
        public override string OperatorName => "filetextsearchcount";
        public override Type RuleType => typeof(FileTextSearchCountRule);
    }
}