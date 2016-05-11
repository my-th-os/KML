using KML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KML_Test.Data
{
    class TestData
    {
        public List<KmlItem> Roots { get; private set; }
        public KmlAttrib RootAttrib1 { get; private set; }
        public KmlNode Root1 { get; private set; }
        public KmlNode Root2 { get; private set; }
        public int RootCount { get; private set; }
        public KmlNode Node1 { get; private set; }
        public KmlNode Node2 { get; private set; }
        public int Root1AttribCount { get; private set; }
        public int Root1ChildCount { get; private set; }
        public KmlAttrib Node1Attrib1 { get; private set; }
        public KmlAttrib Node1Attrib2 { get; private set; }
        public KmlNode Node1Child1 { get; private set; }
        public KmlNode Node1Child2 { get; private set; }
        public int Node1AttribCount { get; private set; }
        public int Node1ChildCount { get; private set; }
        public KmlAttrib Node2Attrib1 { get; private set; }
        public KmlNode Node2Child1 { get; private set; }
        public int Node2AttribCount { get; private set; }
        public int Node2ChildCount { get; private set; }

        public TestData()
        {
            Roots = new List<KmlItem>();

            Generate();
        }

        private void Generate()
        {
            RootAttrib1 = NewAttrib("RootAttrib1", "This is some test data");
            Add(null, RootAttrib1);
            Root1 = NewNode("Root1");
            Add(null, Root1);
            Root2 = NewNode("Root2");
            Add(null, Root2);

            Node1 = NewNode("Node1Tag", "Node1Name");
            // Count the name attrib
            Node1AttribCount++;
            Add(Root1, Node1);
            Node2 = NewNode("Node2Tag", "Node2Name");
            // Count the name attrib
            Node2AttribCount++;
            Add(Root1, Node2);

            Node1Attrib1 = NewAttrib("Attrib1", "Value1");
            Add(Node1, Node1Attrib1);
            Node1Attrib2 = NewAttrib("Attrib2", "Value2");
            Add(Node1, Node1Attrib2);
            Node1Child1 = NewNode("Child1");
            Add(Node1, Node1Child1);
            Node1Child2 = NewNode("Child2");
            Add(Node1, Node1Child2);

            Node2Attrib1 = NewAttrib("Attrib1", "Value1");
            Add(Node2, Node2Attrib1);
            Node2Child1 = NewNode("Child1");
            Add(Node2, Node2Child1);
        }

        private void Add(KmlNode parent, KmlItem child)
        {
            if (parent == null)
            {
                Roots.Add(child);
            }
            else
            {
                parent.Add(child);
            }
            Count(parent, child);
        }

        private void Count(KmlNode parent, KmlItem child)
        {
            if (parent == null)
            {
                RootCount++;
            }
            else
            {
                if (parent == Root1 && child is KmlAttrib)
                    Root1AttribCount++;
                else if (parent == Root1 && child is KmlNode)
                    Root1ChildCount++;
                else if (parent == Node1 && child is KmlAttrib)
                    Node1AttribCount++;
                else if (parent == Node1 && child is KmlNode)
                    Node1ChildCount++;
                else if (parent == Node2 && child is KmlAttrib)
                    Node2AttribCount++;
                else if (parent == Node2 && child is KmlNode)
                    Node2ChildCount++;
            }
        }

        private KmlNode NewNode(string tag, string name)
        {
            KmlNode node = KmlItem.CreateItem(tag) as KmlNode;
            if (name != null)
            {
                Add(node, NewAttrib("name", name));
            }
            return node;
        }

        private KmlNode NewNode(string tag)
        {
            return NewNode(tag, null);
        }

        private KmlAttrib NewAttrib(string name, string value)
        {
            KmlAttrib attrib = KmlItem.CreateItem(name + " = " + value) as KmlAttrib;
            return attrib;
        }
    }
}
