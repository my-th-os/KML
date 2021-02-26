namespace KML
{
    /// <summary>
    /// This items represents a data line wich reads "}".
    /// This closes the reading of the current node and continues
    /// reading data at same or higher level.
    /// A KmlEnd is not stored as a child of its parent node.
    /// To write the line with "}" is part of writing a KmlNode item.
    /// </summary>
    public class KmlEnd : KmlItem
    {
        /// <summary>
        /// Creates a KmlEnd with a line read from data file.
        /// </summary>
        /// <param name="line">String with only one line from data file</param>
        public KmlEnd(string line)
            : base(line)
        {
        }

        /// <summary>
        /// Creates a standard KmlEnd to be written.
        /// </summary>
        public KmlEnd()
            : base("}")
        {
        }
    }
}
