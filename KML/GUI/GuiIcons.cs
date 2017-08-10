using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace KML
{
    /// <summary>
    /// GuiIcons ist just a collection of Image references
    /// used for dynamic creation of GuiTreeNodes, GuiKerbalsNodes or GuiVesselsNodes.
    /// </summary>
    class GuiIcons
    {
        /// <summary>
        /// The icon for menu items to add content.
        /// </summary>
        public Image Add = new Image();

        /// <summary>
        /// The icon for menu items to delete content.
        /// </summary>
        public Image Delete = new Image();

        /// <summary>
        /// The icon for items in the warnings list with type error.
        /// </summary>
        public Image Error = new Image();

        /// <summary>
        /// The icon for non-data but visual-only nodes in the KML tree.
        /// For example the fictive root node in a *.craft file
        /// </summary>
        public Image Ghost = new Image();

        /// <summary>
        /// The icon for kerbals in the kerbals list or the KML tree.
        /// Will be used for crew and unknown kerbal types.
        /// </summary>
        public Image Kerbal = new Image();

        /// <summary>
        /// The icon for kerbal applicants in the kerbals list or the KML tree.
        /// </summary>
        public Image KerbalApplicant = new Image();

        /// <summary>
        /// The icon for kerbal tourists in the kerbals list or the KML tree.
        /// </summary>
        public Image KerbalTourist = new Image();

        /// <summary>
        /// The additional icon for kerbal pilots in the kerbals list.
        /// </summary>
        public Image KerbalPilot = new Image();

        /// <summary>
        /// The additional icon for kerbal engineers in the kerbals list.
        /// </summary>
        public Image KerbalEngineer = new Image();

        /// <summary>
        /// The additional icon for kerbal scientists in the kerbals list.
        /// </summary>
        public Image KerbalScience = new Image();

        /// <summary>
        /// The additional icon for kerbal tourists in the kerbals list.
        /// Additional icon because tourist is a type and a trait.
        /// </summary>
        public Image KerbalCamera = new Image();

        /// <summary>
        /// The icon for any non-special item in the KML tree.
        /// </summary>
        public Image Node = new Image();

        /// <summary>
        /// The icon for a vessel part in the KML tree or the vessels list.
        /// </summary>
        public Image Part = new Image();

        /// <summary>
        /// The icon for a vessel docking part in the KML tree or the vessels list.
        /// </summary>
        public Image PartDock = new Image();

        /// <summary>
        /// The icon for a vessel grappling part in the KML tree or the vessels list.
        /// </summary>
        public Image PartGrapple = new Image();

        /// <summary>
        /// The icon for a vessel KAS CPort part in the KML tree or the vessels list.
        /// </summary>
        public Image PartKasCPort = new Image();

        /// <summary>
        /// The icon for part resources in the KML tree.
        /// Will be used only for context menu since the icon is replaced 
        /// by a progress bar to display the current amount.
        /// </summary>
        public Image Resource = new Image();

        /// <summary>
        /// The icon for vessels in the vessels list or the KML tree.
        /// Will be used for ships and unknown vessel types.
        /// </summary>
        public Image Vessel = new Image();

        /// <summary>
        /// The icon for base vessels in the vessels list or the KML tree.
        /// </summary>
        public Image VesselBase = new Image();

        /// <summary>
        /// The icon for debris vessels in the vessels list or the KML tree.
        /// </summary>
        public Image VesselDebris = new Image();

        /// <summary>
        /// The icon for eva vessels in the vessels list or the KML tree.
        /// </summary>
        public Image VesselEVA = new Image();

        /// <summary>
        /// The icon for flag vessels in the vessels list or the KML tree.
        /// </summary>
        public Image VesselFlag = new Image();

        /// <summary>
        /// The icon for lander vessels in the vessels list or the KML tree.
        /// </summary>
        public Image VesselLander = new Image();

        /// <summary>
        /// The icon for plane vessels in the vessels list or the KML tree.
        /// </summary>
        public Image VesselPlane= new Image();

        /// <summary>
        /// The icon for probe vessels in the vessels list or the KML tree.
        /// </summary>
        public Image VesselProbe = new Image();

        /// <summary>
        /// The icon for relay vessels in the vessels list or the KML tree.
        /// </summary>
        public Image VesselRelay = new Image();

        /// <summary>
        /// The icon for rover vessels in the vessels list or the KML tree.
        /// </summary>
        public Image VesselRover = new Image();

        /// <summary>
        /// The icon for space object vessels in the vessels list or the KML tree.
        /// </summary>
        public Image VesselSpaceObject = new Image();

        /// <summary>
        /// The icon for station vessels in the vessels list or the KML tree.
        /// </summary>
        public Image VesselStation = new Image();

        /// <summary>
        /// The icon for items in the warnings list with type warning.
        /// </summary>
        public Image Warning = new Image();

        /// <summary>
        /// Creates a copy of a given image.
        /// </summary>
        /// <param name="image">The Image to copy</param>
        /// <returns>A copy of the Image</returns>
        public Image CreateImage(Image image)
        {
            Image newImage = new Image();
            newImage.Source = image.Source;
            newImage.Height = image.Height;
            newImage.Width = image.Width;
            return newImage;
        }
    }
}
