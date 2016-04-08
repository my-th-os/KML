using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KML;
using System.Collections.Generic;
using System.Windows;

namespace KML_Test.KML
{
    [TestClass]
    public class KmlNode_Test
    {
        private bool _testEventHandlerVisited = false;

        private void TestEventHandler(object sender, RoutedEventArgs e)
        {
            _testEventHandlerVisited = true;
        }

        [TestMethod]
        public void Create()
        {
            KmlNode root = new KmlNode("root");
            Assert.IsNull(root.Parent);
            Assert.AreEqual("root", root.Tag);
            Assert.AreEqual("", root.Name);
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
            Assert.AreEqual("", root.Name);
        }

        [TestMethod]
        public void AttribAdd()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlAttrib attrib = KmlItem.CreateItem("attrib = value", root) as KmlAttrib;
            root.Add(attrib);
            Assert.AreEqual(root, attrib.Parent);
            Assert.AreEqual(1, root.Attribs.Count);
            Assert.AreEqual(1, root.AllItems.Count);
            Assert.AreEqual(attrib, root.Attribs[0]);
        }

        [TestMethod]
        public void AttribAddName()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlAttrib attrib = KmlItem.CreateItem("name = rootname", root) as KmlAttrib;
            root.Add(attrib);
            Assert.AreEqual(attrib.Value, root.Name);
        }

        [TestMethod]
        public void AttribChangeName()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlAttrib attrib = KmlItem.CreateItem("name = rootname", root) as KmlAttrib;
            root.Add(attrib);
            Assert.AreEqual("rootname", root.Name);
            attrib.Value = "newname";
            Assert.AreEqual("newname", root.Name);
        }

        [TestMethod]
        public void AttribChangedEvent()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            root.AttribChanged += TestEventHandler;
            _testEventHandlerVisited = false;
            KmlAttrib attrib = KmlItem.CreateItem("attrib = value", root) as KmlAttrib;
            root.Add(attrib);
            Assert.IsTrue(_testEventHandlerVisited);
            _testEventHandlerVisited = false;
            root.Delete(attrib);
            Assert.IsTrue(_testEventHandlerVisited);
        }

        [TestMethod]
        public void AttribAddToEnd()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlAttrib attrib1 = KmlItem.CreateItem("attrib1 = value1", root) as KmlAttrib;
            root.Add(attrib1);
            KmlAttrib attrib2 = KmlItem.CreateItem("attrib2 = value2", root) as KmlAttrib;
            root.Add(attrib2);
            Assert.AreEqual(2, root.Attribs.Count);
            Assert.AreEqual(2, root.AllItems.Count);
            Assert.AreEqual(attrib1, root.Attribs[0]);
            Assert.AreEqual(attrib2, root.Attribs[1]);
        }

        [TestMethod]
        public void AttribAddRange()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            List<KmlItem> list = new List<KmlItem>();
            KmlAttrib attrib1 = KmlItem.CreateItem("attrib1 = value1", root) as KmlAttrib;
            list.Add(attrib1);
            KmlAttrib attrib2 = KmlItem.CreateItem("attrib2 = value2", root) as KmlAttrib;
            list.Add(attrib2);
            root.AddRange(list);
            Assert.AreEqual(2, root.Attribs.Count);
            Assert.AreEqual(2, root.AllItems.Count);
            Assert.AreEqual(attrib1, root.Attribs[0]);
            Assert.AreEqual(attrib2, root.Attribs[1]);
        }

        [TestMethod]
        public void AttribInsertAfter()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlAttrib attrib1 = KmlItem.CreateItem("attrib1 = value1", root) as KmlAttrib;
            root.Add(attrib1);
            KmlAttrib attrib2 = KmlItem.CreateItem("attrib2 = value2", root) as KmlAttrib;
            root.Add(attrib2);
            KmlAttrib attrib3 = KmlItem.CreateItem("attrib3 = value3", root) as KmlAttrib;
            root.InsertAfter(attrib1, attrib3);
            Assert.AreEqual(3, root.Attribs.Count);
            Assert.AreEqual(3, root.AllItems.Count);
            Assert.AreEqual(attrib1, root.Attribs[0]);
            Assert.AreEqual(attrib3, root.Attribs[1]);
            Assert.AreEqual(attrib2, root.Attribs[2]);
        }

        [TestMethod]
        public void AttribInsertAfterEmpty()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlAttrib attrib = KmlItem.CreateItem("attrib = value", root) as KmlAttrib;
            root.InsertAfter(null, attrib);
            Assert.AreEqual(1, root.Attribs.Count);
            Assert.AreEqual(1, root.AllItems.Count);
            Assert.AreEqual(attrib, root.Attribs[0]);
        }

        [TestMethod]
        public void AttribInsertAfterNotContained()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlAttrib attrib1 = KmlItem.CreateItem("attrib1 = value1", root) as KmlAttrib;
            root.Add(attrib1);
            KmlAttrib other = KmlItem.CreateItem("other = other value", root) as KmlAttrib;
            KmlAttrib attrib2 = KmlItem.CreateItem("attrib2 = value2", root) as KmlAttrib;
            root.InsertAfter(other, attrib2);
            Assert.AreEqual(2, root.Attribs.Count);
            Assert.AreEqual(2, root.AllItems.Count);
            Assert.AreEqual(attrib1, root.Attribs[0]);
            Assert.AreEqual(attrib2, root.Attribs[1]);
        }

        [TestMethod]
        public void AttribInsertBefore()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlAttrib attrib1 = KmlItem.CreateItem("attrib1 = value1", root) as KmlAttrib;
            root.Add(attrib1);
            KmlAttrib attrib2 = KmlItem.CreateItem("attrib2 = value2", root) as KmlAttrib;
            root.InsertBefore(attrib1, attrib2);
            Assert.AreEqual(2, root.Attribs.Count);
            Assert.AreEqual(2, root.AllItems.Count);
            Assert.AreEqual(attrib2, root.Attribs[0]);
            Assert.AreEqual(attrib1, root.Attribs[1]);
        }

        [TestMethod]
        public void AttribInsertBeforeEmpty()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlAttrib attrib = KmlItem.CreateItem("attrib = value", root) as KmlAttrib;
            root.InsertBefore(null, attrib);
            Assert.AreEqual(1, root.Attribs.Count);
            Assert.AreEqual(1, root.AllItems.Count);
            Assert.AreEqual(attrib, root.Attribs[0]);
        }

        [TestMethod]
        public void AttribInsertBeforeNotContained()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlAttrib attrib1 = KmlItem.CreateItem("attrib1 = value1", root) as KmlAttrib;
            root.Add(attrib1);
            KmlAttrib other = KmlItem.CreateItem("other = other value", root) as KmlAttrib;
            KmlAttrib attrib2 = KmlItem.CreateItem("attrib2 = value2", root) as KmlAttrib;
            root.InsertBefore(other, attrib2);
            Assert.AreEqual(2, root.Attribs.Count);
            Assert.AreEqual(2, root.AllItems.Count);
            Assert.AreEqual(attrib1, root.Attribs[0]);
            Assert.AreEqual(attrib2, root.Attribs[1]);
        }

        [TestMethod]
        public void AttribDeleteAtAttrib()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlAttrib attrib1 = KmlItem.CreateItem("attrib1 = value1", root) as KmlAttrib;
            root.Add(attrib1);
            KmlAttrib attrib2 = KmlItem.CreateItem("attrib2 = value2", root) as KmlAttrib;
            root.Add(attrib2);
            KmlAttrib attrib3 = KmlItem.CreateItem("attrib3 = value3", root) as KmlAttrib;
            attrib3.CanBeDeleted = false;
            root.Add(attrib3);
            Assert.IsTrue(attrib2.Delete());
            Assert.IsFalse(attrib3.Delete());
            Assert.AreEqual(2, root.Attribs.Count);
            Assert.AreEqual(2, root.AllItems.Count);
            Assert.AreEqual(attrib1, root.Attribs[0]);
            Assert.AreEqual(attrib3, root.Attribs[1]);
        }

        [TestMethod]
        public void AttribDeleteAtNode()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlAttrib attrib1 = KmlItem.CreateItem("attrib1 = value1", root) as KmlAttrib;
            root.Add(attrib1);
            KmlAttrib attrib2 = KmlItem.CreateItem("attrib2 = value2", root) as KmlAttrib;
            root.Add(attrib2);
            KmlAttrib attrib3 = KmlItem.CreateItem("attrib3 = value3", root) as KmlAttrib;
            attrib3.CanBeDeleted = false;
            root.Add(attrib3);
            Assert.IsTrue(root.Delete(attrib2));
            Assert.IsFalse(root.Delete(attrib3));
            Assert.AreEqual(2, root.Attribs.Count);
            Assert.AreEqual(2, root.AllItems.Count);
            Assert.AreEqual(attrib1, root.Attribs[0]);
            Assert.AreEqual(attrib3, root.Attribs[1]);
        }

        [TestMethod]
        public void AttribGet()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlAttrib attrib1 = KmlItem.CreateItem("attrib1 = value1", root) as KmlAttrib;
            root.Add(attrib1);
            KmlAttrib attrib2 = KmlItem.CreateItem("attrib2 = value2", root) as KmlAttrib;
            root.Add(attrib2);
            KmlAttrib attrib3 = KmlItem.CreateItem("attrib3 = value3", root) as KmlAttrib;
            root.Add(attrib3);
            KmlAttrib test = root.GetAttrib("attrib2");
            Assert.AreEqual(attrib2, test);
            KmlAttrib nonsense = root.GetAttrib("nonsense");
            Assert.IsNull(nonsense);
        }

        [TestMethod]
        public void AttribGetOrCreate()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlAttrib attrib1 = KmlItem.CreateItem("attrib1 = value1", root) as KmlAttrib;
            root.Add(attrib1);
            KmlAttrib attrib2 = KmlItem.CreateItem("attrib2 = value2", root) as KmlAttrib;
            root.Add(attrib2);
            KmlAttrib attrib3 = KmlItem.CreateItem("attrib3 = value3", root) as KmlAttrib;
            root.Add(attrib3);
            Assert.AreEqual(3, root.Attribs.Count);

            KmlAttrib test = root.GetOrCreateAttrib("attrib2", "default2");
            Assert.AreEqual(attrib2, test);
            Assert.AreEqual("value2", attrib2.Value);
            Assert.AreEqual(3, root.Attribs.Count);

            KmlAttrib other = root.GetOrCreateAttrib("other", "default");
            Assert.IsTrue(root.Attribs.Contains(other));
            Assert.AreEqual("other", other.Name);
            Assert.AreEqual("default", other.Value);
            Assert.AreEqual(4, root.Attribs.Count);
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
            Assert.AreEqual(1, root.AllItems.Count);
            Assert.AreEqual(node, root.Children[0]);
        }

        [TestMethod]
        public void ChildrenChangedEvent()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            root.ChildrenChanged += TestEventHandler;
            _testEventHandlerVisited = false;
            KmlNode node = KmlItem.CreateItem("node", root) as KmlNode;
            root.Add(node);
            Assert.IsTrue(_testEventHandlerVisited);
            _testEventHandlerVisited = false;
            root.Delete(node);
            Assert.IsTrue(_testEventHandlerVisited);
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
            Assert.AreEqual(2, root.AllItems.Count);
            Assert.AreEqual(node1, root.Children[0]);
            Assert.AreEqual(node2, root.Children[1]);
        }

        [TestMethod]
        public void ChildAddRange()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            List<KmlItem> list = new List<KmlItem>();
            KmlNode node1 = KmlItem.CreateItem("node1", root) as KmlNode;
            list.Add(node1);
            KmlNode node2 = KmlItem.CreateItem("node2", root) as KmlNode;
            list.Add(node2);
            root.AddRange(list);
            Assert.AreEqual(2, root.Children.Count);
            Assert.AreEqual(2, root.AllItems.Count);
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
            Assert.AreEqual(3, root.AllItems.Count);
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
            Assert.AreEqual(1, root.AllItems.Count);
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
            Assert.AreEqual(2, root.AllItems.Count);
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
            Assert.AreEqual(2, root.AllItems.Count);
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
            Assert.AreEqual(1, root.AllItems.Count);
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
            Assert.AreEqual(2, root.AllItems.Count);
            Assert.AreEqual(node1, root.Children[0]);
            Assert.AreEqual(node2, root.Children[1]);
        }

        [TestMethod]
        public void ChildDeleteAtChild()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlNode node1 = KmlItem.CreateItem("node1", root) as KmlNode;
            root.Add(node1);
            KmlNode node2 = KmlItem.CreateItem("node2", root) as KmlNode;
            root.Add(node2);
            KmlNode node3 = KmlItem.CreateItem("node3", root) as KmlNode;
            node3.CanBeDeleted = false;
            root.Add(node3);
            Assert.IsTrue(node2.Delete());
            Assert.IsFalse(node3.Delete());
            Assert.AreEqual(2, root.Children.Count);
            Assert.AreEqual(2, root.AllItems.Count);
            Assert.AreEqual(node1, root.Children[0]);
            Assert.AreEqual(node3, root.Children[1]);
        }

        [TestMethod]
        public void ChildDeleteAtNode()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlNode node1 = KmlItem.CreateItem("node1", root) as KmlNode;
            root.Add(node1);
            KmlNode node2 = KmlItem.CreateItem("node2", root) as KmlNode;
            root.Add(node2);
            KmlNode node3 = KmlItem.CreateItem("node3", root) as KmlNode;
            node3.CanBeDeleted = false;
            root.Add(node3);
            Assert.IsTrue(root.Delete(node2));
            Assert.IsFalse(root.Delete(node3));
            Assert.AreEqual(2, root.Children.Count);
            Assert.AreEqual(2, root.AllItems.Count);
            Assert.AreEqual(node1, root.Children[0]);
            Assert.AreEqual(node3, root.Children[1]);
        }

        [TestMethod]
        public void ChildGetTag()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlNode node1 = KmlItem.CreateItem("node1", root) as KmlNode;
            root.Add(node1);
            KmlNode node2 = KmlItem.CreateItem("node2", root) as KmlNode;
            root.Add(node2);
            KmlNode node3 = KmlItem.CreateItem("node3", root) as KmlNode;
            root.Add(node3);
            KmlNode test = root.GetChildNode("node2");
            Assert.AreEqual(node2, test);
            KmlNode nonsense = root.GetChildNode("nonsense");
            Assert.IsNull(nonsense);
        }

        [TestMethod]
        public void ChildGetName()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlNode node1 = KmlItem.CreateItem("node", root) as KmlNode;
            node1.Add(KmlItem.CreateItem("name = name1", node1));
            root.Add(node1);
            KmlNode node2 = KmlItem.CreateItem("node", root) as KmlNode;
            node2.Add(KmlItem.CreateItem("name = name2", node2));
            root.Add(node2);
            KmlNode node3 = KmlItem.CreateItem("node", root) as KmlNode;
            node3.Add(KmlItem.CreateItem("name = name3", node3));
            root.Add(node3);
            KmlNode test = root.GetChildNode("node", "name2");
            Assert.AreEqual(node2, test);
            KmlNode badname = root.GetChildNode("node", "badname");
            Assert.IsNull(badname);
            KmlNode badtag = root.GetChildNode("badtag", "name2");
            Assert.IsNull(badtag);
            KmlNode nonsense = root.GetChildNode("non", "sense");
            Assert.IsNull(nonsense);
        }

        [TestMethod]
        public void ChildGetOrCreate()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlNode node1 = KmlItem.CreateItem("node", root) as KmlNode;
            node1.Add(KmlItem.CreateItem("name = name1", node1));
            root.Add(node1);
            KmlNode node2 = KmlItem.CreateItem("node", root) as KmlNode;
            node2.Add(KmlItem.CreateItem("name = name2", node2));
            root.Add(node2);
            KmlNode node3 = KmlItem.CreateItem("node", root) as KmlNode;
            node3.Add(KmlItem.CreateItem("name = name3", node3));
            root.Add(node3);
            Assert.AreEqual(3, root.Children.Count);

            KmlNode test = root.GetOrCreateChildNode("node", "name2");
            Assert.AreEqual(node2, test);
            Assert.AreEqual(3, root.Children.Count);

            KmlNode badname = root.GetOrCreateChildNode("node", "badname");
            Assert.IsTrue(root.Children.Contains(badname));
            Assert.AreEqual("node", badname.Tag);
            Assert.AreEqual("badname", badname.Name);
            Assert.AreEqual(4, root.Children.Count);

            KmlNode badtag = root.GetOrCreateChildNode("badtag", "name2");
            Assert.IsTrue(root.Children.Contains(badtag));
            Assert.AreEqual("badtag", badtag.Tag);
            Assert.AreEqual("name2", badtag.Name);
            Assert.AreEqual(5, root.Children.Count);

            KmlNode nonsense = root.GetOrCreateChildNode("non", "sense");
            Assert.IsTrue(root.Children.Contains(nonsense));
            Assert.AreEqual("non", nonsense.Tag);
            Assert.AreEqual("sense", nonsense.Name);
            Assert.AreEqual(6, root.Children.Count);
        }

        [TestMethod]
        public void MixedAdd()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlNode node1 = KmlItem.CreateItem("node1", root) as KmlNode;
            root.Add(node1);
            KmlAttrib attrib1 = KmlItem.CreateItem("attrib1 = value1", root) as KmlAttrib;
            root.Add(attrib1);
            // KmlItem constructor is only way to create a KmlItem (unknown) instead of KmlNode or KmlAttrib
            KmlItem unknown = new KmlItem("unknown");
            root.Add(unknown);
            KmlNode node2 = KmlItem.CreateItem("node2", root) as KmlNode;
            root.Add(node2);
            KmlAttrib attrib2 = KmlItem.CreateItem("attrib2 = value2", root) as KmlAttrib;
            root.Add(attrib2);
            Assert.AreEqual(2, root.Attribs.Count);
            Assert.AreEqual(2, root.Children.Count);
            Assert.AreEqual(1, root.Unknown.Count);
            Assert.AreEqual(5, root.AllItems.Count);
            Assert.AreEqual(node1, root.AllItems[0]);
            Assert.AreEqual(attrib1, root.AllItems[1]);
            Assert.AreEqual(unknown, root.AllItems[2]);
            Assert.AreEqual(node2, root.AllItems[3]);
            Assert.AreEqual(attrib2, root.AllItems[4]);
        }

        [TestMethod]
        public void MixedInsert()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            KmlNode node1 = KmlItem.CreateItem("node1", root) as KmlNode;
            root.Add(node1);
            KmlAttrib attrib1 = KmlItem.CreateItem("attrib1 = value1", root) as KmlAttrib;
            root.InsertBefore(node1, attrib1);
            // KmlItem constructor is only way to create a KmlItem (unknown) instead of KmlNode or KmlAttrib
            KmlItem unknown = new KmlItem("unknown");
            root.InsertBefore(node1, unknown);
            KmlNode node2 = KmlItem.CreateItem("node2", root) as KmlNode;
            root.InsertBefore(node1, node2);
            KmlAttrib attrib2 = KmlItem.CreateItem("attrib2 = value2", root) as KmlAttrib;
            root.InsertBefore(attrib1, attrib2);
            Assert.AreEqual(2, root.Attribs.Count);
            Assert.AreEqual(2, root.Children.Count);
            Assert.AreEqual(1, root.Unknown.Count);
            Assert.AreEqual(5, root.AllItems.Count);
            Assert.AreEqual(attrib2, root.AllItems[0]);
            Assert.AreEqual(attrib1, root.AllItems[1]);
            Assert.AreEqual(unknown, root.AllItems[2]);
            Assert.AreEqual(node2, root.AllItems[3]);
            Assert.AreEqual(node1, root.AllItems[4]);
        }

        [TestMethod]
        public void ToStringChangedEvent()
        {
            KmlNode root = KmlItem.CreateItem("root", null) as KmlNode;
            root.ToStringChanged += TestEventHandler;

            //_testEventHandlerVisited = false;
            //root.Tag = "other";
            //Assert.IsTrue(_testEventHandlerVisited);

            _testEventHandlerVisited = false;
            KmlAttrib attrib = KmlItem.CreateItem("name = rootname", root) as KmlAttrib;
            root.Add(attrib);
            Assert.IsTrue(_testEventHandlerVisited);

            _testEventHandlerVisited = false;
            attrib.Value = "other";
            Assert.IsTrue(_testEventHandlerVisited);

            // attrib.CanBeDeleted will be false
            _testEventHandlerVisited = false;
            Assert.IsFalse(root.Delete(attrib));
            Assert.IsFalse(_testEventHandlerVisited);
        }
    }
}
