using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// using System.Windows.Media.Media3D; // seems to be not supported by Mono

namespace KML
{
    /// <summary>
    /// KmlPart represents a KmlNode with the "PART" tag.
    /// </summary>
    public class KmlPart : KmlNode
    {
        /// <summary>
        /// Possible origins where this node is found.
        /// Regular PART nodes are children of a "VESSEL" node.
        /// </summary>
        public enum PartOrigin 
        { 
            /// <summary>
            /// Part found under a VESSEL node
            /// </summary>
            Vessel, 

            /// <summary>
            /// Part found anywhere else
            /// </summary>
            Other 
        };

        /// <summary>
        /// Get the origin of this node.
        /// <see cref="KML.KmlPart.PartOrigin"/>
        /// </summary>
        public PartOrigin Origin { get; private set; }

        /// <summary>
        /// Get a list of all KmlResource children.
        /// </summary>
        public List<KmlResource> Resources { get; private set; }

        /// <summary>
        /// Get a set of all types (names) of resources in this part.
        /// </summary>
        public SortedSet<string> ResourceTypes { get; private set; }

        /// <summary>
        /// Get the uid this part is identified by.
        /// </summary>
        public string Uid { get; private set; }

        /// <summary>
        /// Get x, y, z coordinates of this part relative to the vessel.
        /// </summary>
        public Point3D Position { get; private set; }

        /// <summary>
        /// Get the flag name of this part.
        /// </summary>
        public string Flag { get; private set; }

        /// <summary>
        /// Get the parent part index from vessel structure.
        /// This data is read from KML attributes within this part.
        /// </summary>
        public int ParentPartIndex { get; private set; }

        /// <summary>
        /// Get the parent part from vessel structure.
        /// This data is identified after vessel reading is completed.
        /// </summary>
        public KmlPart ParentPart { get; private set; }

        // TODO KmlPart: Not make lists public, better have a Add(method)

        /// <summary>
        /// Get a list of part indexes this part is node attached to.
        /// This data is read from KML attributes within this part.
        /// </summary>
        public List<int> AttachedToNodeIndices { get; private set; }

        /// <summary>
        /// Get a part index this part is surface attached to.
        /// This data is read from KML attributes within this part.
        /// </summary>
        public int AttachedToSurfaceIndex { get; private set; }

        /// <summary>
        /// Get the surface attachment collider string if present.
        /// This data is read from KML attributes within this part.
        /// </summary>
        public string AttachedToSurfaceCollider { get; private set; }

        /// <summary>
        /// Get a list of KmlParts this part is node attached to in top direction.
        /// The list will be filled by static BuildAttachmentStructure() method after a complete vessel is read,
        /// based on the indices and index lists that are filled on reading this part alone.
        /// </summary>
        public List<KmlPart> AttachedToPartsTop { get; private set; }

        /// <summary>
        /// Get a list of KmlParts this part is node attached to in bottom direction.
        /// The list will be filled by static BuildAttachmentStructure() method after a complete vessel is read,
        /// based on the indices and index lists that are filled on reading this part alone.
        /// </summary>
        public List<KmlPart> AttachedToPartsBottom { get; private set; }

        /// <summary>
        /// Get a list of KmlParts this part is node attached to in front direction.
        /// The list will be filled by static BuildAttachmentStructure() method after a complete vessel is read,
        /// based on the indices and index lists that are filled on reading this part alone.
        /// </summary>
        public List<KmlPart> AttachedToPartsFront { get; private set; }

        /// <summary>
        /// Get a list of KmlParts this part is node attached to in back direction.
        /// The list will be filled by static BuildAttachmentStructure() method after a complete vessel is read,
        /// based on the indices and index lists that are filled on reading this part alone.
        /// </summary>
        public List<KmlPart> AttachedToPartsBack { get; private set; }

        /// <summary>
        /// Get a list of KmlParts this part is node attached to in left direction.
        /// The list will be filled by static BuildAttachmentStructure() method after a complete vessel is read,
        /// based on the indices and index lists that are filled on reading this part alone.
        /// </summary>
        public List<KmlPart> AttachedToPartsLeft { get; private set; }

        /// <summary>
        /// Get a list of KmlParts this part is node attached to in right direction.
        /// The list will be filled by static BuildAttachmentStructure() method after a complete vessel is read,
        /// based on the indices and index lists that are filled on reading this part alone.
        /// </summary>
        public List<KmlPart> AttachedToPartsRight { get; private set; }

        /// <summary>
        /// Get the KmlPart this node is surface atteached to. In a correct persistence file this should only be one.
        /// The part will be assigned by static BuildAttachmentStructure() method after a complete vessel is read,
        /// based on the AttachedToSurfaceIndex that is filled on reading this part alone.
        /// </summary>
        public KmlPart AttachedToPartSurface { get; private set; }

        /// <summary>
        /// Get a list of KmlParts node attached to this part in top direction.
        /// The list will be filled by static BuildAttachmentStructure() method after a complete vessel is read,
        /// based on the indices and index lists that are filled on reading the other parts.
        /// </summary>
        public List<KmlPart> AttachedPartsTop { get; private set; }

        /// <summary>
        /// Get a list of KmlParts node attached to this part in bottom direction.
        /// The list will be filled by static BuildAttachmentStructure() method after a complete vessel is read,
        /// based on the indices and index lists that are filled on reading the other parts.
        /// </summary>
        public List<KmlPart> AttachedPartsBottom { get; private set; }

        /// <summary>
        /// Get a list of KmlParts node attached to this part in front direction.
        /// The list will be filled by static BuildAttachmentStructure() method after a complete vessel is read,
        /// based on the indices and index lists that are filled on reading the other parts.
        /// </summary>
        public List<KmlPart> AttachedPartsFront { get; private set; }

