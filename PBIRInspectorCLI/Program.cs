using PBIRInspectorLibrary;
using PBIRInspectorWinLibrary;
using PBIRInspectorWinLibrary.Utils;

internal partial class Program
{
    private static Args _parsedArgs = null;

    private static void Main(string[] args)
    {
#if DEBUG
        Console.WriteLine("Attach debugger to process? Press any key to continue.");
        Console.ReadLine();
#endif

        try
        {
            _parsedArgs = ArgsUtils.ParseArgs(args);

            Welcome();

            PBIRInspectorWinLibrary.Main.WinMessageIssued += Main_MessageIssued;
            PBIRInspectorWinLibrary.Main.Run(_parsedArgs);

            Exit();
        }
        catch (ArgumentException e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            PBIRInspectorWinLibrary.Main.WinMessageIssued -= Main_MessageIssued;
            PBIRInspectorWinLibrary.Main.CleanUp();
        }
    }

    private static void Main_MessageIssued(object? sender, PBIRInspectorLibrary.MessageIssuedEventArgs e)
    {
        if (e.MessageType == MessageTypeEnum.Dialog)
        {
            if (_parsedArgs.ADOOutput || _parsedArgs.GITHUBOutput)
            {
                //Running in non-interactive mode on Azure DevOps or GitHub.
                e.DialogOKResponse = true;
            }
            else
            {
                Console.WriteLine(string.Concat(e.Message, " Y/N"));
                var a = Console.ReadLine();
                e.DialogOKResponse = !string.IsNullOrEmpty(a) && a.Equals("Y", StringComparison.OrdinalIgnoreCase);
            }
        }
        else
        {
            //Console and ADO/GitHub outputs
            if ((!_parsedArgs.ADOOutput && !_parsedArgs.GITHUBOutput) || ((_parsedArgs.ADOOutput || _parsedArgs.GITHUBOutput) && (e.MessageType == MessageTypeEnum.Error || e.MessageType == MessageTypeEnum.Warning)))
            {
                Console.WriteLine(FormatConsoleMessage(e.MessageType, e.Message));
            }

            //ADO output only
            if (_parsedArgs.ADOOutput && e.MessageType == MessageTypeEnum.Complete)
            {
                string completionStatus = PBIRInspectorWinLibrary.Main.ErrorCount > 0 ? "Failed" : ((PBIRInspectorWinLibrary.Main.WarningCount > 0) ? "SucceededWithIssues" : "Succeeded");

                Console.WriteLine(Constants.ADOCompleteTemplate, completionStatus);
            }

            //GitHub output only
            if (_parsedArgs.GITHUBOutput && e.MessageType == MessageTypeEnum.Complete)
            {
                int exitCode = PBIRInspectorWinLibrary.Main.ErrorCount > 0 ? 1 : 0;
                Environment.ExitCode = exitCode;
            }
        }
    }

    private static String FormatConsoleMessage(MessageTypeEnum messageType, string message)
    {
        string template = _parsedArgs.ADOOutput ? Constants.ADOLogIssueTemplate : _parsedArgs.GITHUBOutput ? Constants.GitHubMsgTemplate : "{0}";
        string msgType = _parsedArgs.ADOOutput || _parsedArgs.GITHUBOutput ? messageType.ToString().ToLower() : messageType.ToString();
        string msgSeparator = _parsedArgs.ADOOutput || _parsedArgs.GITHUBOutput ? "" : ": ";
        string messageTypeFormat = string.Format(template, msgType);

        return string.Concat(messageTypeFormat, msgSeparator, message);
    }

    private static void Welcome()
    {
#if !DEBUG
     if (!_parsedArgs.CONSOLEOutput || _parsedArgs.ADOOutput || _parsedArgs.GITHUBOutput) return;
#endif

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(AppUtils.About());
        Console.ResetColor();
    }

    private static void Exit()
    {
        if (_parsedArgs.CONSOLEOutput || !(_parsedArgs.ADOOutput || _parsedArgs.GITHUBOutput))
        {
            Console.ResetColor();
            Console.WriteLine("\nPress any key to quit application.");
            Console.ReadLine();
        }

        var exitCode = PBIRInspectorWinLibrary.Main.ErrorCount > 0 ? 1 : 0;
        Environment.Exit(exitCode);
    }
}