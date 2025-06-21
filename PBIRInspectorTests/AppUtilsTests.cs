using PBIRInspectorClientLibrary.Utils;

namespace PBIRInspectorTests
{
    [TestFixture]
    public class AppUtilsTests
    {
        [Test]
        public void AboutTest()
        {
            var about = AppUtils.About();
            Assert.That(string.IsNullOrEmpty(about), Is.False);
        }
    }
}
