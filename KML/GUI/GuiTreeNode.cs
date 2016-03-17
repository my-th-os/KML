using System;
using System.Collections.Generic;
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
                : base (new KmlGhostNode("", null))
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

        /// <summary>
        /// Creates a GuiTreeNode containing the given DataNode.
        /// </summary>
        /// <param name="dataNode">The KmlNode to contain</param>
        /// <param name="withImage">Do you want an Image in your tree node?</param>
        /// <param name="withText">Do you want text in your tree node?</param>
        /// <param name="withMenu">Do you want a context menu for your tree node?</param>
        /// <param name="withChildren">Do you want tree children to expand under this node?</param>
        public GuiTreeNode(KmlNode dataNode, bool withImage, bool withText, bool withMenu, bool withChildren)
        {
            DataNode = dataNode;
            TemplateWithImage = withImage;
            TemplateWithText = withText;

            AssignTemplate(withImage, withText);
            if (withChildren)
            {
                PrepareChildren();
            }

            // Get notified when KmlNode ToString() changes
            DataNode.ToStringChanged += DataNode_ToStringChanged;

            // Build a context menu with tools
            if (withMenu)
            {
                BuildContextMenu();
            }
        }

        /// <summary>
        /// Creates a GuiTreeNode containing the given DataNode.
        /// </summary>
        /// <param name="dataNode">The KmlNode to contain</param>
        public GuiTreeNode(KmlNode dataNode)
            : this (dataNode, true, true, true, true)
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

        private void LoadChildren()
        {
            if (Items.Count == 1 && Items[0] is DummyTreeNode)
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
                    image.Source = Icons.KerbalTorist.Source;
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

        private void BuildContextMenu()
        {
            ContextMenu menu = new ContextMenu();
            MenuItem title = new MenuItem();
            title.Icon = GenerateImage(DataNode);
            title.Header = DataNode.ToString();
            title.IsEnabled = false;
            title.Background = new SolidColorBrush(Colors.Black);
            title.BorderThickness = new Thickness(1);
            title.BorderBrush = new SolidColorBrush(Colors.Gray);
            menu.Items.Add(title);
            menu.Items.Add(new Separator());

            if (DataNode is KmlResource)
            {
                MenuItem m = new MenuItem();
                m.DataContext = DataNode;
                m.Icon = Icons.CreateImage(Icons.Resource);
                m.Header = "Refill this resource";
                m.Click += ResourceRefill_Click;
                menu.Items.Add(m);
            }
            else if (DataNode is KmlPart)
            {
                KmlPart node = (KmlPart)DataNode;
                if (node.HasResources)
                {
                    MenuItem m = new MenuItem();
                    m.DataContext = DataNode;
                    m.Icon = Icons.CreateImage(Icons.Resource);
                    m.Header = "Refill all resources in this part";
                    m.Click += PartRefill_Click;
                    menu.Items.Add(m);
                    foreach (string resType in node.ResourceTypes)
                    {
                        m = new MenuItem();
                        m.DataContext = DataNode;
                        m.Icon = Icons.CreateImage(Icons.Resource);
                        m.Header = "Refill '" + resType + "' resource in this part";
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
                        if (menu.Items.Count > 2)
                        {
                            menu.Items.Add(new Separator());
                        }
                        MenuItem m = new MenuItem();
                        m.DataContext = DataNode;
                        m.Icon = Icons.CreateImage(Icons.PartDock);
                        m.Header = "Repair docking connection of this and connected part";
                        m.Click += DockRepair_Click;
                        menu.Items.Add(m);
                    }
                }
            }
            else if (DataNode is KmlVessel)
            {
                KmlVessel node = (KmlVessel)DataNode;
                if (node.HasResources)
                {
                    MenuItem m = new MenuItem();
                    m.DataContext = DataNode;
                    m.Icon = Icons.CreateImage(Icons.Resource);
                    m.Header = "Refill all resources in all parts of this vessel";
                    m.Click += VesselRefill_Click;
                    menu.Items.Add(m);
                    foreach (string resType in node.ResourceTypes)
                    {
                        m = new MenuItem();
                        m.DataContext = DataNode;
                        m.Icon = Icons.CreateImage(Icons.Resource);
                        m.Header = "Refill all '" + resType + "' resources in all parts of this vessel";
                        m.Tag = resType;
                        m.Click += VesselRefill_Click;
                        menu.Items.Add(m);
                    }
                }
            }

            // Need to have a seperate menu for each item, even if it is empty.
            // If ContextMenu is null, the parent's contextmenu will be used (WTF).
            // Item[0] is the menu title, Item[1] a Seperator, both always created.
            ContextMenu = menu;
            if (menu.Items.Count < 3)
            {
                ContextMenu.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void GuiTreeNode_Expanded(object sender, RoutedEventArgs e)
        {
            Expanded -= GuiTreeNode_Expanded;
            LoadChildren();
        }

        private void DockRepair_Click(object sender, RoutedEventArgs e)
        {
            ((sender as MenuItem).DataContext as KmlPartDock).Repair();
            DlgMessage.ShowAndClear(Syntax.Messages);
            // TODO GuiTreeNode.DockRepair_Click(): Need to refresh view?
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

        private void DataNode_ToStringChanged(object sender, RoutedEventArgs e)
        {
            AssignTemplate(TemplateWithImage, TemplateWithText);
        }
    }
}
