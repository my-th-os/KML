using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KML
{
    /// <summary>
    /// This items represents a data line wich reads "{".
    /// Finding this identifies the previous line as a KmlNode.
    /// Following data will be reading recursive and stored als children
    /// to that KmlNode until a "}" is found.
    /// A KmlBegin is not stored as a child of its parent node.
    /// To write the line with "{" is part of writing a KmlNode item.
    /// </summary>
    public class KmlBegin : KmlItem
    {
        /// <summary>
        /// Creates a KmlBegin with a line read from data file.
        /// </summary>
        /// <param name="line">String with only one line from data file</param>
        public KmlBegin(string line)
            : base(line)
        {
        }

        /// <summary>
        /// Creates a standard KmlBegin to be written.
        /// </summary>
        public KmlBegin()
            : base("{")
        {
        }
    }
}
