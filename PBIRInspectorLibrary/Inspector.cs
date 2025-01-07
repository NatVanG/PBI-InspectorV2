using Json.Logic;
using Json.More;
using Json.Patch;
using Json.Path;
using Json.Pointer;
using PBIRInspectorLibrary.Exceptions;
using PBIRInspectorLibrary.Output;
using PBIRInspectorLibrary.Part;
using System.Data;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PBIRInspectorLibrary
{
    /// <summary>
    /// Iterates through input rules and runs them against input PBI files
    /// </summary>
    public class Inspector : InspectorBase
    {
        private const string JSONPOINTERSTART = "/";
        private const string CONTEXTNODE = ".";
        internal const char DRILLCHAR = '>';

        private string? _pbiFilePath, _rulesFilePath;
        private InspectionRules? _inspectionRules;

        public event EventHandler<MessageIssuedEventArgs>? MessageIssued;

        public Inspector() : base()
        {
            AddCustomRulesToRegistry();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pbiFilePath"></param>
        /// <param name="inspectionRules"></param>
        public Inspector(string pbiFilePath, InspectionRules inspectionRules) : base(pbiFilePath, inspectionRules)
        {
            this._pbiFilePath = pbiFilePath;
            this._inspectionRules = inspectionRules;
            AddCustomRulesToRegistry();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pbiFilePath">Local PBI file path</param>
        /// <param name="rulesFilePath">Local rules json file path</param>
        public Inspector(string pbiFilePath, string rulesFilePath) : base(pbiFilePath, rulesFilePath)
        {
            this._pbiFilePath = pbiFilePath;
            this._rulesFilePath = rulesFilePath;

            try
            {
                var inspectionRules = this.DeserialiseRulesFromFilePath<InspectionRules>(rulesFilePath);

                if (inspectionRules == null || inspectionRules.Rules == null || inspectionRules.Rules.Count == 0)
                {
                    throw new PBIRInspectorException(string.Format("No rule definitions were found within rules file at \"{0}\".", rulesFilePath));
                }
                else
                {
                    this._inspectionRules = inspectionRules;
                }
            }
            catch (System.IO.FileNotFoundException e)
            {
                throw new PBIRInspectorException(string.Format("Rules file with path \"{0}\" not found.", rulesFilePath), e);
            }
            catch (System.Text.Json.JsonException e)
            {
                throw new PBIRInspectorException(string.Format("Could not deserialise rules file with path \"{0}\". Check that the file is valid json and following the correct schema for PBI Inspector rules.", rulesFilePath), e);
            }

            AddCustomRulesToRegistry();
        }

        public List<TestResult> Inspect()
        {
            var testResults = new List<TestResult>();
            var rules = this._inspectionRules.Rules.Where(_ => !_.Disabled);


            //TODO: determine which flavour of IPBIPartQuery to instantiate.
            IPBIPartQuery partQuery = new PBIRPartQuery(_pbiFilePath);
            ContextService.GetInstance().PartQuery = partQuery;


            foreach (var rule in rules)
            {
                ContextService.GetInstance().Part = partQuery.RootPart;

                OnMessageIssued(MessageTypeEnum.Information, string.Format("Running Rule \"{0}\".", rule.Name));
                Json.Logic.Rule? jrule = null;

                try
                {
                    jrule = System.Text.Json.JsonSerializer.Deserialize<Json.Logic.Rule>(rule.Test.Logic);
                }
                catch (System.Text.Json.JsonException e)
                {
                    OnMessageIssued(MessageTypeEnum.Error, string.Format("Parsing of logic for rule \"{0}\" failed, resuming to next rule.", rule.Name));
                    continue;
                }

                bool result = false;

                try
                {
                    var parts = new List<Part.Part>();

                    if (!string.IsNullOrEmpty(rule.Part))
                    {
                        var part = partQuery.Invoke(rule.Part, ContextService.GetInstance().Part);

                        if (part is List<Part.Part>)
                        {
                            parts.AddRange((List<Part.Part>)part);
                        }
                        else
                        {
                            parts.Add((Part.Part)part);
                        }
                    }
                    else
                    {
                        parts.Add(partQuery.RootPart);
                    }

                    foreach (var part in parts)
                    {
                        if (part == null)
                        {
                            MessageTypeEnum msgType = rule.PathErrorWhenNoMatch ? MessageTypeEnum.Error : MessageTypeEnum.Warning;
                            OnMessageIssued(msgType, string.Format("Rule \"{0}\" - Part(s) \"{1}\" not found.", rule.Name, rule.Part));
                            continue;
                        }

                        ContextService.GetInstance().Part = part;
                        var node = partQuery.ToJsonNode(part);
                        var newdata = MapRuleDataPointersToValues(node, rule, node);

                        var parentPageName = part.FileSystemName.ToLowerInvariant().EndsWith("page.json") ? partQuery.PartName(part) : null; 
                        var parentPageDisplayName = part.FileSystemName.ToLowerInvariant().EndsWith("page.json") ? partQuery.PartDisplayName(part) ?? partQuery.PartName(part) : "N/A";

                        var jruleresult = jrule.Apply(newdata);
                        result = rule.Test.Expected.IsEquivalentTo(jruleresult);
                        var ruleLogType = ConvertRuleLogType(rule.LogType);
                        string resultString = string.Format("Rule \"{0}\" {1} with result: {2}, expected: {3}.", rule != null ? rule.Name : string.Empty, result ? "PASSED" : "FAILED", jruleresult != null ? jruleresult.ToString() : string.Empty, rule.Test.Expected != null ? rule.Test.Expected.ToString() : string.Empty);
                        testResults.Add(new TestResult { RuleId = rule.Id, RuleName = rule.Name, LogType = ruleLogType, RuleDescription = rule.Description, ParentName = parentPageName, ParentDisplayName = parentPageDisplayName, Pass = result, Message = resultString, Expected = rule.Test.Expected, Actual = jruleresult });

                        //PATCH
                        if (!result && rule.ApplyPatch && rule.Patch != null && rule.Patch.Ops != null)
                        {
                            if (jruleresult != null && jruleresult is JsonArray arr)
                            {
                                if (arr.Count() > 0)
                                {
                                    var allPatchParts = (List<Part.Part>)partQuery.Invoke(rule.Patch.PartName, part);
                                    //TODO: use another method to filter parts i.e. other than ToJSonString
                                    var filteredPatchParts = allPatchParts.Where(_ => arr.ToJsonString().Contains(partQuery.PartName(_)));
                                    foreach (var filteredPart in filteredPatchParts)
                                    {
                                        ApplyPatch(partQuery, rule, filteredPart);
                                    }
                                }
                                else
                                {
                                    var patchPart = (Part.Part)partQuery.Invoke(rule.Patch.PartName, part);
                                    ApplyPatch(partQuery, rule, patchPart);
                                }
                            }
                        }
                    }
                }
                catch (PBIRInspectorException e)
                {
                    testResults.Add(new TestResult { RuleId = rule.Id, RuleName = rule.Name, LogType = MessageTypeEnum.Error, RuleDescription = rule.Description, Pass = false, Message = e.Message, Expected = rule.Test.Expected, Actual = null });
                    continue;
                }

            }
            return testResults;
        }

        private void ApplyPatch(IPBIPartQuery partQuery, Rule? rule, Part.Part? partToPatch)
        {
            var node = partQuery.ToJsonNode(partToPatch);
            var patchResult = rule.Patch.Ops.Apply(node);
            if (patchResult.IsSuccess)
            {
                partToPatch.JsonContent = patchResult.Result;
                partToPatch.Save();
            }
            else
            {
                OnMessageIssued(MessageTypeEnum.Error, string.Format("Rule \"{0}\" - Patch failed for part \"{1}\". Error: \"{2}\".", rule.Id, partToPatch.FileSystemPath, patchResult.Error));
            }
        }

        protected void OnMessageIssued(MessageTypeEnum messageType, string message)
        {
            var args = new MessageIssuedEventArgs(message, messageType);
            OnMessageIssued(args);
        }

        protected virtual void OnMessageIssued(MessageIssuedEventArgs e)
        {
            EventHandler<MessageIssuedEventArgs>? handler = MessageIssued;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #region private methods

        private void AddCustomRulesToRegistry()
        {
            PBIRInspectorSerializerContext context = new PBIRInspectorSerializerContext();
            Json.Logic.RuleRegistry.AddRule<CustomRules.IsNullOrEmptyRule>(context);
            Json.Logic.RuleRegistry.AddRule<CustomRules.CountRule>(context);
            Json.Logic.RuleRegistry.AddRule<CustomRules.StringContains>(context);
            Json.Logic.RuleRegistry.AddRule<CustomRules.ToString>(context);
            Json.Logic.RuleRegistry.AddRule<CustomRules.ToRecordRule>(context);
            Json.Logic.RuleRegistry.AddRule<CustomRules.DrillVariableRule>(context);
            Json.Logic.RuleRegistry.AddRule<CustomRules.RectOverlapRule>(context);
            Json.Logic.RuleRegistry.AddRule<CustomRules.SetIntersectionRule>(context);
            Json.Logic.RuleRegistry.AddRule<CustomRules.SetUnionRule>(context);
            Json.Logic.RuleRegistry.AddRule<CustomRules.SetDifferenceRule>(context);
            Json.Logic.RuleRegistry.AddRule<CustomRules.SetSymmetricDifferenceRule>(context);
            Json.Logic.RuleRegistry.AddRule<CustomRules.SetEqualRule>(context);
            Json.Logic.RuleRegistry.AddRule<CustomRules.PartRule>(context);
            Json.Logic.RuleRegistry.AddRule<CustomRules.QueryRule>(context);
            Json.Logic.RuleRegistry.AddRule<CustomRules.PathRule>(context);
        }

        private MessageTypeEnum ConvertRuleLogType(string ruleLogType)
                {
                    if (string.IsNullOrEmpty(ruleLogType)) return MessageTypeEnum.Warning;

                    MessageTypeEnum logType;

                    switch (ruleLogType.ToLower().Trim())
                    {
                        case "error":
                            logType = MessageTypeEnum.Error;
                            break;
                        case "warning":
                            logType = MessageTypeEnum.Warning;
                            break;
                        default:
                            logType = MessageTypeEnum.Warning;
                            break;
                    }

                    return logType;
                }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        private JsonNode? MapRuleDataPointersToValues(JsonNode? target, Rule rule, JsonNode? contextNode)
        {
            if (target == null || rule.Test.Data == null || rule.Test.Data is not JsonObject) return rule.Test.Data;

            var newdata = new JsonObject();

            var olddata = rule.Test.Data.AsObject();

            if (target != null)
            {
                try
                {
                    if (olddata != null && olddata.Count() > 0)
                    {
                        foreach (var item in olddata)
                        {
                            if (item.Value is JsonValue)
                            {
                                var value = item.Value.AsValue().Stringify();
                                if (value.StartsWith(JSONPOINTERSTART)) //check for JsonPointer syntax
                                {
                                    try
                                    {
                                        //var pointer = JsonPointer.Parse(value);
                                        //var evalsuccess = pointer.TryEvaluate(target, out var newval);
                                        var evalsuccess = EvalPath(value, target, out var newval);
                                        if (evalsuccess)
                                        {
                                            if (newval != null)
                                            {
                                                newdata.Add(new KeyValuePair<string, JsonNode?>(item.Key, newval?.DeepClone()));
                                            }
                                            else
                                            {
                                                //TODO: handle null value?
                                            }
                                        }
                                        else
                                        {
                                            if (rule.PathErrorWhenNoMatch)
                                            {
                                                throw new PBIRInspectorException(string.Format("Rule \"{0}\" - Could not evaluate json pointer \"{1}\".", rule.Name, value));
                                            }
                                            else
                                            {
                                                OnMessageIssued(MessageTypeEnum.Information, string.Format("Rule \"{0}\" - Could not evaluate json pointer \"{1}\".", rule.Name, value));
                                                continue;
                                            }
                                        }
                                    }
                                    catch (PointerParseException e)
                                    {
                                        if (rule.PathErrorWhenNoMatch)
                                        {
                                            throw new PBIRInspectorException(string.Format("Rule \"{0}\" - Pointer parse exception for value \"{1}\".", rule.Name, value));
                                        }
                                        else
                                        {
                                            OnMessageIssued(MessageTypeEnum.Error, string.Format("Rule \"{0}\" - Pointer parse exception for value \"{1}\". Inner Exception: \"{2}\".", rule.Name, value, e.Message));
                                            continue;
                                        }
                                    }
                                }
                                else if (value.Equals(CONTEXTNODE))
                                {
                                    //context array token was used so pass in the parent array
                                    newdata.Add(new KeyValuePair<string, JsonNode?>(item.Key, contextNode?.DeepClone()));
                                }
                                else
                                {
                                    //looks like a literal value
                                    newdata.Add(new KeyValuePair<string, JsonNode?>(item.Key, item.Value?.DeepClone()));
                                }
                            }
                            else
                            {
                                //might be a JsonArray
                                newdata.Add(new KeyValuePair<string, JsonNode?>(item.Key, item.Value?.DeepClone()));
                            }
                        }
                    }
                }
                catch (System.Text.Json.JsonException e)
                {
                    throw new PBIRInspectorException("JsonException", e);
                }

            }

            return newdata;
        }

        private bool EvalPath(string pathString, JsonNode? data, out JsonNode? result)
        {
            if (pathString.Contains(DRILLCHAR))
            {
                var leftString = pathString.Substring(0, pathString.IndexOf(DRILLCHAR));
                var rightString = pathString.Substring(pathString.IndexOf(DRILLCHAR) + 1);

                var leftStringPath = string.Concat(leftString.StartsWith(JSONPOINTERSTART) ? string.Empty : JSONPOINTERSTART, leftString.Replace('.', '/'));
                var pointer = JsonPointer.Parse(leftStringPath);
                if (pointer.TryEvaluate(data, out result))
                {
                    if (result is JsonValue val)
                    {
                        //remove single quotes from beginning and end of string if any.
                        string strVal;
                        if (val.ToString()!.StartsWith("'") && val.ToString()!.EndsWith("'"))
                        {
                            strVal = val.ToString()!.Substring(1, val.ToString()!.Length - 2);
                        }
                        else
                        {
                            strVal = val.ToString()!;
                        }

                        var pathEvalNode = JsonNode.Parse(strVal);
                        return EvalPath(rightString, pathEvalNode, out result);
                    }
                    else
                    {
                        return EvalPath(rightString, result, out result);
                    }
                }
            }
            else if (pathString.Trim().Equals(CONTEXTNODE))
            {
                result = data;
                return true;
            }
            else
            {
                var pathStringPath = string.Concat(pathString.StartsWith(JSONPOINTERSTART) ? string.Empty : JSONPOINTERSTART, pathString.Replace('.', '/'));
                var pointer = JsonPointer.Parse(pathStringPath);
                return pointer.TryEvaluate(data, out result);
            }

            result = null;
            return false;
        }

        #endregion
    }
}
