using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace KML
{
    /// <summary>
    /// GuiTreeIcons ist just a collection of Image references
    /// used for dynamic creation of GuiTreeNodes.
    /// These are preset with 48x48 pixel images from project resource.
    /// </summary>
    class GuiIcons48 : GuiIcons
    {
        /// <summary>
        /// Load default Icons for GuiTreeNodes
        /// </summary>
        public GuiIcons48()
        {
            Add.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Add48.png"));
            Clipboard.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Clipboard48.png"));
            Delete.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Delete48.png"));
            Error.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Error48.png"));
            Ghost.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Document48.png"));
            Kerbal.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Astronaut48.png"));
            KerbalApplicant.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Student48.png"));
            KerbalTourist.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Photographer48.png"));
            KerbalPilot.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/ApolloCsm48.png"));
            KerbalEngineer.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Wrench48.png"));
            KerbalScience.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Science48.png"));
            KerbalCamera.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Camera48.png"));
            Node.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Point16.png")); // TODO GuiIcons48.GuiIcons48(): Find icon Point48.png
            Part.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Box48.png"));
            PartDock.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Port48.png"));
            PartGrapple.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/GrapplingHook48.png"));
            PartKasCPort.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/KasCPort48.png"));
            Paste.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Paste48.png"));
            Resource.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Battery48.png"));
            Vessel.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/ApolloCsm48.png"));
            VesselBase.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Base48.png"));
            VesselDebris.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Trash48.png"));
            VesselEVA.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Astronaut48.png"));
            VesselFlag.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Flag48.png"));
            VesselLander.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/LunarModule48.png"));
            VesselPlane.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Plane48.png"));
            VesselProbe.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Satellite48.png"));
            VesselRelay.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Radar48.png"));
            VesselRover.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Rover48.png"));
            VesselSpaceObject.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/GlobeGray48.png"));
            VesselStation.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Station48.png"));
            Warning.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Warning48.png"));
        }
    }
}
