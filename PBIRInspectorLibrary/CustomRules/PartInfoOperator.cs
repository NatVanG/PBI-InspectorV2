using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBIRInspectorLibrary.CustomRules
{
    public class PartInfoOperator : BaseJsonLogicOperator
    {
        public override string OperatorName => "partinfo";
        public override Type RuleType => typeof(PartInfoRule);
    }
}
