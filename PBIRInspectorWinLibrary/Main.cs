using PBIRInspectorClientLibrary.Utils;
using PBIRInspectorLibrary;
using PBIRInspectorLibrary.Exceptions;
using PBIRInspectorLibrary.Output;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;

namespace PBIRInspectorClientLibrary
{
    public class Main
    {
        public static event EventHandler<MessageIssuedEventArgs>? WinMessageIssued;
        private static Args? _args = null;
        private static int _errorCount = 0;
        private static int _warningCount = 0;

        public static int ErrorCount
        {
            get
            {
                return _errorCount;
            }
        }

        public static void IncrementErrorCount()
        {
            Interlocked.Increment(ref _errorCount);
        }

        public static int WarningCount
        {
            get
            {
                return _warningCount;
            }
        }

        public static void IncrementWarningCount()
        {
            Interlocked.Increment(ref _warningCount);
        }

        public static void Run(string pbiFilePath, string rulesFilePath, string outputPath, bool verbose, bool parallel, bool jsonOutput, bool htmlOutput, IReportPageWireframeRenderer pageRenderer)
        {
            var formatsString = string.Concat(jsonOutput ? "JSON" : string.Empty, ",", htmlOutput ? "HTML" : string.Empty);
            var verboseString = verbose.ToString();
            var parallelString = parallel.ToString();

            string resolvedPbiFilePath = string.Empty;

            var args = new Args { PBIFilePath = pbiFilePath, RulesFilePath = rulesFilePath, OutputPath = outputPath, FormatsString = formatsString, VerboseString = verboseString, ParallelString = parallelString };

            Run(args, pageRenderer);
        }

        public static void Run(Args args, IReportPageWireframeRenderer pageRenderer)
        {
            if (!args.Parallel)
            {
                RunSingleThreaded(args, pageRenderer);
            }
            else
            {
                RunParallel(args, pageRenderer);
            }
        }

        public static void RunSingleThreaded(Args args, IReportPageWireframeRenderer pageRenderer)
        {
            _args = args;
            IEnumerable<TestResult> testResults = null;
            Inspector? insp = null;

            OnMessageIssued(MessageTypeEnum.Information, string.Concat("Test run started at (UTC): ", DateTime.Now.ToUniversalTime()));

            var rules = Inspector.DeserialiseRulesFromPath<InspectionRules>(Main._args.RulesFilePath);
            testResults = RunSingleThreaded(rules);

            if (testResults != null && testResults.Any())
            {
                OutputResults(testResults.OrderBy(_ => _.RuleId), pageRenderer);
            }
            else
            {
                OnMessageIssued(MessageTypeEnum.Information, "No test results found.");
            }
            OnMessageIssued(MessageTypeEnum.Complete, string.Concat("Test run completed at (UTC): ", DateTime.Now.ToUniversalTime()));
        }

        public static void RunParallel(Args args, IReportPageWireframeRenderer pageRenderer)
        {
            _args = args;
            var rules = Inspector.DeserialiseRulesFromPath<InspectionRules>(Main._args.RulesFilePath);
            var ruleBuckets = ChunkInspectionRules(rules);
            var globalResults = new ConcurrentBag<TestResult>();

            OnMessageIssued(MessageTypeEnum.Information, string.Concat("Parallel test run started at (UTC): ", DateTime.Now.ToUniversalTime()));

            Parallel.ForEach(ruleBuckets, _ =>
            {
                var localResults = RunSingleThreaded(_);

                foreach (var result in localResults ?? Enumerable.Empty<TestResult>())
                {
                    globalResults.Add(result);
                }
            });

            OutputResults(globalResults.ToList().OrderBy(_ => _.RuleId), pageRenderer);
            OnMessageIssued(MessageTypeEnum.Complete, string.Concat("Test run completed at (UTC): ", DateTime.Now.ToUniversalTime()));
        }

