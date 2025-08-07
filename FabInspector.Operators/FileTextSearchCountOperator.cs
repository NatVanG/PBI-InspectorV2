using System.Text.Json.Serialization;
using PBIRInspectorLibrary;

namespace FabInspector.Operators;

public class FileTextSearchCountOperator : BaseJsonLogicOperator
{
    public override string OperatorName => "filetextsearchcount";
    public override Type RuleType => typeof(FileTextSearchCountRule);
}