using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace KML
{
    /// <summary>
    /// A GuiTreeAttrib is a specialized ListViewItem to display
    /// and contain a KmlAttrib.
    /// </summary>
    class GuiTreeAttrib : ListViewItem
    {
        /// <summary>
        /// Get the contained KmlAttrib from this GuiTreeAttrib.
        /// </summary>
        public KmlAttrib DataAttrib
        {
            get
            {
                return (KmlAttrib)DataContext;
            }
            private set
            {
                DataContext = value;
                Content = value;
            }
        }

        private static GuiIcons Icons = new GuiIcons16();

        private MenuItem MenuInsert { get; set; }
        private MenuItem MenuDelete { get; set; }
        private MenuItem MenuCopy { get; set; }
        private MenuItem MenuPaste { get; set; }
        private MenuItem MenuMoveUp { get; set; }
        private MenuItem MenuMoveDown { get; set; }

        /// <summary>
        /// Creates a GuiTreeNode containing the given dataAttrib.
        /// </summary>
        /// <param name="dataAttrib">The KmlAttrib to contain</param>
        public GuiTreeAttrib(KmlAttrib dataAttrib)
        {
            DataAttrib = dataAttrib;

            BuildContextMenu();
        }

        /// <summary>
        /// Some key was pressed.
        /// </summary>
        public void CommandExec(string Command)
        {
            ContextMenuUpdate(ContextMenu);
            switch (Command)
            {
                case "Insert":
                    if (MenuInsert != null && MenuInsert.IsEnabled)
                        AttribInsertBefore_Click(MenuInsert, null);
                    break;
                case "Delete":
                    if (MenuDelete != null && MenuDelete.IsEnabled)
                        AttribDelete_Click(MenuDelete, null);
                    break;
                case "Copy":
                    if (MenuCopy != null && MenuCopy.IsEnabled)
                        AttribCopy_Click(MenuCopy, null);
                    break;
                case "Paste":
                    if (MenuPaste != null && MenuPaste.IsEnabled)
                        AttribPasteBefore_Click(MenuPaste, null);
                    break;
                case "MoveUp":
                    if (MenuMoveUp != null && MenuMoveUp.IsEnabled)
                        AttribMoveUp_Click(MenuMoveUp, null);
                    break;
                case "MoveDown":
                    if (MenuMoveDown != null && MenuMoveDown.IsEnabled)
                        AttribMoveDown_Click(MenuMoveDown, null);
                    break;
            }
        }

        private void BuildContextMenu()
        {
            string shortHeader = DataAttrib.ToString();
            if (shortHeader.Length > 30)
            {
                shortHeader = shortHeader.Substring(0, 27) + "...";
            }

            ContextMenu menu = new ContextMenu();
            MenuItem title = new MenuItem();
            title.Header = shortHeader;
            title.IsEnabled = false;
            title.Background = new SolidColorBrush(Colors.Black);
            title.BorderThickness = new Thickness(1);
            title.BorderBrush = new SolidColorBrush(Colors.Gray);
            menu.Items.Add(title);
            menu.Items.Add(new Separator());
            menu.Opened += ContextMenu_Opened;

            // So far it's the default Menu, wich should not be shown if no items follow
            int defaultMenuCount = menu.Items.Count;

            MenuMoveUp = new MenuItem();
            MenuMoveUp.DataContext = DataAttrib;
            MenuMoveUp.Icon = Icons.CreateImage(Icons.Up);
            MenuMoveUp.Header = "Move up";
            MenuMoveUp.InputGestureText = "[Alt+Up]";
            MenuMoveUp.Click += AttribMoveUp_Click;
            MenuMoveUp.IsEnabled = DataAttrib.Parent != null;
            MenuMoveUp.Tag = "Attrib.MoveUp";
            menu.Items.Add(MenuMoveUp);

            MenuMoveDown = new MenuItem();
            MenuMoveDown.Icon = Icons.CreateImage(Icons.Down);
            MenuMoveDown.DataContext = DataAttrib;
            MenuMoveDown.Header = "Move down";
            MenuMoveDown.InputGestureText = "[Alt+Down]";
            MenuMoveDown.Click += AttribMoveDown_Click;
            MenuMoveDown.IsEnabled = DataAttrib.Parent != null;
            MenuMoveDown.Tag = "Attrib.MoveDown";
            menu.Items.Add(MenuMoveDown);

            menu.Items.Add(new Separator());

            MenuItem m = new MenuItem();
            m.DataContext = DataAttrib;
            m.Icon = Icons.CreateImage(Icons.Clipboard);
            m.Header = "_Copy attribute";
            m.InputGestureText = "[Ctrl+C]";
            m.Click += AttribCopy_Click;
            MenuCopy = m;
            menu.Items.Add(m);

            m = new MenuItem();
            m.DataContext = DataAttrib;
            m.Icon = Icons.CreateImage(Icons.Paste);
            m.Header = "_Paste attribute(s)";
            m.InputGestureText = "[Ctrl+V]";
            m.Click += AttribPasteBefore_Click;
            // m.IsEnabled = Clipboard.ContainsText(TextDataFormat.UnicodeText);
            MenuPaste = m;
            m.Tag = "Clipboard.Paste";
            menu.Items.Add(m);

            menu.Items.Add(new Separator());

            m = new MenuItem();
            m.DataContext = DataAttrib;
            m.Icon = Icons.CreateImage(Icons.Add);
            m.Header = "_Insert attribute...";
            m.InputGestureText = "[Ins]";
            m.Click += AttribInsertBefore_Click;
            MenuInsert = m;
            menu.Items.Add(m);

            menu.Items.Add(new Separator());

            m = new MenuItem();
            m.DataContext = DataAttrib;
            m.Icon = Icons.CreateImage(Icons.Delete);
            m.Header = "_Delete this attribute...";
            m.InputGestureText = "[Del]";
            m.Click += AttribDelete_Click;
            m.IsEnabled = DataAttrib.CanBeDeleted;
            if (!m.IsEnabled && m.Icon != null)
            {
                (m.Icon as Image).Opacity = 0.3;
            }
            MenuDelete = m;
            menu.Items.Add(m);


            // Need to have a seperate menu for each item, even if it is empty.
            // If ContextMenu is null, the parent's contextmenu will be used (WTF).
            // Item[0] is the menu title, Item[1] a Seperator, both always created.
            ContextMenu = menu;
            if (menu.Items.Count <= defaultMenuCount)
            {
                ContextMenu.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void ContextMenuUpdate(ContextMenu menu)
        {
            foreach (object o in menu.Items)
                if (o is MenuItem)
                {
                    MenuItem m = (MenuItem)o;
                    if (m.Tag != null && (m.Tag as string).Equals("Clipboard.Paste"))
                        m.IsEnabled = Clipboard.ContainsText(TextDataFormat.UnicodeText);
                    else if (m.Tag != null && (m.Tag as string).Equals("Attrib.MoveUp"))
                        m.IsEnabled = AttribCanMoveUp(DataAttrib);
                    else if (m.Tag != null && (m.Tag as string).Equals("Attrib.MoveDown"))
                        m.IsEnabled = AttribCanMoveDown(DataAttrib);
                    if (m.Icon != null && menu.Items[0] != m)
                    {
                        (m.Icon as Image).Opacity = m.IsEnabled ? 1.0 : 0.3;
                    }
                }
        }

        private bool AttribCanMoveUp(KmlAttrib attrib)
        {
            return attrib.Parent != null && attrib.Parent.Attribs.IndexOf(attrib) > 0;
        }

        private bool AttribCanMoveDown(KmlAttrib attrib)
        {
            return attrib.Parent != null && attrib.Parent.Attribs.IndexOf(attrib) < attrib.Parent.Attribs.Count - 1;
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            ContextMenuUpdate(sender as ContextMenu);
        }

        private void AttribCopy_Click(object sender, RoutedEventArgs e)
        {
            KmlAttrib attrib = ((sender as MenuItem).DataContext as KmlAttrib);

            var sr = new StringWriter();

            KmlItem.WriteItem(sr, attrib, 0);

            sr.Flush();

            var textNode = sr.GetStringBuilder().ToString();

            Clipboard.SetDataObject(textNode);
        }

        private void AttribPasteBefore_Click(object sender, RoutedEventArgs e)
        {
            KmlAttrib attrib = ((sender as MenuItem).DataContext as KmlAttrib);
            if (attrib.Parent != null)
            {
                var textNode = Clipboard.GetText(TextDataFormat.UnicodeText);

                var items = KmlItem.ParseItems(new StringReader(textNode)).Where(i => i is KmlAttrib).ToList();

                if (!items.Any())
                    DlgMessage.Show("Can not paste attribute from clipboard", "Paste attribute", Icons.Warning);

                attrib.Parent.InsertBeforeRange(attrib, items);
            }
            else
            {
                DlgMessage.Show("Can not insert, attribute has no parent", "Paste attribute", Icons.Warning);
            }
        }

        private void AttribInsertBefore_Click(object sender, RoutedEventArgs e)
        {
            // TODO GuiTreeAttrib.AttribInsertBefore_Click(): Almost same code as private GuiTreeNode.AddAttrib()

            KmlAttrib beforeItem = ((sender as MenuItem).DataContext as KmlAttrib);
            KmlNode node = beforeItem.Parent;
            if (node != null)
            {
                string input;
                string preset = "";
                bool loop = true;
                while (loop && DlgInput.Show("Enter the name for the new attribute:", "NEW attribute", Icons.Add, preset, out input))
                {
                    string attrib = input;
                    if (attrib.Length > 0 && attrib.IndexOf('=') < 0)
                    {
                        attrib = attrib + "=";
                    }
                    KmlItem item = KmlItem.CreateItem(attrib);
                    if (item is KmlAttrib && beforeItem != null)
                    {
                        node.InsertBefore(beforeItem, (KmlAttrib)item);
                        loop = false;
                        // View will be refreshed in AttribChanged event
                    }
                    else if (item is KmlAttrib)
                    {
                        node.Add((KmlAttrib)item);
                        loop = false;
                        // View will be refreshed in AttribChanged event
                    }
                    else
                    {
                        DlgMessage.Show("Attribute name is not allowed to be empty or contain following characters: {}", "NEW attribute", Icons.Warning);
                        preset = input;
                        // Input will pop up again while loop == true
                    }
                }
            }
            else
            {
                DlgMessage.Show("Can not insert, attribute has no parent", "NEW attribute", Icons.Warning);
            }
        }

        private void AttribDelete_Click(object sender, RoutedEventArgs e)
        {
            KmlAttrib attrib = ((sender as MenuItem).DataContext as KmlAttrib);
            if (DlgConfirmation.Show("Do your really want to delete this attribute?\n" + attrib, "DELETE attribue", Icons.Delete))
            {
                attrib.Delete();
                // View will be refreshed in parent's ChildrenChanged event
            }
        }

        private void AttribMoveUp_Click(object sender, RoutedEventArgs e)
        {
            KmlAttrib attrib = ((sender as MenuItem).DataContext as KmlAttrib);
            if (AttribCanMoveUp(attrib))
            {
                int i = attrib.Parent.Attribs.IndexOf(attrib);
                KmlAttrib other = attrib.Parent.Attribs[i - 1];
                if (attrib.Parent.SwapAttribs(attrib, other))
                {
                    // focus changed when list is redrawn, select attrib again
                    GuiTabsManager.GetCurrent().TreeManager.Select(attrib);
                }
            }
        }

        private void AttribMoveDown_Click(object sender, RoutedEventArgs e)
        {
            KmlAttrib attrib = ((sender as MenuItem).DataContext as KmlAttrib);
            if (AttribCanMoveDown(attrib))
            {
                int i = attrib.Parent.Attribs.IndexOf(attrib);
                KmlAttrib other = attrib.Parent.Attribs[i + 1];
                if (attrib.Parent.SwapAttribs(attrib, other))
                {
                    // focus changed when list is redrawn, select attrib again
                    GuiTabsManager.GetCurrent().TreeManager.Select(attrib);
                }
            }
        }
    }
}
