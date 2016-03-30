using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace KML
{
    /// <summary>
    /// KmlNode represents any item in KML structure that is a node, wich is
    /// followed by opening "{", a list of child items and a closing "}".
    /// </summary>
    public class KmlNode : KmlItem
    {
        /// <summary>
        /// The tag of this node similar to a XML tag.
        /// </summary>
        public string Tag { get; private set; }

        /// <summary>
        /// The name of this node which is read from a child KmlAttrib
        /// with the name "Name". Reading this KmlAttrib happens when its
        /// added to the children.
        /// </summary>
        public string Name { get; private set; }

        // TODO KmlNode: Not make lists public, better have a Add(method)

        /// <summary>
        /// Get a list of all direct child items regedless of their class
        /// and in the order that was read from data file.
        /// </summary>
        public List<KmlItem> AllItems { get; private set; }

        /// <summary>
        /// Get a list of all direct child nodes.
        /// </summary>
        public List<KmlNode> Children { get; private set; }

        /// <summary>
        /// Get a list of all KmlAttribute children of this node.
        /// </summary>
        public List<KmlAttrib> Attribs { get; private set; }

        /// <summary>
        /// Get a list of all child items that are neither KmlNode nor KmlAttrib.
        /// When reading correct KSP persistence data this will usually be empty.
        /// </summary>
        public List<KmlItem> Unknown { get; private set; }

        /// <summary>
        /// Event is raised when attributes are added or deleted. 
        /// </summary>
        public event RoutedEventHandler AttribChanged
        {
            add { AttribChangedList.Add(value); }
            remove { AttribChangedList.Remove(value); }
        }

        /// <summary>
        /// Event is raised when child nodes are added or deleted. 
        /// </summary>
        public event RoutedEventHandler ChildrenChanged
        {
            add { ChildrenChangedList.Add(value); }
            remove { ChildrenChangedList.Remove(value); }
        }

        /// <summary>
        /// Event is raised when properties are changed that would cause ToString() 
        /// to give new result.
        /// </summary>
        public event RoutedEventHandler ToStringChanged
        {
            add { ToStringChangedList.Add(value); }
            remove { ToStringChangedList.Remove(value); }
        }

        private List<RoutedEventHandler> AttribChangedList = new List<RoutedEventHandler>();
        private List<RoutedEventHandler> ChildrenChangedList = new List<RoutedEventHandler>();
        private List<RoutedEventHandler> ToStringChangedList = new List<RoutedEventHandler>();

        /// <summary>
        /// Creates a KmlNode with a line read from data file as a child of given parent node.
        /// </summary>
        /// <param name="line">String with only one line from data file</param>
        /// <param name="parent">The parent KmlNode or null to create a root node</param>
        public KmlNode(string line, KmlNode parent)
            : base(line, parent)
        {
            Tag = line.Trim();
            Name = "";

            AllItems = new List<KmlItem>();
            Children = new List<KmlNode>();
            Attribs = new List<KmlAttrib>();
            Unknown = new List<KmlItem>();
        }

        /// <summary>
        /// Creates a node as a copy of given item.
        /// This is used to rebuild the previous KmlItem into a KmlNode after a "{" is found.
        /// </summary>
        /// <param name="item">The KmlItem to copy</param>
        public KmlNode(KmlItem item)
            : this(item.Line, item.Parent)
        {
        }

        /// <summary>
        /// Adds a child KmlItem to this nodes lists of children, depending of its
        /// derived class KmlNode or KmlAttrib or further derived from these.
        /// When an KmlAttrib "Name" is found, its value will be used for the "Name" property
        /// of this node.
        /// </summary>
        /// <param name="item">The KmlItem to add</param>
        public virtual void Add (KmlItem item)
        {
            // ensure that item.Parent is this node
            if (item.Parent != this)
            {
                RemapParent(item, this);
            }
            AllItems.Add(item);
            if (item is KmlNode)
            {
                Children.Add((KmlNode)item);
                InvokeChildrenChanged();
            }
            else if (item is KmlAttrib)
            {
                KmlAttrib attrib = (KmlAttrib)item;
                if (attrib.Name.ToLower() == "name")
                {
                    Name = attrib.Value;

                    // Get notified when Name changes
                    attrib.AttribValueChanged += Name_Changed;
                    attrib.CanBeDeleted = false;
                }
                Attribs.Add(attrib);
                InvokeAttribChanged();
            }
            else
            {
                Unknown.Add(item);
                Syntax.Warning(this, "Unknown line in persistence file: " + item.ToString());
            }
        }

        /// <summary>
        /// Adds each KmlItem in the list like Add(KmlItem) does.
        /// <see cref="KML.KmlNode.Add(KML.KmlItem)"/>
        /// </summary>
        /// <param name="list">A List of KmlItem to add</param>
        public void AddRange (List<KmlItem> list)
        {
            foreach (KmlItem item in list)
            {
                Add(item);
            }
        }

        /// <summary>
        /// Deletes a KmlItem from this nodes lists.
        /// Result will be false if item was not in the lists or couldn't be deleted
        /// because of restrictions.
        /// </summary>
        /// <param name="item">The KmlItem to delete</param>
        /// <returns>True if item was deleted, false otherwise</returns>
        public virtual bool Delete(KmlItem item)
        {
            if(!item.CanBeDeleted)
            {
                return false;
            }
            if (!AllItems.Remove(item))
            {
                // It wasn't in the list, nothing to do
                return false;
            }
            if (item is KmlAttrib)
            {
                bool result = Attribs.Remove((KmlAttrib)item);
                InvokeAttribChanged();
                return result;
            }
            else if (item is KmlNode)
            {
                bool result = Children.Remove((KmlNode)item);
                InvokeChildrenChanged();
                return result;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Search all KmlAttribs of this node.
        /// Does not search recursive.
        /// </summary>
        /// <param name="name">The name of the KmlAttribs to search for</param>
        /// <returns>The found KmlAttrib or null if no such is found</returns>
        public KmlAttrib GetAttrib(string name)
        {
            foreach (KmlAttrib attrib in Attribs)
            {
                if (attrib.Name.ToLower() == name.ToLower())
                {
                    return attrib;
                }
            }
            return null;
        }

        /// <summary>
        /// Search all KmlAttribs of this node, create one if not found.
        /// Does not search recursive.
        /// </summary>
        /// <param name="name">The name of the KmlAttribs to search for</param>
        /// <returns>The found or created KmlAttrib</returns>
        public KmlAttrib GetOrCreateAttrib(string name)
        {
            KmlAttrib attrib = GetAttrib(name);
            if (attrib == null)
            {
                attrib = KmlItem.CreateItem(name + "=", this) as KmlAttrib;
                Add(attrib);
            }
            return attrib;
        }

        /// <summary>
        /// Search all child nodes of this node for a certain tag and name.
        /// Does not search recursive.
        /// </summary>
        /// <param name="tag">The tag of the KmlNode to search for</param>
        /// <param name="name">The name of the KmlNode to search for</param>
        /// <returns>The found KmlNode or null if no such is found</returns>
        public KmlNode GetChildNode(string tag, string name)
        {
            foreach (KmlNode node in Children)
            {
                if (node.Tag.ToLower() == tag.ToLower() && (name == null || name.Length == 0 || node.Name.ToLower() == name.ToLower()))
                {
                    return node;
                }
            }
            return null;
        }

        /// <summary>
        /// Search all child nodes of this node for a certain tag.
        /// Does not search recursive.
        /// </summary>
        /// <param name="tag">The tag of the KmlNode to search for</param>
        /// <returns>The found KmlNode or null if no such is found</returns>
        public KmlNode GetChildNode(string tag)
        {
            return GetChildNode(tag, null);
        }

        /// <summary>
        /// Search all child nodes of this node for a certain tag and name, 
        /// create one if not found. Does not search recursive.
        /// </summary>
        /// <param name="tag">The tag of the KmlNode to search for</param>
        /// <param name="name">The name of the KmlNode to search for</param>
        /// <returns>The found or created KmlNode</returns>
        public KmlNode GetOrCreateChildNode(string tag, string name)
        {
            KmlNode node = GetChildNode(tag, name);
            if (node == null)
            {
                node = KmlItem.CreateItem(tag, this) as KmlNode;
                if (name != null && name.Length > 0)
                {
                    // Add name attribute
                    node.Add(KmlItem.CreateItem("name=" + name, node));
                }
                Add(node);
            }
            return node;
        }

        /// <summary>
        /// Search all child nodes of this node for a certain tag,
        /// create one if not found. Does not search recursive.
        /// </summary>
        /// <param name="tag">The tag of the KmlNode to search for</param>
        /// <returns>The found or created KmlNode</returns>
        public KmlNode GetOrCreateChildNode(string tag)
        {
            return GetOrCreateChildNode(tag, null);
        }

        /// <summary>
        /// Generates a line to be written to data file from (changed) properties.
        /// </summary>
        /// <returns>A string with one line representing this item</returns>
        public override string ToLine()
        {
            return Tag;
        }

        /// <summary>
        /// Generates a nice informative string to be used in display for this node.
        /// Usually there's a child attribute with the name "Name" and its value will be used
        /// in display additional to the node's tag.
        /// </summary>
        /// <returns>A string to display this node</returns>
        public override string ToString()
        {
            if (Name.Length > 0)
            {
                return base.ToString() + " (" + Name + ")";
            }
            else
            {
                return base.ToString();
            }
        }

        /// <summary>
        /// Get KmlAttrib from sender. Use in any event handler for KmlAttrib.AttribValueChanged.
        /// </summary>
        /// <param name="sender">Sender from within event handler for KmlAttrib.AttribValueChanged</param>
        /// <returns>The KmlAttribute that had the value changed</returns>
        protected KmlAttrib GetAttribWhereValueChanged(object sender)
        {
            return (KmlAttrib)sender;
        }

        /// <summary>
        /// Call this Method when attributes are added or deleted.
        /// All registered event handlers will be invoked.
        /// </summary>
        protected void InvokeAttribChanged()
        {
            foreach (RoutedEventHandler handler in AttribChangedList)
            {
                handler.Invoke(this, new RoutedEventArgs());
            }
        }

        /// <summary>
        /// Call this Method when child nodes are added or deleted.
        /// All registered event handlers will be invoked.
        /// </summary>
        protected void InvokeChildrenChanged()
        {
            foreach (RoutedEventHandler handler in ChildrenChangedList)
            {
                handler.Invoke(this, new RoutedEventArgs());
            }
        }

        /// <summary>
        /// Call this Method when any property is changed that cause ToString() to give new result.
        /// All registered event handlers will be invoked.
        /// </summary>
        protected void InvokeToStringChanged()
        {
            foreach (RoutedEventHandler handler in ToStringChangedList)
            {
                handler.Invoke(this, new RoutedEventArgs());
            }
        }

        private void Name_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            Name = GetAttribWhereValueChanged(sender).Value;
            InvokeToStringChanged();
        }
    }
}