        /// <summary>
        /// Get a list of KmlParts node attached to this part in back direction.
        /// The list will be filled by static BuildAttachmentStructure() method after a complete vessel is read,
        /// based on the indices and index lists that are filled on reading the other parts.
        /// </summary>
        public List<KmlPart> AttachedPartsBack { get; private set; }

        /// <summary>
        /// Get a list of KmlParts node attached to this part in left direction.
        /// The list will be filled by static BuildAttachmentStructure() method after a complete vessel is read,
        /// based on the indices and index lists that are filled on reading the other parts.
        /// </summary>
        public List<KmlPart> AttachedPartsLeft { get; private set; }

        /// <summary>
        /// Get a list of KmlParts node attached to this part in right direction.
        /// The list will be filled by static BuildAttachmentStructure() method after a complete vessel is read,
        /// based on the indices and index lists that are filled on reading the other parts.
        /// </summary>
        public List<KmlPart> AttachedPartsRight { get; private set; }

        /// <summary>
        /// Get a list of KmlParts surface attached to this part.
        /// The list will be filled by static BuildAttachmentStructure() method after a complete vessel is read,
        /// based on the indices and index lists that are filled on reading the other parts.
        /// </summary>
        public List<KmlPart> AttachedPartsSurface { get; private set; }

        /// <summary>
        /// Get info whether this part has resources or not.
        /// </summary>
        public bool HasResources
        {
            get
            {
                return Resources.Count > 0;
            }
        }

        /// <summary>
        /// Get the worst ratio of all resources in this part.
        /// If there is no resource it returns -1.0;
        /// </summary>
        public double WorstResourceRatio 
        { 
            get
            {
                if (HasResources)
                {
                    double worstRatio = 1.0;
                    foreach (KmlResource res in Resources)
                    {
                        if (res.AmountRatio < worstRatio)
                        {
                            worstRatio = res.AmountRatio;
                        }
                    }
                    return worstRatio;
                }
                else
                {
                    return -1.0;
                }
            }
        }

        /// <summary>
        /// This bool value has no meaning within the part itself,
        /// but could be used from other methods to traverse over all parts
        /// and mark the ones it has already been to.
        /// </summary>
        public bool Visited { get; set; }

        private string CraftName { get; set; }

        /// <summary>
        /// Creates a KmlPart as a copy of a given KmlNode.
        /// </summary>
        /// <param name="node">The KmlNode to copy</param>
        public KmlPart(KmlNode node)
            : base(node.Line)
        {
            // First parent is null, will be set later when added to parent,
            // then  IdentifyParent() will set Origin.
            Origin = PartOrigin.Other;

            Resources = new List<KmlResource>();
            ResourceTypes = new SortedSet<string>();
            Uid = "";
            Position = new Point3D(0.0, 0.0, 0.0);
            Flag = "";

            ParentPartIndex = -1;
            ParentPart = null;

            AttachedToNodeIndices = new List<int>();
            AttachedToSurfaceIndex = -1;

            AttachedToPartsTop = new List<KmlPart>();
            AttachedToPartsBottom = new List<KmlPart>();
            AttachedToPartsFront = new List<KmlPart>();
            AttachedToPartsBack = new List<KmlPart>();
            AttachedToPartsLeft = new List<KmlPart>();
            AttachedToPartsRight = new List<KmlPart>();
            AttachedToPartSurface = null;
            
            AttachedPartsTop = new List<KmlPart>();
            AttachedPartsBottom = new List<KmlPart>();
            AttachedPartsFront = new List<KmlPart>();
            AttachedPartsBack = new List<KmlPart>();
            AttachedPartsLeft = new List<KmlPart>();
            AttachedPartsRight = new List<KmlPart>();
            AttachedPartsSurface = new List<KmlPart>();

            Visited = false;
            CraftName = "";

            // Default is true for possible inserted empty nodes with "PART" tag
            CanBeDeleted = true;

            AddRange(node.AllItems);
        }

