using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KML
{
    /// <summary>
    /// KmlPartKmlPartDock represents a KmlPart that is a docking port or a grappling device.
    /// </summary>
    public partial class KmlPartDock : KmlPart
    {
        /// <summary>
        /// Possible types of docking parts (docking port or grappling device).
        /// </summary>
        public enum DockTypes 
        { 
            /// <summary>
            /// Docking port
            /// </summary>
            Dock, 

            /// <summary>
            /// Grappling device
            /// </summary>
            Grapple,

            /// <summary>
            /// CPort from KAS addon
            /// </summary>
            KasCPort
        };

        /// <summary>
        /// Get the actual docking type of this part (docking port or grappling device).
        /// </summary>
        public DockTypes DockType { get; private set; }

        /// <summary>
        /// Get the docking port name.
        /// </summary>
        public string DockName { get; private set; }

        /// <summary>
        /// Get the docking state (Disabled, Ready, Docked (dockee), Docked (docker), Docked (same vessel), Grappled).
        /// </summary>
        public string DockState { get; private set; }

        /// <summary>
        /// Get the vessel name information of this docking part.
        /// This is another vessel if the state is Docked (docker) or Grappled.
        /// It will be the current vessel otherwise.
        /// </summary>
        public string DockedVesselName { get; private set; }

        /// <summary>
        /// Get the vessel type (Ship, Lander, etc.) of this docking part.
        /// This is another vessel if the state is Docked (docker) or Grappled.
        /// It will be the current vessel otherwise.
        /// </summary>
        public string DockedVesselType { get; private set; }

        /// <summary>
        /// Get the vessel name information of this grappling part.
        /// This is another vessel if it is a grappling device attached towards
        /// the root part and grappling away from the root part.
        /// </summary>
        public string DockedVesselOtherName { get; private set; }

        /// <summary>
        /// Get the vessel type (Ship, Lander, etc.) of this grappling part.
        /// This is another vessel if it is a grappling device attached towards
        /// the root part and grappling away from the root part.
        /// </summary>
        public string DockedVesselOtherType { get; private set; }

        /// <summary>
        /// Get the UId of the docked part.
        /// </summary>
        public string DockUid { get; private set; }

        /// <summary>
        /// Get the docked KmlPart.
        /// The part will be assigned by static KmlPart.BuildAttachmentStructure() method
        /// calling KmlPartDock.BuildDockStructure() method after a complete vessel is read,
        /// based on the DockUid and ParentPartIndex that is filled on reading this part alone.
        /// </summary>
        public KmlPart DockedPart { get; private set; }

        /// <summary>
        /// Get status about the need to repair the connection of this dock.
        /// Will be set by KmlPart.BuildAttachmentStructure() and read to 
        /// have a context menu to repair.
        /// </summary>
        public bool NeedsRepair { get; set; }

        /// <summary>
        /// Creates a KmlPartDock as a copy of a given KmlPart.
        /// </summary>
        /// <param name="part">The KmlPart to copy</param>
        public KmlPartDock(KmlPart part)
            : base (part)
        {
            DockType = DockTypes.Dock;
            DockName = "";
            DockState = "";
            DockedVesselName = "";
            DockedVesselType = "";
            DockedVesselOtherName = "";
            DockedVesselOtherType = "";
            DockUid = "";
            DockedPart = null;
            NeedsRepair = false;

            foreach (KmlNode node in Children)
            {
                if (node.Tag.ToLower() == "module" && 
                    (node.Name.ToLower() == "moduledockingnode" ||
                    node.Name.ToLower() == "modulegrapplenode" ||
                    node.Name.ToLower() == "kasmodulestrut"))
                {
                    if (node.Name.ToLower() == "modulegrapplenode")
                        DockType = DockTypes.Grapple;
                    else if (node.Name.ToLower() == "kasmodulestrut")
                        DockType = DockTypes.KasCPort;

                    foreach (KmlAttrib attrib in node.Attribs)
                    {
                        if (attrib.Name.ToLower() == "state")
                        {
                            DockState = attrib.Value;
                            attrib.AttribValueChanged += DockState_Changed;
                            attrib.CanBeDeleted = false;
                        }
                        else if (attrib.Name.ToLower() == "dockuid")
                        {
                            DockUid = attrib.Value;
                            attrib.AttribValueChanged += DockUid_Changed;
                            attrib.CanBeDeleted = false;
                        }
                        else if (DockType == DockTypes.KasCPort && attrib.Name.ToLower() == "dockedpartid")
                        {
                            DockUid = attrib.Value;
                            attrib.AttribValueChanged += DockUid_Changed;
                            attrib.CanBeDeleted = false;
                        }
                    }
                    foreach (KmlNode subnode in node.Children)
                    {
                        if (subnode.Tag.ToLower() == "dockedvessel")
                        {
                            foreach (KmlAttrib attrib in subnode.Attribs)
                            {
                                if (attrib.Name.ToLower() == "vesselname")
                                {
                                    DockedVesselName = attrib.Value;
                                    attrib.AttribValueChanged += DockedVesselName_Changed;
                                    attrib.CanBeDeleted = false;
                                }
                                else if (attrib.Name.ToLower() == "vesseltype")
                                {
                                    DockedVesselType = TranslateDockedVesselType(attrib.Value);
                                    attrib.AttribValueChanged += DockedVesselType_Changed;
                                    attrib.CanBeDeleted = false;
                                }
                            }
                        }
                        // Grapple-links with grapplingdevice on master side 
                        // (the one that's name survived) need to read "dockedvessel_other" to get slave side name
                        else if (subnode.Tag.ToLower() == "dockedvessel_other")
                        {
                            foreach (KmlAttrib attrib in subnode.Attribs)
                            {
                                if (attrib.Name.ToLower() == "vesselname")
                                {
                                    DockedVesselOtherName = attrib.Value;
                                    attrib.AttribValueChanged += DockedVesselOtherName_Changed;
                                    attrib.CanBeDeleted = false;
                                }
                                else if (attrib.Name.ToLower() == "vesseltype")
                                {
                                    DockedVesselOtherType = TranslateDockedVesselType(attrib.Value);
                                    attrib.AttribValueChanged += DockedVesselOtherType_Changed;
                                    attrib.CanBeDeleted = false;
                                }
                            }
                        }
                    }
                }
                else if (node.Tag.ToLower() == "module" && node.Name.ToLower() == "moduledockingnodenamed")
                {
                    foreach (KmlAttrib attrib in node.Attribs)
                    {
                        if (attrib.Name.ToLower() == "portname")
                        {
                            DockName = attrib.Value;
                            attrib.AttribValueChanged += DockName_Changed;
                            attrib.CanBeDeleted = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clear all child nodes and attributes from this node.
        /// </summary>
        public override void Clear()
        {
            DockType = DockTypes.Dock;
            DockName = "";
            DockState = "";
            DockedVesselName = "";
            DockedVesselType = "";
            DockedVesselOtherName = "";
            DockedVesselOtherType = "";
            DockUid = "";
            DockedPart = null;
            NeedsRepair = false;
            base.Clear();
        }

        /// <summary>
        /// Identify whether a KmlPart is a docking port part, 
        /// so it should be replaced by an intance of KmlPartDock.
        /// </summary>
        /// <param name="part">The KmlPart to check</param>
        /// <returns>True if KmlPart should be replaced by KmlPartDock</returns>
        public static bool PartIsDock(KmlPart part)
        {
            foreach (KmlNode node in part.Children)
            {
                if (node.Tag.ToLower() != "module")
                {
                    continue;
                }

                if (node.Name.ToLower() == "moduledockingnode")
                {
                    return true;
                }
                else if (node.Name.ToLower() == "moduledockingnodenamed")
                {
                    return true;
                }
                else if (node.Name.ToLower() == "modulegrapplenode")
                {
                    return true;
                }
                else if (node.Name.ToLower() == "kasmodulestrut")
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// This method is called by KmlPart.BuildAttachmentStructure() after a complete vessel is read and all parts exist, 
        /// so we can build KmlPart references from the index or UId information got from reading this part anlone.
        /// </summary>
        /// <param name="dock">The part identified as KmlPartDock</param>
        /// <param name="other">The other docked KmlPart (could be KmlPartDock also)</param>
        public static void BuildDockStructure(KmlPartDock dock, KmlPart other)
        {
            if (dock.DockUid == other.Uid)
            {
                dock.DockedPart = other;
            }
            else
            {
                Syntax.Warning(dock, "Dock part is attached to (UId " + dock.DockUid + ") but supposed to be attached to: " + other.ToString() + " (UId " + other.Uid + ")");
                dock.NeedsRepair = true;
            }

            if (dock.DockType == DockTypes.Dock && dock.DockState.ToLower() == "docked (docker)")
            {
                KmlNode module = dock.GetChildNode("MODULE", "ModuleDockingNode");
                if (module == null)
                {
                    Syntax.Warning(dock, "Dock sub-node MODULE with name = 'ModuleDockingNode' is missing. Please copy one from functional dock part or older save file");
                    dock.NeedsRepair = true;
                }
                else if (module.GetChildNode("DOCKEDVESSEL") == null)
                {
                    Syntax.Warning(dock, "Dock sub-sub-node DOCKEDVESSEL is missing");
                    dock.NeedsRepair = true;
                }
            }
            else if (dock.DockType == DockTypes.Grapple && dock.DockState.ToLower() == "grappled")
            {
                KmlNode module = dock.GetChildNode("MODULE", "ModuleGrappleNode");
                if (module == null)
                {
                    Syntax.Warning(dock, "Grapple sub-node MODULE with name = 'ModuleGrappleNode' is missing. Please copy one from functional dock part or older save file");
                    dock.NeedsRepair = true;
                }
                else
                {
                    if (module.GetChildNode("DOCKEDVESSEL") == null)
                    {
                        Syntax.Warning(dock, "Grapple sub-sub-node DOCKEDVESSEL is missing");
                        dock.NeedsRepair = true;
                    }
                    if (module.GetChildNode("DOCKEDVESSEL_other") == null)
                    {
                        Syntax.Warning(dock, "Grapple sub-sub-node DOCKEDVESSEL_other is missing");
                        dock.NeedsRepair = true;
                    }
                }
            }
            else if (dock.DockType == DockTypes.KasCPort)
            {
                KmlNode module = dock.GetChildNode("MODULE", "KASModuleStrut");
                if (module == null)
                {
                    Syntax.Warning(dock, "KAS CPort sub-node MODULE with name = 'KASModuleStrut' is missing. Please copy one from functional CPort part or older save file");
                    dock.NeedsRepair = true;
                }
            }
        }

        /// <summary>
        /// Generates a nice informative string to be used in display for this part.
        /// It will contain the tag, the index in parent's part-list, a root marker, the part name,
        /// the docking port name and the docking port state.
        /// </summary>
        /// <returns>A string to display this node</returns>
        public override string ToString()
        {
            string s = base.ToString();
            if (DockName.Length > 0)
            {
                if (s[s.Length - 1] == ')')
                {
                    s = s.Substring(0, s.Length - 1);
                }
                else
                {
                    s += " (";
                }
                s += " '" + DockName + "')";
            }
            if (DockType == DockTypes.KasCPort)
            {
                if (DockedVesselName.Length > 0)
                {
                    if (DockedVesselName == Parent.Name)
                        s += ": Linked (dockee)";
                    else
                        s += ": Linked (docker): " + DockedVesselName; // +", " + DockedVesselType;
                }
                else
                {
                    s += ": Ready";
                }
            }
            else if (DockState.Length > 0)
            {
                s += ": " + DockState;
                if (DockState.ToLower() == "docked (docker)")
                {
                    if (DockedVesselName.Length > 0)
                    {
                        s += ": " + DockedVesselName; // +", " + DockedVesselType;
                    }
                }
                else if (DockState.ToLower() == "grappled" && DockedPart == ParentPart)
                {
                    if (DockedVesselName.Length > 0)
                    {
                        s += ": " + DockedVesselName + " (this side)"; // +", " + DockedVesselType;
                    }
                }
                else if (DockState.ToLower() == "grappled")
                {
                    if (DockedVesselOtherName.Length > 0)
                    {
                        s += ": " + DockedVesselOtherName + " (other side)"; // +", " + DockedVesselType;
                    }
                }
            }
            return s;
        }

        private string TranslateDockedVesselType(string nr)
        {
            switch (nr)
            {
                case "0": return "Debris";
                case "1": return "SpaceObject";
                case "2": return "Unknown";
                case "3": return "Probe";
                case "4": return "Rover";
                case "5": return "Lander";
                case "6": return "Ship";
                case "7": return "Station";
                case "8": return "Base";
                case "9": return "EVA";
                // KSP 1.2 does not work with numbers but with same strings now
                // so no need to translate type Plane or Relay here
                default: return nr;
            }
        }

        private void DockName_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            DockName = GetAttribWhereValueChanged(sender).Value;
            InvokeToStringChanged();
        }

        private void DockState_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            DockState = GetAttribWhereValueChanged(sender).Value;
            InvokeToStringChanged();
        }

        private void DockedVesselName_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            DockedVesselName = GetAttribWhereValueChanged(sender).Value;
            InvokeToStringChanged();
        }

        private void DockedVesselType_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            DockedVesselType = TranslateDockedVesselType(GetAttribWhereValueChanged(sender).Value);
            InvokeToStringChanged();
        }

        private void DockedVesselOtherName_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            DockedVesselOtherName = GetAttribWhereValueChanged(sender).Value;
            InvokeToStringChanged();
        }

        private void DockedVesselOtherType_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            DockedVesselOtherType = TranslateDockedVesselType(GetAttribWhereValueChanged(sender).Value);
            InvokeToStringChanged();
        }

        private void DockUid_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            DockUid = GetAttribWhereValueChanged(sender).Value;
        }
    }
}