        private static List<InspectionRules> ChunkInspectionRules(InspectionRules rules)
        {
            var processorCount = Environment.ProcessorCount;
            var allRules = rules.Rules;
            int totalRules = allRules.Count;
            int chunkSize = (int)Math.Ceiling((double)totalRules / processorCount);

            var ruleBuckets = allRules
                .Select((rule, index) => new { rule, index })
                .GroupBy(x => x.index / chunkSize)
                .Select(g => new InspectionRules { Rules = g.Select(x => x.rule).ToList() })
                .ToList();

            return ruleBuckets;
        }

        private static IEnumerable<TestResult>? RunSingleThreaded(InspectionRules rules)
        {
            Inspector? insp = null;

            try
            {
                insp = new Inspector(Main._args.PBIFilePath, rules);

                insp.MessageIssued += Insp_MessageIssued;
                var testResults = insp.Inspect().Where(_ => (!Main._args.Verbose && !_.Pass) || (Main._args.Verbose));
                return testResults;
            }
            catch (PBIRInspectorException e)
            {
                OnMessageIssued(MessageTypeEnum.Error, e.Message);
            }
            catch (ArgumentException e)
            {
                OnMessageIssued(MessageTypeEnum.Error, e.Message);
            }
            catch (Exception e)
            {
                OnMessageIssued(MessageTypeEnum.Error, e.Message);
            }
            finally
            {
                
                if (insp != null)
                {
                    insp.MessageIssued -= Insp_MessageIssued;
                }
            }

            // Ensure all code paths return a value
            return Enumerable.Empty<TestResult>();
        }

