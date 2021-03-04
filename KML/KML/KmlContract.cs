using System;
using System.Collections.Generic;
using System.Linq;

namespace KML
{
    /// <summary>
    /// KmlContract represents a KmlNode with the "CONTRACT" tag.
    /// </summary>
    public class KmlContract : KmlNode
    {
        /// <summary>
        /// Possible origins where this node is found.
        /// Regular PART nodes are children of a "VESSEL" node.
        /// </summary>
        public enum ContractOrigin
        {
            /// <summary>
            /// Contract found under GAME/SCENARIO (ContractSystem)/CONTRACTS node
            /// </summary>
            Contracts,

            /// <summary>
            /// Contract found anywhere else
            /// </summary>
            Other
        };

        /// <summary>
        /// Get the origin of this node.
        /// <see cref="KML.KmlContract.ContractOrigin"/>
        /// </summary>
        public ContractOrigin Origin { get; private set; }

        /// <summary>
        /// Get the "Type" attribute as a property.
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// Get the "State" attribute as a property.
        /// </summary>
        public string State { get; private set; }

        /// <summary>
        /// Get the "Agent" attribute as a property.
        /// </summary>
        public string Agent { get; private set; }

        /// <summary>
        /// Get status about the need to repair the contract parameters.
        /// Will be set by CheckContractParameters() and read to 
        /// have a context menu to repair.
        /// </summary>
        public bool NeedsRepair { get; set; }

        /// <summary>
        /// Get the KmlVessel this contract relates to
        /// </summary>
        public KmlVessel RelatedVessel { get; private set; }

        /// <summary>
        /// Get the KmlPart this contract relates to
        /// </summary>
        public KmlPart RelatedPart { get; private set; }

        /// <summary>
        /// Creates a KmlContract as a copy of a given KmlNode.
        /// </summary>
        /// <param name="node">The KmlNode to copy</param>
        public KmlContract(KmlNode node)
            : base(node.Line)
        {
            // First parent is null, will be set later when added to parent,
            // then  IdentifyParent() will set Origin.
            Origin = ContractOrigin.Other;
            Type = "";
            State = "";
            Agent = "";
            NeedsRepair = false;
            AddRange(node.AllItems);
        }

        /// <summary>
        /// Adds a child KmlItem to this nodes lists of children, depending of its
        /// derived class KmlNode, KmlAttrib or further derived from these.
        /// When an KmlAttrib "Type" is found, its value 
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
                else if (attrib.Name.ToLower() == "state" && State.Length == 0)
                {
                    State = attrib.Value;

                    // Get notified when State changes
                    attrib.AttribValueChanged += State_Changed;
                    attrib.CanBeDeleted = false;
                }
                else if (attrib.Name.ToLower() == "agent" && Agent.Length == 0)
                {
                    Agent = attrib.Value;

                    // Get notified when Agent changes
                    attrib.AttribValueChanged += Agent_Changed;
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
            Origin = ContractOrigin.Other;
            Type = "";
            State = "";
            Agent = "";
            NeedsRepair = false;
            RelatedVessel = null;
            RelatedPart = null;
            base.Clear();
        }

        /// <summary>
        /// Generates a nice informative string to be used in display for this contract.
        /// It will contain the "Type".
        /// </summary>
        /// <returns>A string to display this node</returns>
        public override string ToString()
        {
            // all state=completed have tag=contract_finished, showing that would be redundant
            return Tag + BracketString((Tag.ToLower() == "contract" ? State : ""), Name, Type, Agent);
        }

