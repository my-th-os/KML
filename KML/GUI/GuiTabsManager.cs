using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace KML
{
    /// <summary>
    /// The GuiTabsManager is the master manager to link all visual elements to the background logic.
    /// In this class the Select() method can be called to switch between different tabs.
    /// Instances of all other manager classes are stored within.
    /// </summary>
    class GuiTabsManager : IGuiManager
    {
        /// <summary>
        /// Get the GuiTreeManager to manage the tree view.
        /// </summary>
        public GuiTreeManager TreeManager { get; private set; }

        /// <summary>
        /// Get the GuiVesselsManager to manage the vessels list.
        /// </summary>
        public GuiVesselsManager VesselsManager { get; private set; }
        
        /// <summary>
        /// Get the GuiKebalsManager to manage the kerbals list.
        /// </summary>
        public GuiKebalsManager KerbalsManager { get; private set; }

        /// <summary>
        /// Get the GuiWarningsManager to manage the warnings list.
        /// </summary>
        public GuiWarningsManager WarningsManager { get; private set; }

        /// <summary>
        /// Get the filename loaded.
        /// </summary>
        public string Filename
        {
            get
            {
                return _filename;
            }
            private set
            {
                _filename = value;
                string ext = Path.GetExtension(_filename).ToLower();
                FileIsCraft = ext == ".craft";
                FileKspDirectory = "";

                DirectoryInfo dir = Directory.GetParent(_filename);
                int parentDirs = 2;
                if (FileIsCraft)
                {
                    parentDirs += 2;
                }
                for (int i = 0; i < parentDirs && dir != null; i++)
                {
                    dir = dir.Parent;
                }
                if (dir != null)
                {
                    FileKspDirectory = dir.FullName;
                }
                FileGamedataDirectory = Path.Combine(FileKspDirectory, "GameData");
            }
        }
        private string _filename = "";

        /// <summary>
        /// Get true if *.craft file was loaded, false otherwise.
        /// </summary>
        public bool FileIsCraft { get; private set; }

        /// <summary>
        /// Get the KSP program directory from loaded filename.
        /// </summary>
        public string FileKspDirectory { get; private set; }

        /// <summary>
        /// Get the KSP GameData directory from loaded filename.
        /// </summary>
        public string FileGamedataDirectory { get; private set; }

        private TabControl Tabs { get; set; }

        private TabItem TreeTab { get; set; }
        private TabItem VesselsTab { get; set; }
        private TabItem KerbalsTab { get; set; }
        private TabItem WarningsTab { get; set; }

        private static GuiTabsManager _currentTabsManager;

        /// <summary>
        /// Creates a GuiTabsManager to manage all the given visual elements.
        /// </summary>
        /// <param name="treeTab">The TabItem for the KML tree</param>
        /// <param name="tree">The TreeView to manage the KML tree in</param>
        /// <param name="treeDetails">The ListView to manage KML tree details</param>
        /// <param name="vesselsTab">The TabItem for the vessels list</param>
        /// <param name="vesselsList">The ListView to manage the vessels list</param>
        /// <param name="vesselsDetails">The ListView to manage the vessels details</param>
        /// <param name="vesselsCount">The Label to display the item count in the vessels list</param>
        /// <param name="kerbalsTab">The TabItem for the kerbals list</param>
        /// <param name="kerbalsList">The ListView to manage the kerbals list</param>
        /// <param name="kerbalsDetails">The ListView to manage the kerbals details</param>
        /// <param name="kerbalsCount">The Label to display the item count in the kerbals list</param>
        /// <param name="warningsTab">The TabItem for the warnings list</param>
        /// <param name="warningsList">The ListView to manage the warnings list</param>
        public GuiTabsManager (
            TabItem treeTab, TreeView tree, ListView treeDetails,
            TabItem vesselsTab, ListView vesselsList, Canvas vesselsDetails, Label vesselsCount,
            TabItem kerbalsTab, ListView kerbalsList, ListView kerbalsDetails, Label kerbalsCount,
            TabItem warningsTab, ListView warningsList)
        {
            FileIsCraft = false;
            FileKspDirectory = "";
            FileGamedataDirectory = "";

            if (treeTab.Parent is TabControl && 
                treeTab.Parent == vesselsTab.Parent && treeTab.Parent == kerbalsTab.Parent && treeTab.Parent == warningsTab.Parent)
            {
                Tabs = (TabControl)treeTab.Parent;

                TreeTab = treeTab;
                TreeManager = new GuiTreeManager(this, tree, treeDetails);

                VesselsTab = vesselsTab;
                VesselsManager = new GuiVesselsManager(this, vesselsList, vesselsDetails, vesselsCount);

                KerbalsTab = kerbalsTab;
                KerbalsManager = new GuiKebalsManager(this, kerbalsList, kerbalsDetails, kerbalsCount);

                WarningsTab = warningsTab;
                WarningsManager = new GuiWarningsManager(this, warningsList);
            }
            else
            {
                throw new ArgumentException("Given TabItems don't have the same parent TabControl or it is not valid");
            }

            _currentTabsManager = this;
        }

        /// <summary>
        /// Get the current instance of GuiTabsManager.
        /// </summary>
        /// <returns>The current GuiTabsManager</returns>
        public static GuiTabsManager GetCurrent()
        {
            return _currentTabsManager;
        }

        /// <summary>
        /// Loads a KML tree from data file to the tree view
        /// </summary>
        /// <param name="filename">The full path and filename of the data file to read</param>
        public void Load(string filename)
        {
            Filename = filename;
            WarningsManager.BeforeTreeLoad();
            TreeManager.Load(filename);
            VesselsManager.Load(TreeManager);
            KerbalsManager.Load(TreeManager);
            WarningsManager.AfterTreeLoad();

            // Show warnings tab if there are any and its not a craft file
            if (!WarningsManager.IsEmpty && !FileIsCraft)
            {
                WarningsTab.Visibility = System.Windows.Visibility.Visible;
                Tabs.SelectedItem = WarningsTab;
                WarningsManager.Focus();
            }
            else
            {
                WarningsTab.Visibility = System.Windows.Visibility.Collapsed;
            }

            // Show vessels and kerbals list if its not a craft file
            if (FileIsCraft)
            {
                VesselsTab.Visibility = System.Windows.Visibility.Collapsed;
                KerbalsTab.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                VesselsTab.Visibility = System.Windows.Visibility.Visible;
                KerbalsTab.Visibility = System.Windows.Visibility.Visible;
            }

            if (Tabs.SelectedItem is TabItem && (Tabs.SelectedItem as TabItem).Visibility != System.Windows.Visibility.Visible)
            {
                // Switch to another tab when hidden
                Tabs.SelectedItem = TreeTab;
                TreeManager.Focus();
            }

            IGuiManager mgr = GetActiveGuiManager();
            if (mgr != null)
            {
                mgr.Focus();
            }
        }

        /// <summary>
        /// Saves the KML tree from tree view to file
        /// </summary>
        /// <param name="filename">The full path and filename of the data file to write</param>
        public void Save(string filename)
        {
            TreeManager.Save(filename);
            Filename = filename;
        }

        /// <summary>
        /// Focus the standard control. Also possibly select first item in a list, 
        /// if there is one and none is selected.
        /// </summary>
        public void Focus()
        {
            GetActiveGuiManager().Focus();
        }

        /// <summary>
        /// Toolbar navigation next was clicked.
        /// Implementing classes should react to this.
        /// </summary>
        public void Next()
        {
            IGuiManager mgr = GetActiveGuiManager();
            if (mgr != null)
            {
                mgr.Next();
            }
        }

        /// <summary>
        /// Toolbar navigation previous was clicked.
        /// Implementing classes should react to this.
        /// </summary>
        public void Previous()
        {
            IGuiManager mgr = GetActiveGuiManager();
            if (mgr != null)
            {
                mgr.Previous();
            }
        }

        /// <summary>
        /// Selects the given item in the tree view or in the list, the item fits into.
        /// If called when tree is active it will switch to the list when possible.
        /// Otherwise when list is active it will switch to the tree view.
        /// </summary>
        /// <param name="item">The KmlItem to select</param>
        public void Select(KmlItem item)
        {
            if(item is KmlVessel)
            {
                KmlVessel vessel = (KmlVessel)item;
                if (Tabs.SelectedItem == TreeTab && vessel.Origin == KmlVessel.VesselOrigin.Flightstate)
                {
                    Tabs.SelectedItem = VesselsTab;
                    VesselsManager.Select(vessel);
                }
                else
                {
                    Tabs.SelectedItem = TreeTab;
                    TreeManager.Select(vessel);
                }
            }
            else if(item is KmlKerbal)
            {
                KmlKerbal kerbal = (KmlKerbal)item;
                if (Tabs.SelectedItem == TreeTab && kerbal.Origin == KmlKerbal.KerbalOrigin.Roster)
                {
                    Tabs.SelectedItem = KerbalsTab;
                    KerbalsManager.Select(kerbal);
                }
                else
                {
                    Tabs.SelectedItem = TreeTab;
                    TreeManager.Select(kerbal);
                }
            }
            else
            {
                Tabs.SelectedItem = TreeTab;
                TreeManager.Select(item);
            }
        }

        /// <summary>
        /// Get the selected KmlItem. Will be needed to check if
        /// views have to be refreshed.
        /// </summary>
        /// <returns>The currently selected KmlItem</returns>
        public KmlItem GetSelectedItem()
        {
            IGuiManager manager = GetActiveGuiManager();
            if (manager != null)
            {
                return manager.GetSelectedItem();
            }
            return null;
        }

        private IGuiManager GetActiveGuiManager()
        {
            if (Tabs.SelectedItem == TreeTab)
            {
                return TreeManager;
            }
            else if (Tabs.SelectedItem == VesselsTab)
            {
                return VesselsManager;
            }
            else if (Tabs.SelectedItem == KerbalsTab)
            {
                return KerbalsManager;
            }
            else if (Tabs.SelectedItem == WarningsTab)
            {
                return WarningsManager;
            }
            else
            {
                return null;
            }
        }
    }
}