        /// <summary>
        /// Adds a child KmlItem to this nodes lists of children, depending of its
        /// derived class KmlNode, KmlAttrib or further derived from these.
        /// When an KmlAttrib "Name" is found, its value 
        /// will be used for the corresponding property of this node.
        /// </summary>
        /// <param name="beforeItem">The KmlItem where the new item should be inserted before</param>
        /// <param name="newItem">The KmlItem to add</param>
        protected override void Add(KmlItem beforeItem, KmlItem newItem)
        {
            if (newItem is KmlAttrib)
            {
                KmlAttrib attrib = (KmlAttrib)newItem;
                if (attrib.Name.ToLower() == "part")
                {
                    CraftName = attrib.Value;
                    attrib.AttribValueChanged += PartName_Changed;
                    attrib.CanBeDeleted = false;
                }
                else if (attrib.Name.ToLower() == "uid")
                {
                    Uid = attrib.Value;
                    attrib.AttribValueChanged += Uid_Changed;
                    attrib.CanBeDeleted = false;
                }
                else if (attrib.Name.ToLower() == "flag")
                {
                    Flag = attrib.Value;
                    attrib.AttribValueChanged += Flag_Changed;
                    attrib.CanBeDeleted = false;
                }
                else if (attrib.Name.ToLower() == "parent")
                {
                    int p = ParentPartIndex;
                    if (int.TryParse(attrib.Value, out p))
                    {
                        ParentPartIndex = p;
                    }
                    else
                    {
                        Syntax.Warning(this, "Unreadable parent part: " + attrib.ToString());
                    }
                    attrib.AttribValueChanged += ParentPart_Changed;
                    attrib.CanBeDeleted = false;
                }
                else if (attrib.Name.ToLower() == "attn")
                {
                    // Value looks like "top, 12", "bottom, -1", "left, 1", "top2, 3", etc.
                    string[] items = attrib.Value.Split(new char[] {','});
                    int index = -1;
                    if (items.Count() == 2 && int.TryParse(items[1], out index))
                    {
                        if (index >= 0)
                        {
                            AttachedToNodeIndices.Add(index);
                            attrib.CanBeDeleted = false;
                        }
                    }
                    else
                    {
                        Syntax.Warning(this, "Bad formatted part node attachment: " + attrib.ToString());
                    }
                    attrib.AttribValueChanged += AttachmentNode_Changed;
                }
                else if (attrib.Name.ToLower() == "srfn")
                {
                    // Value looks like "srfAttach, 12"
                    string[] items = attrib.Value.Split(new char[] { ',' });
                    int index = -1;
                    if ((items.Count() == 2 || items.Count() == 3) && int.TryParse(items[1], out index))
                    {
                        if (index >= 0)
                        {
                            if (AttachedToSurfaceIndex < 0)
                            {
                                AttachedToSurfaceIndex = index;
                                attrib.CanBeDeleted = false;
								if (items.Count() == 3)
								{
									AttachedToSurfaceCollider = items[2];
								}
                            }
                            else
                            {
                                Syntax.Warning(this, "More than one surface attachment is not allowed, already attached to [" + AttachedToSurfaceIndex + "], could not attach to [" + index + "]");
                            }
                        }
                    }
                    else
                    {
                        Syntax.Warning(this, "Bad formatted part surface attachment: " + attrib.ToString());
                    }
                    attrib.AttribValueChanged += AttachmentSurface_Changed;
                }
                else if (attrib.Name.ToLower() == "position")
                {
                    // Value looks like "0.1,0,-0.3E-07"
                    string[] items = attrib.Value.Split(new char[] { ',' });
                    double x = 0;
                    double y = 0;
                    double z = 0;
                    if (items.Count() == 3 &&
                        double.TryParse(items[0], NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out x) &&
                        double.TryParse(items[1], NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out y) &&
                        double.TryParse(items[2], NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out z))
                    {
                        Position = new Point3D(x, y, z);
                        attrib.CanBeDeleted = false;
                    }
                    else
                    {
                        Syntax.Warning(this, "Bad formatted part position: " + attrib.ToString());
                    }
                }
            }
            else if (newItem is KmlResource)
            {
                KmlResource res = (KmlResource)newItem;
                Resources.Add(res);
                ResourceTypes.Add(res.Name);

                // Get notified when resources change
                res.MaxAmount.AttribValueChanged += Resources_Changed;
                res.MaxAmount.CanBeDeleted = false;
                res.Amount.AttribValueChanged += Resources_Changed;
                res.Amount.CanBeDeleted = false;
                // TODO KmlPart.Add(): Get notified when resource is deleted
                // Or make resource not deletable?
            }
            base.Add(beforeItem, newItem);
        }

        /// <summary>
        /// Clear all child nodes and attributes from this node.
        /// </summary>
        public override void Clear()
        {
            Resources.Clear();
            ResourceTypes.Clear();
            Uid = "";
            Position = new Point3D(0.0, 0.0, 0.0);
            Flag = "";
            ParentPartIndex = -1;
            ParentPart = null;
            ClearAttachments();
            CraftName = "";
            base.Clear();
        }

        private void ClearAttachments()
        {
            AttachedToNodeIndices.Clear();
            AttachedToSurfaceIndex = -1;
            ClearVirtualAttachments();
        }

        private void ClearVirtualAttachments()
        {
            AttachedPartsBack.Clear();
            AttachedPartsBottom.Clear();
            AttachedPartsFront.Clear();
            AttachedPartsLeft.Clear();
            AttachedPartsRight.Clear();
            AttachedPartsSurface.Clear();
            AttachedPartsTop.Clear();
            AttachedToPartsBack.Clear();
            AttachedToPartsBottom.Clear();
            AttachedToPartsFront.Clear();
            AttachedToPartsLeft.Clear();
            AttachedToPartsRight.Clear();
            AttachedToPartsTop.Clear();
            AttachedToPartSurface = null;
        }

        /// <summary>
        /// After a part is completely loaded the Intentify() method is called.
        /// Then can be determined, if this part represents a docking port part or not,
        /// checking all child nodes and their attributes.
        /// If it doesen't need to be replaced, null is returned.
        /// <see cref="KML.KmlItem.Identify()"/>
        /// </summary>
        /// <returns>A KmlPortDock if this KmlPart needs to be replaced by or null otherwise</returns>
        protected override KmlItem Identify()
        {
            if (KmlPartDock.PartIsDock(this))
            {
                return new KmlPartDock(this);
            }
            else
            {
                return base.Identify();
            }
        }

        /// <summary>
        /// When Parent is set or changed IdentifyParent will be called.
        /// Deriving classes can override this method and check for the new parent.
        /// </summary>
        protected override void IdentifyParent()
        {
            if (Parent != null && Parent.Tag.ToLower() == "vessel")
            {
                Origin = PartOrigin.Vessel;
            }
            else
            {
                Origin = PartOrigin.Other;
            }
            base.IdentifyParent();
        }

        /// <summary>
        /// Call on the part where to insert before to prepare part index structure.
        /// </summary>
        public void InsertionPreparation()
        {
            if (Parent is KmlVessel)
            {
                KmlVessel vessel = (KmlVessel)Parent;
                ReIndexStructureForPartInsertion(this, vessel);
            }
        }

        /// <summary>
        /// Call on the part where to insert before after insertion happened.
        /// </summary>
        public void InsertionFinalization()
        {
            if (Parent is KmlVessel)
            {
                KmlVessel vessel = (KmlVessel)Parent;
                int index = vessel.Parts.IndexOf(this);
                for (int i = index; i < vessel.Parts.Count; i++)
                {
                    vessel.Parts[i].InvokeToStringChanged();
                }
            }
        }

        /// <summary>
        /// Refill all resources of this part.
        /// </summary>
        public void Refill()
        {
            foreach(KmlResource res in Resources)
            {
                res.Refill();
            }
        }

