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
        private GuiTreeNode BaseGuiNode;

        /// <summary>
        /// Creates a GuiWarningsNode containing the given Syntax.Message.
        /// </summary>
        /// <param name="dataMessage">The Syntax.Message to contain</param>
        public GuiWarningsNode(Syntax.Message dataMessage)
        {
            DataMessage = dataMessage;
            KmlNode node;
            if (DataMessage.Source is KmlNode)
            {
                node = (KmlNode)DataMessage.Source;
            }
            else
            {
                node = DataMessage.Source.Parent;
            }
            if (node != null)
            {
                BaseGuiNode = new GuiTreeNode(node, true, true, true, false, false, false);
            }

            AssignTemplate();
            BuildContextMenu();
        }

        /// <summary>
        /// Some key was pressed.
        /// </summary>
        public void CommandExec(string Command)
        {
            if (BaseGuiNode != null)
                BaseGuiNode.CommandExec(Command);
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
            if (BaseGuiNode != null)
            {
                ContextMenu = BaseGuiNode.ContextMenu;
                // To avoid follwing error output (uncritical), we need to have a parent for the TreeViewItem, so we also make a dummy
                // System.Windows.Data Error: 4 : Cannot find source for binding with reference 'RelativeSource FindAncestor, AncestorType='System.Windows.Controls.ItemsControl', AncestorLevel='1''. BindingExpression:Path=HorizontalContentAlignment; DataItem=null; target element is 'GuiTreeNode' (Name=''); target property is 'HorizontalContentAlignment' (type 'HorizontalAlignment')
                // System.Windows.Data Error: 4 : Cannot find source for binding with reference 'RelativeSource FindAncestor, AncestorType='System.Windows.Controls.ItemsControl', AncestorLevel='1''. BindingExpression:Path=VerticalContentAlignment; DataItem=null; target element is 'GuiTreeNode' (Name=''); target property is 'VerticalContentAlignment' (type 'VerticalAlignment')
                TreeView dummyTree = new TreeView();
                dummyTree.Items.Add(BaseGuiNode);
            }
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