        /// <summary>
        /// When Parent is set or changed IdentifyParent will be called.
        /// Deriving classes can override this method and check for the new parent.
        /// </summary>
        protected override void IdentifyParent()
        {
            // can not check for Parent.Parent here, is not yet identified
            if (Parent != null && Parent.Tag.ToLower() == "contracts")
            {
                Origin = ContractOrigin.Contracts;
                Parent.CanBeDeleted = false;
            }
            else
            {
                Origin = ContractOrigin.Other;
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

            // now we can check for Parent.Parent
            if (Origin == ContractOrigin.Contracts && Parent.Parent != null && 
                Parent.Parent.Tag.ToLower() == "scenario" && Parent.Parent.Name.ToLower() == "contractsystem")
            {
                Parent.Parent.CanBeDeleted = false;
            }
            else
            {
                Origin = ContractOrigin.Other;
            }

            if (Origin == ContractOrigin.Contracts && State.ToLower() == "active")
            {
                CheckContractParameters(roots);
            }
        }

        private void CheckContractParameters(List<KmlItem> roots)
        {
            KmlItem context;
            string checkmain, check, expected;

            switch (Type)
            {
                case "OrbitalConstructionContract":
                    context = GetContext("vesselName");
                    // expect the id to match that of the vessel with this name:
                    expected = GetAttribValueDef("vesselName");
                    
                    checkmain = GetAttribValueDef("constructionVslId");
                    CheckVesselId(roots, context, "persistentId", checkmain, expected);
                    
                    check = GetParamAttribValueDef("ConstructionParameter", "vesselPersistentId");
                    if (check != checkmain) CheckVesselId(roots, context, "persistentId", check, expected);
                    break;
                case "RecoverAsset":
                    // params may be optional
                    if (GetChildNode("PARAM", "AcquirePart") != null || GetChildNode("PARAM", "RecoverPart") != null)
                    {
                        context = GetContext("partName");
                        // expect the id to match that of the this kind of part:
                        expected = GetAttribValueDef("partName");

                        checkmain = GetAttribValueDef("partID");
                        CheckPartId(roots, context, "uid", checkmain, expected);

                        check = GetParamAttribValueDef("AcquirePart", "part");
                        if (check.Length > 0 && check != checkmain) CheckPartId(roots, context, "uid", check, expected);

                        check = GetParamAttribValueDef("RecoverPart", "part");
                        if (check.Length > 0 && check != checkmain) CheckPartId(roots, context, "uid", check, expected);
                    }
                    break;
                case "RoverConstructionContract":
                    context = GetContext("bodyName");
                    // expect the id to match that of the vessel where the name contains 'Unfinished' and the body:
                    expected = GetAttribValueDef("bodyName");

                    checkmain = GetAttribValueDef("roverVslId");
                    CheckVesselId(roots, context, "persistentId", checkmain, new [] { "Unfinished", expected });

                    check = GetParamAttribValueDef("RoverWayPointParameter", "roverVslId");
                    if (check != checkmain) CheckVesselId(roots, context, "persistentId", check, new[] { "Unfinished", expected });
                    break;
                case "VesselRepairContract":
                    context = GetContext("vesselName");
                    // expect the id to match that of the vessel with this name:
                    expected = GetAttribValueDef("vesselName");

                    check = GetParamAttribValueDef("RepairPartParameter", "vesselPersistentId");
                    CheckVesselId(roots, context, "persistentId", check, expected);

                    // expect the id to match that of the this kind of part:
                    expected = GetAttribValueDef("repairPartName");

                    check = GetParamAttribValueDef("RepairPartParameter", "partPersistentId");
                    CheckPartId(roots, context, "persistentId", check, expected);
                    break;
                default:
                    if (GetChildNode("PARAM", "SpecificVesselParameter") != null)
                    {
                        // it's about the param, don't know what to pick here, but must be attrib from contract itself
                        context = GetContext("type");
                        // expect the id to match that of the vessel with this name:
                        expected = GetParamAttribValueDef("SpecificVesselParameter", "targetVesselName");

                        check = GetParamAttribValueDef("SpecificVesselParameter", "targetVesselID");
                        CheckVesselId(roots, context, "pid", check, expected);
                    }
                    break;
            }
        }

        private string GetAttribValueDef(string attribName)
        {
            return GetAttribValueDef(this, attribName);
        }

        private string GetAttribValueDef(KmlNode node, string attribName)
        {
            string defaultValue = "";
            if (node == null) return defaultValue;
            KmlAttrib attrib = node.GetAttrib(attribName);
            if (attrib == null) return defaultValue;
            return attrib.Value;
        }

        private string GetParamAttribValueDef(string paramName, string attribName)
        {
            string defaultValue = "";
            foreach (var param in Children)
            {
                if (param.Tag.ToLower() == "param" && param.Name.ToLower() == paramName.ToLower())
                {
                    KmlAttrib attrib = param.GetAttrib(attribName);
                    if (attrib == null) return defaultValue;
                    return attrib.Value;
                }
            }
            return defaultValue;
        }

        private KmlItem GetContext(string attribName)
        {
            KmlItem attrib = GetAttrib(attribName);
            if (attrib == null)
            {
                // we know that exists, because we operate from within a switch case on that
                attrib = GetAttrib("type");
                Syntax.Warning(attrib, "Contract attribute expected but not found: " + attribName);
            }
            return attrib;
        }

        private void CheckVesselId(List<KmlItem> roots, KmlItem context, string idName, string vesselId, string[] vesselNameContains)
        {
            CheckVesselId(roots, context, idName, vesselId, null, vesselNameContains);
        }

        private void CheckVesselId(List<KmlItem> roots, KmlItem context, string idName, string vesselId, string vesselName)
        {
            CheckVesselId(roots, context, idName, vesselId, vesselName, null);
        }

        private void CheckVesselId(List<KmlItem> roots, KmlItem context, string idName, string vesselId, string vesselName, string[] vesselNameContains)
        {
            List<KmlVessel> vesselById = GetVessels(roots, idName, vesselId);
            if (vesselById.Count == 0)
            {
                List<KmlVessel> vesselByName;
                if (vesselName != null)
                {
                    vesselByName = GetVessels(roots, "name", vesselName);
                }
                else if (vesselNameContains != null)
                {
                    vesselByName = GetVessels(roots, "name", vesselNameContains);
                    vesselName = String.Join(" ... ", vesselNameContains) + " ...";
                }
                else
                {
                    vesselByName = new List<KmlVessel>();
                }

                if (vesselByName.Count == 0)
                {
                    Syntax.Warning(context, "Contract refers to vessel '" + vesselName + "' (" + idName + " = " + vesselId +
                        "), but no vessels with either that ID or name are found");
                }
                else if (vesselByName.Count > 1)
                {
                    Syntax.Warning(context, "Contract refers to vessel '" + vesselName + "' (" + idName + " = " + vesselId +
                        "), but no vessels with that ID and multiple vessels with that name are found: " + String.Join(", ", vesselByName));
                }
                else
                {
                    // this is the case we really care about
                    string vId = GetAttribValueDef(vesselByName[0], idName);
                    Syntax.Warning(context, "Contract refers to vessel '" + vesselName + "' (" + idName + " = " + vesselId +
                        "), but only a vessel '" + vesselByName[0].Name + "' (" + idName + " = " + vId + ") exists");
                    NeedsRepair = true;
                    RelatedVessel = vesselByName[0];
                }
            }
            else if (vesselById.Count > 1)
            {
                Syntax.Warning(context, "Contract refers to vessel '" + vesselName + "' (" + idName + " = " + vesselId +
                    "), but multiple vessels with same ID are found: " + String.Join(", ", vesselById));
            }
            else
            {
                // all is fine
                RelatedVessel = vesselById[0];
            }
        }

        private void CheckPartId(List<KmlItem> roots, KmlItem context, string idName, string partId, string partName)
        {
            // CheckVesselId() has to come first, so that relatedVessel might be already set

            List<KmlPart> partById = GetParts(roots, RelatedVessel, idName, partId);
            if (partById.Count == 0)
            {
                List<KmlPart> partByName = GetParts(roots, RelatedVessel, "name", partName);

                if (partByName.Count == 0)
                {
                    Syntax.Warning(context, "Contract refers to part '" + partName + "' (" + idName + " = " + partId +
                        "), but no parts with either that ID or name are found");
                    // might be true from vessel check, but we can not fully repair and a success message would be misleading
                    NeedsRepair = false;
                }
                else if (partByName.Count > 1)
                {
                    Syntax.Warning(context, "Contract refers to part '" + partName + "' (" + idName + " = " + partId +
                        "), but no parts with that ID and multiple parts with that name are found"); // no part list, probably too long
                    NeedsRepair = false;
                }
                else
                {
                    // this is the case we really care about
                    string pId = GetAttribValueDef(partByName[0], idName);
                    Syntax.Warning(context, "Contract refers to part '" + partName + "' (" + idName + " = " + partId +
                        "), but only a part '" + partByName[0].Name + "' (" + idName + " = " + pId + ") exists");
                    NeedsRepair = true;
                    RelatedPart = partByName[0];
                    // we may have found the vessel that was unknown so far?
                    if (RelatedVessel == null)
                    {
                        RelatedVessel = RelatedPart.Parent as KmlVessel;
                    }
                }
            }
            else if (partById.Count > 1)
            {
                Syntax.Warning(context, "Contract refers to part '" + partName + "' (" + idName + " = " + partId +
                    "), but multiple parts with same ID are found: " + String.Join(", ", partById));
                NeedsRepair = false;
            }
            else
            {
                // all is fine
                RelatedPart = partById[0];
                // we may have found the vessel that was unknown so far?
                if (RelatedVessel == null)
                {
                    RelatedVessel = RelatedPart.Parent as KmlVessel;
                }
            }
        }

        private List<KmlVessel> GetVessels(List<KmlItem> roots, string attribName, string attribValue)
        {
            List<KmlVessel> result = new List<KmlVessel>();

            string[] tags = { "game", "flightstate" };
            KmlNode flightStateNode = GetNodeFromDeep(roots, tags);
            if (flightStateNode != null)
            {
                foreach (KmlNode vesselNode in flightStateNode.Children)
                {
                    if (vesselNode is KmlVessel)
                    {
                        KmlVessel vessel = (KmlVessel)vesselNode;
                        KmlAttrib attrib = vessel.GetAttrib(attribName);
                        if (attrib != null && attrib.Value.ToLower() == attribValue.ToLower())
                        {
                            result.Add(vessel);
                        }
                    }
                }
            }
            return result;
        }

        private List<KmlVessel> GetVessels(List<KmlItem> roots, string attribName, string[] attribValueContains)
        {
            List<KmlVessel> result = new List<KmlVessel>();

            string[] tags = { "game", "flightstate" };
            KmlNode flightStateNode = GetNodeFromDeep(roots, tags);
            if (flightStateNode != null)
            {
                foreach (KmlNode vesselNode in flightStateNode.Children)
                {
                    if (vesselNode is KmlVessel)
                    {
                        KmlVessel vessel = (KmlVessel)vesselNode;
                        KmlAttrib attrib = vessel.GetAttrib(attribName);
                        if (attrib != null && attribValueContains.All(x => attrib.Value.ToLower().Contains(x.ToLower())))
                        {
                            result.Add(vessel);
                        }
                    }
                }
            }
            return result;
        }

        private List<KmlPart> GetParts(List<KmlItem> roots, KmlVessel vesselKnown, string attribName, string attribValue)
        {
            List<KmlPart> result = new List<KmlPart>();

            if (vesselKnown == null)
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
                                KmlAttrib attrib = part.GetAttrib(attribName);
                                if (attrib != null && attrib.Value.ToLower() == attribValue.ToLower())
                                {
                                    result.Add(part);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (KmlPart part in vesselKnown.Parts)
                {
                    KmlAttrib attrib = part.GetAttrib(attribName);
                    if (attrib != null && attrib.Value.ToLower() == attribValue.ToLower())
                    {
                        result.Add(part);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Repair the contract parameters.
        /// </summary>
        public void Repair()
        {
            string id;
            KmlNode node;
            KmlItem context;

            switch (Type)
            {
                case "OrbitalConstructionContract":
                    context = GetContext("vesselName");
                    if (RelatedVessel == null)
                    {
                        Syntax.Warning(context, "Don't know how to repair contract parameters, no vessel found");
                    }
                    else
                    {
                        id = RelatedVessel.GetAttrib("persistentId").Value;
                        GetOrCreateAttrib("constructionVslId", id).Value = id;
                        node = GetOrCreateChildNode("PARAM", "ConstructionParameter");
                        node.GetOrCreateAttrib("vesselPersistentId", id).Value = id;
                        Syntax.Info(context, "Successfully repaired contract parameters");
                    }
                    break;
                case "RecoverAsset":
                    // params may be optional
                    if (GetChildNode("PARAM", "AcquirePart") != null || GetChildNode("PARAM", "RecoverPart") != null)
                    {
                        context = GetContext("partName");
                        if (RelatedPart == null)
                        {
                            Syntax.Warning(context, "Don't know how to repair contract parameters, no part found");
                        }
                        else
                        {
                            id = RelatedPart.GetAttrib("uid").Value;
                            GetOrCreateAttrib("partID", id).Value = id;

                            node = GetChildNode("PARAM", "AcquirePart");
                            if (node != null)
                            {
                                node.GetOrCreateAttrib("part", id).Value = id;
                            }
                            node = GetChildNode("PARAM", "RecoverPart");
                            if (node != null)
                            {
                                node.GetOrCreateAttrib("part", id).Value = id;
                            }
                            Syntax.Info(context, "Successfully repaired contract parameters");
                        }
                    }
                    break;
                case "RoverConstructionContract":
                    context = GetContext("bodyName");
                    if (RelatedVessel == null)
                    {
                        Syntax.Warning(context, "Don't know how to repair contract parameters, no vessel found");
                    }
                    else
                    {
                        id = RelatedVessel.GetAttrib("persistentId").Value;
                        GetOrCreateAttrib("roverVslId", id).Value = id;
                        node = GetOrCreateChildNode("PARAM", "RoverWayPointParameter");
                        node.GetOrCreateAttrib("roverVslId", id).Value = id;
                        Syntax.Info(context, "Successfully repaired contract parameters");
                    }
                    break;
                case "VesselRepairContract":
                    context = GetContext("vesselName");
                    if (RelatedVessel == null)
                    {
                        Syntax.Warning(context, "Don't know how to repair contract parameters, no vessel found");
                    }
                    else
                    {
                        id = RelatedVessel.GetAttrib("persistentId").Value;
                        node = GetOrCreateChildNode("PARAM", "RepairPartParameter");
                        node.GetOrCreateAttrib("vesselPersistentId", id).Value = id;

                        if (RelatedPart == null)
                        {
                            Syntax.Warning(context, "Don't know how to repair contract parameters, no part found");
                        }
                        else
                        {
                            id = RelatedPart.GetAttrib("persistentId").Value;
                            node = GetOrCreateChildNode("PARAM", "RepairPartParameter");
                            node.GetOrCreateAttrib("partPersistentId", id).Value = id;
                            Syntax.Info(context, "Successfully repaired contract parameters");
                        }
                    }
                    break;
                default:
                    node = GetChildNode("PARAM", "SpecificVesselParameter");
                    if (node != null)
                    {
                        context = GetContext("type");
                        if (RelatedVessel == null)
                        {
                            // it's about the param, don't know what attrib to pick here, but must be attrib from contract itself
                            Syntax.Warning(context, "Don't know how to repair contract parameters, no vessel found");
                        }
                        else
                        {
                            id = RelatedVessel.GetAttrib("pid").Value;
                            node.GetOrCreateAttrib("targetVesselID", id).Value = id;
                            Syntax.Info(context, "Successfully repaired contract parameters");
                        }
                    }
                    break;
            }
            // done, or in case of failure not try again and repeat same problem
            NeedsRepair = false;
            
            // this updates the context menu, not to include repair this time
            InvokeToStringChanged();
        }

        private void Type_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            Type = GetAttribWhereValueChanged(sender).Value;
            InvokeToStringChanged();
        }

        private void State_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            State = GetAttribWhereValueChanged(sender).Value;
            InvokeToStringChanged();
        }

        private void Agent_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            Agent = GetAttribWhereValueChanged(sender).Value;
            InvokeToStringChanged();
        }
    }
}
