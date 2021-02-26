using Microsoft.VisualStudio.TestTools.UnitTesting;
using KML;
using System.Collections.Generic;
using System.Windows;
using KML_Test.Data;

namespace KML_Test.KML
{
    [TestClass]
    public class KmlNode_Test
    {
        private TestData data = new TestData();
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
            Assert.IsNotNull(data.Root1);
            Assert.IsTrue(data.Root1 is KmlNode);
            Assert.IsNull(data.Root1.Parent);
            Assert.AreEqual("Root1", data.Root1.Tag);
            Assert.AreEqual("", data.Root1.Name);
        }

        [TestMethod]
        public void CreateTestData()
        {
            Assert.AreEqual(data.RootCount, data.Roots.Count);
            Assert.AreEqual(data.RootAttrib1, data.Roots[0]);
            Assert.AreEqual(data.Root1, data.Roots[1]);
            Assert.AreEqual(data.Root2, data.Roots[2]);
            Assert.AreEqual(data.Root1AttribCount, data.Root1.Attribs.Count);
            Assert.AreEqual(data.Root1ChildCount, data.Root1.Children.Count);
            Assert.AreEqual(data.Root1AttribCount + data.Root1ChildCount, data.Root1.AllItems.Count);
            Assert.AreEqual(data.Node1, data.Root1.Children[0]);
            Assert.AreEqual(data.Node2, data.Root1.Children[1]);
        }

        [TestMethod]
        public void AttribAdd()
        {
            Assert.AreEqual(data.Node1, data.Node1Attrib1.Parent);
            Assert.AreEqual(data.Node1AttribCount, data.Node1.Attribs.Count);
            Assert.AreEqual(data.Node1AttribCount + data.Node1ChildCount, data.Node1.AllItems.Count);
            // data.Node1.Attribs[0] is the name attrib
            Assert.AreEqual(data.Node1Attrib1, data.Node1.Attribs[1]);
            Assert.AreEqual(data.Node1Attrib2, data.Node1.Attribs[2]);

            Assert.AreEqual(data.Node2, data.Node2Attrib1.Parent);
            Assert.AreEqual(data.Node2AttribCount, data.Node2.Attribs.Count);
            Assert.AreEqual(data.Node2AttribCount + data.Node2ChildCount, data.Node2.AllItems.Count);
            // data.Node2.Attribs[0] is the name attrib
            Assert.AreEqual(data.Node2Attrib1, data.Node2.Attribs[1]);
        }

        [TestMethod]
        public void AttribAddName()
        {
            Assert.AreEqual("Node1Name", data.Node1.Name);
        }

        [TestMethod]
        public void AttribChangeName()
        {
            KmlAttrib attrib = KmlItem.CreateItem("name = rootname") as KmlAttrib;
            data.Root1.Add(attrib);

            Assert.AreEqual("rootname", data.Root1.Name);
            attrib.Value = "newname";
            Assert.AreEqual("newname", data.Root1.Name);
        }

        [TestMethod]
        public void AttribChangedEvent()
        {
            data.Root1.AttribChanged += TestEventHandler;
            _testEventHandlerVisited = false;
            KmlAttrib attrib = KmlItem.CreateItem("attrib = value") as KmlAttrib;
            data.Root1.Add(attrib);
            Assert.IsTrue(_testEventHandlerVisited);
            _testEventHandlerVisited = false;
            data.Root1.Delete(attrib);
            Assert.IsTrue(_testEventHandlerVisited);
        }

        [TestMethod]
        public void AttribAddToEnd()
        {
            KmlAttrib attrib1 = KmlItem.CreateItem("attrib1 = value1") as KmlAttrib;
            data.Root2.Add(attrib1);
            KmlAttrib attrib2 = KmlItem.CreateItem("attrib2 = value2") as KmlAttrib;
            data.Root2.Add(attrib2);

            Assert.AreEqual(2, data.Root2.Attribs.Count);
            Assert.AreEqual(2, data.Root2.AllItems.Count);
            Assert.AreEqual(attrib1, data.Root2.Attribs[0]);
            Assert.AreEqual(attrib2, data.Root2.Attribs[1]);
        }

