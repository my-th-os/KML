using Microsoft.VisualStudio.TestTools.UnitTesting;
using KML;
using KML_Test.Data;
using System.Windows;

namespace KML_Test.KML
{
    [TestClass]
    public class KmlKerbal_Test
    {
        private TestData data = new TestData();
        private bool _testEventHandlerVisited = false;

        private void TestEventHandler(object sender, RoutedEventArgs e)
        {
            _testEventHandlerVisited = true;
        }

        [TestMethod]
        public void CreateItem()
        {
            KmlItem item = KmlItem.CreateItem("KERBAL");
            Assert.IsNotNull(item);
            Assert.IsTrue(item is KmlKerbal);
            KmlKerbal kerbal = (KmlKerbal)item;
            Assert.AreEqual("", kerbal.Name);
            Assert.AreEqual(KmlKerbal.KerbalOrigin.Other, kerbal.Origin);
            Assert.IsNull(kerbal.Parent);
            Assert.AreEqual("", kerbal.State);
            Assert.AreEqual("", kerbal.Type);
            Assert.AreEqual("", kerbal.Trait);
            Assert.AreEqual(0.0, kerbal.Brave);
            Assert.AreEqual(0.0, kerbal.Dumb);
            Assert.IsNull(kerbal.AssignedPart);
            Assert.IsNull(kerbal.AssignedVessel);
        }

        [TestMethod]
        public void AssignAttribs()
        {
            Assert.AreEqual("Kerbal1", data.Kerbal1.Name);
            Assert.AreEqual("Assigned", data.Kerbal1.State);
            Assert.AreEqual("Crew", data.Kerbal1.Type);
            Assert.AreEqual("Pilot", data.Kerbal1.Trait);
            Assert.AreEqual(0.1, data.Kerbal1.Brave);
            Assert.AreEqual(0.2, data.Kerbal1.Dumb);

            Assert.AreEqual("Kerbal2", data.Kerbal2.Name);
            Assert.AreEqual("Available", data.Kerbal2.State);
            Assert.AreEqual("Crew", data.Kerbal2.Type);
            Assert.AreEqual("Scientist", data.Kerbal2.Trait);
            Assert.AreEqual(0.2, data.Kerbal2.Brave);
            Assert.AreEqual(0.4, data.Kerbal2.Dumb);
        }

        [TestMethod]
        public void AssignParent()
        {
            KmlKerbal kerbal = KmlItem.CreateItem("KERBAL") as KmlKerbal;
            Assert.AreEqual(KmlKerbal.KerbalOrigin.Other, kerbal.Origin);
            Assert.IsNull(kerbal.Parent);
            data.Roster.Add(kerbal);
            Assert.AreEqual(KmlKerbal.KerbalOrigin.Roster, kerbal.Origin);
            Assert.AreEqual(data.Roster, kerbal.Parent);
            data.Root2.Add(kerbal);
            Assert.AreEqual(KmlKerbal.KerbalOrigin.Other, kerbal.Origin);
            Assert.AreEqual(data.Root2, kerbal.Parent);
        }

        [TestMethod]
        public void Clear()
        {
            data.Kerbal1.Clear();
            Assert.AreEqual("", data.Kerbal1.State);
            Assert.AreEqual("", data.Kerbal1.Type);
            Assert.AreEqual("", data.Kerbal1.Trait);
            Assert.AreEqual(0.0, data.Kerbal1.Brave);
            Assert.AreEqual(0.0, data.Kerbal1.Dumb);
        }

        [TestMethod]
        public void SendHome()
        {
            Assert.AreEqual(data.Vessel1, data.Kerbal1.AssignedVessel);
            Assert.AreEqual(data.Vessel1Part1, data.Kerbal1.AssignedPart);
            Assert.AreEqual("Assigned", data.Kerbal1.State);
            data.Kerbal1.SendHome();
            Assert.IsNull(data.Kerbal1.AssignedVessel);
            Assert.IsNull(data.Kerbal1.AssignedPart);
            Assert.AreEqual("Available", data.Kerbal1.State);
        }

        [TestMethod]
        public void ToStringChanged()
        {
            data.Kerbal2.ToStringChanged += TestEventHandler;

            _testEventHandlerVisited = false;
            data.Kerbal2.GetAttrib("name").Value = "NewName";
            Assert.IsTrue(_testEventHandlerVisited);
            Assert.AreEqual("NewName", data.Kerbal2.Name);

            _testEventHandlerVisited = false;
            data.Kerbal2.GetAttrib("type").Value = "NewType";
            Assert.IsTrue(_testEventHandlerVisited);
            Assert.AreEqual("NewType", data.Kerbal2.Type);

            _testEventHandlerVisited = false;
            data.Kerbal2.GetAttrib("trait").Value = "NewTrait";
            Assert.IsTrue(_testEventHandlerVisited);
            Assert.AreEqual("NewTrait", data.Kerbal2.Trait);

            _testEventHandlerVisited = false;
            data.Kerbal2.GetAttrib("state").Value = "NewState";
            Assert.IsTrue(_testEventHandlerVisited);
            Assert.AreEqual("NewState", data.Kerbal2.State);

            // TODO KmlKerbal_Test.ToStringChanged: There's a TODO to move brave and dump to another event
            // than ToStringChanged
            _testEventHandlerVisited = false;
            data.Kerbal2.GetAttrib("brave").Value = "0.9";
            Assert.IsTrue(_testEventHandlerVisited);
            Assert.AreEqual(0.9, data.Kerbal2.Brave);

            _testEventHandlerVisited = false;
            data.Kerbal2.GetAttrib("dumb").Value = "0.8";
            Assert.IsTrue(_testEventHandlerVisited);
            Assert.AreEqual(0.8, data.Kerbal2.Dumb);
        }
    }
}
