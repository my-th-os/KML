using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace KML
{
    /// <summary>
    /// A GuiTreeNode is a specialized TreeViewItem to display
    /// and contain a KmlNode.
    /// Children are loaded lazy, not recursive in advance but
    /// only whem parent node is expanded.
    /// </summary>
    class GuiTreeNode : TreeViewItem
    {
        private class DummyTreeNode : GuiTreeNode
        {
            public DummyTreeNode()
                : base (new KmlGhostNode(""))
            {
            }
        }

        private static GuiIcons Icons = new GuiIcons16();

        /// <summary>
        /// Get the contained KmlNode from this GuiTreeNode.
        /// </summary>
        public KmlNode DataNode
        {
            get
            {
                return (KmlNode)DataContext;
            }
            private set
            {
                DataContext = value;
            }
        }

        private bool TemplateWithImage { get; set; }
        private bool TemplateWithText { get; set; }
        private bool TemplateWithMenu { get; set; }
        private bool TemplateWithAddMenu { get; set; }
        private bool TemplateWithDeleteMenu { get; set; }

        /// <summary>
        /// Creates a GuiTreeNode containing the given dataNode.
        /// </summary>
        /// <param name="dataNode">The KmlNode to contain</param>
        /// <param name="withImage">Do you want an Image in your tree node?</param>
        /// <param name="withText">Do you want text in your tree node?</param>
        /// <param name="withMenu">Do you want a context menu for your tree node?</param>
        /// <param name="withAddMenu">Do you want a context menu for adding nodes?</param>
        /// <param name="withDeleteMenu">Do you want a context menu for deletion of this tree node?</param>
        /// <param name="withChildren">Do you want tree children to expand under this node?</param>
        public GuiTreeNode(KmlNode dataNode, bool withImage, bool withText, bool withMenu, bool withAddMenu, bool withDeleteMenu, bool withChildren)
        {
            DataNode = dataNode;
            TemplateWithImage = withImage;
            TemplateWithText = withText;
            TemplateWithMenu = withMenu;
            TemplateWithAddMenu = withAddMenu;
            TemplateWithDeleteMenu = withDeleteMenu;

            AssignTemplate(withImage, withText);
            if (withChildren)
            {
                PrepareChildren();
            }

            // Get notified when KmlNode ToString() changes
            DataNode.ToStringChanged += DataNode_ToStringChanged;
            // Get notified when attributes / children are added / deleted
            DataNode.AttribChanged += DataNode_AttribChanged;
            DataNode.ChildrenChanged += DataNode_ChildrenChanged;


            // Build a context menu with tools
            if (withMenu)
            {
                BuildContextMenu(withAddMenu, withDeleteMenu);
            }
        }

        /// <summary>
        /// Creates a GuiTreeNode containing the given DataNode.
        /// </summary>
        /// <param name="dataNode">The KmlNode to contain</param>
        public GuiTreeNode(KmlNode dataNode)
            : this (dataNode, true, true, true, true, true, true)
        {
        }

        /// <summary>
        /// Add a child GuiTreeNode to this node.
        /// </summary>
        /// <param name="child">The GuiTreeNode to add</param>
        public void Add(GuiTreeNode child)
        {
            Items.Add(child);
        }

        /// <summary>
        /// Add a child KmlNode to this node.
        /// A GuiTreeNode will be built around.
        /// </summary>
        /// <param name="child">The KmlNode to add</param>
        public void Add(KmlNode child)
        {
            GuiTreeNode node = new GuiTreeNode(child);
            Add(node);
        }

        private bool NeedsLoadingChildren()
        {
            return Items.Count == 1 && Items[0] is DummyTreeNode;
        }

        private void LoadChildren()
        {
            if (NeedsLoadingChildren())
            {
                Items.Clear();
                if (DataNode != null)
                {
                    foreach (KmlNode child in DataNode.Children)
                    {
                        Add(child);
                    }
                }
            }
        }

        private void ReLoadChildren()
        {
            if (NeedsLoadingChildren())
            {
                LoadChildren();
            }
            else
            {
                // Check all KmlNodes
                for (int i = 0; i < DataNode.Children.Count; i++)
                {
                    KmlNode child = DataNode.Children[i];
                    if (Items.Count > i && (Items[i] as GuiTreeNode).DataNode != child)
                    {
                        Items.Insert(i, new GuiTreeNode(child));
                    }
                    else
                    {
                        Add(child);
                    }
                }
                // Check if there are Items left to delete
                for (int i = Items.Count - 1; i > DataNode.Children.Count - 1; i--)
                {
                    Items.RemoveAt(i);
                }
            }
        }

        private void PrepareChildren()
        {
            // loading children lazy, only when node is expanded --> GuiTreeNode_Expanded
            if (DataNode != null && DataNode.Children.Count > 0)
            {
                Items.Add(new DummyTreeNode());
                Expanded += GuiTreeNode_Expanded;
            }
        }

        private void AssignTemplate(bool withImage, bool withText)
        {
            // Fit an Image and a TextBlock into a Stackpanel,
            // have the Stackpanel as node Header

            StackPanel pan = new StackPanel();
            pan.Orientation = Orientation.Horizontal;
            if (withImage)
            {
                if (DataNode is KmlResource)
                {
                    KmlResource res = (KmlResource)DataNode;
                    pan.Children.Add(GenerateProgressBar(res.AmountRatio));
                }
                else if (DataNode is KmlPart)
                {
                    KmlPart part = (KmlPart)DataNode;
                    pan.Children.Add(GenerateImage(DataNode));
                    if (part.WorstResourceRatio >= 0.0)
                    {
                        ProgressBar prog = GenerateProgressBar(part.WorstResourceRatio);
                        prog.Height = 4;
                        prog.Margin = new Thickness(-19, 6, 3, 0);
                        pan.Children.Add(prog);
                    }
                }
                else
                {
                    pan.Children.Add(GenerateImage(DataNode));
                }
            }
            if (withText)
            {
                pan.Children.Add(new TextBlock(new Run(DataNode.ToString())));
            }
            Header = pan;
        }

        private Image GenerateImage(KmlNode node)
        {
            Image image = new Image();
            image.Height = 16;
            if (node is KmlVessel)
            {
                KmlVessel vessel = (KmlVessel)node;
                if (vessel.Type.ToLower() == "base")
                {
                    image.Source = Icons.VesselBase.Source;
                }
                else if (vessel.Type.ToLower() == "debris")
                {
                    image.Source = Icons.VesselDebris.Source;
                }
                else if (vessel.Type.ToLower() == "eva")
                {
                    image.Source = Icons.VesselEVA.Source;
                }
                else if (vessel.Type.ToLower() == "flag")
                {
                    image.Source = Icons.VesselFlag.Source;
                }
                else if (vessel.Type.ToLower() == "lander")
                {
                    image.Source = Icons.VesselLander.Source;
                }
                else if (vessel.Type.ToLower() == "plane")
                {
                    image.Source = Icons.VesselPlane.Source;
                }
                else if (vessel.Type.ToLower() == "probe")
                {
                    image.Source = Icons.VesselProbe.Source;
                }
                else if (vessel.Type.ToLower() == "spaceobject")
                {
                    image.Source = Icons.VesselSpaceObject.Source;
                }
                else if (vessel.Type.ToLower() == "station")
                {
                    image.Source = Icons.VesselStation.Source;
                }
                else if (vessel.Type.ToLower() == "relay")
                {
                    image.Source = Icons.VesselRelay.Source;
                }
                else if (vessel.Type.ToLower() == "rover")
                {
                    image.Source = Icons.VesselRover.Source;
                }
                else
                {
                    image.Source = Icons.Vessel.Source;
                }
            }
            else if (node is KmlPartDock)
            {
                KmlPartDock dock = (KmlPartDock)node;
                if (dock.DockType == KmlPartDock.DockTypes.Grapple)
                {
                    image.Source = Icons.PartGrapple.Source;
                }
                else if (dock.DockType == KmlPartDock.DockTypes.KasCPort)
                {
                    image.Source = Icons.PartKasCPort.Source;
                }
                else
                {
                    image.Source = Icons.PartDock.Source;
                }
            }
            else if (node is KmlPart)
            {
                image.Source = Icons.Part.Source;
            }
            else if (node is KmlResource)
            {
                image.Source = Icons.Resource.Source;
            }
            else if (node is KmlKerbal)
            {
                KmlKerbal kerbal = (KmlKerbal)node;
                if (kerbal.Type.ToLower() == "applicant")
                {
                    image.Source = Icons.KerbalApplicant.Source;
                }
                else if (kerbal.Type.ToLower() == "tourist")
                {
                    image.Source = Icons.KerbalTourist.Source;
                }
                else
                {
                    image.Source = Icons.Kerbal.Source;
                }
            }
            else if (node is KmlGhostNode)
            {
                image.Source = Icons.Ghost.Source;
            }
            else
            {
                image.Source = Icons.Node.Source;
            }
            image.Margin = new Thickness(0, 0, 3, 0);

            return image;
        }

        private ProgressBar GenerateProgressBar(double ratio)
        {
            ProgressBar prog = new ProgressBar();
            prog.Maximum = 1.0;
            prog.Value = ratio;
            prog.Height = 8;
            prog.Width = 16;
            prog.Margin = new Thickness(0, 0, 3, 0);

            DropShadowEffect eff = new DropShadowEffect();
            eff.ShadowDepth = 1;
            prog.Effect = eff;

            prog.Background = new SolidColorBrush(Colors.Black);
            if (ratio < 0.1)
            {
                prog.Foreground = new SolidColorBrush(Colors.Red);
            }
            else if (ratio < 0.3)
            {
                prog.Foreground = new SolidColorBrush(Colors.Red);
            }
            else if (ratio < 0.5)
            {
                prog.Foreground = new SolidColorBrush(Colors.Yellow);
            }

            return prog;
        }

        private void BuildContextMenu(bool withAddMenu, bool withDeleteMenu)
        {
            ContextMenu menu = new ContextMenu();
            MenuItem title = new MenuItem();
            Image img = GenerateImage(DataNode);
            img.Margin = new Thickness(0);
            title.Icon = img;
            title.Header = DataNode.ToString();
            title.IsEnabled = false;
            title.Background = new SolidColorBrush(Colors.Black);
            title.BorderThickness = new Thickness(1);
            title.BorderBrush = new SolidColorBrush(Colors.Gray);
            menu.Items.Add(title);
            menu.Items.Add(new Separator());

            // So far it's the default Menu, wich should not be shown if no items follow
            int defaultMenuCount = menu.Items.Count;
            
            // Give menu item a more descriptive name
            string nodeName = "node";

            if (DataNode is KmlResource)
            {
                nodeName = "resource";
                MenuItem m = new MenuItem();
                m.DataContext = DataNode;
                m.Icon = Icons.CreateImage(Icons.Resource);
                m.Header = "_Refill this resource";
                m.Click += ResourceRefill_Click;
                menu.Items.Add(m);
            }
            else if (DataNode is KmlPart)
            {
                nodeName = "part";
                KmlPart node = (KmlPart)DataNode;
                if (node.HasResources)
                {
                    MenuItem m = new MenuItem();
                    m.DataContext = DataNode;
                    m.Icon = Icons.CreateImage(Icons.Resource);
                    m.Header = "_Refill all resources in this part";
                    m.Click += PartRefill_Click;
                    menu.Items.Add(m);
                    foreach (string resType in node.ResourceTypes)
                    {
                        m = new MenuItem();
                        m.DataContext = DataNode;
                        m.Icon = Icons.CreateImage(Icons.Resource);
                        m.Header = "_Refill '" + resType + "' resource in this part";
                        m.Tag = resType;
                        m.Click += PartRefill_Click;
                        menu.Items.Add(m);
                    }
                }
                if (node is KmlPartDock)
                {
                    KmlPartDock dock = (KmlPartDock)node;
                    if (dock.NeedsRepair)
                    {
                        if (menu.Items.Count > defaultMenuCount)
                        {
                            menu.Items.Add(new Separator());
                        }
                        MenuItem m = new MenuItem();
                        m.DataContext = DataNode;
                        m.Icon = Icons.CreateImage(Icons.PartDock);
                        m.Header = "Re_pair docking connection of this and connected part";
                        m.Click += DockRepair_Click;
                        menu.Items.Add(m);
                    }
                }
            }
            else if (DataNode is KmlVessel)
            {
                nodeName = "vessel";
                KmlVessel node = (KmlVessel)DataNode;

                MenuItem m = new MenuItem();
                m.DataContext = DataNode;
                img = GenerateImage(DataNode);
                img.Margin = new Thickness(0);
                m.Icon = img;
                m.Header = "S_witch view";
                m.Click += SwitchView_Click;
                menu.Items.Add(m);

                menu.Items.Add(new Separator());
                m = new MenuItem();
                m.DataContext = DataNode;
                m.Icon = Icons.CreateImage(Icons.VesselSpaceObject);
                m.Header = "Send to _low kerbin orbit";
                m.Click += VesselToLKO_Click;
                menu.Items.Add(m);

                if (node.HasResources)
                {
                    menu.Items.Add(new Separator());
                    m = new MenuItem();
                    m.DataContext = DataNode;
                    m.Icon = Icons.CreateImage(Icons.Resource);
                    m.Header = "_Refill all resources in all parts of this vessel";
                    m.Click += VesselRefill_Click;
                    menu.Items.Add(m);
                    foreach (string resType in node.ResourceTypes)
                    {
                        m = new MenuItem();
                        m.DataContext = DataNode;
                        m.Icon = Icons.CreateImage(Icons.Resource);
                        m.Header = "_Refill all '" + resType + "' resources in all parts of this vessel";
                        m.Tag = resType;
                        m.Click += VesselRefill_Click;
                        menu.Items.Add(m);
                    }
                }
                if (node.Flags.Count > 0)
                {
                    menu.Items.Add(new Separator());
                    m = new MenuItem();
                    m.DataContext = DataNode;
                    m.Icon = Icons.CreateImage(Icons.VesselFlag);
                    m.Header = "Change _flag in all parts of this vessel...";
                    m.Click += VesselFlagExchange_Click;
                    menu.Items.Add(m);
                }
            }
            else if (DataNode is KmlKerbal)
            {
                nodeName = "kerbal";
                KmlKerbal node = (KmlKerbal)DataNode;

                MenuItem m = new MenuItem();
                m.DataContext = DataNode;
                img = GenerateImage(DataNode);
                img.Margin = new Thickness(0);
                m.Icon = img;
                m.Header = "S_witch view";
                m.Click += SwitchView_Click;
                menu.Items.Add(m);

                if (node.AssignedVessel != null || node.AssignedPart != null)
                {
                    if (menu.Items.Count > defaultMenuCount)
                    {
                        menu.Items.Add(new Separator());
                    }
                    if (node.AssignedVessel != null)
                    {
                        m = new MenuItem();
                        m.DataContext = DataNode;
                        img = GenerateImage(node.AssignedVessel);
                        img.Margin = new Thickness(0);
                        m.Icon = img;
                        m.Header = "Select assigned _vessel: " + node.AssignedVessel.Name;
                        m.Click += KerbalSelectAssignedVessel_Click;
                        menu.Items.Add(m);
                    }
                    if (node.AssignedPart != null)
                    {
                        m = new MenuItem();
                        m.DataContext = DataNode;
                        img = GenerateImage(node.AssignedPart);
                        img.Margin = new Thickness(0);
                        m.Icon = img;
                        m.Header = "Select assigned _part: " + node.AssignedPart.Name;
                        m.Click += KerbalSelectAssignedPart_Click;
                        menu.Items.Add(m);
                    }
                }

                if (node.AssignedVessel != null || node.AssignedPart != null || node.State.ToLower() == "missing")
                {
                    if (menu.Items.Count > defaultMenuCount)
                    {
                        menu.Items.Add(new Separator());
                    }
                    m = new MenuItem();
                    m.DataContext = DataNode;
                    img = Icons.CreateImage(Icons.VesselSpaceObject);
                    m.Icon = img;
                    m.Header = "Send _home to astronaut complex";
                    m.Click += KerbalSendHome_Click;
                    menu.Items.Add(m);
                }
            }

            // Adding / deleting
            if (withAddMenu)
            {
                if (menu.Items.Count > defaultMenuCount)
                {
                    menu.Items.Add(new Separator());
                }

                MenuItem m = new MenuItem();
                m.DataContext = DataNode;
                m.Icon = Icons.CreateImage(Icons.Clipboard);
                m.Header = "_Copy node";
                m.Click += CopyNode_Click;
                m.IsEnabled = DataNode.Parent != null;
                menu.Items.Add(m);

                m = new MenuItem();
                m.DataContext = DataNode;
                m.Icon = Icons.CreateImage(Icons.Paste);
                m.Header = "_Paste child node(s)";
                m.Click += PasteNode_Click;
                m.IsEnabled = Clipboard.ContainsText(TextDataFormat.UnicodeText);
                menu.Items.Add(m);

                m = new MenuItem();
                m.DataContext = DataNode;
                m.Icon = Icons.CreateImage(Icons.Paste);
                m.Header = "Paste inserting node(s) before";
                m.Click += PasteBeforeNode_Click;
                m.IsEnabled = Clipboard.ContainsText(TextDataFormat.UnicodeText);
                menu.Items.Add(m);

                menu.Items.Add(new Separator());

                m = new MenuItem();
                m.DataContext = DataNode;
                m.Icon = Icons.CreateImage(Icons.Add);
                m.Header = "Add _attribute...";
                m.Click += NodeAddAttrib_Click;
                menu.Items.Add(m);

                m = new MenuItem();
                m.DataContext = DataNode;
                m.Icon = Icons.CreateImage(Icons.Add);
                m.Header = "Add _child node...";
                m.Click += NodeAddChild_Click;
                menu.Items.Add(m);

                m = new MenuItem();
                m.DataContext = DataNode;
                m.Icon = Icons.CreateImage(Icons.Add);
                m.Header = "_Insert node before...";
                m.Click += NodeInsertBefore_Click;
                m.IsEnabled = DataNode.Parent != null;
                menu.Items.Add(m);
            }
            if (withDeleteMenu)
            {
                if (menu.Items.Count > defaultMenuCount)
                {
                    menu.Items.Add(new Separator());
                }
                MenuItem m = new MenuItem();
                m.DataContext = DataNode;
                m.Icon = Icons.CreateImage(Icons.Delete);
                m.Header = "_Delete this " + nodeName + "...";
                m.Click += NodeDelete_Click;
                m.IsEnabled = DataNode.CanBeDeleted;
                menu.Items.Add(m);
            }

            // Need to have a seperate menu for each item, even if it is empty.
            // If ContextMenu is null, the parent's contextmenu will be used (WTF).
            // Item[0] is the menu title, Item[1] a Seperator, both always created.
            ContextMenu = menu;
            if (menu.Items.Count <= defaultMenuCount)
            {
                ContextMenu.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                // Apply look and feel
                foreach (object o in menu.Items)
                {
                    if (o is MenuItem)
                    {
                        MenuItem m = (MenuItem)o;
                        if (!m.IsEnabled && m.Icon != null)
                        {
                            (m.Icon as Image).Opacity = 0.3;
                        }
                    }
                }
            }
        }

        private void PasteNode_Click(object sender, RoutedEventArgs e)
        {
            KmlNode node = ((sender as MenuItem).DataContext as KmlNode);

            var textNode = Clipboard.GetText(TextDataFormat.UnicodeText);

            var items = KmlItem.ParseItems(new StringReader(textNode)).Where(i => i is KmlNode || i is KmlAttrib).ToList();

            if (!items.Any())
                DlgMessage.Show("Can not paste node from clipboard", "Paste node", Icons.Warning);

            node.AddRange(items);
        }

        private void PasteBeforeNode_Click(object sender, RoutedEventArgs e)
        {
            KmlNode node = ((sender as MenuItem).DataContext as KmlNode);
            if (node.Parent != null)
            {
                var textNode = Clipboard.GetText(TextDataFormat.UnicodeText);

                var items = KmlItem.ParseItems(new StringReader(textNode)).Where(i => i is KmlNode || i is KmlAttrib).ToList();

                if (!items.Any())
                    DlgMessage.Show("Can not paste node from clipboard", "Paste node", Icons.Warning);

                node.Parent.InsertBeforeRange(node, items);
            }
            else
            {
                DlgMessage.Show("Can not insert, node has no parent", "Paste node", Icons.Warning);
            }
        }

        private void CopyNode_Click(object sender, RoutedEventArgs e)
        {
            KmlNode node = ((sender as MenuItem).DataContext as KmlNode);

            var sr = new StringWriter();

            KmlItem.WriteItem(sr, node, 0);

            sr.Flush();

            var textNode = sr.GetStringBuilder().ToString();

            Clipboard.SetText(textNode, TextDataFormat.UnicodeText);
        }

        private void AddChildNode(KmlNode toItem, KmlNode beforeItem)
        {
            KmlNode node = toItem;
            string input;
            string preset = "";
            bool loop = true;
            while (loop && DlgInput.Show("Enter the tag for the new node:", "NEW node", Icons.Add, preset, out input))
            {
                KmlItem item = KmlItem.CreateItem(input);
                if (item is KmlNode && beforeItem != null)
                {
                    node.InsertBefore(beforeItem, (KmlNode)item);
                    loop = false;
                    // View will be refreshed in ChildrenChanged event
                }
                else if (item is KmlNode)
                {
                    node.Add((KmlNode)item);
                    loop = false;
                    // View will be refreshed in ChildrenChanged event
                }
                else
                {
                    DlgMessage.Show("Tag is not allowed to be empty or contain following characters: ={}", "NEW node", Icons.Warning);
                    preset = input;
                    // Input will pop up again while loop == true
                }
            }
        }

        private void AddAttrib(KmlNode toItem, KmlAttrib beforeItem)
        {
            KmlNode node = toItem;
            string input;
            string preset = "";
            bool loop = true;
            while (loop && DlgInput.Show("Enter the name for the new attribute:", "NEW attribute", Icons.Add, preset, out input))
            {
                string attrib = input;
                if (attrib.Length > 0 && attrib.IndexOf('=') < 0)
                {
                    attrib = attrib + "=";
                }
                KmlItem item = KmlItem.CreateItem(attrib);
                if (item is KmlAttrib && beforeItem != null)
                {
                    node.InsertBefore(beforeItem, (KmlAttrib)item);
                    loop = false;
                    // View will be refreshed in AttribChanged event
                }
                else if (item is KmlAttrib)
                {
                    node.Add((KmlAttrib)item);
                    loop = false;
                    // View will be refreshed in AttribChanged event
                }
                else
                {
                    DlgMessage.Show("Attribute name is not allowed to be empty or contain following characters: {}", "NEW attribute", Icons.Warning);
                    preset = input;
                    // Input will pop up again while loop == true
                }
            }
        }

        private void GuiTreeNode_Expanded(object sender, RoutedEventArgs e)
        {
            Expanded -= GuiTreeNode_Expanded;
            LoadChildren();
        }

        private void SwitchView_Click(object sender, RoutedEventArgs e)
        {
            KmlItem item = (sender as MenuItem).DataContext as KmlItem;
            GuiTabsManager.GetCurrent().Select(item);
        }

        private void DockRepair_Click(object sender, RoutedEventArgs e)
        {
            KmlPartDock dock = (sender as MenuItem).DataContext as KmlPartDock;
            dock.Repair();
            DlgMessage.ShowAndClear(Syntax.Messages);
            
            // Refresh view
            IGuiManager manager = GuiTabsManager.GetCurrent().VesselsManager;
            if (dock.Parent is KmlVessel && dock.Parent == manager.GetSelectedItem())
            {
                manager.Select(dock.Parent);
            }
        }

        private void VesselToLKO_Click(object sender, RoutedEventArgs e)
        {
            KmlVessel vessel = (sender as MenuItem).DataContext as KmlVessel;
            if (DlgConfirmation.Show("Do you really want to send " + vessel.Name + " to low kerbin orbit?\n\n" +
                "- The vessel situation and orbit data will be changed\n" +
                "- Orbit height will be 80km\n" +
                "- Do not use when your kerbin is scaled bigger (RSS)",
                "Send vessel to LKO", (sender as MenuItem).Icon as Image))
            {
                vessel.SendToKerbinOrbit(80000.0);
            }
        }

        private void VesselRefill_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem).Tag != null)
            {
                ((sender as MenuItem).DataContext as KmlVessel).Refill((sender as MenuItem).Tag.ToString());
            }
            else
            {
                ((sender as MenuItem).DataContext as KmlVessel).Refill();
            }
            // Possibly need to update the details ListView, so force Selected event to be fired
            IsSelected = false;
            IsSelected = true;
        }

        private void VesselFlagExchange_Click(object sender, RoutedEventArgs e)
        {
            KmlVessel vessel = ((sender as MenuItem).DataContext as KmlVessel);
            if (vessel.Flags.Count > 0)
            {
                string oldFlag = vessel.Flags[0];
                string otherFlags = "";
                foreach(string flag in vessel.Flags)
                {
                    if (flag.ToLower() != oldFlag.ToLower())
                    {
                        otherFlags += "\n - " + flag;
                    }
                }
                if (otherFlags.Length > 0)
                {
                    otherFlags = "\nThese other flags will not be changed" + otherFlags;
                }
                string newFlag;
                if (DlgInput.Show("All parts with flag '" + oldFlag + "' will be changed." + otherFlags + "\n\nEnter the new flag:", "Change vessel flag", Icons.VesselFlag, oldFlag, out newFlag))
                {
                    vessel.FlagExchange(oldFlag, newFlag);
                }
            }
        }

        private void PartRefill_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem).Tag != null)
            {
                ((sender as MenuItem).DataContext as KmlPart).Refill((sender as MenuItem).Tag.ToString());
            }
            else
            {
                ((sender as MenuItem).DataContext as KmlPart).Refill();
            }
            // Possibly need to update the details ListView, so force Selected event to be fired
            IsSelected = false;
            IsSelected = true;
        }

        private void ResourceRefill_Click(object sender, RoutedEventArgs e)
        {
            ((sender as MenuItem).DataContext as KmlResource).Refill();
            // Possibly need to update the details ListView, so force Selected event to be fired
            IsSelected = false;
            IsSelected = true;
        }

        private void KerbalSendHome_Click(object sender, RoutedEventArgs e)
        {
            KmlKerbal kerbal = (sender as MenuItem).DataContext as KmlKerbal;
            if (DlgConfirmation.Show("Do you really want to send " + kerbal.Name + " home to astronaut complex?\n\n"+
                "- The kerbal will be removed from assigned crew part\n"+
                "- State will be set to 'Available'\n"+
                "- Experience or contract progress may get lost", 
                "Send home kerbal", (sender as MenuItem).Icon as Image))
            {
                kerbal.SendHome();
            }
        }

        private void KerbalSelectAssignedVessel_Click(object sender, RoutedEventArgs e)
        {
            KmlKerbal kerbal = (sender as MenuItem).DataContext as KmlKerbal;
            if (kerbal.AssignedVessel != null)
            {
                GuiTabsManager.GetCurrent().Select(kerbal.AssignedVessel);
            }
        }

        private void KerbalSelectAssignedPart_Click(object sender, RoutedEventArgs e)
        {
            KmlKerbal kerbal = (sender as MenuItem).DataContext as KmlKerbal;
            if (kerbal.AssignedPart != null)
            {
                GuiTabsManager.GetCurrent().Select(kerbal.AssignedPart);
            }
        }

        private void NodeAddAttrib_Click(object sender, RoutedEventArgs e)
        {
            KmlNode node = ((sender as MenuItem).DataContext as KmlNode);
            AddAttrib(node, null);
        }

        private void NodeAddChild_Click(object sender, RoutedEventArgs e)
        {
            KmlNode node = ((sender as MenuItem).DataContext as KmlNode);
            AddChildNode(node, null);
        }

        private void NodeInsertBefore_Click(object sender, RoutedEventArgs e)
        {
            KmlNode node = ((sender as MenuItem).DataContext as KmlNode);
            if (node.Parent != null)
            {
                AddChildNode(node.Parent, node);
            }
            else
            {
                DlgMessage.Show("Can not insert, node has no parent", "NEW node", Icons.Warning);
            }
        }

        private void NodeDelete_Click(object sender, RoutedEventArgs e)
        {
            KmlNode node = ((sender as MenuItem).DataContext as KmlNode);
            string nodeName = "node";
            string specialText = "";
            if (node is KmlKerbal)
            {
                nodeName = "kerbal";
                if ((node as KmlKerbal).AssignedPart != null)
                {
                    specialText = "\n\n- The kerbal will be removed from assigned crew part";
                }
            }
            else if (node is KmlVessel)
            {
                nodeName = "vessel";
                if ((node as KmlVessel).AssignedCrew.Count > 0)
                {
                    specialText = "\n\n- Kerbal crew will be send home to astronaut complex\n" +
                        "- Their state will be set to 'Available'\n" +
                        "- Experience or contract progress may get lost";
                }
            }
            else if (node is KmlPart)
            {
                nodeName = "part";
                specialText = "\n\n- Part will be removed from vessel structure\n" +
                    "- Attachment indices will be updated";
                if (node.Parent is KmlVessel)
                {
                    foreach (KmlKerbal kerbal in (node.Parent as KmlVessel).AssignedCrew)
                    {
                        if (kerbal.AssignedPart == node)
                        {
                            specialText += "\n- Kerbal crew will be send home to astronaut complex\n" +
                                "- Their state will be set to 'Available'\n" +
                                "- Experience or contract progress may get lost";
                            break;
                        }
                    }
                }
            }
            if (DlgConfirmation.Show("Do you really want to delete this " + nodeName + " and all its content?\n" + 
                node + specialText, "DELETE " + nodeName, Icons.Delete))
            {
                node.Delete();
                // View will be refreshed in parent's ChildrenChanged event
                // Special case: PartGraph is currently shown
                if (Parent is GuiVesselsPartGraphNode)
                {
                    GuiVesselsPartGraphNode pgn = (GuiVesselsPartGraphNode)Parent;
                    if (pgn.Parent is Canvas)
                    {
                        Canvas cnv = (Canvas)pgn.Parent;
                        foreach (var line in pgn.Lines)
                            cnv.Children.Remove(line);
                        cnv.Children.Remove(pgn);
                    }
                }
            }
        }

        void DataNode_ChildrenChanged(object sender, RoutedEventArgs e)
        {
            // If not loaded yet, they will be loaded correctly when expanded
            if (!NeedsLoadingChildren())
            {
                ReLoadChildren();
            }
            // IsExpanded = true;
        }

        void DataNode_AttribChanged(object sender, RoutedEventArgs e)
        {
            // Refresh details view if selected
            if (IsSelected)
            {
                IsSelected = false;
                IsSelected = true;
            }
        }

        private void DataNode_ToStringChanged(object sender, RoutedEventArgs e)
        {
            AssignTemplate(TemplateWithImage, TemplateWithText);
            BuildContextMenu(TemplateWithAddMenu, TemplateWithDeleteMenu);
            if (Parent is GuiVesselsPartGraphNode)
            {
                GuiVesselsPartGraphNode pgn = (GuiVesselsPartGraphNode)Parent;
                pgn.UpdateFromGuiTreeNode();
            }
        }
    }
}