        /// <summary>
        /// Refill all resources of a certain type of this part.
        /// </summary>
        /// <param name="type">The type (name) of the resource to refill (ElectricCharge, LiquidFuel, Oxidizer, etc.)</param>
        public void Refill(string type)
        {
            foreach (KmlResource res in Resources)
            {
                if (res.Name.ToLower() == type.ToLower())
                {
                    res.Refill();
                }
            }
        }

        /// <summary>
        /// Exchange the flag of this part when it matches the oldFlag.
        /// If it doesn't match, nothing will happen.
        /// </summary>
        /// <param name="oldFlag">The old flag name this part should have</param>
        /// <param name="newFlag">The new flag name to apply</param>
        public void FlagExchange(string oldFlag, string newFlag)
        {
            if (Flag.ToLower() != oldFlag.ToLower())
            {
                return;
            }
            foreach (KmlAttrib attrib in Attribs)
            {
                if (attrib.Name.ToLower() == "flag" && attrib.Value.ToLower() == oldFlag.ToLower())
                {
                    attrib.Value = newFlag;
                    Flag = newFlag;
                    return;
                }
            }
        }

        /// <summary>
        /// Gets called before item is deleted.
        /// </summary>
        /// <returns>Return true on success. If false is returned the deletion will be canceled</returns>
        protected override bool BeforeDelete()
        {
            if (Parent is KmlVessel)
            {
                KmlVessel vessel = (KmlVessel)Parent;
                foreach (KmlKerbal kerbal in vessel.AssignedCrew)
                {
                    if (kerbal.AssignedPart == this)
                    {
                        kerbal.SendHome();
                    }
                }
                // Fix Attachment structure on all other parts
                ReIndexStructureForPartDeletion(this, vessel);
            }
            return true;
        }

        private static void ReIndexStructureForPartDeletion(KmlPart delPart, KmlVessel vessel)
        {
            int delIndex = vessel.Parts.IndexOf(delPart);
            if (delIndex < 0)
                return;

            // Remove part from vessels part list
            vessel.Parts.Remove(delPart);

            foreach (KmlPart part in vessel.Parts)
            {
                // This is the essential part to change the persistent file data
                for (int a = part.Attribs.Count - 1; a >= 0; a--)
                {
                    KmlAttrib attrib = part.Attribs[a];
                    attrib.AttribValueChanged -= part.ParentPart_Changed;
                    attrib.AttribValueChanged -= part.AttachmentSurface_Changed;
                    attrib.AttribValueChanged -= part.AttachmentNode_Changed;
                    switch (attrib.Name.ToLower())
                    {
                        case "parent":
                            attrib.Value = ReIndexedValueForPartDeletion(delIndex, attrib.Value);
                            break;
                        case "sym":
                            // Having "sym = -1" or "sym = " could crash KSP (at least version 1.1.3)
                            attrib.Value = ReIndexedValueForPartDeletion(delIndex, attrib.Value);
                            if (attrib.Value == "-1")
                            {
                                part.Attribs.RemoveAt(a);
                                continue;
                            }
                            break;
                        case "srfn":
                        case "attn":
                            char[] separator = { ',' };
                            string[] s = attrib.Value.Split(separator);
                            if (s.Length == 2)
                            {
                                attrib.Value = s[0] + ", " + ReIndexedValueForPartDeletion(delIndex, s[1].Trim());
                            }
                            if (s.Length == 3)
                            {
                                attrib.Value = s[0] + ", " + ReIndexedValueForPartDeletion(delIndex, s[1].Trim()) + "," + s[2];
                            }
                            break;
                    }
                    switch (attrib.Name.ToLower())
                    {
                        case "parent":
                            attrib.AttribValueChanged += part.ParentPart_Changed;
                            break;
                        case "srfn":
                            attrib.AttribValueChanged += part.AttachmentSurface_Changed;
                            break;
                        case "attn":
                            attrib.AttribValueChanged += part.AttachmentNode_Changed;
                            break;
                    }
                }

                // Do additional fixup of redundant attachment informations
                // First we need to fix indices like above
                for (int i = part.AttachedToNodeIndices.Count - 1; i >= 0; i--)
                {
                    if (part.AttachedToNodeIndices[i] > delIndex)
                        part.AttachedToNodeIndices[i]--;
                    else if (part.AttachedToNodeIndices[i] == delIndex)
                        part.AttachedToNodeIndices.RemoveAt(i);
                }
                
                // Parent index needs to be updated here since we unbound ParentPart_Changed from the above changed attribute while changing
                // TODO KmlPart.ReIndexStructureForPartDeletion(): Maybe find better usage of ParentPart_Changed
                if (part.ParentPartIndex > delIndex)
                    part.ParentPartIndex--;
                // must be excluded by preconditions to call delete: else if (part.ParentPartIndex == delIndex)
                
                // Remove the part from any list, call to Remove() causes no problem if it's not contained
                part.AttachedPartsBack.Remove(delPart);
                part.AttachedPartsBottom.Remove(delPart);
                part.AttachedPartsFront.Remove(delPart);
                part.AttachedPartsLeft.Remove(delPart);
                part.AttachedPartsRight.Remove(delPart);
                part.AttachedPartsSurface.Remove(delPart);
                part.AttachedPartsTop.Remove(delPart);
                part.AttachedToPartsBack.Remove(delPart);
                part.AttachedToPartsBottom.Remove(delPart);
                part.AttachedToPartsFront.Remove(delPart);
                part.AttachedToPartsLeft.Remove(delPart);
                part.AttachedToPartsRight.Remove(delPart);
                part.AttachedToPartsTop.Remove(delPart);
                // must be excluded by preconditions to call delete: if (part.AttachedToPartSurface == delPart);
                
                // Based on new attachment structure we can now update if this part can be deleted
                part.CanBeDeleted = part.CanPartBeDeleted();
                // And force GuiTreeNode delete item enabled state to update
                // TODO KmlPart:ReIndexStructureForPartDeletion(): Better event for change of CanBeDeleted
                part.InvokeToStringChanged();
            }
        }

