using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KML;
using KML_Test.Data;
using System.Collections.Generic;
using System.Windows;

namespace KML_Test.KML
{
    [TestClass]
    public class KmlPart_Test
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
            KmlItem item = KmlItem.CreateItem("PART");
            Assert.IsNotNull(item);
            Assert.IsTrue(item is KmlPart);
            KmlPart part = (KmlPart)item;
            Assert.AreEqual("", part.Name);
            Assert.AreEqual("", part.Flag);
            Assert.IsFalse(part.HasResources);
            Assert.AreEqual(-1, part.ParentPartIndex);
            Assert.AreEqual(new System.Windows.Media.Media3D.Point3D(0.0, 0.0, 0.0), part.Position);
            Assert.AreEqual(0, part.Resources.Count);
            Assert.AreEqual(0, part.ResourceTypes.Count);
            Assert.AreEqual("", part.Uid);
        }

        [TestMethod]
        public void AssignAttribs()
        {
            Assert.AreEqual("Vessel1Part1", data.Vessel1Part1.Name);
            Assert.AreEqual("Vessel1Flag1", data.Vessel1Part1.Flag);
            Assert.IsTrue(data.Vessel1Part1.HasResources);
            Assert.AreEqual(0, data.Vessel1Part1.ParentPartIndex);
            Assert.AreEqual(new System.Windows.Media.Media3D.Point3D(1.0, 2.0, 3.0), data.Vessel1Part1.Position);
            Assert.AreEqual(2, data.Vessel1Part1.Resources.Count);
            Assert.AreEqual(data.Vessel1Part1Resource1, data.Vessel1Part1.Resources[0]);
            Assert.AreEqual(data.Vessel1Part1Resource2, data.Vessel1Part1.Resources[1]);
            Assert.AreEqual(2, data.Vessel1Part1.ResourceTypes.Count);
            Assert.IsTrue(data.Vessel1Part1.ResourceTypes.Contains("Resource1"));
            Assert.IsTrue(data.Vessel1Part1.ResourceTypes.Contains("Resource2"));
            Assert.AreEqual("Vessel1Part1Uid", data.Vessel1Part1.Uid);
            Assert.AreEqual(0.5, data.Vessel1Part1.WorstResourceRatio);

            Assert.AreEqual("Vessel1Part2", data.Vessel1Part2.Name);
            Assert.AreEqual("Vessel1Flag2", data.Vessel1Part2.Flag);
            Assert.IsFalse(data.Vessel1Part2.HasResources);
            Assert.AreEqual(1, data.Vessel1Part2.ParentPartIndex);
            Assert.AreEqual(new System.Windows.Media.Media3D.Point3D(2.0, 3.0, 4.0), data.Vessel1Part2.Position);
            Assert.AreEqual(0, data.Vessel1Part2.Resources.Count);
            Assert.AreEqual(0, data.Vessel1Part2.ResourceTypes.Count);
            Assert.AreEqual("Vessel1Part2Uid", data.Vessel1Part2.Uid);
        }

        [TestMethod]
        public void AssignParent()
        {
            KmlPart part = KmlItem.CreateItem("PART") as KmlPart;
            Assert.AreEqual(KmlPart.PartOrigin.Other, part.Origin);
            Assert.IsNull(part.Parent);
            data.Vessel1.Add(part);
            Assert.AreEqual(KmlPart.PartOrigin.Vessel, part.Origin);
            Assert.AreEqual(data.Vessel1, part.Parent);
            data.Root2.Add(part);
            Assert.AreEqual(KmlPart.PartOrigin.Other, part.Origin);
            Assert.AreEqual(data.Root2, part.Parent);
        }

        [TestMethod]
        public void AssignAttachments()
        {
            // TODO KmlPart_Test.AssignAttachments()
        }

        [TestMethod]
        public void AttribsChanged()
        {
            data.Vessel1Part1.ToStringChanged += TestEventHandler;
            _testEventHandlerVisited = false;
            data.Vessel1Part1.GetAttrib("name").Value = "NewName";
            Assert.IsTrue(_testEventHandlerVisited);
            Assert.AreEqual("NewName", data.Vessel1Part1.Name);

            data.Vessel1Part1.GetAttrib("flag").Value = "NewFlag";
            Assert.AreEqual("NewFlag", data.Vessel1Part1.Flag);

            data.Vessel1Part1.GetAttrib("uid").Value = "NewUid";
            Assert.AreEqual("NewUid", data.Vessel1Part1.Uid);
        }

        [TestMethod]
        public void Clear()
        {
            data.Vessel1Part1.Clear();
            Assert.AreEqual("", data.Vessel1Part1.Name);
            Assert.AreEqual("", data.Vessel1Part1.Flag);
            Assert.IsFalse(data.Vessel1Part1.HasResources);
            Assert.AreEqual(-1, data.Vessel1Part1.ParentPartIndex);
            Assert.AreEqual(new System.Windows.Media.Media3D.Point3D(0.0, 0.0, 0.0), data.Vessel1Part1.Position);
            Assert.AreEqual(0, data.Vessel1Part1.Resources.Count);
            Assert.AreEqual(0, data.Vessel1Part1.ResourceTypes.Count);
            Assert.AreEqual("", data.Vessel1Part1.Uid);
        }

        [TestMethod]
        public void RefillAll()
        {
            data.Vessel1Part1.Refill();
            Assert.AreEqual(1.0, data.Vessel1Part1.WorstResourceRatio);
            Assert.AreEqual(data.Vessel1Part1Resource1.MaxAmount.Value, data.Vessel1Part1Resource1.Amount.Value);
            Assert.AreEqual(data.Vessel1Part1Resource2.MaxAmount.Value, data.Vessel1Part1Resource2.Amount.Value);
        }

        [TestMethod]
        public void RefillType()
        {
            data.Vessel1Part1.Refill("nonsense");
            Assert.AreEqual(0.5, data.Vessel1Part1.WorstResourceRatio);
            Assert.AreEqual("50", data.Vessel1Part1Resource1.Amount.Value);
            data.Vessel1Part1.Refill("Resource1");
            Assert.AreEqual(1.0, data.Vessel1Part1.WorstResourceRatio);
            Assert.AreEqual(data.Vessel1Part1Resource1.MaxAmount.Value, data.Vessel1Part1Resource1.Amount.Value);
        }

        [TestMethod]
        public void FlagExchange()
        {
            data.Vessel1Part1.FlagExchange("nonsense", "NewFlag");
            Assert.AreEqual("Vessel1Flag1", data.Vessel1Part1.Flag);
            data.Vessel1Part1.FlagExchange("Vessel1Flag1", "NewFlag");
            Assert.AreEqual("NewFlag", data.Vessel1Part1.Flag);
        }
    }
}