        private static void OutputResults(IEnumerable<TestResult> testResults, IReportPageWireframeRenderer pageRenderer)
        {
            string jsonTestRun = string.Empty;
            Inspector? fieldMapInsp = null;
            IEnumerable<TestResult> fieldMapResults = null;

            if (Main._args.CONSOLEOutput || Main._args.ADOOutput || Main._args.GITHUBOutput)
            {
                foreach (var result in testResults)
                {
                    //TODO: use Test log type json property instead
                    var msgType = result.Pass ? MessageTypeEnum.Information : result.LogType;
                    OnMessageIssued(msgType, result.Message);
                }
            }

            //Ensure output dir exists
            if (!(Main._args.ADOOutput || Main._args.GITHUBOutput) && (Main._args.JSONOutput || Main._args.HTMLOutput || Main._args.PNGOutput))
            {
                if (!Directory.Exists(Main._args.OutputDirPath))
                {
                    Directory.CreateDirectory(Main._args.OutputDirPath);
                }
            }

            if (!(Main._args.ADOOutput || Main._args.GITHUBOutput) && (Main._args.JSONOutput || Main._args.HTMLOutput))
            {
                var outputFilePath = string.Empty;
                var pbiFileNameWOextension = Path.GetFileNameWithoutExtension(Main._args.PBIFilePath);

                if (!string.IsNullOrEmpty(Main._args.OutputDirPath))
                {
                    outputFilePath = Path.Combine(Main._args.OutputDirPath, string.Concat("TestRun_", pbiFileNameWOextension, ".json"));
                }
                else
                {
                    throw new ArgumentException("Directory with path \"{0}\" does not exist", Main._args.OutputDirPath);
                }

                var testRun = new TestRun() { CompletionTime = DateTime.Now, TestedFilePath = Main._args.PBIFilePath, RulesFilePath = Main._args.RulesFilePath, Verbose = Main._args.Verbose, Results = testResults };
                jsonTestRun = JsonSerializer.Serialize(testRun);
                if (Main._args.JSONOutput)
                {
                    OnMessageIssued(MessageTypeEnum.Information, string.Format("Writing JSON output to file at \"{0}\".", outputFilePath));
                    File.WriteAllText(outputFilePath, jsonTestRun, System.Text.Encoding.UTF8);
                }
            }

            if (!(Main._args.ADOOutput || Main._args.GITHUBOutput) && (Main._args.PNGOutput || Main._args.HTMLOutput))
            {
                fieldMapInsp = new Inspector(Main._args.PBIFilePath, Constants.ReportPageFieldMapFilePath);

                fieldMapResults = fieldMapInsp.Inspect();

                var outputPNGDirPath = Path.Combine(Main._args.OutputDirPath, Constants.PNGOutputDir);

                if (Directory.Exists(outputPNGDirPath))
                {
                    var eventArgs = RaiseWinMessage(MessageTypeEnum.Dialog, string.Format("Delete all existing directory content at \"{0}\"?", outputPNGDirPath));
                    if (eventArgs.DialogOKResponse)
                    {
                        Directory.Delete(outputPNGDirPath, true);
                    }
                }
                Directory.CreateDirectory(outputPNGDirPath);
                OnMessageIssued(MessageTypeEnum.Information, string.Format("Writing report page wireframe images to files at \"{0}\".", outputPNGDirPath));
                pageRenderer.DrawReportPages(fieldMapResults, testResults, outputPNGDirPath);
            }

            if (!(Main._args.ADOOutput || Main._args.GITHUBOutput) && Main._args.HTMLOutput)
            {
                string pbiinspectorlogobase64 = string.Concat(Constants.Base64ImgPrefix, pageRenderer.ConvertBitmapToBase64(Constants.PBIInspectorPNG));
                //string nowireframebase64 = string.Concat(Base64ImgPrefix, ImageUtils.ConvertBitmapToBase64(@"Files\png\nowireframe.png"));
                string template = File.ReadAllText(Constants.TestRunHTMLTemplate);
                string html = template.Replace(Constants.LogoPlaceholder, pbiinspectorlogobase64, StringComparison.OrdinalIgnoreCase);
                html = html.Replace(Constants.VersionPlaceholder, AppUtils.About(), StringComparison.OrdinalIgnoreCase);
                html = html.Replace(Constants.JsonPlaceholder, jsonTestRun, StringComparison.OrdinalIgnoreCase);

                var outputHTMLFilePath = Path.Combine(Main._args.OutputDirPath, Constants.TestRunHTMLFileName);

                OnMessageIssued(MessageTypeEnum.Information, string.Format("Writing HTML output to file at \"{0}\".", outputHTMLFilePath));
                File.WriteAllText(outputHTMLFilePath, html);

                //Results have been written to a temporary directory so show output to user automatically.
                if (Main._args.DeleteOutputDirOnExit)
                {
                    AppUtils.WinOpen(outputHTMLFilePath);
                }
            }
        }

        public static void CleanUp()
        {
            if (_args != null && _args.DeleteOutputDirOnExit && Directory.Exists(_args.OutputDirPath))
            {
                Directory.Delete(_args.OutputDirPath, true);
            }
        }

        private static void Insp_MessageIssued(object? sender, MessageIssuedEventArgs e)
        {
            MessageIssued(e);
        }

        private static MessageIssuedEventArgs RaiseWinMessage(MessageTypeEnum messageType, string message)
        {
            var args = new MessageIssuedEventArgs(message, messageType);
            WinMessageIssued?.Invoke(null, args);
            return args;
        }

        private static void OnMessageIssued(MessageTypeEnum messageType, string message)
        {
            var e = new MessageIssuedEventArgs(message, messageType);
            MessageIssued(e);
        }

        private static void MessageIssued(MessageIssuedEventArgs e)
        {
            if (_args != null && (_args.ADOOutput || _args.GITHUBOutput))
            {
                if (e.MessageType == MessageTypeEnum.Error) IncrementErrorCount();
                if (e.MessageType == MessageTypeEnum.Warning) IncrementWarningCount();
            }

            EventHandler<MessageIssuedEventArgs>? handler = WinMessageIssued;
            if (handler != null)
            {
                handler(null, e);
            }
        }
    }
}