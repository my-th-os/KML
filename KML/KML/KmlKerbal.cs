using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            // TODO KmlKerbal.KmlKerbal(): Make kerbals deletable
            CanBeDeleted = false;

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
                if (attrib.Name.ToLower() == "type")
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
            Brave = 0.0;
            Dumb = 0.0;
            base.Clear();
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
        /// Generates a nice informative string to be used in display for this kerbal.
        /// It will contain the "Tag", "Name", "Type" and "Trait".
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
