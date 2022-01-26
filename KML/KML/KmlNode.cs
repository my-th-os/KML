using System.Collections.Generic;
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

        // TODO KmlNode.Tag: Allow to change Tag and invoke ToStringChanged?

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
        public KmlNode(string line)
            : base(line)
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
            : this(item.Line)
        {
        }

        /// <summary>
        /// Adds (inserts before) a child KmlItem to this nodes lists of children.
        /// If item to insert before is null or not contained, it will be added at the end.
        /// This is the basic add method, derived classes can override but should
        /// always call base.Add(beforeItem, newItem) within.
        /// Public Add, AddRange, InsertBefore and InsertAfter all use this protected
        /// method to access the lists.
        /// <see cref="KML.KmlNode.Add(KML.KmlItem)"/>
        /// </summary>
        /// <param name="beforeItem">The KmlItem where the new item should be inserted before</param>
        /// <param name="newItem">The KmlItem to add</param>
        protected virtual void Add (KmlItem beforeItem, KmlItem newItem)
        {
            // ensure that item.Parent is this node
            if (newItem.Parent != this)
            {
                RemapParent(newItem, this);
            }

            // Not add always to end of AllItems, add well ordered: attribs first, then nodes.
            // Like Add(attrib), Add(Node), Add(attrib) should result in attrib, attrib, node
            if (newItem is KmlAttrib && !(beforeItem is KmlAttrib) && Children.Count > 0)
            {
                Syntax.Warning(newItem, "KML attribute should not come after nodes, will be fixed when saved");
                beforeItem = Children[0];
            }

            if (beforeItem != null && AllItems.Contains(beforeItem))
            {
                AllItems.Insert(AllItems.IndexOf(beforeItem), newItem);
            }
            else
            {
                AllItems.Add(newItem);
            }

            if (newItem is KmlNode)
            {
                if (beforeItem is KmlNode && Children.Contains((KmlNode)beforeItem))
                {
                    Children.Insert(Children.IndexOf((KmlNode)beforeItem), (KmlNode)newItem);
                }
                else
                {
                    Children.Add((KmlNode)newItem);
                }
                InvokeChildrenChanged();
            }
            else if (newItem is KmlAttrib)
            {
                KmlAttrib attrib = (KmlAttrib)newItem;
                if (attrib.Name.ToLower() == "name")
                {
                    if (Name.Length == 0)
                    {
                        Name = attrib.Value;

                        // Get notified when Name changes
                        attrib.AttribValueChanged += Name_Changed;
                        attrib.CanBeDeleted = false;

                        // And notify that the name changed
                        InvokeToStringChanged();
                    }
                }

                if (beforeItem is KmlAttrib && Attribs.Contains((KmlAttrib)beforeItem))
                {
                    Attribs.Insert(Attribs.IndexOf((KmlAttrib)beforeItem), attrib);
                }
                else
                {
                    Attribs.Add(attrib);
                }
                InvokeAttribChanged();
            }
            else
            {
                if (beforeItem != null && Unknown.Contains(newItem))
                {
                    Unknown.Insert(Unknown.IndexOf(beforeItem), newItem);
                }
                else
                {
                    Unknown.Add(newItem);
                }
                Syntax.Warning(this, "Unknown line in persistence file: " + newItem.ToString());
            }
        }

        /// <summary>
        /// Adds a child KmlItem to this nodes lists of children, depending of its
        /// derived class KmlNode or KmlAttrib or further derived from these.
        /// When an KmlAttrib "Name" is found, its value will be used for the "Name" property
        /// of this node.
        /// </summary>
        /// <param name="item">The KmlItem to add</param>
        public void Add(KmlItem item)
        {
            Add(null, item);
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
                Add(null, item);
            }
        }

        /// <summary>
        /// Inserts a child KmlItem after given KmlItem to this nodes lists of children, 
        /// depending of its derived class KmlNode or KmlAttrib or further derived from these.
        /// If item to insert after is null or not contained, it will be added at the end.
        /// <see cref="KML.KmlNode.Add(KML.KmlItem)"/>
        /// </summary>
        /// <param name="afterItem">The KmlItem where the new item should be inserted after</param>
        /// <param name="newItem">The KmlItem to add</param>
        public void InsertAfter(KmlItem afterItem, KmlItem newItem)
        {
            KmlItem beforeItem = GetNextSibling(afterItem);
            Add(beforeItem, newItem);
        }

        /// <summary>
        /// Inserts a child KmlItem before given KmlItem to this nodes lists of children, 
        /// depending of its derived class KmlNode or KmlAttrib or further derived from these.
        /// If item to insert before is null or not contained, it will be added at the end.
        /// <see cref="KML.KmlNode.Add(KML.KmlItem)"/>
        /// </summary>
        /// <param name="beforeItem">The KmlItem where the new item should be inserted before</param>
        /// <param name="newItem">The KmlItem to add</param>
        public void InsertBefore(KmlItem beforeItem, KmlItem newItem)
        {
            Add(beforeItem, newItem);
        }

        /// <summary>
        /// Adds each KmlItem in the list before KmlItem like InsertBefore(KmlItem, KmlItem) does.
        /// <see cref="KML.KmlNode.InsertBefore(KML.KmlItem, KML.KmlItem)"/>
        /// </summary>
        /// <param name="beforeItem">The KmlItem where the new item should be inserted before</param>
        /// <param name="list">A List of KmlItem to add</param>
        public void InsertBeforeRange(KmlItem beforeItem, List<KmlItem> list)
        {
            foreach (KmlItem item in list)
            {
                Add(beforeItem, item);
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
            if (!item.CanBeDeleted)
            {
                return false;
            }
            // Call itme's Delete to call it's BeforeDelete()
            // But there would usually be called this method,
            // to avoid loop we set parent to null.
            // We also ignore result, will be false in that case.
            // Item can prevent this by setting Parent == null
            // (after memorizing the real parent, otherwise we woul not get here).
            // TODO KmlNode.Delete(): This cycle is a mess, especially with item.DeleteRaw() joining the dance
            if (item.Parent != null)
            {
                RemapParent(item, null);
                item.Delete();
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
                return Unknown.Remove(item);
            }
        }

        /// <summary>
        /// Clear all child nodes and attributes from this node.
        /// Deriving classes should override this and clear their stuff,
        /// but call base.Clear() within.
        /// </summary>
        public virtual void Clear()
        {
            Attribs.Clear();
            Children.Clear();
            Unknown.Clear();
            AllItems.Clear();
            Name = "";
        }

        /// <summary>
        /// Swaps appearance in lists of two attributes.
        /// </summary>
        /// <param name="attrib1">One attribute to swap</param>
        /// <param name="attrib2">The other attribute to swap</param>
        public bool SwapAttribs(KmlAttrib attrib1, KmlAttrib attrib2)
        {
            int i = Attribs.IndexOf(attrib1);
            int j = Attribs.IndexOf(attrib2);
            // there would be more work to do for parts, restricted for now
            if (i >= 0 && j >= 0)
            {
                Attribs[i] = attrib2;
                Attribs[j] = attrib1;
                i = AllItems.IndexOf(attrib1);
                j = AllItems.IndexOf(attrib2);
                AllItems[i] = attrib2;
                AllItems[j] = attrib1;
                InvokeAttribChanged();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Swaps appearance in lists of two child nodes.
        /// </summary>
        /// <param name="child1">One child node to swap</param>
        /// <param name="child2">The other child node to swap</param>
        public bool SwapChildren(KmlNode child1, KmlNode child2)
        {
            int i = Children.IndexOf(child1);
            int j = Children.IndexOf(child2);
            // there would be more work to do for parts, restricted for now
            if (i >= 0 && j >= 0 && !(child1 is KmlPart) && !(child2 is KmlPart))
            {
                Children[i] = child2;
                Children[j] = child1;
                i = AllItems.IndexOf(child1);
                j = AllItems.IndexOf(child2);
                AllItems[i] = child2;
                AllItems[j] = child1;
                InvokeChildrenChanged();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Counts all the lines contained within this node, including itself and those of child nodes
        /// </summary>
        /// <returns>The number of lines</returns>
        public int TotalLineCount()
        {
            int count = 3; // the node itself with opening and closing brackets
            count += Attribs.Count;
            count += Unknown.Count;
            foreach (var child in Children)
            {
                count += child.TotalLineCount();
            }
            return count;
        }

        private KmlAttrib GetNextSibling(KmlAttrib attrib)
        {
            int index = Attribs.IndexOf(attrib);
            if (index >= 0 && index < Attribs.Count - 1)
            {
                return Attribs[index + 1];
            }
            else
            {
                return null;
            }
        }

        private KmlNode GetNextSibling(KmlNode node)
        {
            int index = Children.IndexOf(node);
            if (index >= 0 && index < Children.Count - 1)
            {
                return Children[index + 1];
            }
            else
            {
                return null;
            }
        }

        private KmlItem GetNextSibling(KmlItem item)
        {
            if (item is KmlAttrib)
            {
                return GetNextSibling((KmlAttrib)item);
            }
            else if (item is KmlNode)
            {
                return GetNextSibling((KmlNode)item);
            }
            else
            {
                int index = Unknown.IndexOf(item);
                if (index >= 0 && index < Unknown.Count - 1)
                {
                    return Unknown[index + 1];
                }
                else
                {
                    return null;
                }
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
        /// Does not search recursive. Default value will only be used on creation.
        /// </summary>
        /// <param name="name">The name of the KmlAttribs to search for</param>
        /// <param name="defaultValue">The default value for a created attribute</param>
        /// <returns>The found or created KmlAttrib</returns>
        public KmlAttrib GetOrCreateAttrib(string name, string defaultValue)
        {
            KmlAttrib attrib = GetAttrib(name);
            if (attrib == null)
            {
                string line = name + "=";
                if (defaultValue != null && defaultValue.Length > 0)
                {
                    line += defaultValue;
                }
                attrib = KmlItem.CreateItem(line) as KmlAttrib;
                Add(attrib);
            }
            return attrib;
        }

        /// <summary>
        /// Search all KmlAttribs of this node, create one if not found.
        /// Does not search recursive.
        /// </summary>
        /// <param name="name">The name of the KmlAttribs to search for</param>
        /// <returns>The found or created KmlAttrib</returns>
        public KmlAttrib GetOrCreateAttrib(string name)
        {
            return GetOrCreateAttrib(name, null);
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
        /// Serach all child nodes of sourceNode for a certain tag.
        /// Does not search recursive.
        /// </summary>
        /// <param name="sourceNode">The node to search in its chgild nodes</param>
        /// <param name="tag">The tag of the KmlNode to search for</param>
        /// <returns>The found KmlNode or null if no such is found</returns>
        public static KmlNode GetChildNodeFrom(KmlNode sourceNode, string tag)
        {
            if (sourceNode != null)
            {
                return sourceNode.GetChildNode(tag);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Serach all nodes in sourceItems for a certain tag.
        /// Does not search recursive.
        /// </summary>
        /// <param name="sourceItems">The list of KmlItems to search in</param>
        /// <param name="tag">The tag of the KmlNode to search for</param>
        /// <returns>The found KmlNode or null if no such is found</returns>
        public static KmlNode GetNodeFrom(List<KmlItem> sourceItems, string tag)
        {
            foreach (KmlItem item in sourceItems)
            {
                if (item is KmlNode && (item as KmlNode).Tag.ToLower() == tag.ToLower())
                {
                    return (KmlNode)item;
                }
            }
            return null;
        }

        /// <summary>
        /// Serach all nodes in sourceItems for the first tag in tag.
        /// Then search recursive in the found node's children for next tag in tags.
        /// </summary>
        /// <param name="sourceItems">The list of KmlItems to search in</param>
        /// <param name="tags">An array of tags of the KmlNodes to search for</param>
        /// <returns>The found KmlNode or null if no such is found</returns>
        public static KmlNode GetNodeFromDeep(List<KmlItem> sourceItems, string[] tags)
        {
            if (tags.Length > 0)
            {
                KmlNode node = GetNodeFrom(sourceItems, tags[0]);
                for (int i = 1; i < tags.Length && node != null; i++)
                {
                    node = GetChildNodeFrom(node, tags[i]);
                }
                return node;
            }
            return null;
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
                node = KmlItem.CreateItem(tag) as KmlNode;
                if (name != null && name.Length > 0)
                {
                    // Add name attribute
                    node.Add(KmlItem.CreateItem("name=" + name));
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
            return base.ToString() + BracketString(Name);
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
