using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KML
{
    /// <summary>
    /// KmlPartKmlPartDock represents a KmlPart that is a docking port or a grappling device.
    /// This methods should repair docking connections.
    /// </summary>
    public partial class KmlPartDock
    {
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
                        Syntax.Warning(this, "Could not search for connected parts, parent vessel is not valid");
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
                        // Syntax.Warning(this, "Didn't find another part in all parts of the vessel to fix this dock with");
                        RepairClearDocking(this);
                    }
                }
                else if (!(DockedPart is KmlPartDock))
                {
                    Syntax.Warning(this, "Don't know how to repair this docking connection, this one is no dock: " + DockedPart);
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
                        Syntax.Warning(this, "Could not search for connected parts, parent vessel is not valid");
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
                        // Syntax.Warning(this, "Didn't find another part in all parts of the vessel to fix this grappling device with");
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
                        Syntax.Warning(docker, "Couldn't find sub-node DOCKEDVESSEL, you should try to copy it from older save file.");
                    }
                    else
                    {
                        dockerOk = true;
                    }
                }
                catch (NullReferenceException)
                {
                    Syntax.Warning(docker, "Couldn't fix docker node, there are sub-nodes missing.\n" +
                        "You should copy a MODULE node from a functional 'Docked (docker)' part.\n" +
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
                    Syntax.Warning(dockee, "Couldn't fix dockee node, there are sub-nodes missing.\n" +
                        "You should copy a MODULE node from a functional 'Docked (dockee)' part.\n" +
                        "Dockee should be: " + dockee);
                }
                if (dockerOk && dockeeOk)
                {
                    Syntax.Info(docker, "Successfully repaired docker-dockee. Please save and reload to see the rebuilt part structure.");
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
                    Syntax.Warning(same, "Couldn't fix same vessel docking node, there are sub-nodes missing.\n" +
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
                    Syntax.Warning(dockee, "Couldn't fix dockee node, there are sub-nodes missing.\n" +
                        "You should copy a MODULE node from a functional 'Docked (dockee)' part.\n" +
                        "Dockee should be: " + dockee);
                }
                if (sameOk && dockeeOk)
                {
                    Syntax.Info(same, "Successfully repaired same vessel docking. Please save and reload to see the rebuilt part structure.");
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
                Syntax.Info(dock1, "Successfully reset docking to ready. Please save and reload to see the rebuilt part structure.");
            }
            catch (NullReferenceException)
            {
                Syntax.Warning(dock1, "Couldn't reset docking node, there are sub-nodes missing.\n" +
                    "You should copy a MODULE node from a functional state 'Ready' part.\n");
            }
            if (dock2 != null)
            {
                try
                {
                    KmlNode module = dock2.GetChildNode("MODULE", "ModuleDockingNode");
                    module.GetAttrib("state").Value = "Ready";
                    //module.GetAttrib("dockUId").Value = "";
                    KmlNode events = module.GetChildNode("EVENTS");
                    events.GetChildNode("Undock").GetAttrib("active").Value = "False";
                    events.GetChildNode("UndockSameVessel").GetAttrib("active").Value = "False";
                    Syntax.Info(dock2, "Successfully reset docking to ready. Please save and reload to see the rebuilt part structure.");
                }
                catch (NullReferenceException)
                {
                    Syntax.Warning(dock2, "Couldn't reset docking node, there are sub-nodes missing.\n" +
                        "You should copy a MODULE node from a functional state 'Ready' part.\n");
                }
            }
            // TODO KmlPartDock:RepairClearDocking(): Refresh structure without save / reload
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
                Syntax.Warning(grapple, "Could not search for connected parts, parent vessel is not valid");
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
                        Syntax.Warning(grapple, "Couldn't find sub-node DOCKEDVESSEL, you should try to copy it from older save file.");
                        dockedVesselOk = false;
                    }
                    if (module.GetChildNode("DOCKEDVESSEL_other") == null)
                    {
                        Syntax.Warning(grapple, "Couldn't find sub-node DOCKEDVESSEL_other, you should try to copy it from older save file.");
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
                        Syntax.Info(grapple, "Successfully repaired grappling. Please save and reload to see the rebuilt part structure.");
                        // TODO KmlPartDock:RepairGrappling(): Refresh structure without save / reload
                    }
                }
                catch (NullReferenceException)
                {
                    Syntax.Warning(grapple, "Couldn't fix grappling node, there are sub-nodes missing.\n" +
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
    }
}
