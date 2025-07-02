using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PBIRInspectorLibrary.Output;

namespace PBIRInspectorLibrary
{
    //TODO: move to PBIRInspectorClientLibrary
    public interface IReportPageWireframeRenderer
    {
        /// <summary>
        /// Draws the report page wireframe.
        /// </summary>
        void DrawReportPages(IEnumerable<TestResult> fieldMapResults, IEnumerable<TestResult> testResults, string outputDir);

        string ConvertBitmapToBase64(string bitmapPath);
    }
}