        [TestMethod]
        public void AttribAddRange()
        {
            List<KmlItem> list = new List<KmlItem>();
            KmlAttrib attrib1 = KmlItem.CreateItem("attrib1 = value1") as KmlAttrib;
            list.Add(attrib1);
            KmlAttrib attrib2 = KmlItem.CreateItem("attrib2 = value2") as KmlAttrib;
            list.Add(attrib2);
            data.Root2.AddRange(list);

            Assert.AreEqual(2, data.Root2.Attribs.Count);
            Assert.AreEqual(2, data.Root2.AllItems.Count);
            Assert.AreEqual(attrib1, data.Root2.Attribs[0]);
            Assert.AreEqual(attrib2, data.Root2.Attribs[1]);
        }

        [TestMethod]
        public void AttribInsertAfter()
        {
            KmlAttrib attrib3 = KmlItem.CreateItem("attrib3 = value3") as KmlAttrib;
            data.Node1.InsertAfter(data.Node1Attrib1, attrib3);

            Assert.AreEqual(data.Node1AttribCount + 1, data.Node1.Attribs.Count);
            Assert.AreEqual(data.Node1AttribCount + data.Node1ChildCount + 1, data.Node1.AllItems.Count);
            // data.Node1.Attribs[0] is name attrib
            Assert.AreEqual(data.Node1Attrib1, data.Node1.Attribs[1]);
            Assert.AreEqual(attrib3, data.Node1.Attribs[2]);
            Assert.AreEqual(data.Node1Attrib2, data.Node1.Attribs[3]);
        }

        [TestMethod]
        public void AttribInsertAfterEmpty()
        {
            KmlAttrib attrib = KmlItem.CreateItem("attrib = value") as KmlAttrib;
            data.Node1.InsertAfter(null, attrib);

            Assert.AreEqual(data.Node1AttribCount + 1, data.Node1.Attribs.Count);
            Assert.AreEqual(data.Node1AttribCount + data.Node1ChildCount + 1, data.Node1.AllItems.Count);
            Assert.AreEqual(attrib, data.Node1.Attribs[data.Node1AttribCount]);
        }

        [TestMethod]
        public void AttribInsertAfterNotContained()
        {
            KmlAttrib attrib = KmlItem.CreateItem("attrib = value") as KmlAttrib;
            data.Node1.InsertAfter(data.Node2Attrib1, attrib);

            Assert.AreEqual(data.Node1AttribCount + 1, data.Node1.Attribs.Count);
            Assert.AreEqual(data.Node1AttribCount + data.Node1ChildCount + 1, data.Node1.AllItems.Count);
            Assert.AreEqual(attrib, data.Node1.Attribs[data.Node1AttribCount]);
        }

        [TestMethod]
        public void AttribInsertBefore()
        {
            KmlAttrib attrib = KmlItem.CreateItem("attrib = value") as KmlAttrib;
            data.Node1.InsertBefore(data.Node1Attrib1, attrib);

            Assert.AreEqual(data.Node1AttribCount + 1, data.Node1.Attribs.Count);
            Assert.AreEqual(data.Node1AttribCount + data.Node1ChildCount + 1, data.Node1.AllItems.Count);
            // data.Node1.Attribs[0] is name attrib
            Assert.AreEqual(attrib, data.Node1.Attribs[1]);
            Assert.AreEqual(data.Node1Attrib1, data.Node1.Attribs[2]);
        }

        [TestMethod]
        public void AttribInsertBeforeEmpty()
        {
            KmlAttrib attrib = KmlItem.CreateItem("attrib = value") as KmlAttrib;
            data.Node1.InsertBefore(null, attrib);

            Assert.AreEqual(data.Node1AttribCount + 1, data.Node1.Attribs.Count);
            Assert.AreEqual(data.Node1AttribCount + data.Node1ChildCount + 1, data.Node1.AllItems.Count);
            Assert.AreEqual(attrib, data.Node1.Attribs[data.Node1AttribCount]);
        }

        [TestMethod]
        public void AttribInsertBeforeNotContained()
        {
            KmlAttrib attrib = KmlItem.CreateItem("attrib = value") as KmlAttrib;
            data.Node1.InsertBefore(data.Node2Attrib1, attrib);

            Assert.AreEqual(data.Node1AttribCount + 1, data.Node1.Attribs.Count);
            Assert.AreEqual(data.Node1AttribCount + data.Node1ChildCount + 1, data.Node1.AllItems.Count);
            Assert.AreEqual(attrib, data.Node1.Attribs[data.Node1AttribCount]);
        }

