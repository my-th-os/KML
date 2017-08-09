using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KML
{
    /// <summary>
    /// A GuiKerbalsFilter is used to filter for different kerbal types and traits.
    /// </summary>
    class GuiKerbalsFilter
    {
        /// <summary>
        /// Get or set whether type "Crew" is visible
        /// </summary>
        public bool Crew { get; set;}

        /// <summary>
        /// Get or set whether type "Applicant" is visible
        /// </summary>
        public bool Applicants { get; set; }

        /// <summary>
        /// Get or set whether type / trait "Tourist" is visible
        /// </summary>
        public bool Tourists { get; set; }

        /// <summary>
        /// Get or set whether trait "Pilot" is visible
        /// </summary>
        public bool Pilots { get; set; }

        /// <summary>
        /// Get or set whether trait "Engineeer" is visible
        /// </summary>
        public bool Engineeers { get; set; }

        /// <summary>
        /// Get or set whether trait "Scientist" is visible
        /// </summary>
        public bool Scientists { get; set; }

        /// <summary>
        /// Get or set whether kerbals are visible with type / trait unidentified
        /// </summary>
        public bool Others { get; set; }

        /// <summary>
        /// Creates a GuiKerbalsFilter. By default anything is visible.
        /// </summary>
        public GuiKerbalsFilter()
        {
            SetAll(true);
        }

        /// <summary>
        /// Copies a GuiKerbalsFilter. 
        /// </summary>
        public GuiKerbalsFilter(GuiKerbalsFilter copyFrom)
        {
            Crew = copyFrom.Crew;
            Applicants = copyFrom.Applicants;
            Tourists = copyFrom.Tourists;
            Pilots = copyFrom.Pilots;
            Engineeers = copyFrom.Engineeers;
            Scientists = copyFrom.Scientists;
            Others = copyFrom.Others;
        }

        /// <summary>
        /// Compares equality with other GuiKerbalsFilter. 
        /// </summary>
        public bool Equals(GuiKerbalsFilter other)
        {
            return (Crew == other.Crew &&
                Applicants == other.Applicants &&
                Tourists == other.Tourists &&
                Pilots == other.Pilots &&
                Engineeers == other.Engineeers &&
                Scientists == other.Scientists &&
                Others == other.Others);
        }

        /// <summary>
        /// Sets all filter settings to a given value.
        /// </summary>
        /// <param name="value">The bool value to apply to all settings</param>
        public void SetAll(bool value)
        {
            SetAllType(value);
            SetAllTrait(value);
        }

        /// <summary>
        /// Sets all type filter settings to a given value.
        /// </summary>
        /// <param name="value">The bool value to apply to all settings</param>
        public void SetAllType(bool value)
        {
            Crew = value;
            Applicants = value;
            Tourists = value;
            Others = value;
        }

        /// <summary>
        /// Sets all trait filter settings to a given value.
        /// </summary>
        /// <param name="value">The bool value to apply to all settings</param>
        public void SetAllTrait(bool value)
        {
            Tourists = value;
            Pilots = value;
            Engineeers = value;
            Scientists = value;
        }
    }
}
