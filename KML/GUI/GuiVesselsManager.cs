using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace KML
{
    /// <summary>
    /// The GuiVesselsManager combines two Listviews.
    /// Into the vessel ListView all KmlVessels can be loaded.
    /// The part structure of the selected vessel will be displayed in the details Canvas.
    /// </summary>
    class GuiVesselsManager : IGuiManager
    {
        /// <summary>
        /// Get the GuiKerbalsFilter to filter the list for different kerbal types
        /// </summary>
        public GuiVesselsFilter Filter { get; private set; }

        private GuiTabsManager Master { get; set; }
        private List<KmlVessel> Vessels { get; set; }
        private ListView VesselsList { get; set; }
        private Canvas VesselsDetails { get; set; }
        private Label VesselsCount { get; set; }
        private KmlNode Flightstate { get; set; }

        private GuiVesselsPartGraph PartGraph { get; set; }

        /// <summary>
        /// Creates a GuiVesselsManager to link and manage the given two ListViews.
        /// </summary>
        /// <param name="master">The master GuiTabsManager</param>
        /// <param name="vesselsList">The ListView to manage the vessel list</param>
        /// <param name="vesselsDetails">The ListView to manage the vessel details</param>
        /// <param name="vesselsCount">The Label to display the visible items count</param>
        public GuiVesselsManager(GuiTabsManager master, ListView vesselsList, Canvas vesselsDetails, Label vesselsCount)
        {
            Filter = new GuiVesselsFilter();

            Master = master;

            Vessels = new List<KmlVessel>();
            VesselsList = vesselsList;
            VesselsDetails = vesselsDetails;
            VesselsCount = vesselsCount;

            PartGraph = new GuiVesselsPartGraph(vesselsDetails, master);

            VesselsList.SelectionChanged += VesselsList_SelectionChanged;
            vesselsDetails.SizeChanged += vesselsDetails_SizeChanged;
        }

        /// <summary>
        /// Load all vessels from KML Tree.
        /// </summary>
        /// <param name="master">The GuiTreeManager that manages the loaded tree</param>
        public void Load(GuiTreeManager master)
        {
            Vessels.Clear();
            VesselsList.Items.Clear();
            VesselsDetails.Children.Clear();

            List<KmlVessel> list = master.GetFlatList<KmlVessel>();
            KmlNode flightstate = null;
            foreach (KmlVessel vessel in list)
            {
                if (vessel.Origin == KmlVessel.VesselOrigin.Flightstate)
                {
                    flightstate = vessel.Parent;
                    Vessels.Add(vessel);
                }
            }
            if (flightstate != null && flightstate != Flightstate)
            {
                if (Flightstate != null)
                {
                    Flightstate.ChildrenChanged -= VesselsChanged;
                }
                flightstate.ChildrenChanged += VesselsChanged;
                Flightstate = flightstate;
            }

            // Sort the list
            Vessels = Vessels.OrderBy(x => x.Name).ToList();

            foreach (KmlVessel vessel in Vessels)
            {
                GuiVesselsNode node = new GuiVesselsNode(vessel);
                node.MouseDoubleClick += VesselsNode_MouseDoubleClick;
                VesselsList.Items.Add(node);
            }

            // Apply current filter and count visible items
            UpdateVisibility();
        }

        /// <summary>
        /// Focus the standard control. Also select first item in the list, 
        /// if there is one and none is selected.
        /// </summary>
        public void Focus()
        {
            if (VesselsList.SelectedIndex < 0)
            {
                Next();
            }
            else
            {
                Application.Current.MainWindow.UpdateLayout();
                (VesselsList.SelectedItem as ListViewItem).Focus();
            }
        }
        
        /// <summary>
        /// Selects next vessel in the list.
        /// </summary>
        public void Next()
        {
            int selectIndex = VesselsList.SelectedIndex + 1;
            while (selectIndex < VesselsList.Items.Count)
            {
                if ((VesselsList.Items[selectIndex] as ListViewItem).Visibility == Visibility.Visible)
                {
                    VesselsList.SelectedIndex = selectIndex;
                    Focus();
                    break;
                }
                selectIndex++;
            }
        }

        /// <summary>
        /// Selects previous vessel in the list.
        /// </summary>
        public void Previous()
        {
            int selectIndex = VesselsList.SelectedIndex - 1;
            while (selectIndex >= 0)
            {
                if ((VesselsList.Items[selectIndex] as ListViewItem).Visibility == Visibility.Visible)
                {
                    VesselsList.SelectedIndex = selectIndex;
                    Focus();
                    break;
                }
                selectIndex--;
            }
        }

        /// <summary>
        /// Select should be called from within other GuiManagers
        /// and wants this manager to get avtive and go to given item.
        /// </summary>
        /// <param name="item">The KmlItem to select</param>
        public void Select(KmlItem item)
        {
            foreach (GuiVesselsNode node in VesselsList.Items)
            {
                if (node.DataVessel == item)
                {
                    // Force a refreh, by causing SelectionChanged to invoke
                    VesselsList.SelectedItem = null;
                    VesselsList.SelectedItem = node;
                    VesselsList.ScrollIntoView(node);
                    Focus();
                    return;
                }
            }
        }

        /// <summary>
        /// Get the selected KmlItem. Will be needed to check if
        /// views have to be refreshed.
        /// </summary>
        /// <returns>The currently selected KmlItem</returns>
        public KmlItem GetSelectedItem()
        {
            if (VesselsList.SelectedItem is GuiVesselsNode)
            {
                return (VesselsList.SelectedItem as GuiVesselsNode).DataVessel;
            }
            return null;
        }

        /// <summary>
        /// After changing properties of "Filter", UpdateVisibility() applies this settings to the list.
        /// This needs to be called afterwards. 
        /// </summary>
        public void UpdateVisibility()
        {
            // TODO GuiKebalsManager.UpdateVisibility(): Strange behaviour with scrolling and focus, maybe I should not mess with visibility but rebuild the complete list

            int oldSelectedIndex = VesselsList.SelectedIndex;
            int count = 0;

            foreach (GuiVesselsNode node in VesselsList.Items)
            {
                bool visible = true;
                switch (node.DataVessel.Type.ToLower())
                {
                    case "base":
                        visible = visible && Filter.Base;
                        break;
                    case "debris":
                        visible = visible && Filter.Debris;
                        break;
                    case "eva":
                        visible = visible && Filter.EVA;
                        break;
                    case "flag":
                        visible = visible && Filter.Flag;
                        break;
                    case "lander":
                        visible = visible && Filter.Lander;
                        break;
                    case "plane":
                        visible = visible && Filter.Plane;
                        break;
                    case "probe":
                        visible = visible && Filter.Probe;
                        break;
                    case "relay":
                        visible = visible && Filter.Relay;
                        break;
                    case "rover":
                        visible = visible && Filter.Rover;
                        break;
                    case "ship":
                        visible = visible && Filter.Ships;
                        break;
                    case "spaceobject":
                        visible = visible && Filter.SpaceObject;
                        break;
                    case "station":
                        visible = visible && Filter.Station;
                        break;
                    default:
                        visible = visible && Filter.Others;
                        break;
                }
                if (visible)
                {
                    node.Visibility = Visibility.Visible;
                    count++;
                }
                else
                {
                    node.Visibility = Visibility.Collapsed;
                }
            }

            // Display visible count
            if (VesselsCount != null)
            {
                VesselsCount.Content = "(" + count.ToString() + " Vessel" + (count == 1 ? ")" : "s)");
            }

            // Try to have a visible item selected
            if (VesselsList.SelectedIndex < 0)
            {
                Next();
            }
            else if ((VesselsList.SelectedItem as ListViewItem).Visibility != Visibility.Visible)
            {
                Next();
                Previous();
            }

            // If selected item still invisible, there is no visible item
            if (VesselsList.SelectedIndex >= 0 && (VesselsList.SelectedItem as ListViewItem).Visibility != Visibility.Visible)
            {
                VesselsList.SelectedIndex = -1;
            }

            // Don't know why this should be necessary, but when selection didn't change it loses focus
            if (VesselsList.SelectedIndex >= 0 && VesselsList.SelectedIndex == oldSelectedIndex)
            {
                // VesselsList.SelectedIndex = oldSelectedIndex - 1;
                // Next();
                Focus();
            }
        }

        private void VesselsChanged(object sender, RoutedEventArgs e)
        {
            // Vessel was added or deleted
            KmlVessel oldSelected = null;
            if (VesselsList.SelectedItem is GuiVesselsNode)
            {
                oldSelected = (VesselsList.SelectedItem as GuiVesselsNode).DataVessel;
            }
            Load(Master.TreeManager);
            if (oldSelected != null)
            {
                Select(oldSelected);
            }
        }

        private void vesselsDetails_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PartGraph.DrawPartStructure(VesselsList.SelectedItem as GuiVesselsNode);
        }

        private void VesselsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PartGraph.DrawPartStructure(VesselsList.SelectedItem as GuiVesselsNode);
        }

        private void VesselsNode_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Master.Select((sender as GuiVesselsNode).DataVessel);
        }
    }
}
