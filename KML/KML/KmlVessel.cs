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
        /// Get a list of all flags used on this vessel's parts
        /// </summary>
        public List<string> Flags { get; private set; }

        /// <summary>
        /// Get a list of all assigned crew KmlKerbal
        /// </summary>
        public List<KmlKerbal> AssignedCrew { get; private set; }

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
            : base(node.Line)
        {
            // First parent is null, will be set later when added to parent,
            // then  IdentifyParent() will set Origin.
            Origin = VesselOrigin.Other;

            Type = "";
            Situation = "";
            Parts = new List<KmlPart>();
            Flags = new List<string>();
            AssignedCrew = new List<KmlKerbal>();
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
        /// <param name="beforeItem">The KmlItem where the new item should be inserted before</param>
        /// <param name="newItem">The KmlItem to add</param>
        protected override void Add(KmlItem beforeItem, KmlItem newItem)
        {
            if (newItem is KmlAttrib)
            {
                KmlAttrib attrib = (KmlAttrib)newItem;
                if (attrib.Name.ToLower() == "type" && Type.Length == 0)
                {
                    Type = attrib.Value;

                    // Get notified when Type changes
                    attrib.AttribValueChanged += Type_Changed;
                    attrib.CanBeDeleted = false;
                }
                else if (attrib.Name.ToLower() == "sit" && Situation.Length == 0)
                {
                    Situation = attrib.Value;

                    // Get notified when Type changes
                    attrib.AttribValueChanged += Situation_Changed;
                    attrib.CanBeDeleted = false;
                }
                else if (attrib.Name.ToLower() == "root" && RootPart == null)
                {
                    SetRootPart(attrib.Value);

                    // Get notified when Type changes
                    attrib.AttribValueChanged += Root_Changed;
                    attrib.CanBeDeleted = false;
                }
            }
            else if (newItem is KmlPart)
            {
                KmlPart part = (KmlPart)newItem;
                if (beforeItem != null)
                {
                    KmlPart beforePart = null;
                    int allIndex = AllItems.IndexOf(beforeItem);
                    for (int i = allIndex; i < AllItems.Count; i++ )
                    {
                        if (AllItems[i] is KmlPart && Parts.Contains((KmlPart)AllItems[i]))
                        {
                            beforePart = (KmlPart)AllItems[i];
                            break;
                        }
                    }
                    if (beforePart != null)
                    {
                        beforePart.InsertionPreparation();
                        Parts.Insert(Parts.IndexOf(beforePart), part);
                        beforePart.InsertionFinalization();
                    }
                    else
                    {
                        Parts.Add(part);
                    }
                }
                else
                {
                    Parts.Add(part);
                }
                if (Parts.Count == rootPartIndex + 1)
                {
                    RootPart = Parts[rootPartIndex];
                }
                if (part.Flag != "" && !Flags.Any(x => x.ToLower() == part.Flag.ToLower()))
                {
                    Flags.Add(part.Flag);
                }
                if (part.HasResources)
                {
                    foreach (string resType in part.ResourceTypes)
                    {
                        ResourceTypes.Add(resType);
                    }
                }
                KmlAttrib flag = part.GetAttrib("flag");
                if (flag != null)
                {
                    flag.AttribValueChanged += Flag_Changed;
                }
            }
            base.Add(beforeItem, newItem);
        }

        /// <summary>
        /// Clear all child nodes and attributes from this node.
        /// </summary>
        public override void Clear()
        {
            Type = "";
            Situation = "";
            Parts.Clear();
            Flags.Clear();
            ResourceTypes.Clear();
            RootPart = null;
            rootPartIndex = 0;
            base.Clear();
        }

        /// <summary>
        /// Gets called before item is deleted.
        /// </summary>
        /// <returns>Return true on success. If false is returned the deletion will be canceled</returns>
        protected override bool BeforeDelete()
        {
            foreach (KmlKerbal kerbal in AssignedCrew)
            {
                kerbal.SendHome();
            }
            return true;
        }

        /// <summary>
        /// Send vessel to low kerbin orbit.
        /// Situation and orbit data will be changed.
        /// <param name="altitude">The altitude of the orbit in km</param>
        /// </summary>
        public void SendToKerbinOrbit(double altitude)
        {
            double radius = 600000.0 + altitude;
            foreach (KmlNode node in Children)
            {
                if (node.Tag.ToLower() == "orbit")
                {
                    foreach (KmlAttrib attrib in node.Attribs)
                    {
                        if (attrib.Name.ToLower() == "sma")
                        {
                            attrib.Value = radius.ToString("F", System.Globalization.CultureInfo.InvariantCulture);
                        }
                        else if (attrib.Name.ToLower() == "ecc")
                        {
                            attrib.Value = "0.0";
                        }
                        else if (attrib.Name.ToLower() == "inc")
                        {
                            attrib.Value = "0.0";
                        }
                        else if (attrib.Name.ToLower() == "ref")
                        {
                            attrib.Value = "1";
                        }
                    }
                }
            }
            foreach (KmlAttrib attrib in Attribs)
            {
                if (attrib.Name.ToLower() == "sit")
                {
                    attrib.Value = "ORBITING";
                }
                else if (attrib.Name.ToLower() == "landed")
                {
                    attrib.Value = "False";
                }
            }
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
        /// Exchange the flag of all parts that match the oldFlag.
        /// Party where it doesn't match, will not be changed.
        /// </summary>
        /// <param name="oldFlag">The old flag name to change</param>
        /// <param name="newFlag">The new flag name to apply</param>
        public void FlagExchange(string oldFlag, string newFlag)
        {
            if (Flags.All(x => x.ToLower() != oldFlag.ToLower()))
            {
                return;
            }
            Flags.Clear();
            foreach (KmlPart part in Parts)
            {
                part.FlagExchange(oldFlag, newFlag);
                if (!Flags.Contains(part.Flag))
                {
                    Flags.Add(part.Flag);
                }
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
            // No need to replace this by another class, but after vessel is completed 
            // it's the right time to build parts structure
            List<KmlPart> roots = KmlPart.BuildAttachmentStructure(Parts);
            if (roots.Count > 1)
            {
                Syntax.Warning(this, "Vessel has more than one root part identified from part structure: " + string.Join<KmlPart>(", ", roots.ToArray()));
            }
            if(RootPart == null && roots.Count > 0)
            {
                Syntax.Warning(this, "Vessel's root part attribute does not point to a valid part. Set root to: " + string.Join<KmlPart>(", ", roots.ToArray()));
            }
            else if (RootPart != null && !roots.Contains(RootPart))
            {
                Syntax.Warning(this, "Vessel's root part attribute is not equal to root identified from part structure: " + RootPart.ToString() + " <-> " + string.Join<KmlPart>(", ", roots.ToArray()));
            }
            return base.Identify();
        }

        /// <summary>
        /// When Parent is set or changed IdentifyParent will be called.
        /// Deriving classes can override this method and check for the new parent.
        /// </summary>
        protected override void IdentifyParent()
        {
            if (Parent != null && Parent.Tag.ToLower() == "flightstate")
            {
                Origin = VesselOrigin.Flightstate;
                Parent.CanBeDeleted = false;
                if (RootPart == null)
                {
                    Syntax.Warning(this, "Vessel's root part attribute does not point to a valid part");
                }
            }
            else
            {
                Origin = VesselOrigin.Other;
            }
            base.IdentifyParent();
        }

        /// <summary>
        /// After all items are loaded, each items Finalize is called.
        /// The roots list will contain all loaded items in KML tree structure.
        /// Each item can then check for other items to get further properties.
        /// </summary>
        /// <param name="roots">The loaded root items list</param>
        protected override void Finalize(List<KmlItem> roots)
        {
            base.Finalize(roots);
            if (Origin == VesselOrigin.Flightstate)
            {
                AssignedCrew.Clear();
                string[] tags = { "game", "roster" };
                KmlNode rosterNode = GetNodeFromDeep(roots, tags);
                if (rosterNode != null)
                {
                    foreach (KmlPart part in Parts)
                    {
                        foreach (KmlAttrib attrib in part.Attribs)
                        {
                            if (attrib.Name.ToLower() == "crew" && attrib.Value.Length > 0)
                            {
                                int oldCrewCount = AssignedCrew.Count;
                                foreach (KmlNode kerbalNode in rosterNode.Children)
                                {
                                    if (kerbalNode is KmlKerbal)
                                    {
                                        KmlKerbal kerbal = (KmlKerbal)kerbalNode;
                                        if (attrib.Value.ToLower() == kerbal.Name.ToLower())
                                        {
                                            // Duplicate entries are checked on the kerbal side,
                                            // here only duplicates within one vessel could be checked, 
                                            // there all vessels are checked
                                            AssignedCrew.Add(kerbal);
                                        }
                                    }
                                }
                                if (AssignedCrew.Count < oldCrewCount + 1)
                                {
                                    Syntax.Warning(attrib, "Crew could not be assigned, this kerbal does not exist: " + attrib.Value);
                                }
                                else if (AssignedCrew.Count > oldCrewCount + 1)
                                {
                                    Syntax.Warning(attrib, "Crew member not unique, there are multiple kerbals with name: " + attrib.Value);
                                }
                            }
                        }
                    }
                }
            }
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

        private void Flag_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            Flags.Clear();
            foreach (KmlPart part in Parts)
            {
                if (part.Flag != "" && !Flags.Any(x => x.ToLower() == part.Flag.ToLower()))
                {
                    Flags.Add(part.Flag);
                }
            }
        }
    }
}
