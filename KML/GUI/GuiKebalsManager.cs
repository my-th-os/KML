using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KML
{
    /// <summary>
    /// The GuiTreeManager combines two Listviews.
    /// Into the kerbal ListView all KmlKerbals can be loaded.
    /// The attributes of the selected kerbal are displayed in the details ListView.
    /// </summary>
    class GuiKebalsManager : IGuiManager
    {
        /// <summary>
        /// Get the GuiKerbalsFilter to filter the list for different kerbal types
        /// </summary>
        public GuiKerbalsFilter Filter { get; private set; }

        private GuiTabsManager Master { get; set; }
        private List<KmlKerbal> Kerbals { get; set; }
        private ListView KerbalsList { get; set; }
        private ListView KerbalsDetails { get; set; }
        private Label KerbalsCount { get; set; }
        private KmlNode Roster { get; set; }

        /// <summary>
        /// Creates a GuiKebalsManager to link and manage the given two ListViews.
        /// </summary>
        /// <param name="master">The master GuiTabsManager</param>
        /// <param name="kerbalsList">The ListView to manage the kerbal list</param>
        /// <param name="kerbalsDetails">The ListView to manage the kerbal details</param>
        /// <param name="kerbalsCount">The Label to display the visible items count</param>
        public GuiKebalsManager(GuiTabsManager master, ListView kerbalsList, ListView kerbalsDetails, Label kerbalsCount)
        {
            Filter = new GuiKerbalsFilter();

            Master = master;

            Kerbals = new List<KmlKerbal>();
            KerbalsList = kerbalsList;
            KerbalsDetails = kerbalsDetails;
            KerbalsCount = kerbalsCount;
            Roster = null;

            KerbalsList.SelectionChanged += KerbalsList_SelectionChanged;
        }

        /// <summary>
        /// Load all kerbals from KML Tree.
        /// </summary>
        /// <param name="master">The GuiTreeManager that manages the loaded tree</param>
        public void Load(GuiTreeManager master)
        {
            Kerbals.Clear();
            KerbalsList.Items.Clear();
            KerbalsDetails.Items.Clear();

            List<KmlKerbal> list = master.GetFlatList<KmlKerbal>();
            KmlNode roster = null;
            foreach (KmlKerbal kerbal in list)
            {
                if (kerbal.Origin == KmlKerbal.KerbalOrigin.Roster)
                {
                    roster = kerbal.Parent;
                    Kerbals.Add(kerbal);
                }
            }
            if (roster != null && roster != Roster)
            {
                if (Roster != null)
                {
                    Roster.ChildrenChanged -= KerbalsChanged;
                }
                roster.ChildrenChanged += KerbalsChanged;
                Roster = roster;
            }

            // Sort the list
            Kerbals = Kerbals.OrderBy(x => x.Name).ToList();

            foreach (KmlKerbal kerbal in Kerbals)
            {
                GuiKerbalsNode node = new GuiKerbalsNode(kerbal);
                node.MouseDoubleClick += KerbalsNode_MouseDoubleClick;
                KerbalsList.Items.Add(node);
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
            if (KerbalsList.SelectedIndex < 0)
            {
                Next();
            }
            else
            {
                Application.Current.MainWindow.UpdateLayout();
                (KerbalsList.SelectedItem as ListViewItem).Focus();
            }
        }

        /// <summary>
        /// Selects next kerbal in the list.
        /// </summary>
        public void Next()
        {
            int selectIndex = KerbalsList.SelectedIndex + 1;
            while (selectIndex < KerbalsList.Items.Count)
            {
                if ((KerbalsList.Items[selectIndex] as ListViewItem).Visibility == Visibility.Visible)
                {
                    KerbalsList.SelectedIndex = selectIndex;
                    Focus();
                    break;
                }
                selectIndex++;
            }
        }

        /// <summary>
        /// Selects previous kerbal in the list.
        /// </summary>
        public void Previous()
        {
            int selectIndex = KerbalsList.SelectedIndex - 1;
            while (selectIndex >= 0)
            {
                if ((KerbalsList.Items[selectIndex] as ListViewItem).Visibility == Visibility.Visible)
                {
                    KerbalsList.SelectedIndex = selectIndex;
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
            foreach (GuiKerbalsNode node in KerbalsList.Items)
            {
                if (node.DataKerbal == item)
                {
                    // Force a refreh, by causing SelectionChanged to invoke
                    KerbalsList.SelectedItem = null;
                    KerbalsList.SelectedItem = node;
                    KerbalsList.ScrollIntoView(node);
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
            if (KerbalsList.SelectedItem is GuiKerbalsNode)
            {
                return (KerbalsList.SelectedItem as GuiKerbalsNode).DataKerbal;
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

            int oldSelectedIndex = KerbalsList.SelectedIndex;
            int count = 0;

            foreach (GuiKerbalsNode node in KerbalsList.Items)
            {
                bool visible = true;
                switch (node.DataKerbal.Type.ToLower())
                {
                    case "crew":
                        visible = visible && Filter.Crew;
                        break;
                    case "applicant":
                        visible = visible && Filter.Applicants;
                        break;
                    case "tourist":
                        visible = visible && Filter.Tourists;
                        break;
                    default:
                        visible = visible && Filter.Others;
                        break;
                }
                switch (node.DataKerbal.Trait.ToLower())
                {
                    case "pilot":
                        visible = visible && Filter.Pilots;
                        break;
                    case "engineer":
                        visible = visible && Filter.Engineeers;
                        break;
                    case "scientist":
                        visible = visible && Filter.Scientists;
                        break;
                    case "tourist":
                        visible = visible && Filter.Tourists;
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
            if (KerbalsCount != null)
            {
                KerbalsCount.Content = "(" + count.ToString() + " Kerbal" + (count == 1 ? ")" : "s)");
            }

            // Try to have a visible item selected
            if (KerbalsList.SelectedIndex < 0)
            {
                Next();
            }
            else if ((KerbalsList.SelectedItem as ListViewItem).Visibility != Visibility.Visible)
            {
                Next();
                Previous();
            }

            // If selected item still invisible, there is no visible item
            if (KerbalsList.SelectedIndex >= 0 && (KerbalsList.SelectedItem as ListViewItem).Visibility != Visibility.Visible)
            {
                KerbalsList.SelectedIndex = -1;
            }

            // Don't know why this should be necessary, but when selection didn't change it loses focus
            if (KerbalsList.SelectedIndex >= 0 && KerbalsList.SelectedIndex == oldSelectedIndex)
            {
                //KerbalsList.SelectedIndex = oldSelectedIndex - 1;
                //Next();
                Focus();
            }
        }

        private void KerbalsChanged(object sender, RoutedEventArgs e)
        {
            // Kerbal was added or deleted
            KmlKerbal oldSelected = null;
            if (KerbalsList.SelectedItem is GuiKerbalsNode)
            {
                oldSelected = (KerbalsList.SelectedItem as GuiKerbalsNode).DataKerbal;
            }
            Load(Master.TreeManager);
            if (oldSelected != null)
            {
                Select(oldSelected);
            }
        }

        private void KerbalsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            KerbalsDetails.Items.Clear();
            if (KerbalsList.SelectedItem != null)
            {
                GuiKerbalsNode Node = (GuiKerbalsNode)KerbalsList.SelectedItem;
                foreach (KmlAttrib attrib in Node.DataKerbal.Attribs)
                {
                    KerbalsDetails.Items.Add(attrib);
                }
            }
        }

        private void KerbalsNode_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Master.Select((sender as GuiKerbalsNode).DataKerbal);
        }
    }
}