        [TestMethod]
        public void AttribDeleteAtAttrib()
        {
            data.Node1Attrib2.CanBeDeleted = false;

            Assert.IsTrue(data.Node1Attrib1.Delete());
            Assert.IsFalse(data.Node1Attrib2.Delete());
            Assert.AreEqual(data.Node1AttribCount - 1, data.Node1.Attribs.Count);
            Assert.AreEqual(data.Node1AttribCount + data.Node1ChildCount - 1, data.Node1.AllItems.Count);
            // data.Node1.Attribs[0] is name attrib
            Assert.AreEqual(data.Node1Attrib2, data.Node1.Attribs[1]);
        }

        [TestMethod]
        public void AttribDeleteAtNode()
        {
            data.Node1Attrib2.CanBeDeleted = false;

            Assert.IsTrue(data.Node1.Delete(data.Node1Attrib1));
            Assert.IsFalse(data.Node1.Delete(data.Node1Attrib2));
            Assert.AreEqual(data.Node1AttribCount - 1, data.Node1.Attribs.Count);
            Assert.AreEqual(data.Node1AttribCount + data.Node1ChildCount - 1, data.Node1.AllItems.Count);
            // data.Node1.Attribs[0] is name attrib
            Assert.AreEqual(data.Node1Attrib2, data.Node1.Attribs[1]);
        }

        [TestMethod]
        public void AttribGet()
        {
            KmlAttrib test = data.Node1.GetAttrib("Attrib2");
            Assert.AreEqual(data.Node1Attrib2, test);
            KmlAttrib nonsense = data.Node1.GetAttrib("nonsense");
            Assert.IsNull(nonsense);
        }

        [TestMethod]
        public void AttribGetOrCreate()
        {
            KmlAttrib test = data.Node1.GetOrCreateAttrib("Attrib2", "default2");
            Assert.AreEqual(data.Node1Attrib2, test);
            Assert.AreEqual("Value2", test.Value);
            Assert.AreEqual(data.Node1AttribCount, data.Node1.Attribs.Count);

            KmlAttrib other = data.Node1.GetOrCreateAttrib("other", "default");
            Assert.IsTrue(data.Node1.Attribs.Contains(other));
            Assert.AreEqual("other", other.Name);
            Assert.AreEqual("default", other.Value);
            Assert.AreEqual(data.Node1AttribCount + 1, data.Node1.Attribs.Count);
        }

        [TestMethod]
        public void ChildAdd()
        {
            Assert.AreEqual(data.Node1, data.Node1Child1.Parent);
            Assert.AreEqual(data.Node1ChildCount, data.Node1.Children.Count);
            Assert.AreEqual(data.Node1AttribCount + data.Node1ChildCount, data.Node1.AllItems.Count);
            Assert.AreEqual(data.Node1Child1, data.Node1.Children[0]);
            Assert.AreEqual(data.Node1Child2, data.Node1.Children[1]);

            Assert.AreEqual(data.Node2, data.Node2Child1.Parent);
            Assert.AreEqual(data.Node2ChildCount, data.Node2.Children.Count);
            Assert.AreEqual(data.Node2AttribCount + data.Node2ChildCount, data.Node2.AllItems.Count);
            Assert.AreEqual(data.Node2Child1, data.Node2.Children[0]);
        }

        [TestMethod]
        public void ChildrenChangedEvent()
        {
            data.Root1.ChildrenChanged += TestEventHandler;
            _testEventHandlerVisited = false;
            KmlNode child = KmlItem.CreateItem("child") as KmlNode;
            data.Root1.Add(child);
            Assert.IsTrue(_testEventHandlerVisited);
            _testEventHandlerVisited = false;
            data.Root1.Delete(child);
            Assert.IsTrue(_testEventHandlerVisited);
        }

        [TestMethod]
        public void ChildAddToEnd()
        {
            KmlNode node1 = KmlItem.CreateItem("node1") as KmlNode;
            data.Root2.Add(node1);
            KmlNode node2 = KmlItem.CreateItem("node2") as KmlNode;
            data.Root2.Add(node2);

            Assert.AreEqual(2, data.Root2.Children.Count);
            Assert.AreEqual(2, data.Root2.AllItems.Count);
            Assert.AreEqual(node1, data.Root2.Children[0]);
            Assert.AreEqual(node2, data.Root2.Children[1]);
        }

        [TestMethod]
        public void ChildAddRange()
        {
            List<KmlItem> list = new List<KmlItem>();
            KmlNode node1 = KmlItem.CreateItem("node1") as KmlNode;
            list.Add(node1);
            KmlNode node2 = KmlItem.CreateItem("node2") as KmlNode;
            list.Add(node2);
            data.Root2.AddRange(list);

            Assert.AreEqual(2, data.Root2.Children.Count);
            Assert.AreEqual(2, data.Root2.AllItems.Count);
            Assert.AreEqual(node1, data.Root2.Children[0]);
            Assert.AreEqual(node2, data.Root2.Children[1]);
        }

