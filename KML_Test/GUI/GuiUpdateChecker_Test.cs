using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KML;

namespace KML_Test.GUI
{
    [TestClass]
    public class GuiUpdateChecker_Test
    {
        // This was the first version, any version should be greater
        private Version minVersion = Version.Parse("0.5");

        [TestMethod]
        public void VersionFromAssembly()
        {
            Version version = GuiUpdateChecker.GetAssemblyVersion();

            Assert.IsTrue(version.CompareTo(minVersion) > 0);
        }

        [TestMethod]
        public void VersionFromGitHub()
        {
            Tuple<Version, Uri> result = GuiUpdateChecker.GetGitHubLatest();
            Version version = result.Item1;
            Uri uri = result.Item2;

            Assert.IsTrue(version.CompareTo(minVersion) > 0);

            // Typically the current code has newest or even newer version
            Assert.IsTrue(version.CompareTo(GuiUpdateChecker.GetAssemblyVersion()) <= 0);

            Assert.AreNotEqual("", uri.ToString());
        }
    }
}
