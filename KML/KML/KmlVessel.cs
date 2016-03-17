using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KML
{
    /// <summary>
    /// KmlVessel represents a KmlNode with the "VESSEL" tag.
    /// </summary>
    public class KmlVessel : KmlNode
    {
        /// <summary>
        /// Possible origins where this node is found.
        /// Regular VESSEL nodes are children of the "FLIGHTSTATE" node.
        /// </summary>
        public enum VesselOrigin 
        { 
            /// <summary>
            /// Vessel found under FLIGHTSTATE node
            /// </summary>
            Flightstate, 

            /// <summary>
            /// Vessel found anywhere else
            /// </summary>
            Other 
        };

        /// <summary>
        /// Get the origin of this node.
        /// <see cref="KML.KmlVessel.VesselOrigin"/>
        /// </summary>
        public VesselOrigin Origin { get; private set; }

        /// <summary>
        /// Get the "Type" attribute as a property (Debris, Flag, Probe, Ship, Lander, Station, Base, EVA, SpaceObject).
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// Get the "Sit" attribute as a property (LANDED, ORBITING, etc.).
        /// </summary>
        public string Situation { get; private set; }

        // TODO KmlVessel: Not make lists public, better have a Add(method)

        /// <summary>
        /// Get a list of all KmlPart children.
        /// </summary>
        public List<KmlPart> Parts { get; private set; }

        /// <summary>
        /// Get a set of all types (names) of resources in this vessel.
        /// </summary>
        public SortedSet<string> ResourceTypes { get; private set; }

        /// <summary>
        /// Get info whether this vessel has resources or not.
        /// </summary>
        public bool HasResources
        {
            get
            {
                return ResourceTypes.Count > 0;
            }
        }

        /// <summary>
        /// Get the root KmlPart.
        /// </summary>
        public KmlPart RootPart { get; private set; }

        private int rootPartIndex = 0;

        /// <summary>
        /// Creates a KmlVessel as a copy of a given KmlNode.
        /// </summary>
        /// <param name="node">The KmlNode to copy</param>
        public KmlVessel(KmlNode node)
            : base(node.Line, node.Parent)
        {
            if (node.Parent != null && node.Parent.Tag.ToLower() == "flightstate")
            {
                Origin = VesselOrigin.Flightstate;
            }
            else
            {
                Origin = VesselOrigin.Other;
            }

            Type = "";
            Situation = "";
            Parts = new List<KmlPart>();
            ResourceTypes = new SortedSet<string>();
            RootPart = null;

            AddRange(node.AllItems);
        }

        /// <summary>
        /// Adds a child KmlItem to this nodes lists of children, depending of its
        /// derived class KmlNode, KmlPart, KmlAttrib or further derived from these.
        /// When an KmlAttrib "Name", "Type" or "Root" are found, their value 
        /// will be used for the corresponding property of this node.
        /// </summary>
        /// <param name="item">The KmlItem to add</param>
        public override void Add(KmlItem item)
        {
            KmlItem newItem = item;
            if (item is KmlAttrib)
            {
                KmlAttrib attrib = (KmlAttrib)item;
                if (attrib.Name.ToLower() == "type")
                {
                    Type = attrib.Value;

                    // Get notified when Type changes
                    attrib.AttribValueChanged += Type_Changed;
                    attrib.CanBeDeleted = false;
                }
                else if (attrib.Name.ToLower() == "sit")
                {
                    Situation = attrib.Value;

                    // Get notified when Type changes
                    attrib.AttribValueChanged += Situation_Changed;
                    attrib.CanBeDeleted = false;
                }
                else if (attrib.Name.ToLower() == "root")
                {
                    SetRootPart(attrib.Value);

                    // Get notified when Type changes
                    attrib.AttribValueChanged += Root_Changed;
                    attrib.CanBeDeleted = false;
                }
            }
            else if (item is KmlPart)
            {
                KmlPart part = (KmlPart)item;
                Parts.Add(part);
                if (Parts.Count == rootPartIndex + 1)
                {
                    RootPart = Parts[rootPartIndex];
                }
                if (part.HasResources)
                {
                    foreach (string resType in part.ResourceTypes)
                    {
                        ResourceTypes.Add(resType);
                    }
                }
            }
            /*else if (Item is KmlNode)
            {
                KmlNode node = (KmlNode)Item;
                if (node.Tag.ToLower() == "part")
                {
                    newItem = new KmlPart(node);
                    Parts.Add((KmlPart)newItem);
                    if (Parts.Count == rootPartNr + 1)
                    {
                        RootPart = Parts[rootPartNr];
                    }
                }
            }*/
            base.Add(newItem);
        }

        /// <summary>
        /// Refill all resources in all parts of this vessel.
        /// </summary>
        public void Refill()
        {
            foreach (KmlPart part in Parts)
            {
                part.Refill();
            }
        }

        /// <summary>
        /// Refill all resources of a certain type in all parts of this vessel.
        /// </summary>
        /// <param name="type">The type (name) of the resource to refill (ElectricCharge, LiquidFuel, Oxidizer, etc.)</param>
        public void Refill(string type)
        {
            foreach (KmlPart part in Parts)
            {
                part.Refill(type);
            }
        }

        /// <summary>
        /// Generates a nice informative string to be used in display for this kerbal.
        /// It will contain the "Tag", "Name", "Type" and "Situation".
        /// </summary>
        /// <returns>A string to display this node</returns>
        public override string ToString()
        {
            string s = Name;
            if (Type.Length > 0)
            {
                if (s.Length > 0)
                {
                    s += ", ";
                }
                s += Type;
            }
            if (Situation.Length > 0)
            {
                if (s.Length > 0)
                {
                    s += ", ";
                }
                s += Situation;
            }
            if (s.Length > 0)
            {
                s = " (" + s + ")";
            }
            return Tag + s;
        }

        /// <summary>
        /// After an item is completely loaded the Intentify() method is called.
        /// This is the right moment to build the connection structure of the parts.
        /// There is no class derived from KmlVessel so it doesn't need to be replaced.
        /// </summary>
        /// <returns>There is no class derived from KmlVessel so always null is returned</returns>
        protected override KmlItem Identify()
        {
            if (Origin == VesselOrigin.Flightstate)
            {
                // No need to replace this by another class, but after vessel is completed 
                // it's the right time to build parts structure
                List<KmlPart> roots = KmlPart.BuildAttachmentStructure(Parts);
                if (roots.Count != 1)
                {
                    Syntax.Warning(this, "Vessel has more than one root part identified from part structure: " + string.Join<KmlPart>(", ", roots.ToArray()));
                }
                if(RootPart == null)
                {
                    Syntax.Warning(this, "Vessel's root part attribute does not point to a valid part. Set root to: " + string.Join<KmlPart>(", ", roots.ToArray()));
                }
                else if (!roots.Contains(RootPart))
                {
                    Syntax.Warning(this, "Vessel's root part attribute is not equal to root identified from part structure: " + RootPart.ToString() + " <-> " + string.Join<KmlPart>(", ", roots.ToArray()));
                }
            }
            return base.Identify();
        }

        private void SetRootPart(string value)
        {
            if (int.TryParse(value, out rootPartIndex))
            {
                if (Parts.Count > rootPartIndex)
                {
                    RootPart = Parts[rootPartIndex];
                }
            }
        }

        private void Type_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            Type = GetAttribWhereValueChanged(sender).Value;
            InvokeToStringChanged();
        }

        private void Situation_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            Situation = GetAttribWhereValueChanged(sender).Value;
            InvokeToStringChanged();
        }

        private void Root_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            SetRootPart(GetAttribWhereValueChanged(sender).Value);

            // TODO KmlVessel.Root_Changed(): Change display on old and new root part in TreeView
        }
    }
}
