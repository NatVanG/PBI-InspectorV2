using System.Text.Json.Serialization;

namespace PBIRInspectorLibrary.CustomRules
{
    public class FileSizeOperator : BaseJsonLogicOperator
    {
        public override string OperatorName => "filesize";
        public override Type RuleType => typeof(FileSizeRule);
    }
}
