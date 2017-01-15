using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KML
{
    /// <summary>
    /// A GuiVesselsFilter is used to filter for different vessel types.
    /// </summary>
    class GuiVesselsFilter
    {
        /// <summary>
        /// Get or set whether type "Base" is visible
        /// </summary>
        public bool Base { get; set; }

        /// <summary>
        /// Get or set whether type "Debris" is visible
        /// </summary>
        public bool Debris { get; set; }

        /// <summary>
        /// Get or set whether type "EVA" is visible
        /// </summary>
        public bool EVA { get; set; }

        /// <summary>
        /// Get or set whether type "Flag" is visible
        /// </summary>
        public bool Flag { get; set; }

        /// <summary>
        /// Get or set whether type "Lander" is visible
        /// </summary>
        public bool Lander { get; set; }

        /// <summary>
        /// Get or set whether type "Plane" is visible
        /// </summary>
        public bool Plane { get; set; }

        /// <summary>
        /// Get or set whether type "Probe" is visible
        /// </summary>
        public bool Probe { get; set; }

        /// <summary>
        /// Get or set whether type "Relay" is visible
        /// </summary>
        public bool Relay { get; set; }

        /// <summary>
        /// Get or set whether type "Rover" is visible
        /// </summary>
        public bool Rover { get; set; }

        /// <summary>
        /// Get or set whether type "Ship" is visible
        /// </summary>
        public bool Ships { get; set; }

        /// <summary>
        /// Get or set whether type "SpaceObject" is visible
        /// </summary>
        public bool SpaceObject { get; set; }

        /// <summary>
        /// Get or set whether type "Station" is visible
        /// </summary>
        public bool Station { get; set; }

        /// <summary>
        /// Get or set whether vessels are visible with type unidentified
        /// </summary>
        public bool Others { get; set; }

        /// <summary>
        /// Creates a GuiVesselsFilter. By default anything is visible.
        /// </summary>
        public GuiVesselsFilter()
        {
            SetAll(true);
        }

        /// <summary>
        /// Sets all filter settings to a given value.
        /// </summary>
        /// <param name="value">The bool value to apply to all settings</param>
        public void SetAll(bool value)
        {
            Base = value;
            Debris = value;
            EVA = value;
            Flag = value;
            Lander = value;
            Plane = value;
            Probe = value;
            Relay = value;
            Rover = value;
            Ships = value;
            SpaceObject = value;
            Station = value;
            Others = value;
        }
    }
}