        private static void ReIndexStructureForPartInsertion(KmlPart insPart, KmlVessel vessel)
        {
            int insIndex = vessel.Parts.IndexOf(insPart);
            if (insIndex < 0)
                return;

            foreach (KmlPart part in vessel.Parts)
            {
                // This is the essential part to change the persistent file data
                for (int a = part.Attribs.Count - 1; a >= 0; a--)
                {
                    KmlAttrib attrib = part.Attribs[a];
                    attrib.AttribValueChanged -= part.ParentPart_Changed;
                    attrib.AttribValueChanged -= part.AttachmentSurface_Changed;
                    attrib.AttribValueChanged -= part.AttachmentNode_Changed;
                    switch (attrib.Name.ToLower())
                    {
                        case "parent":
                            attrib.Value = ReIndexedValueForPartInsertion(insIndex, attrib.Value);
                            break;
                        case "sym":
                            // Having "sym = -1" or "sym = " could crash KSP (at least version 1.1.3)
                            attrib.Value = ReIndexedValueForPartInsertion(insIndex, attrib.Value);
                            if (attrib.Value == "-1")
                            {
                                part.Attribs.RemoveAt(a);
                                continue;
                            }
                            break;
                        case "srfn":
                        case "attn":
                            char[] separator = { ',' };
                            string[] s = attrib.Value.Split(separator);
                            if (s.Length == 2)
                            {
                                attrib.Value = s[0] + ", " + ReIndexedValueForPartInsertion(insIndex, s[1].Trim());
                            }
                            if (s.Length == 3)
                            {
                                attrib.Value = s[0] + ", " + ReIndexedValueForPartInsertion(insIndex, s[1].Trim()) + "," + s[2];
                            }
                            break;
                    }
                    switch (attrib.Name.ToLower())
                    {
                        case "parent":
                            attrib.AttribValueChanged += part.ParentPart_Changed;
                            break;
                        case "srfn":
                            attrib.AttribValueChanged += part.AttachmentSurface_Changed;
                            break;
                        case "attn":
                            attrib.AttribValueChanged += part.AttachmentNode_Changed;
                            break;
                    }
                }

                // Do additional fixup of redundant attachment informations
                // First we need to fix indices like above
                for (int i = part.AttachedToNodeIndices.Count - 1; i >= 0; i--)
                {
                    if (part.AttachedToNodeIndices[i] >= insIndex)
                        part.AttachedToNodeIndices[i]++;
                }

                // Parent index needs to be updated here since we unbound ParentPart_Changed from the above changed attribute while changing
                // TODO KmlPart.ReIndexStructureForPartInsertion(): Maybe find better usage of ParentPart_Changed
                if (part.ParentPartIndex >= insIndex)
                    part.ParentPartIndex++;
            }
        }

        private static string ReIndexedValueForPartDeletion(int delIndex, string value)
        {
            int i;
            if (!int.TryParse(value, out i))
                return value;
            if (i == delIndex)
                return "-1";
            if (i > delIndex)
                return (i - 1).ToString();
            return value;
        }

        private static string ReIndexedValueForPartInsertion(int insIndex, string value)
        {
            int i;
            if (!int.TryParse(value, out i))
                return value;
            if (i >= insIndex)
                return (i + 1).ToString();
            return value;
        }

