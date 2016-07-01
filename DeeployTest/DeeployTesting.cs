using Microsoft.VisualStudio.TestTools.UnitTesting;
using Deeploy;

namespace DeeployTest
{
    [TestClass]
    public class DeeployTesting
    {
        [TestMethod]
        public async void ManifestBuilding()
        {
            var s_Path = @"C:\Remote";

            var s_Builder = new Builder();

            var s_Manifest = await s_Builder.GenerateManifest(s_Path, 0, string.Empty, "http://mydomain.lan/updates");

            Assert.AreNotEqual(0, s_Manifest.Entries.Length);
        }
    }
}