        [TestMethod]
        public void ChildInsertAfter()
        {
            KmlNode child3 = KmlItem.CreateItem("child3") as KmlNode;
            data.Node1.InsertAfter(data.Node1Child1, child3);

            Assert.AreEqual(data.Node1ChildCount + 1, data.Node1.Children.Count);
            Assert.AreEqual(data.Node1AttribCount + data.Node1ChildCount + 1, data.Node1.AllItems.Count);
            Assert.AreEqual(data.Node1Child1, data.Node1.Children[0]);
            Assert.AreEqual(child3, data.Node1.Children[1]);
            Assert.AreEqual(data.Node1Child2, data.Node1.Children[2]);
        }

        [TestMethod]
        public void ChildInsertAfterEmpty()
        {
            KmlNode child = KmlItem.CreateItem("child") as KmlNode;
            data.Node1.InsertAfter(null, child);

            Assert.AreEqual(data.Node1ChildCount + 1, data.Node1.Children.Count);
            Assert.AreEqual(data.Node1AttribCount + data.Node1ChildCount + 1, data.Node1.AllItems.Count);
            Assert.AreEqual(child, data.Node1.Children[data.Node1ChildCount]);
        }

        [TestMethod]
        public void ChildInsertAfterNotContained()
        {
            KmlNode child = KmlItem.CreateItem("child") as KmlNode;
            data.Node1.InsertAfter(data.Node2Child1, child);

            Assert.AreEqual(data.Node1ChildCount + 1, data.Node1.Children.Count);
            Assert.AreEqual(data.Node1AttribCount + data.Node1ChildCount + 1, data.Node1.AllItems.Count);
            Assert.AreEqual(child, data.Node1.Children[data.Node1ChildCount]);
        }

        [TestMethod]
        public void ChildInsertBefore()
        {
            KmlNode child = KmlItem.CreateItem("child") as KmlNode;
            data.Node1.InsertBefore(data.Node1Child1, child);

            Assert.AreEqual(data.Node1ChildCount + 1, data.Node1.Children.Count);
            Assert.AreEqual(data.Node1AttribCount + data.Node1ChildCount + 1, data.Node1.AllItems.Count);
            Assert.AreEqual(child, data.Node1.Children[0]);
            Assert.AreEqual(data.Node1Child1, data.Node1.Children[1]);
        }

        [TestMethod]
        public void ChildInsertBeforeEmpty()
        {
            KmlNode child = KmlItem.CreateItem("child") as KmlNode;
            data.Node1.InsertBefore(null, child);

            Assert.AreEqual(data.Node1ChildCount + 1, data.Node1.Children.Count);
            Assert.AreEqual(data.Node1AttribCount + data.Node1ChildCount + 1, data.Node1.AllItems.Count);
            Assert.AreEqual(child, data.Node1.Children[data.Node1ChildCount]);
        }

        [TestMethod]
        public void ChildInsertBeforeNotContained()
        {
            KmlNode child = KmlItem.CreateItem("child") as KmlNode;
            data.Node1.InsertBefore(data.Node2Child1, child);

            Assert.AreEqual(data.Node1ChildCount + 1, data.Node1.Children.Count);
            Assert.AreEqual(data.Node1AttribCount + data.Node1ChildCount + 1, data.Node1.AllItems.Count);
            Assert.AreEqual(child, data.Node1.Children[data.Node1ChildCount]);
        }

        [TestMethod]
        public void ChildDeleteAtChild()
        {
            data.Node1Child2.CanBeDeleted = false;

            Assert.IsTrue(data.Node1Child1.Delete());
            Assert.IsFalse(data.Node1Child2.Delete());
            Assert.AreEqual(data.Node1ChildCount - 1, data.Node1.Children.Count);
            Assert.AreEqual(data.Node1AttribCount + data.Node1ChildCount - 1, data.Node1.AllItems.Count);
            Assert.AreEqual(data.Node1Child2, data.Node1.Children[0]);
        }