        /// <summary>
        /// Parts of a vessel are first read one after the other, so the first part can only store indices of later parts it is connected to.
        /// After a complete vessel is read also all parts are read and the indices can be translated in references to now existing KmlParts.
        /// This is done by this method. Also reverse information (what parts are connected to this one) is then stored in a part.
        /// </summary>
        /// <param name="parts">The list of KmlParts, a KmlVessel will have one</param>
        /// <returns>A list of root parts (not pointing to another parent part). Could be more than one, if some connections are broken.</returns>
        public static List<KmlPart> BuildAttachmentStructure(List<KmlPart> parts)
        {
            foreach (KmlPart part in parts)
            {
                part.ClearVirtualAttachments();
                if (part is KmlPartDock)
                {
                    (part as KmlPartDock).NeedsRepair = false;
                    // TODO KmlPart.BuildAttachmentStructure(): Invoke another event than ToStringChanged
                    // For the moment this will cause the GuiTreeNode to rebuild, including the context menu
                    part.InvokeToStringChanged();
                }
            }

            List<KmlPart> roots = new List<KmlPart>();
            for (int i = 0; i < parts.Count; i++ )
            {
                // Check parent connection
                KmlPart part = parts[i];
                if(part.ParentPartIndex == i)
                {
                    // Parent part is itself, so ParentPart property stays null
                    roots.Add(part);
                }
                else if (part.ParentPartIndex < 0 || part.ParentPartIndex >= parts.Count)
                {
                    Syntax.Warning(part, "Part's parent part index [" + part.ParentPartIndex + "] does not point to a valid part");
                    roots.Add(part);
                }
                else 
                {
                    part.ParentPart = parts[part.ParentPartIndex];
                    if (!part.AttachedToNodeIndices.Contains(part.ParentPartIndex) && part.AttachedToSurfaceIndex != part.ParentPartIndex)
                    {
                        // Part could be docked to parent
                        if ((part is KmlPartDock) && (part.ParentPart is KmlPartDock))
                        {
                            KmlPartDock docker = (KmlPartDock)part;
                            KmlPartDock dockee = (KmlPartDock)part.ParentPart;

                            // In case of "Docked (same vessel)" there has to be another docker-dockee connection and that would have connection via parent part
                            if (docker.DockState.ToLower() == "docked (docker)")
                            {
                                if (dockee.DockState.ToLower() != "docked (dockee)")
                                {
                                    Syntax.Warning(dockee, "Dock part is parent of other docker part. Docking state should be 'Docked (dockee)' but is '" + dockee.DockState + "', other dock: " + docker);
                                    docker.NeedsRepair = true;
                                    dockee.NeedsRepair = true;
                                }
                                else
                                {
                                    KmlPartDock.BuildDockStructure(dockee, docker);
                                    KmlPartDock.BuildDockStructure(docker, dockee);
                                }
                            }
                            else if (docker.DockState.ToLower() == "docked (dockee)")
                            {
                                if (dockee.DockState.ToLower() != "docked (docker)")
                                {
                                    Syntax.Warning(dockee, "Dock part is parent of other dockee part. Docking state should be 'Docked (docker)' but is '" + dockee.DockState + "', other dock: " + docker);
                                    docker.NeedsRepair = true;
                                    dockee.NeedsRepair = true;
                                }
                                else
                                {
                                    KmlPartDock.BuildDockStructure(dockee, docker);
                                    KmlPartDock.BuildDockStructure(docker, dockee);
                                }
                            }
                            else if (docker.DockType == KmlPartDock.DockTypes.KasCPort && dockee.DockType == KmlPartDock.DockTypes.KasCPort)
                            {
                                // TODO KmlPart.BuildAttachmentStructure(): Some sanity checks on KAS links?
                                KmlPartDock.BuildDockStructure(dockee, docker);
                                KmlPartDock.BuildDockStructure(docker, dockee);
                            }
                            else
                            {
                                if (dockee.DockState.ToLower() == "docked (dockee)")
                                {
                                    Syntax.Warning(docker, "Dock part is docked to parent dockee part. Docking state should be 'Docked (docker)' but is '" + docker.DockState + "', parent dock: " + dockee);
                                }
                                else if (dockee.DockState.ToLower() == "docked (docker)")
                                {
                                    Syntax.Warning(docker, "Dock part is docked to parent docker part. Docking state should be 'Docked (dockee)' but is '" + docker.DockState + "', parent dock: " + dockee);
                                }
                                else
                                {
                                    Syntax.Warning(docker, "Dock part is docked to parent dock part. Docking state should be 'Docked (docker)' or 'Docked (dockee)' but is '" + docker.DockState + "', parent dock: " + dockee);
                                    Syntax.Warning(dockee, "Dock part is parent of other dock part. Docking state should be 'Docked (dockee)' or 'Docked (docker)' but is '" + dockee.DockState + "', other dock: " + docker);
                                }
                                docker.NeedsRepair = true;
                                dockee.NeedsRepair = true;
                            }
                        }
                        // Part could be grappled by parent
                        else if ((part.ParentPart is KmlPartDock) && (part.ParentPart as KmlPartDock).DockType == KmlPartDock.DockTypes.Grapple)
                        {
                            KmlPartDock grapple = (KmlPartDock)part.ParentPart;

                            if (grapple.DockUid != part.Uid)
                            {
                                Syntax.Warning(part, "Part not attached or grappled by parent grappling part: " + grapple);
                                Syntax.Warning(grapple, "Grappling part is parent of other part, but is not grappled to it: " + part);
                                grapple.NeedsRepair = true;
                            }
                            else if (grapple.DockState.ToLower() != "grappled")
                            {
                                Syntax.Warning(part, "Part grappled by parent part. Docking state should be 'Grappled' but is '" + grapple.DockState + "', parent grapple: " + grapple);
                                Syntax.Warning(grapple, "Grappling part is parent of grappled part. Docking state should be 'Grappled' but is '" + grapple.DockState + "', grappled part: " + part);
                                grapple.NeedsRepair = true;
                            }
                            else
                            {
                                // It's docked but grappling needs a node attachment
                                KmlPartDock.BuildDockStructure(grapple, part);
                                Syntax.Warning(part, "Part is docked but not attached to parent grappling part: " + grapple);
                                Syntax.Warning(grapple, "Grappling part is parent and docked but not attached to grappled part: " + part);
                                grapple.NeedsRepair = true;
                            }
                        }

                        // Usually you can only attach a new part by a node to the surface of parent
                        // and not attach a part by surface to parents node. But if you have vessels docked
                        // this situation may happen and this leads to this additional check
                        else if (part.ParentPart.AttachedToSurfaceIndex != i && !part.ParentPart.AttachedToNodeIndices.Contains(i))
                        {
                            Syntax.Warning(part, "Part not attached to parent part: " + part.ParentPart);
                        }
                    }
                }

                // Check attachments
                foreach (int p in part.AttachedToNodeIndices)
                {
                    if(p >= 0 && p < parts.Count)
                    {
                        KmlPart other = parts[p];
                        // Sort attached part in the corresponding list, identified by position not by node name
                        double diffX = part.Position.X - other.Position.X;
                        double diffY = part.Position.Y - other.Position.Y;
                        double diffZ = part.Position.Z - other.Position.Z;
                        if (Math.Abs(diffX) > Math.Abs(diffY) && Math.Abs(diffX) > Math.Abs(diffZ))
                        {
                            if (diffX > 0)
                            {
                                other.AttachedPartsRight.Add(part);
                                part.AttachedToPartsLeft.Add(other);
                            }
                            else
                            {
                                other.AttachedPartsLeft.Add(part);
                                part.AttachedToPartsRight.Add(other);
                            }
                        }
                        else if (Math.Abs(diffZ) > Math.Abs(diffX) && Math.Abs(diffZ) > Math.Abs(diffY))
                        {
                            if (diffZ > 0)
                            {
                                other.AttachedPartsFront.Add(part);
                                part.AttachedToPartsBack.Add(other);
                            }
                            else
                            {
                                other.AttachedPartsBack.Add(part);
                                part.AttachedToPartsFront.Add(other);
                            }
                        }
                        else
                        {
                            if (diffY > 0)
                            {
                                other.AttachedPartsTop.Add(part);
                                part.AttachedToPartsBottom.Add(other);
                            }
                            else
                            {
                                other.AttachedPartsBottom.Add(part);
                                part.AttachedToPartsTop.Add(other);
                            }
                        }
                        if(!other.AttachedToNodeIndices.Contains(parts.IndexOf(part)))
                        {
                            if ((other is KmlPartDock) && (other as KmlPartDock).DockType == KmlPartDock.DockTypes.Grapple)
                            {
                                KmlPartDock grapple = (KmlPartDock)other;
                                if (grapple.DockUid != part.Uid)
                                {
                                    Syntax.Warning(part, "Part node attachment not responded from other grappling part: " + grapple);
                                    grapple.NeedsRepair = true;
                                }
                                else if (grapple.DockState.ToLower() != "grappled")
                                {
                                    Syntax.Warning(part, "Part grappled by other grappling part. Docking state should be 'Grappled' but is '" + grapple.DockState + ", other grapple: " + grapple);
                                    grapple.NeedsRepair = true;
                                }
                                else
                                {
                                    KmlPartDock.BuildDockStructure(grapple, part);
                                }
                            }
                            else if ((part is KmlPartDock) && (part as KmlPartDock).DockType == KmlPartDock.DockTypes.Grapple)
                            {
                                KmlPartDock grapple = (KmlPartDock)part;
                                if (grapple.DockUid != other.Uid)
                                {
                                    Syntax.Warning(grapple, "Grappling part node attachment not responded from other grappled part: " + other);
                                    grapple.NeedsRepair = true;
                                }
                                else if (grapple.DockState.ToLower() != "grappled")
                                {
                                    Syntax.Warning(grapple, "Grappling part grappled attached part. Docking state should be 'Grappled' but is '" + grapple.DockState + ", attached part: " + other);
                                    grapple.NeedsRepair = true;
                                }
                                else
                                {
                                    KmlPartDock.BuildDockStructure(grapple, other);
                                }
                            }
                            else if (!part.AttachedToNodeIndices.Contains(parts.IndexOf(other)))
                            {
                                Syntax.Warning(part, "Part node attachment not responded from other part: " + other.ToString());
                            }
                        }
                    }
                    else
                    {
                        Syntax.Warning(part, "Part supposed to be node attached to part index [" + p + "], which does not point to a valid part");
                    }
                }
                if(part.AttachedToSurfaceIndex >= 0 && part.AttachedToSurfaceIndex < parts.Count)
                {
                    part.AttachedToPartSurface = parts[part.AttachedToSurfaceIndex];
                    part.AttachedToPartSurface.AttachedPartsSurface.Add(part);
                }
                else if (part.AttachedToSurfaceIndex != -1)
                {
                    Syntax.Warning(part, "Part supposed to be surface attached to part index [" + part.AttachedToSurfaceIndex + "], which does not point to a valid part");
                }

                // Check symmetry attributes
                foreach (KmlAttrib attrib in part.Attribs)
                {
                    if (attrib.Name.ToLower() == "sym")
                    {
                        int v;
                        if (!int.TryParse(attrib.Value, out v) || v < 0 || v >= parts.Count || v == i)
                        {
                            Syntax.Warning(attrib, "Invalid symmetry entry '" + attrib.ToString() + "', you should delete this attribute");
                        }
                    }
                }

                // Check docking (with parent involved is already checked above, here needs to e checked a 'Docked (same vessel)'
                // Need to check one side only, other part will be touched here in another iteration of the loop
                if (part is KmlPartDock)
                {
                    KmlPartDock dock = (KmlPartDock)part;
                    KmlPart other = null;
                    foreach(KmlPart p in parts)
                    {
                        if (p.Uid == dock.DockUid)
                        {
                            other = p;
                            break;
                        }
                    }
                    if (other == null)
                    {
                        // This happens a lot, parts show UId 0 or some other UId they have been attached to before
                        if (dock.DockState.ToLower() == "docked (docker)" || dock.DockState.ToLower() == "docked (dockee)" ||
                            dock.DockState.ToLower() == "docked (same vessel)" || dock.DockState.ToLower() == "grappled")
                        {
                            Syntax.Warning(dock, "Dock part supposed to be attached to (UId " + dock.DockUid + "), which does not point to a valid part");
                            dock.NeedsRepair = true;
                        }
                        // If it still says 'Disengage' something went wron on undocking. We can repair if undocked part is now another vessel
                        // so other will be null because not found in this vessel
                        // Also could be 'Disarmed', 'Disabled' etc., so it's not checked to be 'Ready'.
                        else if (dock.DockState.ToLower() == "disengage")
                        {
                            Syntax.Warning(dock, "Dock part state should be 'Ready' but is '" + dock.DockState + "'");
                            dock.NeedsRepair = true;
                        }
                    }
                    // Docking with parent already checked
                    else if (other != dock.ParentPart && other.ParentPartIndex != i)
                    {
                        if (dock.DockState.ToLower() == "docked (docker)" || dock.DockState.ToLower() == "docked (dockee)" ||
                            dock.DockState.ToLower() == "docked (same vessel)" || dock.DockState.ToLower() == "grappled")
                        {
                            KmlPartDock.BuildDockStructure(dock, other);
                            if (other is KmlPartDock)
                            {
                                KmlPartDock otherDock = (KmlPartDock)other;
                                if (otherDock.DockUid != dock.Uid)
                                {
                                    Syntax.Warning(dock, "Dock part docked to other dock part, but docking not responded from other side. Other dock: " + otherDock);
                                    dock.NeedsRepair = true;
                                    otherDock.NeedsRepair = true;
                                }
                                else if (otherDock.DockState.ToLower() == "docked (dockee)")
                                {
                                    if (dock.DockState.ToLower() != "docked (same vessel)")
                                    {
                                        Syntax.Warning(dock, "Dock part is docked to dockee part. Docking state should be 'Docked (same vessel)' but is '" + dock.DockState + "', dockee part: " + otherDock);
                                        dock.NeedsRepair = true;
                                        otherDock.NeedsRepair = true;
                                    }
                                }
                                else if (otherDock.DockState.ToLower() == "docked (same vessel)")
                                {
                                    if (dock.DockState.ToLower() != "docked (dockee)")
                                    {
                                        Syntax.Warning(dock, "Dock part is docked to same vessel docking part. Docking state should be 'Docked (dockee)' but is '" + dock.DockState + "', same vessel docking part: " + otherDock);
                                        dock.NeedsRepair = true;
                                        otherDock.NeedsRepair = true;
                                    }
                                }
                                else
                                {
                                    Syntax.Warning(otherDock, "Dock part is docked to other dock part. Docking state should be 'Docked (same vessel)' or 'Docked (dockee)' but is '" + otherDock.DockState + "', other dock: " + dock);
                                    dock.NeedsRepair = true;
                                    otherDock.NeedsRepair = true;
                                }
                            }
                            else if (dock.DockType != KmlPartDock.DockTypes.Grapple)
                            {
                                Syntax.Warning(dock, "Dock part is no grappling device, so it should be only docked to other dock parts, but is docked to: " + other);
                                dock.NeedsRepair = true;
                            }
                        }
                    }
                }
            }
            foreach (KmlPart part in parts)
            {
                // Based on attachment structure we can now decide if this part can be deleted
                part.CanBeDeleted = part.CanPartBeDeleted();
            }
            return roots;
        }

