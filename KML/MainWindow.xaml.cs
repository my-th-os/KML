using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using Microsoft.Win32;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace KML
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GuiTabsManager TabsManager { get; set; }
        //private GuiTreeManager TreeManager { get; set; }
        //private GuiKebalsManager KerbalsManager { get; set; }
        //private GuiVesselsManager VesselsManager { get; set; }
        //private GuiWarningsManager WarningsManager { get; set; }
        
        private string Filename
        {
            get
            {
                return _filename;
            }
            set
            {
                _filename = value;
                if (value.Length > 0)
                {
                    Title = "KML - " + value;
                }
                else
                {
                    Title = "KML - Kerbal Markup Lister";
                }
            }
        }
        private string _filename = null;

        /// <summary>
        /// Creates a KML main window.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            WarningsTab.Visibility = System.Windows.Visibility.Collapsed;

            //TreeManager = new GuiTreeManager(Tree, TreeDetails);
            //VesselsManager = new GuiVesselsManager(VesselsList, VesselsDetails, VesselsCount);
            //KerbalsManager = new GuiKebalsManager(KerbalsList, KerbalsDetails, KerbalsCount);
            //WarningsManager = new GuiWarningsManager(WarningsList);
            TabsManager = new GuiTabsManager(
                TreeTab, Tree, TreeDetails, 
                VesselsTab, VesselsList, VesselsDetails, VesselsCount,
                KerbalsTab, KerbalsList, KerbalsDetails, KerbalsCount,
                WarningsTab, WarningsList);
        }

        private void CheckCommandLine()
        {
            string[] args = Environment.GetCommandLineArgs();
            foreach (string arg in args)
            {
                if ((System.IO.Path.GetExtension(arg) == ".sfs" || 
                    System.IO.Path.GetExtension(arg) == ".craft") &&
                    System.IO.File.Exists(arg))
                {
                    Load(arg);
                    return;
                }
            }
        }

        private void Load(string filename)
        {
            TabsManager.Load(filename);
            Filename = filename;
        }

        private void Save(string filename)
        {
            TabsManager.Save(filename);
            Filename = filename;
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = System.IO.Path.GetFileName(Filename);
            dlg.InitialDirectory = System.IO.Path.GetDirectoryName(Filename);
            dlg.AddExtension = true;
            dlg.DefaultExt = ".sfs";
            dlg.CheckFileExists = true;
            dlg.Filter = "KSP persistence file|*.sfs;*.craft";
            if (dlg.ShowDialog() == true)
            {
                Load(dlg.FileName);
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            // Save only after a file was loaded with same extension
            if (Filename != null)
            {
                // dlg.DefaultExt supresses the dot, so store in local var for reusage
                string ext = System.IO.Path.GetExtension(Filename);

                SaveFileDialog dlg = new SaveFileDialog();
                dlg.FileName = System.IO.Path.GetFileName(Filename);
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(Filename);
                dlg.AddExtension = true;
                dlg.DefaultExt = ext;
                dlg.OverwritePrompt = true;
                dlg.Filter = "KSP persistence file|*" + ext;
                if (dlg.ShowDialog() == true)
                {
                    Save(dlg.FileName);
                }
            }
        }

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            ButtonSearch.IsChecked = true;
            KmlItem selectedItem;
            if (DlgSearch.Show(ButtonSearch, out selectedItem))
            {
                bool isVessel = selectedItem is KmlVessel && (selectedItem as KmlVessel).Origin == KmlVessel.VesselOrigin.Flightstate;
                bool isKerbal = selectedItem is KmlKerbal && (selectedItem as KmlKerbal).Origin == KmlKerbal.KerbalOrigin.Roster;
                IGuiManager manager;
                if (isVessel && Tabs.SelectedItem == VesselsTab)
                {
                    manager = TabsManager.VesselsManager;
                }
                else if (isKerbal && Tabs.SelectedItem == KerbalsTab)
                {
                    manager = TabsManager.KerbalsManager;
                }
                else
                {
                    manager = TabsManager;
                    // Select once opens in tree view (cases with opening vessels while in vessel view handled above).
                    // We want now: Open vessels in vessel view when not in tree view (e.g. in kerbal view).
                    // Same for kerbals.
                    // We can call Select twice, so it will switch views.
                    if (isVessel || isKerbal)
                    {
                        manager.Select(selectedItem);
                    }
                }
                manager.Select(selectedItem);
            }
            // Being a ToggleButton keeps it highlighted during the popup window is open
            // But we don't want it to stay highlighted, so we uncheck afterwards
            ButtonSearch.IsChecked = false;
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            TabsManager.Next();
        }
        private void ButtonPrevious_Click(object sender, RoutedEventArgs e)
        {
            TabsManager.Previous();
        }

        private void SetContainedImageOpacity(Visual Parent, double Opacity)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(Parent); i++)
            {
                Visual child = (VisualTreeHelper.GetChild(Parent, i) as Visual);
                if (child is Image)
                {
                    (child as Image).Opacity = Opacity;
                }
                SetContainedImageOpacity(child, Opacity);
            }
        }

        private void SetToggleButtonStyle(ToggleButton Button)
        {
            if (Button.IsChecked == true)
            {
                SetContainedImageOpacity(Button, 1.0);
            }
            else
            {
                SetContainedImageOpacity(Button, 0.3);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Loading fromn commandline could cause exceptions, so do it here
            // and not in constructor, so the window can safely be created.
            CheckCommandLine();
            // It also makes this unnecessary:
            // When data loaded in constructor before window is loaded, setting focus does not work
            // For this case do it here once
            // TabsManager.Next();
            // TabsManager.Previous();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        // TODO MainWindow.KerbalsFilter... Capsulate filter handling, make filter changes visible to buttons, right click filter behaviour

        private void KerbalsFilterCrew_Click(object sender, RoutedEventArgs e)
        {
            TabsManager.KerbalsManager.Filter.Crew = (sender as ToggleButton).IsChecked == true;
            TabsManager.KerbalsManager.UpdateVisibility();
            SetToggleButtonStyle(sender as ToggleButton);
        }

        private void KerbalsFilterApplicants_Click(object sender, RoutedEventArgs e)
        {
            TabsManager.KerbalsManager.Filter.Applicants = (sender as ToggleButton).IsChecked == true;
            TabsManager.KerbalsManager.UpdateVisibility();
            SetToggleButtonStyle(sender as ToggleButton);
        }

        private void KerbalsFilterTourists_Click(object sender, RoutedEventArgs e)
        {
            TabsManager.KerbalsManager.Filter.Tourists = (sender as ToggleButton).IsChecked == true;
            TabsManager.KerbalsManager.UpdateVisibility();
            SetToggleButtonStyle(sender as ToggleButton);
        }

        private void KerbalsFilterPilots_Click(object sender, RoutedEventArgs e)
        {
            TabsManager.KerbalsManager.Filter.Pilots = (sender as ToggleButton).IsChecked == true;
            TabsManager.KerbalsManager.UpdateVisibility();
            SetToggleButtonStyle(sender as ToggleButton);
        }

        private void KerbalsFilterEngineers_Click(object sender, RoutedEventArgs e)
        {
            TabsManager.KerbalsManager.Filter.Engineeers = (sender as ToggleButton).IsChecked == true;
            TabsManager.KerbalsManager.UpdateVisibility();
            SetToggleButtonStyle(sender as ToggleButton);
        }

        private void KerbalsFilterScientists_Click(object sender, RoutedEventArgs e)
        {
            TabsManager.KerbalsManager.Filter.Scientists = (sender as ToggleButton).IsChecked == true;
            TabsManager.KerbalsManager.UpdateVisibility();
            SetToggleButtonStyle(sender as ToggleButton);
        }

        private void KerbalsFilterOthers_Click(object sender, RoutedEventArgs e)
        {
            TabsManager.KerbalsManager.Filter.Others = (sender as ToggleButton).IsChecked == true;
            TabsManager.KerbalsManager.UpdateVisibility();
            SetToggleButtonStyle(sender as ToggleButton);
        }

        private void VesselsFilterBases_Click(object sender, RoutedEventArgs e)
        {
            TabsManager.VesselsManager.Filter.Base = (sender as ToggleButton).IsChecked == true;
            TabsManager.VesselsManager.UpdateVisibility();
            SetToggleButtonStyle(sender as ToggleButton);
        }

        private void VesselsFilterDebris_Click(object sender, RoutedEventArgs e)
        {
            TabsManager.VesselsManager.Filter.Debris = (sender as ToggleButton).IsChecked == true;
            TabsManager.VesselsManager.UpdateVisibility();
            SetToggleButtonStyle(sender as ToggleButton);
        }

        private void VesselsFilterEVA_Click(object sender, RoutedEventArgs e)
        {
            TabsManager.VesselsManager.Filter.EVA = (sender as ToggleButton).IsChecked == true;
            TabsManager.VesselsManager.UpdateVisibility();
            SetToggleButtonStyle(sender as ToggleButton);
        }

        private void VesselsFilterFlags_Click(object sender, RoutedEventArgs e)
        {
            TabsManager.VesselsManager.Filter.Flag = (sender as ToggleButton).IsChecked == true;
            TabsManager.VesselsManager.UpdateVisibility();
            SetToggleButtonStyle(sender as ToggleButton);
        }

        private void VesselsFilterLanders_Click(object sender, RoutedEventArgs e)
        {
            TabsManager.VesselsManager.Filter.Lander = (sender as ToggleButton).IsChecked == true;
            TabsManager.VesselsManager.UpdateVisibility();
            SetToggleButtonStyle(sender as ToggleButton);
        }

        private void VesselsFilterProbes_Click(object sender, RoutedEventArgs e)
        {
            TabsManager.VesselsManager.Filter.Probe = (sender as ToggleButton).IsChecked == true;
            TabsManager.VesselsManager.UpdateVisibility();
            SetToggleButtonStyle(sender as ToggleButton);
        }

        private void VesselsFilterShips_Click(object sender, RoutedEventArgs e)
        {
            TabsManager.VesselsManager.Filter.Ships = (sender as ToggleButton).IsChecked == true;
            TabsManager.VesselsManager.UpdateVisibility();
            SetToggleButtonStyle(sender as ToggleButton);
        }

        private void VesselsFilterSpaceObjects_Click(object sender, RoutedEventArgs e)
        {
            TabsManager.VesselsManager.Filter.SpaceObject = (sender as ToggleButton).IsChecked == true;
            TabsManager.VesselsManager.UpdateVisibility();
            SetToggleButtonStyle(sender as ToggleButton);
        }

        private void VesselsFilterStations_Click(object sender, RoutedEventArgs e)
        {
            TabsManager.VesselsManager.Filter.Station = (sender as ToggleButton).IsChecked == true;
            TabsManager.VesselsManager.UpdateVisibility();
            SetToggleButtonStyle(sender as ToggleButton);
        }

        private void VesselsFilterRovers_Click(object sender, RoutedEventArgs e)
        {
            TabsManager.VesselsManager.Filter.Rover = (sender as ToggleButton).IsChecked == true;
            TabsManager.VesselsManager.UpdateVisibility();
            SetToggleButtonStyle(sender as ToggleButton);
        }

        private void VesselsFilterOthers_Click(object sender, RoutedEventArgs e)
        {
            TabsManager.VesselsManager.Filter.Others = (sender as ToggleButton).IsChecked == true;
            TabsManager.VesselsManager.UpdateVisibility();
            SetToggleButtonStyle(sender as ToggleButton);
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }
    }
}
