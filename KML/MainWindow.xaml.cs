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

        // TODO MainWindow.KerbalsFilter... Capsulate filter handling

        private void KerbalsFilter_Click(object sender, RoutedEventArgs e)
        {
            KerbalsFilterSetProperty(sender, (sender as ToggleButton).IsChecked == true);
            SetToggleButtonStyle(sender as ToggleButton);
            TabsManager.KerbalsManager.UpdateVisibility();
        }

        private void KerbalsFilterType_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TabsManager.KerbalsManager.Filter.SetAllType(false);
            KerbalsFilterSetProperty(sender, true);
            KerbalsFilterUpdateAll(sender);
            TabsManager.KerbalsManager.UpdateVisibility();
        }

        private void KerbalsFilterTrait_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TabsManager.KerbalsManager.Filter.SetAllTrait(false);
            KerbalsFilterSetProperty(sender, true);
            KerbalsFilterUpdateAll(sender);
            TabsManager.KerbalsManager.UpdateVisibility();
        }

        private void KerbalsFilterUpdateAll(object sender)
        {
            if (sender is ToggleButton)
            {
                foreach (var c in (((ToggleButton)sender).Parent as StackPanel).Children)
                {
                    if (c is ToggleButton)
                    {
                        ToggleButton b = (ToggleButton)c;
                        b.IsChecked = KerbalsFilterGetProperty(b);
                        SetToggleButtonStyle(b);
                    }
                }
            }
        }

        private bool KerbalsFilterGetProperty(object sender)
        {
            if (sender is ToggleButton)
            {
                ToggleButton b = (ToggleButton)sender;
                if (b.ToolTip.ToString() == "Toggle Crew") return TabsManager.KerbalsManager.Filter.Crew;
                else if (b.ToolTip.ToString() == "Toggle Applicants") return TabsManager.KerbalsManager.Filter.Applicants;
                else if (b.ToolTip.ToString() == "Toggle Tourists") return TabsManager.KerbalsManager.Filter.Tourists;
                else if (b.ToolTip.ToString() == "Toggle Pilots") return TabsManager.KerbalsManager.Filter.Pilots;
                else if (b.ToolTip.ToString() == "Toggle Engineeers") return TabsManager.KerbalsManager.Filter.Engineeers;
                else if (b.ToolTip.ToString() == "Toggle Scientists") return TabsManager.KerbalsManager.Filter.Scientists;
                else if (b.ToolTip.ToString() == "Toggle Others") return TabsManager.KerbalsManager.Filter.Others;
            }
            return false;
        }

        private void KerbalsFilterSetProperty(object sender, bool value)
        {
            if (sender is ToggleButton)
            {
                ToggleButton b = (ToggleButton)sender;
                if (b.ToolTip.ToString() == "Toggle Crew") TabsManager.KerbalsManager.Filter.Crew = value;
                else if (b.ToolTip.ToString() == "Toggle Applicants") TabsManager.KerbalsManager.Filter.Applicants = value;
                else if (b.ToolTip.ToString() == "Toggle Tourists") TabsManager.KerbalsManager.Filter.Tourists = value;
                else if (b.ToolTip.ToString() == "Toggle Pilots") TabsManager.KerbalsManager.Filter.Pilots = value;
                else if (b.ToolTip.ToString() == "Toggle Engineeers") TabsManager.KerbalsManager.Filter.Engineeers = value;
                else if (b.ToolTip.ToString() == "Toggle Scientists") TabsManager.KerbalsManager.Filter.Scientists = value;
                else if (b.ToolTip.ToString() == "Toggle Others") TabsManager.KerbalsManager.Filter.Others = value;
            }
        }

        // TODO MainWindow.VesselsFilter... Capsulate filter handling

        private void VesselsFilter_Click(object sender, RoutedEventArgs e)
        {
            VesselsFilterSetProperty(sender, (sender as ToggleButton).IsChecked == true);
            SetToggleButtonStyle(sender as ToggleButton);
            TabsManager.VesselsManager.UpdateVisibility();
        }

        private void VesselsFilter_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TabsManager.VesselsManager.Filter.SetAll(false);
            VesselsFilterSetProperty(sender, true);
            VesselsFilterUpdateAll(sender);
            TabsManager.VesselsManager.UpdateVisibility();
        }

        private void VesselsFilterUpdateAll(object sender)
        {
            if (sender is ToggleButton)
            {
                foreach (var c in (((ToggleButton)sender).Parent as StackPanel).Children)
                {
                    if (c is ToggleButton)
                    {
                        ToggleButton b = (ToggleButton)c;
                        b.IsChecked = VesselsFilterGetProperty(b);
                        SetToggleButtonStyle(b);
                    }
                }
            }
        }

        private bool VesselsFilterGetProperty(object sender)
        {
            if (sender is ToggleButton)
            {
                ToggleButton b = (ToggleButton)sender;
                if (b.ToolTip.ToString() == "Toggle Debris") return TabsManager.VesselsManager.Filter.Debris;
                else if (b.ToolTip.ToString() == "Toggle Probes") return TabsManager.VesselsManager.Filter.Probe;
                else if (b.ToolTip.ToString() == "Toggle Rovers") return TabsManager.VesselsManager.Filter.Rover;
                else if (b.ToolTip.ToString() == "Toggle Landers") return TabsManager.VesselsManager.Filter.Lander;
                else if (b.ToolTip.ToString() == "Toggle Ships") return TabsManager.VesselsManager.Filter.Ships;
                else if (b.ToolTip.ToString() == "Toggle Stations") return TabsManager.VesselsManager.Filter.Station;
                else if (b.ToolTip.ToString() == "Toggle Bases") return TabsManager.VesselsManager.Filter.Base;
                else if (b.ToolTip.ToString() == "Toggle Planes") return TabsManager.VesselsManager.Filter.Plane;
                else if (b.ToolTip.ToString() == "Toggle Relays") return TabsManager.VesselsManager.Filter.Relay;
                else if (b.ToolTip.ToString() == "Toggle EVA") return TabsManager.VesselsManager.Filter.EVA;
                else if (b.ToolTip.ToString() == "Toggle Flags") return TabsManager.VesselsManager.Filter.Flag;
                else if (b.ToolTip.ToString() == "Toggle SpaceObjects") return TabsManager.VesselsManager.Filter.SpaceObject;
                else if (b.ToolTip.ToString() == "Toggle Others") return TabsManager.VesselsManager.Filter.Others;
            }
            return false;
        }

        private void VesselsFilterSetProperty(object sender, bool value)
        {
            if (sender is ToggleButton)
            {
                ToggleButton b = (ToggleButton)sender;
                if (b.ToolTip.ToString() == "Toggle Debris") TabsManager.VesselsManager.Filter.Debris = value;
                else if (b.ToolTip.ToString() == "Toggle Probes") TabsManager.VesselsManager.Filter.Probe = value;
                else if (b.ToolTip.ToString() == "Toggle Rovers") TabsManager.VesselsManager.Filter.Rover = value;
                else if (b.ToolTip.ToString() == "Toggle Landers") TabsManager.VesselsManager.Filter.Lander = value;
                else if (b.ToolTip.ToString() == "Toggle Ships") TabsManager.VesselsManager.Filter.Ships = value;
                else if (b.ToolTip.ToString() == "Toggle Stations") TabsManager.VesselsManager.Filter.Station = value;
                else if (b.ToolTip.ToString() == "Toggle Bases") TabsManager.VesselsManager.Filter.Base = value;
                else if (b.ToolTip.ToString() == "Toggle Planes") TabsManager.VesselsManager.Filter.Plane = value;
                else if (b.ToolTip.ToString() == "Toggle Relays") TabsManager.VesselsManager.Filter.Relay = value;
                else if (b.ToolTip.ToString() == "Toggle EVA") TabsManager.VesselsManager.Filter.EVA = value;
                else if (b.ToolTip.ToString() == "Toggle Flags") TabsManager.VesselsManager.Filter.Flag = value;
                else if (b.ToolTip.ToString() == "Toggle SpaceObjects") TabsManager.VesselsManager.Filter.SpaceObject = value;
                else if (b.ToolTip.ToString() == "Toggle Others") TabsManager.VesselsManager.Filter.Others = value;
            }
        }
    }
}