        /// <summary>
        /// Get a part's name in a craft file.
        /// </summary>
        /// <returns>The part's name</returns>
        public string GetNameFromCraftName()
        {
            int p = CraftName.IndexOf('_');
            if (p < 0)
            {
                return CraftName;
            }
            else
            {
                return CraftName.Substring(0, p);
            }
        }

        /// <summary>
        /// Generates a nice informative string to be used in display for this part.
        /// It will contain the tag, the index in parent's part-list, a root marker and the name.
        /// </summary>
        /// <returns>A string to display this node</returns>
        public override string ToString()
        {
            string s = "";
            if (Parent is KmlVessel)
            {
                KmlVessel p = (KmlVessel)Parent;
                s += " [" + p.Parts.IndexOf(this).ToString();
                if (p.RootPart == this)
                {
                    s += ", root";
                }
                s += "]";
            }
            if (Name.Length > 0)
            {
                s += " (" + Name + ")";
            } 
            else if (CraftName.Length > 0)
            {
                // In *.craft files there is no Name but a PartName
                s += " (" + CraftName + ")";
            }
            return Tag + s;
        }

        private bool CanPartBeDeleted()
        {
            if (Parent == null) // Parent node, not parent part! Should never happen
                return false;
            if (!(Parent is KmlVessel)) // Just a node with the PART tag somewhere else
                return true;
            // Don't delete the root part
            if ((Parent as KmlVessel).RootPart == this)
                return false;
            // There may only be one part attached which is this parts parent part.
            // Oterhwise some parts are attached having this part as parent, then disallow deletion
            if (AttachedPartsBack.Count + AttachedPartsBottom.Count + AttachedPartsFront.Count + AttachedPartsLeft.Count + 
                AttachedPartsRight.Count + AttachedPartsSurface.Count + AttachedPartsTop.Count > 1)
            {
                return false;
            }
            if (AttachedPartsBack.Count == 1 && AttachedPartsBack[0] != ParentPart)
                return false;
            if (AttachedPartsBottom.Count == 1 && AttachedPartsBottom[0] != ParentPart)
                return false;
            if (AttachedPartsFront.Count == 1 && AttachedPartsFront[0] != ParentPart)
                return false;
            if (AttachedPartsLeft.Count == 1 && AttachedPartsLeft[0] != ParentPart)
                return false;
            if (AttachedPartsRight.Count == 1 && AttachedPartsRight[0] != ParentPart)
                return false;
            if (AttachedPartsSurface.Count == 1 && AttachedPartsSurface[0] != ParentPart)
                return false;
            if (AttachedPartsTop.Count == 1 && AttachedPartsTop[0] != ParentPart)
                return false;
            // A docking port in use must also not be deleted
            if (this is KmlPartDock && ((KmlPartDock)this).DockedPart != null)
                return false;
            // Don't delete parts that are docked but not a docking port (grappled)
            // At least we also don't know the amount of connections of this type
            // It's not stored in the part, we need to check all parts and look into all docks
            foreach (KmlPart part in (Parent as KmlVessel).Parts)
            {
                if (part is KmlPartDock)
                {
                    if ((part as KmlPartDock).DockedPart == this)
                        return false;
                }
            }
            return true;
        }

