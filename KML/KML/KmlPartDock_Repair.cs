using System;

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
                    if (Parent is KmlVessel)
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
                    else
                    {
                        Syntax.Warning(this, "Could not search for connected parts, parent vessel is not valid");
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
                        // Semi-functional same vessel docking would be fixed above, this is no docking
                        RepairClearDocking(this);
                    }
                }
            }

            // One dock and another part
            else if (DockType == DockTypes.Grapple)
            {
                if (DockedPart == null)
                {
                    if (Parent is KmlVessel)
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
                    else
                    {
                        Syntax.Warning(this, "Could not search for connected parts, parent vessel is not valid");
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
                    KmlNode module = docker.GetOrCreateChildNode("MODULE", "ModuleDockingNode");
                    module.GetOrCreateAttrib("isEnabled", "True");
                    module.GetOrCreateAttrib("crossfeed", "True");
                    module.GetOrCreateAttrib("stagingEnabled").Value = "False";
                    module.GetOrCreateAttrib("state").Value = "Docked (docker)";
                    module.GetOrCreateAttrib("dockUId").Value = dockee.Uid;
                    KmlNode events = module.GetOrCreateChildNode("EVENTS");
                    events.GetOrCreateChildNode("Undock").GetOrCreateAttrib("active").Value = "True";
                    events.GetOrCreateChildNode("UndockSameVessel").GetOrCreateAttrib("active").Value = "False";
                    events.GetOrCreateChildNode("Decouple").GetOrCreateAttrib("active").Value = "False";
                    module.GetOrCreateChildNode("ACTIONS");
                    KmlNode dockedVessel = module.GetOrCreateChildNode("DOCKEDVESSEL");
                    string defaultName;
                    if (docker.Parent is KmlVessel)
                    {
                        defaultName = (docker.Parent as KmlVessel).Name + " Docker - repaired by KML";
                    }
                    else
                    {
                        defaultName = "Unknown Docker - repaired by KML";
                    }
                    if (dockedVessel.GetAttrib("vesselName") == null)
                    {
                        KmlAttrib attrib = dockedVessel.GetOrCreateAttrib("vesselName", defaultName);
                        attrib.AttribValueChanged += docker.DockedVesselName_Changed;
                        docker.DockedVesselName_Changed(attrib, new System.Windows.RoutedEventArgs());
                    }
                    if (dockedVessel.GetAttrib("vesselType") == null)
                    {
                        KmlAttrib attrib = dockedVessel.GetOrCreateAttrib("vesselType", "6");
                        attrib.AttribValueChanged += docker.DockedVesselType_Changed;
                        docker.DockedVesselType_Changed(attrib, new System.Windows.RoutedEventArgs());
                    }
                    dockedVessel.GetOrCreateAttrib("rootUId", docker.Uid);
                    dockerOk = true;
                }
                catch (NullReferenceException)
                {
                    Syntax.Warning(docker, "Couldn't fix docker node, there are sub-nodes missing.\n" +
                        "You should copy a MODULE node from a functional 'Docked (docker)' part.\n" +
                        "Docker should be: " + docker);
                }
                try
                {
                    KmlNode module = dockee.GetOrCreateChildNode("MODULE", "ModuleDockingNode");
                    module.GetOrCreateAttrib("isEnabled", "True");
                    module.GetOrCreateAttrib("crossfeed", "True");
                    module.GetOrCreateAttrib("stagingEnabled").Value = "False";
                    module.GetOrCreateAttrib("state").Value = "Docked (dockee)";
                    module.GetOrCreateAttrib("dockUId").Value = docker.Uid;
                    KmlNode events = module.GetOrCreateChildNode("EVENTS");
                    events.GetOrCreateChildNode("Undock").GetOrCreateAttrib("active").Value = "False";
                    events.GetOrCreateChildNode("UndockSameVessel").GetOrCreateAttrib("active").Value = "False";
                    events.GetOrCreateChildNode("Decouple").GetOrCreateAttrib("active").Value = "False";
                    module.GetOrCreateChildNode("ACTIONS");
                    KmlNode dockedVessel = module.GetOrCreateChildNode("DOCKEDVESSEL");
                    string defaultName;
                    string defaultUId;
                    if (dockee.Parent is KmlVessel)
                    {
                        defaultName = (dockee.Parent as KmlVessel).Name;
                        defaultUId = (dockee.Parent as KmlVessel).RootPart.Uid;
                    }
                    else
                    {
                        defaultName = "Unknown Dockee - repaired by KML";
                        defaultUId = dockee.Uid;
                    }
                    if (dockedVessel.GetAttrib("vesselName") == null)
                    {
                        KmlAttrib attrib = dockedVessel.GetOrCreateAttrib("vesselName", defaultName);
                        attrib.AttribValueChanged += dockee.DockedVesselName_Changed;
                        docker.DockedVesselName_Changed(attrib, new System.Windows.RoutedEventArgs());
                    }
                    if (dockedVessel.GetAttrib("vesselType") == null)
                    {
                        KmlAttrib attrib = dockedVessel.GetOrCreateAttrib("vesselType", "6");
                        attrib.AttribValueChanged += dockee.DockedVesselType_Changed;
                        docker.DockedVesselType_Changed(attrib, new System.Windows.RoutedEventArgs());
                    }
                    dockedVessel.GetOrCreateAttrib("rootUId", defaultUId);
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
                    Syntax.Info(docker, "Successfully repaired docker-dockee");
                    if (docker.Parent is KmlVessel)
                    {
                        BuildAttachmentStructure((docker.Parent as KmlVessel).Parts);
                    }
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
                    KmlNode module = same.GetOrCreateChildNode("MODULE", "ModuleDockingNode");
                    module.GetOrCreateAttrib("isEnabled", "True");
                    module.GetOrCreateAttrib("crossfeed", "True");
                    module.GetOrCreateAttrib("stagingEnabled").Value = "False";
                    module.GetOrCreateAttrib("state").Value = "Docked (same vessel)";
                    module.GetOrCreateAttrib("dockUId").Value = dockee.Uid;
                    KmlNode events = module.GetOrCreateChildNode("EVENTS");
                    events.GetOrCreateChildNode("Undock").GetOrCreateAttrib("active").Value = "False";
                    events.GetOrCreateChildNode("UndockSameVessel").GetOrCreateAttrib("active").Value = "True";
                    events.GetOrCreateChildNode("Decouple").GetOrCreateAttrib("active").Value = "False";
                    module.GetOrCreateChildNode("ACTIONS");
                    // TODO KmlPartDock.RepairSameVesselDockee(): Delete same DOCKEDVESSEL node?
                    // It's not present in correct savefile with same vessel docking
                    // KmlNode dockedVessel = module.GetChildNode("DOCKEDVESSEL");
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
                    KmlNode module = dockee.GetOrCreateChildNode("MODULE", "ModuleDockingNode");
                    module.GetOrCreateAttrib("isEnabled", "True");
                    module.GetOrCreateAttrib("crossfeed", "True");
                    module.GetOrCreateAttrib("stagingEnabled").Value = "False";
                    module.GetOrCreateAttrib("state").Value = "Docked (dockee)";
                    module.GetOrCreateAttrib("dockUId").Value = same.Uid;
                    KmlNode events = module.GetOrCreateChildNode("EVENTS");
                    events.GetOrCreateChildNode("Undock").GetOrCreateAttrib("active").Value = "False";
                    events.GetOrCreateChildNode("UndockSameVessel").GetOrCreateAttrib("active").Value = "False";
                    events.GetOrCreateChildNode("Decouple").GetOrCreateAttrib("active").Value = "False";
                    module.GetOrCreateChildNode("ACTIONS");
                    // TODO KmlPartDock.RepairSameVesselDockee(): Delete dockee DOCKEDVESSEL node?
                    // It's not present in correct savefile with same vessel docking
                    // KmlNode dockedVessel = module.GetChildNode("DOCKEDVESSEL");
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
                    Syntax.Info(same, "Successfully repaired same vessel docking");
                    if (same.Parent is KmlVessel)
                    {
                        BuildAttachmentStructure((same.Parent as KmlVessel).Parts);
                    }
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
                Syntax.Info(dock1, "Successfully reset docking to ready");
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
                    Syntax.Info(dock2, "Successfully reset docking to ready");
                }
                catch (NullReferenceException)
                {
                    Syntax.Warning(dock2, "Couldn't reset docking node, there are sub-nodes missing.\n" +
                        "You should copy a MODULE node from a functional state 'Ready' part\n");
                }
            }
            if (dock1.Parent is KmlVessel)
            {
                BuildAttachmentStructure((dock1.Parent as KmlVessel).Parts);
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
            if (grapple.Parent is KmlVessel)
            {
                bool dockedVesselOk = true;
                KmlVessel vessel = (KmlVessel)grapple.Parent;
                grappleIndex = vessel.Parts.IndexOf(grapple);
                partIndex = vessel.Parts.IndexOf(part);
                try
                {
                    KmlNode module = grapple.GetOrCreateChildNode("MODULE", "ModuleGrappleNode");
                    module.GetOrCreateAttrib("isEnabled", "True");
                    //module.GetOrCreateAttrib("stagingEnabled").Value = "False"; // True?!?
                    module.GetOrCreateAttrib("state").Value = "Grappled";
                    module.GetOrCreateAttrib("dockUId").Value = part.Uid;
                    KmlNode events = module.GetOrCreateChildNode("EVENTS");
                    events.GetOrCreateChildNode("Release").GetOrCreateAttrib("active").Value = "True";
                    events.GetOrCreateChildNode("ReleaseSameVessel").GetOrCreateAttrib("active").Value = "False";
                    events.GetOrCreateChildNode("Decouple").GetOrCreateAttrib("active").Value = "False";
                    module.GetOrCreateChildNode("ACTIONS");

                    KmlNode dockedVessel = module.GetOrCreateChildNode("DOCKEDVESSEL");
                    KmlNode dockedVesselOther = module.GetOrCreateChildNode("DOCKEDVESSEL_Other");

                    string vesselName;
                    string vesselRootUId;
                    string defaultName;
                    if (grapple.Parent is KmlVessel)
                    {
                        vesselName = (grapple.Parent as KmlVessel).Name;
                        vesselRootUId = (grapple.Parent as KmlVessel).RootPart.Uid;
                        defaultName = vesselName + " Grappled - repaired by KML";
                    }
                    else
                    {
                        vesselName = "Unknown Vessel - repaired by KML";
                        if (part.ParentPart == grapple)
                        {
                            vesselRootUId = grapple.Uid;
                        }
                        else
                        {
                            vesselRootUId = part.Uid;
                        }
                        defaultName = "Unknown Grappled - repaired by KML";
                    }

                    string thisName;
                    string otherName;
                    string thisUid;
                    string otherUid;
                    if (part.ParentPart == grapple)
                    {
                        thisName = vesselName;
                        thisUid = vesselRootUId;
                        otherName = defaultName;
                        otherUid = part.Uid;
                    }
                    else
                    {
                        thisName = defaultName;
                        thisUid = grapple.Uid;
                        otherName = vesselName;
                        otherUid = vesselRootUId;
                    }
                    if (dockedVessel.GetAttrib("vesselName") == null)
                    {
                        KmlAttrib attrib = dockedVessel.GetOrCreateAttrib("vesselName", thisName);
                        attrib.AttribValueChanged += grapple.DockedVesselName_Changed;
                        grapple.DockedVesselName_Changed(attrib, new System.Windows.RoutedEventArgs());
                    }
                    if (dockedVessel.GetAttrib("vesselType") == null)
                    {
                        KmlAttrib attrib = dockedVessel.GetOrCreateAttrib("vesselType", "6");
                        attrib.AttribValueChanged += grapple.DockedVesselType_Changed;
                        grapple.DockedVesselType_Changed(attrib, new System.Windows.RoutedEventArgs());
                    }
                    dockedVessel.GetOrCreateAttrib("rootUId", thisUid);
                    if (dockedVesselOther.GetAttrib("vesselName") == null)
                    {
                        KmlAttrib attrib = dockedVesselOther.GetOrCreateAttrib("vesselName", otherName);
                        attrib.AttribValueChanged += grapple.DockedVesselOtherName_Changed;
                        grapple.DockedVesselOtherName_Changed(attrib, new System.Windows.RoutedEventArgs());
                    }
                    if (dockedVesselOther.GetAttrib("vesselType") == null)
                    {
                        KmlAttrib attrib = dockedVesselOther.GetOrCreateAttrib("vesselType", "6");
                        attrib.AttribValueChanged += grapple.DockedVesselOtherType_Changed;
                        grapple.DockedVesselOtherType_Changed(attrib, new System.Windows.RoutedEventArgs());
                    }
                    dockedVesselOther.GetOrCreateAttrib("rootUId", otherUid);

                    module = grapple.GetOrCreateChildNode("MODULE", "ModuleAnimateGeneric");
                    module.GetOrCreateAttrib("animSwitch").Value = "False";
                    module.GetOrCreateAttrib("animTime").Value = "1";
                    events = module.GetOrCreateChildNode("EVENTS");
                    KmlNode toggle = events.GetOrCreateChildNode("Toggle");
                    toggle.GetOrCreateAttrib("active").Value = "False";
                    toggle.GetOrCreateAttrib("guiName").Value = "Disarm";
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
                        Syntax.Info(grapple, "Successfully repaired grappling");
                        BuildAttachmentStructure(vessel.Parts);                      
                    }
                }
                catch (NullReferenceException)
                {
                    Syntax.Warning(grapple, "Couldn't fix grappling node, there are sub-nodes missing.\n" +
                        "You should copy a MODULE node from a functional state 'Grappled' part\n" +
                        "grappled part should be: " + part);
                }
            }
            else
            {
                Syntax.Warning(grapple, "Could not search for connected parts, parent vessel is not valid");
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
            // InsertAfter will add to the end if lastItem is null
            part.InsertAfter(lastItem, newAttrib);
            //if (lastItem != null)
            //{
            //    part.Attribs.Insert(part.Attribs.IndexOf(lastItem) + 1, newAttrib);
            //    part.AllItems.Insert(part.AllItems.IndexOf(lastItem) + 1, newAttrib);
            //}
            //else
            //{
            //    part.Add(newAttrib);
            //}
        }
    }
}
