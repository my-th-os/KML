using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KML;
using System.Windows;

namespace KML_Test.KML
{
    [TestClass]
    public class KmlAttrib_Test
    {
        private bool _testEventHandlerVisited = false;

        private void TestEventHandler(object sender, RoutedEventArgs e)
        {
            _testEventHandlerVisited = true;
        }

        [TestMethod]
        public void Create()
        {
            KmlAttrib attrib1 = new KmlAttrib("name1 = value1");
            Assert.AreEqual("name1", attrib1.Name);
            Assert.AreEqual("value1", attrib1.Value);

            KmlAttrib attrib2 = new KmlAttrib("name2=value2");
            Assert.AreEqual("name2", attrib2.Name);
            Assert.AreEqual("value2", attrib2.Value);
        }

        [TestMethod]
        public void CreateItem()
        {
            KmlItem item1 = KmlItem.CreateItem("name1 = value1");
            Assert.IsNotNull(item1);
            Assert.IsTrue(item1 is KmlAttrib);
            KmlAttrib attrib1 = (KmlAttrib)item1;
            Assert.AreEqual("name1", attrib1.Name);
            Assert.AreEqual("value1", attrib1.Value);

            KmlItem item2 = KmlItem.CreateItem("name2=value2");
            Assert.IsNotNull(item2);
            Assert.IsTrue(item2 is KmlAttrib);
            KmlAttrib attrib2 = (KmlAttrib)item2;
            Assert.AreEqual("name2", attrib2.Name);
            Assert.AreEqual("value2", attrib2.Value);
        }

        [TestMethod]
        public void AttribValueChanged()
        {
            KmlAttrib attrib = KmlItem.CreateItem("name =") as KmlAttrib;
            attrib.AttribValueChanged += TestEventHandler;

            _testEventHandlerVisited = false;
            attrib.Value = "value1";
            Assert.IsTrue(_testEventHandlerVisited);

            _testEventHandlerVisited = false;
            attrib.Value = "value2";
            Assert.IsTrue(_testEventHandlerVisited);
        }
    }
}