        private void PartName_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            CraftName = GetAttribWhereValueChanged(sender).Value;
            InvokeToStringChanged();
        }

        private void Resources_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            // Ok it's not the string..
            // but the ProgressBar for resource display also updates by this event
            // TODO KmlPart.Resources_Changed(): Define and invoke another event than ToStringChanged
            InvokeToStringChanged();
        }

        private void ParentPart_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            int p = 0;
            if (int.TryParse(GetAttribWhereValueChanged(sender).Value, out p))
            {
                ParentPartIndex = p;
                // TODO KmlPart.ParentPart_Changed(): Rebuild the whole part structure
                DlgMessage.Show("You need to save and reload to see the changed parent part in the structure");
            }
        }

        private void Uid_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            Uid = GetAttribWhereValueChanged(sender).Value;
        }

        private void Flag_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            Flag = GetAttribWhereValueChanged(sender).Value;
        }

        private void AttachmentSurface_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            // TODO KmlPart.AttachmentSurface_Changed(): Reassign AttachedToSurfaceIndex
            // TODO KmlPart.AttachmentSurface_Changed(): Rebuild the whole part structure
            DlgMessage.Show("You need to save and reload to see the changed surface attachment in the structure");
        }

        private void AttachmentNode_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            // TODO KmlPart.AttachmentNode_Changed(): Delete old index from appropiate list, add new one
            // TODO KmlPart.AttachmentNode_Changed(): Rebuild the whole part structure
            DlgMessage.Show("You need to save and reload to see the changed node attachment in the structure");
        }
    }
}
