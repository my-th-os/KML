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
using System.Windows.Media.Imaging;

namespace KML
{
    /// <summary>
    /// A GuiVesselsNode is a specialized TreeViewItem to display
    /// and contain a KmlVessel.
    /// </summary>
    class GuiVesselsNode : ListViewItem
    {
        /// <summary>
        /// Get the contained KmlKerbal from this GuiKerbalsNode.
        /// </summary>
        public KmlVessel DataVessel
        {
            get
            {
                return (KmlVessel)DataContext;
            }
            private set
            {
                DataContext = value;
            }
        }

        private static GuiIcons Icons = new GuiIcons48();

        /// <summary>
        /// Creates a GuiVesselsNode containing the given DataVessel.
        /// To have nice icons in the tree, a GuiIcons can be
        /// provided.
        /// </summary>
        /// <param name="dataVessel">The KmlVessel to contain</param>
        public GuiVesselsNode(KmlVessel dataVessel)
        {
            DataVessel = dataVessel;

            AssignTemplate();
            BuildContextMenu();

            // Get notified when KmlNode ToString() changes
            DataVessel.ToStringChanged += DataVessel_ToStringChanged;

            // Get notified when flag changes
            if (DataVessel.RootPart != null)
            {
                KmlAttrib flag = DataVessel.RootPart.GetAttrib("flag");
                if (flag != null)
                {
                    flag.AttribValueChanged += DataVessel_FlagChanged;
                }
            }
        }

        private void AssignTemplate()
        {
            // Fit an Image and a TextBlock into a Stackpanel,
            // have the Stackpanel as node Header

            StackPanel pan = new StackPanel();
            pan.Orientation = Orientation.Horizontal;
            pan.Children.Add(GenerateImage(DataVessel));
            pan.Children.Add(GenerateFlag(DataVessel));
            pan.Children.Add(GenerateText(DataVessel));
            Content = pan;
        }

        private void BuildContextMenu()
        {
            // Copy that from a GuiTreeNode
            GuiTreeNode dummy = new GuiTreeNode(DataVessel, true, true, true, false, true, false);
            ContextMenu = dummy.ContextMenu;
            // To avoid follwing error output (uncritical), we need to have a parent for the TreeViewItem, so we also make a dummy
            // System.Windows.Data Error: 4 : Cannot find source for binding with reference 'RelativeSource FindAncestor, AncestorType='System.Windows.Controls.ItemsControl', AncestorLevel='1''. BindingExpression:Path=HorizontalContentAlignment; DataItem=null; target element is 'GuiTreeNode' (Name=''); target property is 'HorizontalContentAlignment' (type 'HorizontalAlignment')
            // System.Windows.Data Error: 4 : Cannot find source for binding with reference 'RelativeSource FindAncestor, AncestorType='System.Windows.Controls.ItemsControl', AncestorLevel='1''. BindingExpression:Path=VerticalContentAlignment; DataItem=null; target element is 'GuiTreeNode' (Name=''); target property is 'VerticalContentAlignment' (type 'VerticalAlignment')
            TreeView dummyTree = new TreeView();
            dummyTree.Items.Add(dummy);
        }

        private Image GenerateImage(KmlVessel vessel)
        {
            Image image = new Image();
            image.Height = 48;
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
            else if (vessel.Type.ToLower() == "rover")
            {
                image.Source = Icons.VesselRover.Source;
            }
            else if (vessel.Type.ToLower() == "spaceobject")
            {
                image.Source = Icons.VesselSpaceObject.Source;
            }
            else if (vessel.Type.ToLower() == "station")
            {
                image.Source = Icons.VesselStation.Source;
            }
            else
            {
                image.Source = Icons.Vessel.Source;
            }
            image.Margin = new Thickness(0, 0, 3, 0);

            return image;
        }

        private Image GenerateFlag(KmlVessel vessel)
        {
            Image image = new Image();
            image.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/DummyFlag.png"));
            image.Height = 40;
            image.Width = image.Height * 1.6;
            image.Margin = new Thickness(0, 0, 3, 0);
            if (vessel.RootPart != null && vessel.RootPart.Flag.Length > 0)
            {
                string flag = vessel.RootPart.Flag;
                flag = flag.Replace('/', '\\');
                flag = Path.Combine(GuiTabsManager.GetCurrent().FileGamedataDirectory, flag);

                flag = Path.ChangeExtension(flag, ".png");
                if (!File.Exists(flag))
                {
                    flag = Path.ChangeExtension(flag, ".dds");
                    if (!File.Exists(flag))
                    {
                        // keep dummy image
                        return image; 
                    }
                    else
                    {
                        // *.dds files are drawn vertically flipped
                        image.RenderTransform = new ScaleTransform(1.0, -1.0, 0.0, image.Height / 2.0);
                    }
                }
                // flag points to existing file here
                try
                {
                    image.Source = (new ImageSourceConverter()).ConvertFromString(flag) as ImageSource;
                }
                catch
                {
                    // there could be any problem loading the flag, ignore it and stay with dummy
                    // one problem so far: Win 10 can load *.dds, Win 7 can't
                }
            }

            return image;
        }

        private TextBlock GenerateText(KmlVessel vessel)
        {
            TextBlock text = new TextBlock();
            text.Inlines.Add(new Bold(new Run(vessel.Name)));
            text.Inlines.Add(new Run("\n" + vessel.Type + "\n" + vessel.Situation));
            text.Margin = new Thickness(3, 0, 0, 0);
            return text;
        }

        private void DataVessel_ToStringChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            AssignTemplate();
        }

        private void DataVessel_FlagChanged(object sender, RoutedEventArgs e)
        {
            AssignTemplate();
        }
    }
}
