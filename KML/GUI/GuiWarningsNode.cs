using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace KML
{
    /// <summary>
    /// A GuiWarningsNode is a specialized ListViewItem to display
    /// and contain a KML Syntax.Message.
    /// </summary>
    class GuiWarningsNode : ListViewItem
    {
        /// <summary>
        /// Get the contained Syntax.Message from this GuiWarningsNode.
        /// </summary>
        public Syntax.Message DataMessage
        {
            get
            {
                return (Syntax.Message)DataContext;
            }
            private set
            {
                DataContext = value;
            }
        }

        private static GuiIcons Icons = new GuiIcons48();

        /// <summary>
        /// Creates a GuiWarningsNode containing the given Syntax.Message.
        /// </summary>
        /// <param name="dataMessage">The Syntax.Message to contain</param>
        public GuiWarningsNode(Syntax.Message dataMessage)
        {
            DataMessage = dataMessage;

            AssignTemplate();
            BuildContextMenu();
        }

        private void AssignTemplate()
        {
            // Fit an Image and a TextBlock into a Stackpanel,
            // have the Stackpanel as node Header

            StackPanel pan = new StackPanel();
            pan.Orientation = Orientation.Horizontal;
            pan.Children.Add(GenerateImage(DataMessage));
            pan.Children.Add(new TextBlock(new Run(DataMessage.ToString(true))));
            Content = pan;
        }

        private void BuildContextMenu()
        {
            // Copy that from a GuiTreeNode
            KmlNode node;
            if (DataMessage.Source is KmlNode)
            {
                node = (KmlNode)DataMessage.Source;
            }
            else
            {
                node = DataMessage.Parent;
            }
            GuiTreeNode dummy = new GuiTreeNode(node, true, true, true, false, false, false);
            ContextMenu = dummy.ContextMenu;
        }

        private Image GenerateImage(Syntax.Message message)
        {
            Image image = new Image();
            //image.Height = 16;
            if (message is Syntax.ErrorMessage)
            {
                image.Source = Icons.Error.Source;
            }
            else if (message is Syntax.WarningMessage)
            {
                image.Source = Icons.Warning.Source;
            }
            image.Margin = new Thickness(0, 0, 3, 0);

            return image;
        }
    }
}
