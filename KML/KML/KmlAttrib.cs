using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KML
{
    /// <summary>
    /// A KmlAttrib represents a line that reads "Name = Value".
    /// Name and value are extracted from the given line.
    /// Such an attribute is usually a child of a KmlNode.
    /// </summary>
    public class KmlAttrib : KmlItem
    {
        /// <summary>
        /// Get the attribute's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Get or set the attribute's value.
        /// Changed value will be considered when building a new line to 
        /// be written to data file.
        /// </summary>
        public string Value 
        {
            get
            {
                return _value;
            }
            set
            {
                if (value != _value)
                {
                    _value = value;
                    foreach (RoutedEventHandler e in AttribValueChangedList)
                    {
                        e.Invoke(this, new RoutedEventArgs());
                    }
                }
            }
        }
        private string _value;

        /// <summary>
        /// Event is raised when attribute value is changed.
        /// </summary>
        public event RoutedEventHandler AttribValueChanged
        {
            add { AttribValueChangedList.Add(value); }
            remove { AttribValueChangedList.Remove(value); }
        }

        private List<RoutedEventHandler> AttribValueChangedList = new List<RoutedEventHandler>();

        /// <summary>
        /// Creates a KmlAttrib with a line read from data file.
        /// That line is parsed into name and value.
        /// </summary>
        /// <param name="line">String with only one line from data file</param>
        public KmlAttrib(string line)
            : base(line)
        {
            string s = line.Trim();
            int p = s.IndexOf('=');

            if (p < 0)
            {
                Name = s;
                Value = "";
            }
            else
            {
                Name = s.Substring(0, p).Trim();
                Value = s.Substring(p + 1, s.Length - p - 1).Trim();
            }
        }

        /// <summary>
        /// Generates a nice informative string to be used in display for this item.
        /// </summary>
        /// <returns>A string to display this item</returns>
        public override string ToString()
        {
            return Name + " = " + Value;
        }
    }
}
