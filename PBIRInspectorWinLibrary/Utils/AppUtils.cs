using PBIRInspectorLibrary;
using PBIRInspectorLibrary.Exceptions;
using System.Diagnostics;

namespace PBIRInspectorClientLibrary.Utils
{
    public class AppUtils
    {
        public static void WinOpen(string url)
        {
            string request = url;
            ProcessStartInfo ps = new()
            {
                FileName = request,
                UseShellExecute = true
            };

            try
            {
                Process.Start(ps);
            }
            catch
            {
                throw new PBIRInspectorException(string.Format("Could not launch browser or Windows Exployer for location \"{0}\".", url));
            }
        }

        public static string About()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            var about = string.Format("PBI Inspector v{0}", version);
            return about;
        }

    }
}
