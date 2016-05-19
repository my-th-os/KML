using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KML;
using KML_Test.Data;
using System.Windows;

namespace KML_Test.KML
{
    [TestClass]
    public class KmlResource_Test
    {
        private TestData data = new TestData();

        [TestMethod]
        public void CreateItem()
        {
            KmlItem item = KmlItem.CreateItem("RESOURCE");
            Assert.IsNotNull(item);
            Assert.IsTrue(item is KmlResource);
            KmlResource res = (KmlResource)item;
            Assert.AreEqual("", res.Name);
            Assert.AreEqual("", res.Amount.Value);
            Assert.AreEqual("", res.MaxAmount.Value);
            Assert.AreEqual(1.0, res.AmountRatio);
        }

        [TestMethod]
        public void AssignAttribs()
        {
            Assert.AreEqual("Resource1", data.Vessel1Part1Resource1.Name);
            Assert.AreEqual("50", data.Vessel1Part1Resource1.Amount.Value);
            Assert.AreEqual("100", data.Vessel1Part1Resource1.MaxAmount.Value);
            Assert.AreEqual(0.5, data.Vessel1Part1Resource1.AmountRatio);

            Assert.AreEqual("Resource2", data.Vessel1Part1Resource2.Name);
            Assert.AreEqual("200", data.Vessel1Part1Resource2.Amount.Value);
            Assert.AreEqual("200", data.Vessel1Part1Resource2.MaxAmount.Value);
            Assert.AreEqual(1.0, data.Vessel1Part1Resource2.AmountRatio);
        }

        [TestMethod]
        public void AttribsChanged()
        {
            data.Vessel1Part1Resource1.GetAttrib("amount").Value = "60";
            Assert.AreEqual("60", data.Vessel1Part1Resource1.Amount.Value);
            Assert.AreEqual(0.6, data.Vessel1Part1Resource1.AmountRatio);

            data.Vessel1Part1Resource2.GetAttrib("maxAmount").Value = "400";
            Assert.AreEqual("400", data.Vessel1Part1Resource2.MaxAmount.Value);
            Assert.AreEqual(0.5, data.Vessel1Part1Resource2.AmountRatio);
        }

        [TestMethod]
        public void Clear()
        {
            data.Vessel1Part1Resource1.Clear();
            Assert.AreEqual("", data.Vessel1Part1Resource1.Name);
            Assert.AreEqual("", data.Vessel1Part1Resource1.Amount.Value);
            Assert.AreEqual("", data.Vessel1Part1Resource1.MaxAmount.Value);
            Assert.AreEqual(1.0, data.Vessel1Part1Resource1.AmountRatio);
        }

        [TestMethod]
        public void Refill()
        {
            data.Vessel1Part1Resource1.Refill();
            Assert.AreEqual(data.Vessel1Part1Resource1.MaxAmount.Value, data.Vessel1Part1Resource1.Amount.Value);
            Assert.AreEqual(1.0, data.Vessel1Part1Resource1.AmountRatio);
        }
    }
}
