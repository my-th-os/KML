using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KML;

namespace KML_Test.KML
{
    [TestClass]
    public class KmlNode_Test
    {
        [TestMethod]
        public void Create()
        {
            KmlNode root = new KmlNode("root", null);
            Assert.IsNull(root.Parent);
            Assert.AreEqual("root", root.Tag);
        }

        [TestMethod]
        public void CreateItem()
        {
            KmlItem item = KmlItem.CreateItem("root", null);
            Assert.IsNotNull(item);
            Assert.IsTrue(item is KmlNode);
            KmlNode root = item as KmlNode;
            Assert.IsNull(root.Parent);
            Assert.AreEqual("root", root.Tag);
        }

        [TestMethod]
        public void ChildAdd()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlItem item = KmlItem.CreateItem("node", root);
            Assert.IsNotNull(item);
            Assert.IsTrue(item is KmlNode);
            KmlNode node = item as KmlNode;
            root.Add(node);
            Assert.AreEqual(root, node.Parent);
            Assert.AreEqual("node", node.Tag);
            Assert.AreEqual(1, root.Children.Count);
            Assert.AreEqual(node, root.Children[0]);
        }

        [TestMethod]
        public void ChildAddToEnd()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlNode node1 = KmlItem.CreateItem("node1", root) as KmlNode;
            root.Add(node1);
            KmlNode node2 = KmlItem.CreateItem("node2", root) as KmlNode;
            root.Add(node2);
            Assert.AreEqual(2, root.Children.Count);
            Assert.AreEqual(node1, root.Children[0]);
            Assert.AreEqual(node2, root.Children[1]);
        }

        [TestMethod]
        public void ChildInsertAfter()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlNode node1 = KmlItem.CreateItem("node1", root) as KmlNode;
            root.Add(node1);
            KmlNode node2 = KmlItem.CreateItem("node2", root) as KmlNode;
            root.Add(node2);
            KmlNode node3 = KmlItem.CreateItem("node3", root) as KmlNode;
            root.InsertAfter(node1, node3);
            Assert.AreEqual(3, root.Children.Count);
            Assert.AreEqual(node1, root.Children[0]);
            Assert.AreEqual(node3, root.Children[1]);
            Assert.AreEqual(node2, root.Children[2]);
        }

        [TestMethod]
        public void ChildInsertAfterEmpty()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlNode node = KmlItem.CreateItem("node", root) as KmlNode;
            root.InsertAfter(null, node);
            Assert.AreEqual(1, root.Children.Count);
            Assert.AreEqual(node, root.Children[0]);
        }

        [TestMethod]
        public void ChildInsertAfterNotContained()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlNode node1 = KmlItem.CreateItem("node1", root) as KmlNode;
            root.Add(node1);
            KmlNode other = KmlItem.CreateItem("other", root) as KmlNode;
            KmlNode node2 = KmlItem.CreateItem("node2", root) as KmlNode;
            root.InsertAfter(other, node2);
            Assert.AreEqual(2, root.Children.Count);
            Assert.AreEqual(node1, root.Children[0]);
            Assert.AreEqual(node2, root.Children[1]);
        }

        [TestMethod]
        public void ChildInsertBefore()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlNode node1 = KmlItem.CreateItem("node1", root) as KmlNode;
            root.Add(node1);
            KmlNode node2 = KmlItem.CreateItem("node2", root) as KmlNode;
            root.InsertBefore(node1, node2);
            Assert.AreEqual(2, root.Children.Count);
            Assert.AreEqual(node2, root.Children[0]);
            Assert.AreEqual(node1, root.Children[1]);
        }

        [TestMethod]
        public void ChildInsertBeforeEmpty()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlNode node = KmlItem.CreateItem("node", root) as KmlNode;
            root.InsertBefore(null, node);
            Assert.AreEqual(1, root.Children.Count);
            Assert.AreEqual(node, root.Children[0]);
        }

        [TestMethod]
        public void ChildInsertBeforeNotContained()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlNode node1 = KmlItem.CreateItem("node1", root) as KmlNode;
            root.Add(node1);
            KmlNode other = KmlItem.CreateItem("other", root) as KmlNode;
            KmlNode node2 = KmlItem.CreateItem("node2", root) as KmlNode;
            root.InsertBefore(other, node2);
            Assert.AreEqual(2, root.Children.Count);
            Assert.AreEqual(node1, root.Children[0]);
            Assert.AreEqual(node2, root.Children[1]);
        }
    }
}
