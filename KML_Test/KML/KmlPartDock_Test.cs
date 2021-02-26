using Microsoft.VisualStudio.TestTools.UnitTesting;
using KML;
using KML_Test.Data;
using System.Collections.Generic;

namespace KML_Test.KML
{
    [TestClass]
    public class KmlPartDock_Test
    {
        private TestData data = new TestData();

        [TestMethod]
        public void CreateItem()
        {
            KmlPart part = (KmlPart)KmlItem.CreateItem("PART");

            KmlNode module = (KmlNode)KmlItem.CreateItem("MODULE");
            part.Add(module);
            KmlAttrib name = (KmlAttrib)KmlItem.CreateItem("name = ModuleDockingNode");
            module.Add(name);
            Assert.IsTrue(KmlPartDock.PartIsDock(part));

            // Need to parse memory roots, to call identify and have the KmlPart
            // replaced by a KmlPartDock
            List<KmlItem> list = new List<KmlItem>();
            list.Add(part);
            list = KmlItem.ParseMemory(list);
            Assert.AreEqual(1, list.Count);
            Assert.AreNotEqual(part, list[0]);
            Assert.IsTrue(list[0] is KmlPartDock);
            KmlPartDock dock = (KmlPartDock)list[0];

            Assert.AreEqual(KmlPartDock.DockTypes.Dock, dock.DockType);
            Assert.AreEqual("", dock.DockName);
            Assert.AreEqual("", dock.DockState);
            Assert.AreEqual("", dock.DockedVesselName);
            Assert.AreEqual("", dock.DockedVesselType);
            Assert.AreEqual("", dock.DockedVesselOtherName);
            Assert.AreEqual("", dock.DockedVesselOtherType);
            Assert.AreEqual("", dock.DockUid);
            Assert.IsNull(dock.DockedPart);
            Assert.IsFalse(dock.NeedsRepair);
        }

        [TestMethod]
        public void PartIsDock()
        {
            Assert.IsFalse(KmlPartDock.PartIsDock(data.Vessel1Part1));
            Assert.IsFalse(KmlPartDock.PartIsDock(data.Vessel1Part2));

            KmlPart part = (KmlPart)KmlItem.CreateItem("PART");
            Assert.IsFalse(KmlPartDock.PartIsDock(part));

            KmlNode module = (KmlNode)KmlItem.CreateItem("WrongTag");
            part.Add(module);
            Assert.IsFalse(KmlPartDock.PartIsDock(part));
            module.Add(KmlItem.CreateItem("name = ModuleDockingNode"));
            Assert.IsFalse(KmlPartDock.PartIsDock(part));

            module = (KmlNode)KmlItem.CreateItem("MODULE");
            part.Add(module);
            Assert.IsFalse(KmlPartDock.PartIsDock(part));
            KmlAttrib name = (KmlAttrib)KmlItem.CreateItem("name = ModuleDockingNode");
            module.Add(name);
            Assert.IsTrue(KmlPartDock.PartIsDock(part));
            name.Value = "WrongName";
            Assert.IsFalse(KmlPartDock.PartIsDock(part));
            name.Value = "ModuleDockingNodeNamed";
            Assert.IsTrue(KmlPartDock.PartIsDock(part));
            name.Value = "ModuleGrappleNode";
            Assert.IsTrue(KmlPartDock.PartIsDock(part));
            name.Value = "KASModuleStrut";
            Assert.IsTrue(KmlPartDock.PartIsDock(part));
        }

        // TODO KmlPartDock_Test: Do tests
        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}
