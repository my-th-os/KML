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
    class KmlPartDock : KmlPart
    {
        /// <summary>
        /// Possible types of docking parts (docking port or grappling device).
        /// </summary>
        public enum DockTypes { Dock, Grapple };

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
            NeedsRepair = false;

            foreach (KmlNode node in Children)
            {
                if (node.Tag.ToLower() == "module" && (node.Name.ToLower() == "moduledockingnode" || node.Name.ToLower() == "modulegrapplenode"))
                {
                    if (node.Name.ToLower() == "modulegrapplenode")
                    {
                        DockType = DockTypes.Grapple;
                    }
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
        /// Identify whether a KmlPart is a docking port part, 
        /// so it should be replaced by an intance of KmlPartDock.
        /// </summary>
        /// <param name="part">The KmlPart to check</param>
        /// <returns>True if KmlPart should be replaced by KmlPartDock</returns>
        public static bool PartIsDock(KmlPart part)
        {
            foreach (KmlNode node in part.Children)
            {
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
                    Syntax.Warning(dock, "Dock sub-node MODULE with name = 'ModuleDockingNode' is missing. Please copy one from functional dock part or older save file.");
                    dock.NeedsRepair = true;
                }
                else if (module.GetChildNode("DOCKEDVESSEL") == null)
                {
                    Syntax.Warning(dock, "Dock sub-sub-node DOCKEDVESSEL is missing. Please copy it from older save file.");
                    dock.NeedsRepair = true;
                }
            }
            else if (dock.DockType == DockTypes.Grapple && dock.DockState.ToLower() == "grappled")
            {
                KmlNode module = dock.GetChildNode("MODULE", "ModuleGrappleNode");
                if (module == null)
                {
                    Syntax.Warning(dock, "Grapple sub-node MODULE with name = 'ModuleGrappleNode' is missing. Please copy one from functional dock part or older save file.");
                    dock.NeedsRepair = true;
                }
                else
                {
                    if (module.GetChildNode("DOCKEDVESSEL") == null)
                    {
                        Syntax.Warning(dock, "Grapple sub-sub-node DOCKEDVESSEL is missing. Please copy it from older save file.");
                        dock.NeedsRepair = true;
                    }
                    if (module.GetChildNode("DOCKEDVESSEL_other") == null)
                    {
                        Syntax.Warning(dock, "Grapple sub-sub-node DOCKEDVESSEL_other is missing. Please copy it from older save file.");
                        dock.NeedsRepair = true;
                    }
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
            if (DockState.Length > 0)
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

        /// <summary>
        /// Repair the docking connection of this port.
        /// </summary>
        public void Repair()
        {
            // Two docks
            if (DockType == DockTypes.Dock)
            {
                if (DockedPart == null)
                {
                    if (Parent == null || !(Parent is KmlVessel))
                    {
                        DlgMessage.Show("Could not search for connected parts, parent vessel is not valid");
                    }
                    else
                    {
                        KmlVessel vessel = (KmlVessel)Parent;
                        int myIndex = vessel.Parts.IndexOf(this);
                        for (int i = 0; i < vessel.Parts.Count; i++)
                        {
                            KmlPart part = vessel.Parts[i];
                            if (part is KmlPartDock)
                            {
                                KmlPartDock otherDock = (KmlPartDock)part;
                                if (otherDock.DockedPart == this)
                                {
                                    // This will choose the right docker / dockee or switch to same vessel if none is parent
                                    RepairDockerChoose(this, otherDock);
                                    return;
                                }
                                else if (otherDock.ParentPart == this || ParentPart == otherDock)
                                {
                                    if (AttachedToSurfaceIndex == i || AttachedToNodeIndices.Contains(i) ||
                                        otherDock.AttachedToSurfaceIndex == myIndex || otherDock.AttachedToNodeIndices.Contains(myIndex))
                                    {
                                        // It's attached and not docked
                                        continue;
                                    }
                                    else
                                    {
                                        RepairDockerChoose(this, otherDock);
                                        return;
                                    }
                                }
                            }
                        }
                        // To avoid getting to here there are returns spread above
                        // DlgMessage.Show("Didn't find another part in all parts of the vessel to fix this dock with");
                        RepairClearDocking(this);
                    }
                }
                else if (!(DockedPart is KmlPartDock))
                {
                    DlgMessage.Show("Don't know how to repair this docking connection, this one is no dock: " + DockedPart);
                }
                else
                {
                    KmlPartDock otherDock = (KmlPartDock)DockedPart;
                    if (otherDock.DockedPart == this || otherDock.ParentPart == this || ParentPart == otherDock)
                    {
                        // They are already linked but there's something to fix unless context menu to this wouldn't be enabled
                        // This will choose the right docker / dockee or switch to same vessel if none is parent
                        RepairDockerChoose(this, otherDock);
                    }
                    else
                    {
                        // TODO KmlPartDock.Repair(): Is it same vessel docking or no docking? 
                        // Semi-functional same vessel docking would be fixed above
                    }
                }
            }

            // One dock and another part
            else if (DockType == DockTypes.Grapple)
            {
                if (DockedPart == null)
                {
                    if (Parent == null || !(Parent is KmlVessel))
                    {
                        DlgMessage.Show("Could not search for connected parts, parent vessel is not valid");
                    }
                    else
                    {
                        KmlVessel vessel = (KmlVessel)Parent;
                        int myIndex = vessel.Parts.IndexOf(this);
                        for (int i = 0; i < vessel.Parts.Count; i++)
                        {
                            KmlPart part = vessel.Parts[i];
                            if (part.ParentPart == this || this.ParentPart == part)
                            {
                                if (AttachedToSurfaceIndex == i || AttachedToNodeIndices.Contains(i) ||
                                    part.AttachedToSurfaceIndex == myIndex || part.AttachedToNodeIndices.Contains(myIndex))
                                {
                                    // It's attached and not grappled
                                    continue;
                                }
                                else
                                {
                                    RepairGrappling(this, part);
                                    return;
                                }
                            }
                        }
                        // To avoid getting to here there are returns spread above
                        // DlgMessage.Show("Didn't find another part in all parts of the vessel to fix this grappling device with");
                        RepairClearDocking(this);
                    }
                }
                else
                {
                    if (DockedPart.ParentPart == this || ParentPart == DockedPart)
                    {
                        RepairGrappling(this, DockedPart);
                    }
                    else
                    {
                        RepairClearDocking(this);
                    }
                }
            }
        }

        private static void RepairDependingWhosParent(KmlPartDock dock1, KmlPartDock dock2)
        {
            if (dock1.ParentPart == dock2)
            {
                RepairDockerDockee(dock1, dock2);
            }
            else if (dock2.ParentPart == dock1)
            {
                RepairDockerDockee(dock2, dock1);
            }
            else
            {
                RepairSameVesselChoose(dock1, dock2);
            }
        }

        private static void RepairDockerDockee(KmlPartDock docker, KmlPartDock dockee)
        {
            if (docker.ParentPart == dockee || dockee.ParentPart == docker)
            {
                bool dockerOk = false;
                bool dockeeOk = false;
                try
                {
                    KmlNode module = docker.GetChildNode("MODULE", "ModuleDockingNode");
                    module.GetAttrib("state").Value = "Docked (docker)";
                    module.GetAttrib("dockUId").Value = dockee.Uid;
                    KmlNode events = module.GetChildNode("EVENTS");
                    events.GetChildNode("Undock").GetAttrib("active").Value = "True";
                    events.GetChildNode("UndockSameVessel").GetAttrib("active").Value = "False";
                    if (module.GetChildNode("DOCKEDVESSEL") == null)
                    {
                        DlgMessage.Show("Couldn't find sub-node DOCKEDVESSEL, you should try to copy it from older save files.");
                    }
                    else
                    {
                        dockerOk = true;
                    }
                }
                catch (NullReferenceException)
                {
                    DlgMessage.Show("Couldn't fix docker node, there are sub-nodes missing.\n"+
                        "You should copy a MODULE node from a functional 'Docked (docker)' part.\n"+
                        "Docker should be: " + docker);
                }
                try
                {
                    KmlNode module = dockee.GetChildNode("MODULE", "ModuleDockingNode");
                    module.GetAttrib("state").Value = "Docked (dockee)";
                    module.GetAttrib("dockUId").Value = docker.Uid;
                    KmlNode events = module.GetChildNode("EVENTS");
                    events.GetChildNode("Undock").GetAttrib("active").Value = "False";
                    events.GetChildNode("UndockSameVessel").GetAttrib("active").Value = "False"; 
                    dockeeOk = true;
                }
                catch (NullReferenceException)
                {
                    DlgMessage.Show("Couldn't fix dockee node, there are sub-nodes missing.\n" +
                        "You should copy a MODULE node from a functional 'Docked (dockee)' part.\n" +
                        "Dockee should be: " + dockee);
                }
                if (dockerOk && dockeeOk)
                {
                    DlgMessage.Show("Successfully repaired docker-dockee. Please save and reload to see the rebuilt part structure.");
                    // TODO KmlPartDock:RepairDockerDockee(): Refresh structure without save / reload
                }
            }
            else
            {
                RepairSameVesselChoose(docker, dockee);
            }
        }

        private static void RepairDockerChoose(KmlPartDock dock1, KmlPartDock dock2)
        {
            if (dock1.DockState.ToLower() == "docked (docker)" && dock2.DockState.ToLower() == "docked (docker)" ||
                dock1.DockState.ToLower() == "docked (dockee)" && dock2.DockState.ToLower() == "docked (dockee)")
            {
                RepairDependingWhosParent(dock1, dock2);
            }
            else if (dock1.DockState.ToLower() == "docked (docker)" || dock2.DockState.ToLower() == "docked (dockee)")
            {
                RepairDockerDockee(dock1, dock2);
            }
            else
            {
                RepairDockerDockee(dock2, dock1);
            }
        }

        private static void RepairSameVesselDockee(KmlPartDock same, KmlPartDock dockee)
        {
            if (same.ParentPart == dockee || dockee.ParentPart == same)
            {
                RepairDependingWhosParent(same, dockee);
            }
            else
            {
                bool sameOk = false;
                bool dockeeOk = false;
                try
                {
                    KmlNode module = same.GetChildNode("MODULE", "ModuleDockingNode");
                    module.GetAttrib("state").Value = "Docked (same vessel)";
                    module.GetAttrib("dockUId").Value = dockee.Uid;
                    KmlNode events = module.GetChildNode("EVENTS");
                    events.GetChildNode("Undock").GetAttrib("active").Value = "False";
                    events.GetChildNode("UndockSameVessel").GetAttrib("active").Value = "True";
                    sameOk = true;
                }
                catch (NullReferenceException)
                {
                    DlgMessage.Show("Couldn't fix same vessel docking node, there are sub-nodes missing.\n" +
                        "You should copy a MODULE node from a functional 'Docked (same vessel)' part.\n" +
                        "Same vessel dock should be: " + same);
                }
                try
                {
                    KmlNode module = dockee.GetChildNode("MODULE", "ModuleDockingNode");
                    module.GetAttrib("state").Value = "Docked (dockee)";
                    module.GetAttrib("dockUId").Value = same.Uid;
                    KmlNode events = module.GetChildNode("EVENTS");
                    events.GetChildNode("Undock").GetAttrib("active").Value = "False";
                    events.GetChildNode("UndockSameVessel").GetAttrib("active").Value = "False";
                    dockeeOk = true;
                }
                catch (NullReferenceException)
                {
                    DlgMessage.Show("Couldn't fix dockee node, there are sub-nodes missing.\n" +
                        "You should copy a MODULE node from a functional 'Docked (dockee)' part.\n" +
                        "Dockee should be: " + dockee);
                }
                if (sameOk && dockeeOk)
                {
                    DlgMessage.Show("Successfully repaired same vessel docking. Please save and reload to see the rebuilt part structure.");
                    // TODO KmlPartDock:RepairDockerDockee(): Refresh structure without save / reload
                }
            }
        }

        private static void RepairSameVesselChoose(KmlPartDock dock1, KmlPartDock dock2)
        {
            if (dock1.DockState.ToLower() == "docked (same vessel)" || dock2.DockState.ToLower() == "docked (dockee)")
            {
                RepairSameVesselDockee(dock1, dock2);
            }
            else
            {
                RepairSameVesselDockee(dock2, dock1);
            }
        }

        private static void RepairClearDocking(KmlPartDock dock1, KmlPartDock dock2)
        {
            try
            {
                KmlNode module = dock1.GetChildNode("MODULE", "ModuleDockingNode");
                module.GetAttrib("state").Value = "Ready";
                //module.GetAttrib("dockUId").Value = "";
                KmlNode events = module.GetChildNode("EVENTS");
                events.GetChildNode("Undock").GetAttrib("active").Value = "False";
                events.GetChildNode("UndockSameVessel").GetAttrib("active").Value = "False";
                if (dock2 != null)
                {
                    module = dock2.GetChildNode("MODULE", "ModuleDockingNode");
                    module.GetAttrib("state").Value = "Ready";
                    //module.GetAttrib("dockUId").Value = "";
                    events = module.GetChildNode("EVENTS");
                    events.GetChildNode("Undock").GetAttrib("active").Value = "False";
                    events.GetChildNode("UndockSameVessel").GetAttrib("active").Value = "False";
                }
                DlgMessage.Show("Successfully reset docking to ready. Please save and reload to see the rebuilt part structure.");
                // TODO KmlPartDock:RepairClearDocking(): Refresh structure without save / reload
            }
            catch (NullReferenceException)
            {
                DlgMessage.Show("Couldn't reset docking node, there are sub-nodes missing.\n" +
                    "You should copy a MODULE node from a functional state 'Ready' part.\n");
            }
        }

        private static void RepairClearDocking(KmlPartDock dock)
        {
            RepairClearDocking(dock, null);
        }

        private static void RepairGrappling(KmlPartDock grapple, KmlPart part)
        {
            int grappleIndex = -1;
            int partIndex = -1;
            if (grapple.Parent == null || !(grapple.Parent is KmlVessel))
            {
                DlgMessage.Show("Could not search for connected parts, parent vessel is not valid");
            }
            else
            {
                bool dockedVesselOk = true;
                KmlVessel vessel = (KmlVessel)grapple.Parent;
                grappleIndex = vessel.Parts.IndexOf(grapple);
                partIndex = vessel.Parts.IndexOf(part);
                try
                {
                    KmlNode module = grapple.GetChildNode("MODULE", "ModuleGrappleNode");
                    module.GetAttrib("state").Value = "Grappled";
                    module.GetAttrib("dockUId").Value = part.Uid;
                    KmlNode events = module.GetChildNode("EVENTS");
                    events.GetChildNode("Release").GetAttrib("active").Value = "True";
                    events.GetChildNode("ReleaseSameVessel").GetAttrib("active").Value = "False";
                    if (module.GetChildNode("DOCKEDVESSEL") == null)
                    {
                        DlgMessage.Show("Couldn't find sub-node DOCKEDVESSEL, you should try to copy it from older save file.");
                        dockedVesselOk = false;
                    }
                    if (module.GetChildNode("DOCKEDVESSEL_other") == null)
                    {
                        DlgMessage.Show("Couldn't find sub-node DOCKEDVESSEL_other, you should try to copy it from older save file.");
                        dockedVesselOk = false;
                    }
                    module = grapple.GetChildNode("MODULE", "ModuleAnimateGeneric");
                    module.GetAttrib("animSwitch").Value = "False";
                    module.GetAttrib("animTime").Value = "1";
                    events = module.GetChildNode("EVENTS");
                    KmlNode toggle = events.GetChildNode("Toggle");
                    toggle.GetAttrib("active").Value = "False";
                    toggle.GetAttrib("guiName").Value = "Disarm";
                    if (part.ParentPart == grapple)
                    {
                        RepairGrappleAttachment(part, grappleIndex);
                    }
                    else if (grapple.ParentPart == part)
                    {
                        RepairGrappleAttachment(grapple, partIndex);
                    }
                    else
                    {
                        // TODO KmlPartDock:RepairGrappling(): Is there a 'Grappled (same vessel)'?
                    }
                    if (dockedVesselOk)
                    {
                        // Maybe RepairGrappleAttachment()  will cause a message to save and reload
                        DlgMessage.Show("Successfully repaired grappling. Please save and reload to see the rebuilt part structure.");
                        // TODO KmlPartDock:RepairGrappling(): Refresh structure without save / reload
                    }
                }
                catch (NullReferenceException)
                {
                    DlgMessage.Show("Couldn't fix grappling node, there are sub-nodes missing.\n" +
                        "You should copy a MODULE node from a functional state 'Grappled' part.\n" +
                        "grappled part should be: " + part);
                }
            }
        }

        private static void RepairGrappleAttachment(KmlPart part, int attachmentIndex)
        {
            KmlAttrib lastItem = null;
            // Repair part 'attN = grapple, <attachmentIndex>'
            for (int i = 0; i < part.Attribs.Count; i++)
            {
                KmlAttrib attrib = part.Attribs[i];
                if (attrib.Name.ToLower() == "attn")
                {
                    lastItem = attrib;
                    if (attrib.Value.ToLower().StartsWith("grapple,") || attrib.Value.EndsWith(", " + attachmentIndex))
                    {
                        attrib.Value = "grapple, " + attachmentIndex;
                        return;
                    }
                }
                else if (attrib.Name.ToLower() == "srfn")
                {
                    lastItem = attrib;
                }
            }
            // If we got here we didn't find it
            KmlAttrib newAttrib = new KmlAttrib("attN = grapple, " + attachmentIndex);
            if (lastItem != null)
            {
                // TODO KmlPartDock.RepairGrappleAttachment(): There is no Insert method that manages both lists yet
                part.Attribs.Insert(part.Attribs.IndexOf(lastItem) + 1, newAttrib);
                part.AllItems.Insert(part.AllItems.IndexOf(lastItem) + 1, newAttrib);
            }
            else
            {
                // Add method manages the Attribs and AllItems lists
                part.Add(newAttrib);
            }
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
