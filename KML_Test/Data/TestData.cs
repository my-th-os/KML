using KML;
using System.Collections.Generic;

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

        public KmlNode Flightstate { get; private set; }
        public KmlNode Roster { get; private set; }
        public KmlVessel Vessel1 { get; private set; }
        public KmlPart Vessel1Part1 { get; private set; }
        public KmlResource Vessel1Part1Resource1 { get; private set; }
        public KmlResource Vessel1Part1Resource2 { get; private set; }
        public KmlPart Vessel1Part2 { get; private set; }
        public KmlPart Vessel1Part3 { get; private set; }
        public KmlPart Vessel1Part4 { get; private set; }
        public KmlPart Vessel1Part5 { get; private set; }
        public KmlPart Vessel1Part6 { get; private set; }
        public KmlPart Vessel1Part7 { get; private set; }
        public KmlPart Vessel1Part8 { get; private set; }
        public KmlVessel Vessel2 { get; private set; }
        public KmlPart Vessel2Part1 { get; private set; }
        public KmlKerbal Kerbal1 { get; private set; }
        public KmlKerbal Kerbal2 { get; private set; }

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

            KmlNode game = NewNode("GAME");
            Add(null, game);
            Flightstate = NewNode("FLIGHTSTATE");
            Add(game, Flightstate);
            Roster = NewNode("ROSTER");
            Add(game, Roster);

            Kerbal1 = NewNode("KERBAL", "Kerbal1") as KmlKerbal;
            Roster.Add(Kerbal1);
            Add(Kerbal1, NewAttrib("state", "Assigned"));
            Add(Kerbal1, NewAttrib("type", "Crew"));
            Add(Kerbal1, NewAttrib("trait", "Pilot"));
            Add(Kerbal1, NewAttrib("brave", "0.1"));
            Add(Kerbal1, NewAttrib("dumb", "0.2"));

            Kerbal2 = NewNode("KERBAL", "Kerbal2") as KmlKerbal;
            Add(Kerbal2, NewAttrib("state", "Available"));
            Add(Kerbal2, NewAttrib("type", "Crew"));
            Add(Kerbal2, NewAttrib("trait", "Scientist"));
            Add(Kerbal2, NewAttrib("brave", "0.2"));
            Add(Kerbal2, NewAttrib("dumb", "0.4"));
            Roster.Add(Kerbal2);

            Vessel1 = NewNode("VESSEL", "Vessel1") as KmlVessel;
            Add(Vessel1, NewAttrib("root", "0"));
            
            Vessel1Part1 = NewNode("PART", "Vessel1Part1") as KmlPart;
            Add(Vessel1Part1, NewAttrib("uid", "Vessel1Part1Uid"));
            Add(Vessel1Part1, NewAttrib("parent", "0"));
            Add(Vessel1Part1, NewAttrib("position", "0,0,0"));
            Add(Vessel1Part1, NewAttrib("attN", "bottom, 1"));
            Add(Vessel1Part1, NewAttrib("attN", "top, 2"));
            Add(Vessel1Part1, NewAttrib("attN", "left, 3"));
            Add(Vessel1Part1, NewAttrib("attN", "right, 4"));
            Add(Vessel1Part1, NewAttrib("attN", "back, 5"));
            Add(Vessel1Part1, NewAttrib("attN", "front, 6"));
            Add(Vessel1Part1, NewAttrib("flag", "Vessel1Flag1"));
            Add(Vessel1Part1, NewAttrib("crew", Kerbal1.Name));
            Vessel1Part1Resource1 = NewNode("RESOURCE", "Resource1") as KmlResource;
            Add(Vessel1Part1Resource1, NewAttrib("amount", "50"));
            Add(Vessel1Part1Resource1, NewAttrib("maxAmount", "100"));
            Add(Vessel1Part1, Vessel1Part1Resource1);
            Vessel1Part1Resource2 = NewNode("RESOURCE", "Resource2") as KmlResource;
            Add(Vessel1Part1Resource2, NewAttrib("amount", "200"));
            Add(Vessel1Part1Resource2, NewAttrib("maxAmount", "200"));
            Add(Vessel1Part1, Vessel1Part1Resource2);
            Add(Vessel1, Vessel1Part1);
            
            Vessel1Part2 = NewNode("PART", "Vessel1Part2") as KmlPart;
            Add(Vessel1Part2, NewAttrib("uid", "Vessel1Part2Uid"));
            Add(Vessel1Part2, NewAttrib("parent", "0"));
            Add(Vessel1Part2, NewAttrib("position", "0.0,-1.0,0.0"));
            Add(Vessel1Part2, NewAttrib("attN", "top, 0"));
            Add(Vessel1Part2, NewAttrib("attN", "bottom, -1"));
            Add(Vessel1Part2, NewAttrib("flag", "Vessel1Flag2"));
            Add(Vessel1, Vessel1Part2);

            Vessel1Part3 = NewNode("PART", "Vessel1Part3") as KmlPart;
            Add(Vessel1Part3, NewAttrib("parent", "0"));
            Add(Vessel1Part3, NewAttrib("position", "0.0,1.0,0.0"));
            Add(Vessel1Part3, NewAttrib("attN", "top, -1"));
            Add(Vessel1Part3, NewAttrib("attN", "bottom, 0"));
            Add(Vessel1, Vessel1Part3);

            Vessel1Part4 = NewNode("PART", "Vessel1Part4") as KmlPart;
            Add(Vessel1Part4, NewAttrib("parent", "0"));
            Add(Vessel1Part4, NewAttrib("position", "-1.0,0.0,0.0"));
            Add(Vessel1Part4, NewAttrib("attN", "top, 0"));
            Add(Vessel1Part4, NewAttrib("attN", "bottom, -1"));
            Add(Vessel1, Vessel1Part4);

            Vessel1Part5 = NewNode("PART", "Vessel1Part5") as KmlPart;
            Add(Vessel1Part5, NewAttrib("parent", "0"));
            Add(Vessel1Part5, NewAttrib("position", "1.0,0.0,0.0"));
            Add(Vessel1Part5, NewAttrib("attN", "top, 0"));
            Add(Vessel1Part5, NewAttrib("attN", "bottom, -1"));
            Add(Vessel1, Vessel1Part5);

            Vessel1Part6 = NewNode("PART", "Vessel1Part6") as KmlPart;
            Add(Vessel1Part6, NewAttrib("parent", "0"));
            Add(Vessel1Part6, NewAttrib("position", "0.0,0.0,-1.0"));
            Add(Vessel1Part6, NewAttrib("attN", "top, 0"));
            Add(Vessel1Part6, NewAttrib("attN", "bottom, -1"));
            Add(Vessel1, Vessel1Part6);

            Vessel1Part7 = NewNode("PART", "Vessel1Part7") as KmlPart;
            Add(Vessel1Part7, NewAttrib("parent", "0"));
            Add(Vessel1Part7, NewAttrib("position", "0.0,0.0,1.0"));
            Add(Vessel1Part7, NewAttrib("attN", "top, 0"));
            Add(Vessel1Part7, NewAttrib("attN", "bottom, -1"));
            Add(Vessel1, Vessel1Part7);

            Vessel1Part8 = NewNode("PART", "Vessel1Part8") as KmlPart;
            Add(Vessel1Part8, NewAttrib("parent", "1"));
            Add(Vessel1Part8, NewAttrib("position", "0.0,-2.0,0.0"));
            Add(Vessel1Part8, NewAttrib("srfN", "srfAttach, 1"));
            Add(Vessel1, Vessel1Part8);
            Add(Flightstate, Vessel1);

            // TODO TestData.Generate(): Maybe one item is replaced within Identify
            // and needs to be reassigned to a property.
            KmlItem.ParseMemory(Roots);
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
