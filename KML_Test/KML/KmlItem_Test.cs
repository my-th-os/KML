using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KML;

namespace KML_Test.KML
{
    [TestClass]
    public class KmlItem_Test
    {
        [TestMethod]
        public void Create()
        {
            KmlItem item = new KmlItem("test");
            Assert.IsNull(item.Parent);
        }

        [TestMethod]
        public void CreateItem()
        {
            Assert.IsNull(KmlItem.CreateItem(null));
            Assert.IsNull(KmlItem.CreateItem("{xy"));
            Assert.IsNull(KmlItem.CreateItem("x{y"));
            Assert.IsNull(KmlItem.CreateItem("xy{"));
            Assert.IsNull(KmlItem.CreateItem("}xy"));
            Assert.IsNull(KmlItem.CreateItem("x}y"));
            Assert.IsNull(KmlItem.CreateItem("xy}"));

            KmlItem test;
            test = KmlItem.CreateItem("attrib = value");
            Assert.IsTrue(test is KmlAttrib);
            test = KmlItem.CreateItem("node");
            Assert.IsTrue(test is KmlNode);
            test = KmlItem.CreateItem("vessel");
            Assert.IsTrue(test is KmlVessel);
            test = KmlItem.CreateItem("kerbal");
            Assert.IsTrue(test is KmlKerbal);
            test = KmlItem.CreateItem("part");
            Assert.IsTrue(test is KmlPart);
            test = KmlItem.CreateItem("resource");
            Assert.IsTrue(test is KmlResource);
        }

        [TestMethod]
        public void CanBeDeleted()
        {
            KmlItem item = new KmlItem("test");
            Assert.IsTrue(item.CanBeDeleted);
            item.CanBeDeleted = false;
            Assert.IsFalse(item.CanBeDeleted);
        }

        [TestMethod]
        public void Delete()
        {
            KmlItem item = new KmlItem("test");
            Assert.IsFalse(item.Delete());
            item.CanBeDeleted = false;
            Assert.IsFalse(item.Delete());

            KmlNode root = new KmlNode("root");
            root.Add(item);
            Assert.AreEqual(root, item.Parent);
            Assert.IsFalse(item.Delete());
            item.CanBeDeleted = true;
            Assert.IsTrue(item.Delete());
        }

        [TestMethod]
        public void PathToString()
        {
            KmlNode root = new KmlNode("root");
            KmlNode node = new KmlNode("node");
            KmlItem item = new KmlNode("item");
            node.Add(item);
            root.Add(node);
            Assert.AreEqual("root", root.PathToString("-"));
            Assert.AreEqual("root-node", node.PathToString("-"));
            Assert.AreEqual("root/node/item", item.PathToString("/"));
        }

        [TestMethod]
        public void ParseFile()
        {
            // TODO KmlItem_Test.ParseFile()
        }

        [TestMethod]
        public void WriteFile()
        {
            // TODO KmlItem_Test.WriteFile()
        }
    }
}
