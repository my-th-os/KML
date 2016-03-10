using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

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
        }

        private void AssignTemplate()
        {
            // Fit an Image and a TextBlock into a Stackpanel,
            // have the Stackpanel as node Header

            StackPanel pan = new StackPanel();
            pan.Orientation = Orientation.Horizontal;
            pan.Children.Add(GenerateImage(DataVessel));
            pan.Children.Add(new TextBlock(new Run(GenerateText(DataVessel))));
            Content = pan;
        }

        private void BuildContextMenu()
        {
            // Copy that from a GuiTreeNode
            GuiTreeNode dummy = new GuiTreeNode(DataVessel);
            ContextMenu = dummy.ContextMenu;
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

        private string GenerateText(KmlVessel vessel)
        {
            return vessel.Name + "\n " + vessel.Type + "\n " + vessel.Situation;
        }

        private void DataVessel_ToStringChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            AssignTemplate();
        }
    }
}