        [TestMethod]
        public void ChildDeleteAtNode()
        {
            data.Node1Child2.CanBeDeleted = false;

            Assert.IsTrue(data.Node1.Delete(data.Node1Child1));
            Assert.IsFalse(data.Node1.Delete(data.Node1Child2));
            Assert.AreEqual(data.Node1ChildCount - 1, data.Node1.Children.Count);
            Assert.AreEqual(data.Node1AttribCount + data.Node1ChildCount - 1, data.Node1.AllItems.Count);
            Assert.AreEqual(data.Node1Child2, data.Node1.Children[0]);
        }

        [TestMethod]
        public void ChildGetTag()
        {
            KmlNode test = data.Root1.GetChildNode("Node2Tag");
            Assert.AreEqual(data.Node2, test);
            KmlAttrib nonsense = data.Root1.GetAttrib("nonsense");
            Assert.IsNull(nonsense);
        }

        [TestMethod]
        public void ChildGetName()
        {
            KmlNode root = data.Root2;
            KmlNode node1 = KmlItem.CreateItem("node") as KmlNode;
            node1.Add(KmlItem.CreateItem("name = name1"));
            root.Add(node1);
            KmlNode node2 = KmlItem.CreateItem("node") as KmlNode;
            node2.Add(KmlItem.CreateItem("name = name2"));
            root.Add(node2);
            KmlNode node3 = KmlItem.CreateItem("node") as KmlNode;
            node3.Add(KmlItem.CreateItem("name = name3"));
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
        public void ChildGetFrom()
        {
            Assert.AreEqual(data.Node2, KmlNode.GetChildNodeFrom(data.Root1, "Node2Tag"));
            Assert.IsNull(KmlNode.GetChildNodeFrom(data.Root1, "AnotherNodeTag"));
            KmlNode empty = null;
            Assert.IsNull(KmlNode.GetChildNodeFrom(empty, "Node2Tag"));

            List<KmlItem> list = new List<KmlItem>();
            list.AddRange(data.Root1.Children);

            Assert.AreEqual(data.Node2, KmlNode.GetNodeFrom(list, "Node2Tag"));
            Assert.IsNull(KmlNode.GetNodeFrom(list, "AnotherNodeTag"));

            list.Clear();
            list.Add(data.Root1);
            string[] tags1 = { "Root1", "Node2Tag" };
            string[] tags2 = { "Root2", "Node2Tag" };
            string[] tags3 = { "Root1", "AnotherNodeTag" };

            Assert.AreEqual(data.Node2, KmlNode.GetNodeFromDeep(list, tags1));
            Assert.IsNull(KmlNode.GetNodeFromDeep(list, tags2));
            Assert.IsNull(KmlNode.GetNodeFromDeep(list, tags3));
        }

        [TestMethod]
        public void ChildGetOrCreate()
        {
            KmlNode root = data.Root2;
            KmlNode node1 = KmlItem.CreateItem("node") as KmlNode;
            node1.Add(KmlItem.CreateItem("name = name1"));
            root.Add(node1);
            KmlNode node2 = KmlItem.CreateItem("node") as KmlNode;
            node2.Add(KmlItem.CreateItem("name = name2"));
            root.Add(node2);
            KmlNode node3 = KmlItem.CreateItem("node") as KmlNode;
            node3.Add(KmlItem.CreateItem("name = name3"));
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
            KmlNode root = data.Root2;
            KmlNode node1 = KmlItem.CreateItem("node1") as KmlNode;
            root.Add(node1);
            KmlAttrib attrib1 = KmlItem.CreateItem("attrib1 = value1") as KmlAttrib;
            root.Add(attrib1);
            // KmlItem constructor is only way to create a KmlItem (unknown) instead of KmlNode or KmlAttrib
            KmlItem unknown = new KmlItem("unknown");
            root.Add(unknown);
            KmlNode node2 = KmlItem.CreateItem("node2") as KmlNode;
            root.Add(node2);
            KmlAttrib attrib2 = KmlItem.CreateItem("attrib2 = value2") as KmlAttrib;
            root.Add(attrib2);

            Assert.AreEqual(2, root.Attribs.Count);
            Assert.AreEqual(2, root.Children.Count);
            Assert.AreEqual(1, root.Unknown.Count);
            Assert.AreEqual(5, root.AllItems.Count);
            Assert.AreEqual(attrib1, root.Attribs[0]);
            Assert.AreEqual(attrib2, root.Attribs[1]);
            Assert.AreEqual(node1, root.Children[0]);
            Assert.AreEqual(node2, root.Children[1]);
            Assert.AreEqual(unknown, root.Unknown[0]);
        }

        [TestMethod]
        public void MixedInsert()
        {
            KmlNode root = data.Root2;
            KmlNode node1 = KmlItem.CreateItem("node1") as KmlNode;
            root.Add(node1);
            KmlAttrib attrib1 = KmlItem.CreateItem("attrib1 = value1") as KmlAttrib;
            root.InsertBefore(node1, attrib1);
            // KmlItem constructor is only way to create a KmlItem (unknown) instead of KmlNode or KmlAttrib
            KmlItem unknown = new KmlItem("unknown");
            root.InsertBefore(node1, unknown);
            KmlNode node2 = KmlItem.CreateItem("node2") as KmlNode;
            root.InsertBefore(node1, node2);
            KmlAttrib attrib2 = KmlItem.CreateItem("attrib2 = value2") as KmlAttrib;
            root.InsertBefore(attrib1, attrib2);

            Assert.AreEqual(2, root.Attribs.Count);
            Assert.AreEqual(2, root.Children.Count);
            Assert.AreEqual(1, root.Unknown.Count);
            Assert.AreEqual(5, root.AllItems.Count);
            Assert.AreEqual(attrib2, root.Attribs[0]);
            Assert.AreEqual(attrib1, root.Attribs[1]);
            Assert.AreEqual(unknown, root.Unknown[0]);
            Assert.AreEqual(node2, root.Children[0]);
            Assert.AreEqual(node1, root.Children[1]);
        }

        [TestMethod]
        public void MixedAddWellOrdered()
        {
            KmlNode root = data.Root2;
            KmlAttrib attrib1 = KmlItem.CreateItem("attrib1 = value1") as KmlAttrib;
            root.Add(attrib1);
            KmlNode node1 = KmlItem.CreateItem("node1") as KmlNode;
            root.Add(node1);
            KmlAttrib attrib2 = KmlItem.CreateItem("attrib2 = value1") as KmlAttrib;
            root.Add(attrib2);
            KmlNode node2 = KmlItem.CreateItem("node2") as KmlNode;
            root.Add(node2);
            KmlAttrib attrib3 = KmlItem.CreateItem("attrib3 = value2") as KmlAttrib;
            root.Add(attrib3);

            Assert.AreEqual(3, root.Attribs.Count);
            Assert.AreEqual(2, root.Children.Count);
            Assert.AreEqual(5, root.AllItems.Count);
            Assert.AreEqual(attrib1, root.AllItems[0]);
            Assert.AreEqual(attrib2, root.AllItems[1]);
            Assert.AreEqual(attrib3, root.AllItems[2]);
            Assert.AreEqual(node1, root.AllItems[3]);
            Assert.AreEqual(node2, root.AllItems[4]);
            
        }

        [TestMethod]
        public void ToStringChangedEvent()
        {
            KmlNode root = data.Root2;
            root.ToStringChanged += TestEventHandler;

            //_testEventHandlerVisited = false;
            //root.Tag = "other";
            //Assert.IsTrue(_testEventHandlerVisited);

            _testEventHandlerVisited = false;
            KmlAttrib attrib = KmlItem.CreateItem("name = rootname") as KmlAttrib;
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

        [TestMethod]
        public void Clear()
        {
            data.Root1.Clear();

            Assert.AreEqual(0, data.Root1.Attribs.Count);
            Assert.AreEqual(0, data.Root1.Children.Count);
            Assert.AreEqual(0, data.Root1.AllItems.Count);
            Assert.AreEqual("", data.Root1.Name);
        }

        [TestMethod]
        public void WarningUnknon()
        {
            // Syntax.Messages are static, there may be some from other tests
            int messageCount = Syntax.Messages.Count;

            KmlItem unknown = new KmlItem("unknown");
            data.Root1.Add(unknown);

            Assert.AreEqual(messageCount + 1, Syntax.Messages.Count);
            Assert.AreEqual(data.Root1, Syntax.Messages[Syntax.Messages.Count - 1].Source);
        }

        [TestMethod]
        public void WarningAttribAddAfterNode()
        {
            // Syntax.Messages are static, there may be some from other tests
            int messageCount = Syntax.Messages.Count;

            KmlAttrib attrib = KmlItem.CreateItem("attrib = value") as KmlAttrib;
            data.Root1.Add(attrib);

            Assert.AreEqual(messageCount + 1, Syntax.Messages.Count);
            Assert.AreEqual(attrib, Syntax.Messages[Syntax.Messages.Count - 1].Source);
        }
    }
}
