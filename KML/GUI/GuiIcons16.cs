using System;
using System.Windows.Media.Imaging;

namespace KML
{
    /// <summary>
    /// GuiIcons16 ist just a collection of Image references
    /// used for dynamic creation of GuiTreeNodes.
    /// These are preset with 16x16 pixel images from project resource.
    /// </summary>
    class GuiIcons16 : GuiIcons
    {
        /// <summary>
        /// Load default Icons for GuiTreeNodes
        /// </summary>
        public GuiIcons16()
        {
            Add.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Add16.png"));
            Clipboard.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Clipboard16.png"));
            Delete.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Delete16.png"));
            Down.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Down16.png"));
            Error.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Error16.png"));
            Ghost.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Document16.png"));
            Kerbal.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Astronaut16.png"));
            KerbalApplicant.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Student16.png"));
            KerbalTourist.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Photographer16.png"));
            KerbalPilot.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/ApolloCsm16.png"));
            KerbalEngineer.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Wrench16.png"));
            KerbalScience.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Science16.png"));
            KerbalCamera.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Camera16.png"));
            Node.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Point16.png"));
            Paste.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Paste16.png"));
            Part.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Box16.png"));
            PartDock.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Port16.png"));
            PartGrapple.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/GrapplingHook16.png"));
            PartKasCPort.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/KasCPort16.png"));
            Resource.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Battery16.png"));
            Up.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Up16.png"));
            Vessel.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/ApolloCsm16.png"));
            VesselBase.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Base16.png"));
            VesselDebris.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Trash16.png"));
            VesselEVA.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Astronaut16.png"));
            VesselFlag.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Flag16.png"));
            VesselLander.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/LunarModule16.png"));
            VesselPlane.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Plane16.png"));
            VesselProbe.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Satellite16.png"));
            VesselRelay.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Radar16.png"));
            VesselRover.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Rover16.png"));
            VesselSpaceObject.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/GlobeGray16.png"));
            VesselStation.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Station16.png"));
            Warning.Source = new BitmapImage(new Uri("pack://application:,,,/KML;component/Images/Warning16.png"));
        }
    }
}
