using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KML
{
    /// <summary>
    /// KmlItem represents any Item in KSP data structure, similar to XML.
    /// Any item that couldn't be identified (and therefor was generated as 
    /// an instance of a class, that's derived from KmlItem) is just a KmlItem.
    /// </summary>
    public class KmlItem
    {
        /// <summary>
        /// Get the line that was read from data file.
        /// </summary>
        public string Line { get; private set; }

        /// <summary>
        /// Get or set whether deletion of this item is allowed.
        /// </summary>
        public bool CanBeDeleted { get; set; }

        /// <summary>
        /// Get this item's parent node or null if there is none.
        /// </summary>
        public KmlNode Parent { get; private set; }

        /// <summary>
        /// Creates a KmlItem with a line read from data file.
        /// </summary>
        /// <param name="line">String with only one line from data file</param>
        public KmlItem(string line)
        {
            this.Line = line;

            // Parent will be set within parent's Add method
            Parent = null;

            // Default is to allow any item deletion.
            // Sepcial ones, used for class properties will be protected.
            CanBeDeleted = true;
        }

        /// <summary>
        /// Delete this node from its parent.
        /// Result will be false if item was not in parent's lists or couldn't be deleted
        /// because of restrictions.
        /// </summary>
        /// <returns>True if item was deleted, false otherwise</returns>
        public bool Delete()
        {
            if (!CanBeDeleted)
            {
                return false;
            }
            BeforeDelete();
            if (Parent != null)
            {
                return Parent.Delete(this);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets called before item is deleted.
        /// Deriving classes can perform needed actions here.
        /// </summary>
        protected virtual void BeforeDelete()
        {
        }

        /// <summary>
        /// Generates a line to be written to data file from (changed) properties.
        /// </summary>
        /// <returns>A string with one line representing this item</returns>
        public virtual string ToLine()
        {
            return ToString();
        }

        /// <summary>
        /// Generates a line to be written with given indent (tab amount).
        /// <see cref="KML.KmlItem.ToLine()"/>
        /// </summary>
        /// <param name="indent">Integer amount of tabs to indent this line</param>
        /// <returns>A string with one line representing this item</returns>
        public string ToLine(int indent)
        {
            string tab = "";
            for (int i = 0; i < indent; i++)
            {
                tab += "\t";
            }
            return tab + ToLine();
        }

        /// <summary>
        /// Generates a nice informative string to be used in display for this item.
        /// </summary>
        /// <returns>A string to display this item</returns>
        public override string ToString()
        {
            return Line.Trim();
        }

        /// <summary>
        /// Generates a string to represent to path from root to here, incl. this item.
        /// </summary>
        /// <param name="separator">The separator to combine parent and child with</param>
        /// <returns>A string to display the path</returns>
        public string PathToString(string separator)
        {
            if (Parent != null)
            {
                return Parent.PathToString(separator) + separator + ToString();
            }
            else
            {
                return ToString();
            }
        }

        /// <summary>
        /// Creates a KmlItem from a string.
        /// This is used for adding items outside of loading from file.
        /// Result will be a KmlAttrib, KmlNode, KmlVessel, etc. or null 
        /// if line is "{", "}" or empty
        /// </summary>
        /// <param name="line">The line to build a KmlItem from</param>
        /// <returns>A KmlItem derived object</returns>
        public static KmlItem CreateItem (string line)
        {
            if (line == null || line.Length == 0)
            {
                return null;
            }
            else if (line.IndexOf('{') >= 0)
            {
                return null;
            }
            else if (line.IndexOf('}') >= 0)
            {
                return null;
            }

            KmlItem item = ParseLine(line);
            if (item is KmlAttrib)
            {
                return item;
            }
            else
            {
                // It's a node
                return ParseNode(item);
            }
        }

        /// <summary>
        /// Changes the parent. Needed when a node is changed to a derived class 
        /// and all existing children need to have a new parent.
        /// </summary>
        /// <param name="item">The KmlItem where parent needs to be changed</param>
        /// <param name="parent">The new parent KmlNode</param>
        protected static void RemapParent(KmlItem item, KmlNode parent)
        {
            if (item.Parent != parent)
            {
                // This often happens when we iterate through the lists,
                // so we can not delete items from that list here.
                // TODO KmlItem.RemapParent(): Delete item from old parent
                // item.Delete(); // from previous parent
                item.Parent = parent;
                item.IdentifyParent();
            }
        }

        private static KmlItem ParseLine (string line)
        {
            string s = line.Trim();

            if (s.IndexOf('=') >= 0)
            {
                return new KmlAttrib(line);
            }
            else if (s.Length == 1 && s[0] == '{')
            {
                return new KmlBegin(line);
            }
            else if (s.Length == 1 && s[0] == '}')
            {
                return new KmlEnd(line);
            }
            else
            {
                return new KmlItem(line);
            }
        }

        private static KmlNode ParseNode(KmlItem item)
        {
            KmlNode newNode = new KmlNode(item);
            if (newNode.Tag.ToLower() == "vessel")
            {
                newNode = new KmlVessel(newNode);
            }
            else if (newNode.Tag.ToLower() == "kerbal")
            {
                newNode = new KmlKerbal(newNode);
            }
            else if (newNode.Tag.ToLower() == "part")
            {
                newNode = new KmlPart(newNode);
            }
            else if (newNode.Tag.ToLower() == "resource")
            {
                newNode = new KmlResource(newNode);
            }
            return newNode;
        }

        private static List<KmlItem> ParseFile(System.IO.StreamReader file)
        {
            List<KmlItem> list = new List<KmlItem>();

            string line;
            while ((line = file.ReadLine()) != null)
            {
                KmlItem newItem = ParseLine(line);
                if (newItem is KmlBegin)
                {
                    KmlItem lastItem;
                    int l = list.Count - 1;
                    if (l < 0)
                    {
                        lastItem = new KmlItem("");
                    }
                    else
                    {
                        lastItem = list[l];
                        list.RemoveAt(l);
                    }
                    KmlNode newNode = ParseNode(lastItem);
                    list.Add(newNode);
                    newNode.AddRange(ParseFile(file));
                }
                else if (newItem is KmlEnd)
                {
                    Identify(list);
                    return list;
                }
                else
                {
                    list.Add(newItem);
                }
            }

            Identify(list);
            return list;
        }

        private static void Identify(List<KmlItem> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                KmlItem item = list[i];
                KmlItem replaceItem = item.Identify();
                if (replaceItem != null)
                {
                    list[i] = replaceItem;
                }
            }
        }

        /// <summary>
        /// After an item is completely loaded the Intentify() method is called.
        /// If this returns a KmlItem, the loaded one is replaced by this.
        /// If it doesen't need to be replaced, null should be returned.
        /// This way classes cann derive from any KmlItem where decision whether this class
        /// should be used or the base class can only be made when loading is done.
        /// </summary>
        /// <returns>A KmlItem this one needs to be replaced by or null otherwise</returns>
        protected virtual KmlItem Identify()
        {
            // Nothing to do by default. Override this in derived classes, that need identification
            return null;
        }

        /// <summary>
        /// When Parent is set or changed IdentifyParent will be called.
        /// Deriving classes can override this method and check for the new parent.
        /// </summary>
        protected virtual void IdentifyParent()
        {
        }

        /// <summary>
        /// After all items are loaded, each items Finalize is called.
        /// The roots list will contain all loaded items in KML tree structure.
        /// Each item can then check for other items to get further properties.
        /// </summary>
        /// <param name="roots">The loaded root items list</param>
        protected virtual void Finalize(List<KmlItem> roots)
        {
        }

        private static void CallFinalize(List<KmlItem> roots)
        {
            CallFinalize(roots, roots);
        }

        private static void CallFinalize(List<KmlItem> roots, List<KmlItem> subItems)
        {
            foreach (KmlItem item in subItems)
            {
                item.Finalize(roots);
                if (item is KmlNode)
                {
                    CallFinalize(roots, (item as KmlNode).AllItems);
                }
            }
        }

        /// <summary>
        /// Parse a KSP persistence file and return a list of the root nodes.
        /// In general there may be more than one item not containing other items.
        /// With correct KSP persistence data this doesn't happen and the list will
        /// usually contain just one item.
        /// </summary>
        /// <param name="filename">The full path and filename of the data file to read</param>
        /// <returns>A list of root / top level KmlItems</returns>
        public static List<KmlItem> ParseFile (string filename)
        {
            List<KmlItem> list = new List<KmlItem>();

            try
            {
                // Explicit setting UTF8 doesn't look the same, if I compare loaded and saved with MinMerge
                // System.IO.StreamReader file = new System.IO.StreamReader(Filename, Encoding.UTF8);
                System.IO.StreamReader file = new System.IO.StreamReader(filename);
                list.AddRange(ParseFile(file));
                file.Close();
                CallFinalize(list);
            }
            catch (Exception e)
            {
                DlgMessage.Show("Error loading from " + filename + "\n\n" + e.Message);
            }

            return list;
        }

        private static void WriteFile (System.IO.StreamWriter file, KmlItem item, int indent)
        {
            bool ghost = item is KmlGhostNode;
            if (!ghost)
            {
                file.WriteLine(item.ToLine(indent));
            }
            if (item is KmlNode)
            {
                int newIndent = indent;
                KmlNode node = (KmlNode)item;
                if (!ghost)
                {
                    file.WriteLine(new KmlBegin().ToLine(indent));
                    newIndent = indent + 1;
                }
                foreach(KmlItem child in node.AllItems)
                {
                    WriteFile(file, child, newIndent);
                }
                if (!ghost)
                {
                    file.WriteLine(new KmlEnd().ToLine(indent));
                }
            }
        }

        /// <summary>
        /// Write the given list of root items to a data file.
        /// Child items of these roots a written recursively.
        /// If a file with given name already exists, it is renamed for backup
        /// to "zKMLBACKUP&lt;timestamp&gt;-&lt;filename&gt;" in same directory.
        /// </summary>
        /// <param name="filename">The full path and filename of the data file to write</param>
        /// <param name="items">A list of root / top level KmlItems</param>
        public static void WriteFile (string filename, List<KmlItem> items)
        {
            try
            {
                string Backupname = "";
                if (System.IO.File.Exists(filename))
                {
                    string dir = System.IO.Path.GetDirectoryName(filename) + @"\";
                    string name = System.IO.Path.GetFileNameWithoutExtension(filename);
                    string ext = System.IO.Path.GetExtension(filename);
                    string timestamp = string.Format("{0:yyyyMMddHHmmss}", DateTime.Now);
                    Backupname = dir + "zKMLBACKUP" + timestamp + "-" + name + ext;
                    System.IO.File.Move(filename, Backupname);
                }

                // Explicit setting UTF8 doesn't look the same, if I compare loaded and saved with MinMerge
                // System.IO.StreamWriter file = new System.IO.StreamWriter(Filename, false, Encoding.UTF8);
                System.IO.StreamWriter file = new System.IO.StreamWriter(filename);
                try
                {
                    foreach (KmlItem item in items)
                    {
                        WriteFile(file, item, 0);
                    }
                    file.Close();
                }
                catch (Exception e)
                {
                    file.Close();
                    if (Backupname.Length > 0)
                    {
                        System.IO.File.Delete(filename);
                        System.IO.File.Move(Backupname, filename);
                    }
                    throw e;
                }
            }
            catch (Exception e)
            {
                DlgMessage.Show("Error saving to " + filename + "\n\n" + e.Message);
            }
        }
    }
}
