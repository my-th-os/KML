using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        /// <summary>
        /// Creates a GuiTreeNode containing the given dataAttrib.
        /// </summary>
        /// <param name="dataAttrib">The KmlAttrib to contain</param>
        public GuiTreeAttrib(KmlAttrib dataAttrib)
        {
            DataAttrib = dataAttrib;

            BuildContextMenu();
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

            // So far it's the default Menu, wich should not be shown if no items follow
            int defaultMenuCount = menu.Items.Count;

            MenuItem m = new MenuItem();
            m.DataContext = DataAttrib;
            m.Icon = Icons.CreateImage(Icons.Add);
            m.Header = "Insert attribute...";
            m.Click += AttribInsertBefore_Click;
            menu.Items.Add(m);
            menu.Items.Add(new Separator());

            m = new MenuItem();
            m.DataContext = DataAttrib;
            m.Icon = Icons.CreateImage(Icons.Delete);
            m.Header = "Delete this attribute...";
            m.Click += AttribDelete_Click;
            m.IsEnabled = DataAttrib.CanBeDeleted;
            if (!m.IsEnabled && m.Icon != null)
            {
                (m.Icon as Image).Opacity = 0.3;
            }
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

    }
}
