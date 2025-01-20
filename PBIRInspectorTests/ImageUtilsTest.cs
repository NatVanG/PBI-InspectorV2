using PBIRInspectorLibrary.Output;
using PBIRInspectorWinLibrary.Drawing;

namespace PBIRInspectorTests
{
    [TestFixture]
    internal class ImageUtilsTest
    {
        [Test]
        public void ConvertBitmapToBase64Test()
        {
            var bitmapPath = string.Empty;

            ArgumentException ex = Assert.Throws<ArgumentException>(
                () => ImageUtils.ConvertBitmapToBase64(bitmapPath));
        }
        [Test]
        public void DrawReportPagesTest()
        {
            var fieldMapResults = new List<TestResult>();
            var testResults = new List<TestResult>();
            var outputDir = "";
            ImageUtils.DrawReportPages(fieldMapResults, testResults, outputDir);
            Assert.That(string.IsNullOrEmpty(outputDir));
        }
    }
}
