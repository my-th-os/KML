using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KML
{
    /// <summary>
    /// KmlKerbal represents a KmlNode with the "KERBAL" tag.
    /// </summary>
    public class KmlKerbal : KmlNode
    {
        /// <summary>
        /// Possible origins where this node is found.
        /// Regular KERBAL nodes are children of the "ROSTER" node.
        /// </summary>
        public enum KerbalOrigin 
        { 
            /// <summary>
            /// Kerbal found under ROSTER node
            /// </summary>
            Roster, 

            /// <summary>
            /// Kerbal found anywhere else
            /// </summary>
            Other 
        };

        /// <summary>
        /// Get the origin of this node.
        /// <see cref="KML.KmlKerbal.KerbalOrigin"/>
        /// </summary>
        public KerbalOrigin Origin { get; private set; }

        /// <summary>
        /// Get the "Type" attribute as a property (Crew, Applicant, Tourist).
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// Get the "Trait" attribute as a property (Pilot, Engineer, Scientist, Tourist).
        /// </summary>
        public string Trait { get; private set; }

        /// <summary>
        /// Get the "State" attribute as a property (Available, Assigned, Missing).
        /// </summary>
        public string State { get; private set; }

        /// <summary>
        /// Get the "Brave" attribute as a property (should be in range 0.0 to 1.0).
        /// </summary>
        public double Brave { get; private set; }

        /// <summary>
        /// Get the "Dumb" attribute as a property (should be in range 0.0 to 1.0)
        /// </summary>
        public double Dumb { get; private set; }

        /// <summary>
        /// Get the KmlVessel this kerbal is assigned to
        /// </summary>
        public KmlVessel AssignedVessel { get; private set; }

        /// <summary>
        /// Get the KmlPart this kerbal is assigned to
        /// </summary>
        public KmlPart AssignedPart { get; private set; }

        private KmlAttrib AssignedCrewAttrib { get; set; }

        /// <summary>
        /// Creates a KmlKerbal as a copy of a given KmlNode.
        /// </summary>
        /// <param name="node">The KmlNode to copy</param>
        public KmlKerbal(KmlNode node)
            : base(node.Line)
        {
            // First parent is null, will be set later when added to parent,
            // then  IdentifyParent() will set Origin.
            Origin = KerbalOrigin.Other;

            Type = "";
            Trait = "";
            State = "";
            Brave = 0.0;
            Dumb = 0.0;
            AssignedVessel = null;
            AssignedPart = null;
            AssignedCrewAttrib = null;

            AddRange(node.AllItems);
        }

        /// <summary>
        /// Adds a child KmlItem to this nodes lists of children, depending of its
        /// derived class KmlNode, KmlAttrib or further derived from these.
        /// When an KmlAttrib "Name", "Type" or "Trait" are found, their value 
        /// will be used for the corresponding property of this node.
        /// </summary>
        /// <param name="beforeItem">The KmlItem where the new item should be inserted before</param>
        /// <param name="newItem">The KmlItem to add</param>
        protected override void Add(KmlItem beforeItem, KmlItem newItem)
        {
            if (newItem is KmlAttrib)
            {
                KmlAttrib attrib = (KmlAttrib)newItem;
                if (attrib.Name.ToLower() == "name")
                {
                    // Name property is managed by KmlNode,
                    // but we need another method to be called on name change event
                    // to rename the crew attrib in assigned vessel part also
                    attrib.AttribValueChanged += CrewName_Changed;
                }
                else if (attrib.Name.ToLower() == "type")
                {
                    Type = attrib.Value;

                    // Get notified when Type changes
                    attrib.AttribValueChanged += Type_Changed;
                    attrib.CanBeDeleted = false;
                }
                else if (attrib.Name.ToLower() == "trait")
                {
                    Trait = attrib.Value;

                    // Get notified when Trait changes
                    attrib.AttribValueChanged += Trait_Changed;
                    attrib.CanBeDeleted = false;
                }
                else if (attrib.Name.ToLower() == "state")
                {
                    State = attrib.Value;

                    // Get notified when State changes
                    attrib.AttribValueChanged += State_Changed;
                    attrib.CanBeDeleted = false;
                }
                else if (attrib.Name.ToLower() == "brave")
                {
                    Brave = GetDoubleValue(attrib.Value, Brave);

                    // Get notified when Trait changes
                    attrib.AttribValueChanged += Brave_Changed;
                    attrib.CanBeDeleted = false;
                }
                else if (attrib.Name.ToLower() == "dumb")
                {
                    Dumb = GetDoubleValue(attrib.Value, Dumb);

                    // Get notified when Dumb changes
                    attrib.AttribValueChanged += Dumb_Changed;
                    attrib.CanBeDeleted = false;
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
            Trait = "";
            State = "";
            Brave = 0.0;
            Dumb = 0.0;
            base.Clear();
        }

        /// <summary>
        /// Gets called before item is deleted.
        /// </summary>
        /// <returns>Return true on success. If false is returned the deletion will be canceled</returns>
        protected override bool BeforeDelete()
        {
            SendHome();
            return true;
        }

        private double GetDoubleValue(string value, double defaultValue)
        {
            double d = 0.0;
            if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out d))
            {
                return d;
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Send the kerbal home to astronaut complex.
        /// Kerbal will be removed from assigned vessel/part
        /// and state will be set to 'Available'.
        /// </summary>
        public void SendHome()
        {
            if (AssignedCrewAttrib != null)
            {
                AssignedCrewAttrib.CanBeDeleted = true;
                AssignedCrewAttrib.Delete();
                AssignedCrewAttrib = null;
                AssignedPart = null;
                AssignedVessel = null;
            }
            foreach (KmlAttrib attrib in Attribs)
            {
                if (attrib.Name.ToLower() == "state")
                {
                    attrib.Value = "Available";
                    // This will invoke ToStringChanged so State property 
                    // and all display items will be updated.
                    // That's why this is the last action in this method
                }
            }
        }

        /// <summary>
        /// Generates a nice informative string to be used in display for this kerbal.
        /// It will contain the "Tag", "Name", "Type", "Trait" and "State".
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
            if (Trait.Length > 0 && Trait != Type)
            {
                if (s.Length > 0)
                {
                    s += ", ";
                }
                s += Trait;
            }
            if (State.Length > 0)
            {
                if (s.Length > 0)
                {
                    s += ", ";
                }
                s += State;
            }
            if (s.Length > 0)
            {
                s = " (" + s + ")"; ;
            }
            return Tag + s;
        }

        /// <summary>
        /// When Parent is set or changed IdentifyParent will be called.
        /// Deriving classes can override this method and check for the new parent.
        /// </summary>
        protected override void IdentifyParent()
        {
            if (Parent != null && Parent.Tag.ToLower() == "roster")
            {
                Origin = KerbalOrigin.Roster;
                Parent.CanBeDeleted = false;
            }
            else
            {
                Origin = KerbalOrigin.Other;
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
            if (Origin == KerbalOrigin.Roster)
            {
                string[] tags = { "game", "flightstate" };
                KmlNode flightStateNode = GetNodeFromDeep(roots, tags);
                if (flightStateNode != null)
                {
                    foreach (KmlNode vesselNode in flightStateNode.Children)
                    {
                        if (vesselNode is KmlVessel)
                        {
                            KmlVessel vessel = (KmlVessel)vesselNode;
                            foreach (KmlPart part in vessel.Parts)
                            {
                                foreach (KmlAttrib attrib in part.Attribs)
                                {
                                    if (attrib.Name.ToLower() == "crew" && attrib.Value.ToLower() == Name.ToLower())
                                    {
                                        if (AssignedCrewAttrib == null)
                                        {
                                            AssignedVessel = vessel;
                                            AssignedPart = part;
                                            AssignedCrewAttrib = attrib;
                                            attrib.CanBeDeleted = false;
                                        }
                                        else
                                        {
                                            Syntax.Warning(attrib, "Kerbal is listed in multiple vessel part's crew. Kerbal: " + Name +
                                                "Assigned vessel: " + AssignedVessel.Name + ", Assigned part: " + AssignedPart +
                                                ", Unused Vessel: " + vessel.Name + ", Unused part: " + part);
                                        }
                                        if (State.ToLower() != "assigned")
                                        {
                                            Syntax.Warning(this, "Kerbal is listed in a vessels crew list, but state is not 'Assigned'. Vessel: " + vessel.Name + ", Part: " + part);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (AssignedCrewAttrib == null && State.ToLower() == "assigned" && Type.ToLower() != "unowned")
                {
                    Syntax.Warning(this, "Kerbal state is 'Assigned' but not listed in any vessels crew list");
                }
            }
        }

        private void CrewName_Changed(object sender, RoutedEventArgs e)
        {
            // Name propert has not changed yet (or maybe), to be sure get the changed attrib
            KmlAttrib nameAttrib = GetAttribWhereValueChanged(sender);
            if (AssignedCrewAttrib != null)
            {
                AssignedCrewAttrib.Value = nameAttrib.Value;
            }
        }

        private void Type_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            Type = GetAttribWhereValueChanged(sender).Value;
            InvokeToStringChanged();
        }

        private void Trait_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            Trait = GetAttribWhereValueChanged(sender).Value;
            InvokeToStringChanged();
        }

        private void State_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            State = GetAttribWhereValueChanged(sender).Value;
            InvokeToStringChanged();
        }

        private void Brave_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            Brave = GetDoubleValue(GetAttribWhereValueChanged(sender).Value, Brave);
            // TODO KmlKerbal.Brave_Changed(): Define another event than ToStringChanged (ToString doesn't involve "Brave")
            InvokeToStringChanged();
        }

        private void Dumb_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            Dumb = GetDoubleValue(GetAttribWhereValueChanged(sender).Value, Dumb);
            // TODO KmlKerbal.Dumb_Changed(): Define another event than ToStringChanged (ToString doesn't involve "Dumb")
            InvokeToStringChanged();
        }
    }
}
