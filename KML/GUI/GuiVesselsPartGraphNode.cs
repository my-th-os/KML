using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace KML
{
    /// <summary>
    /// A GuiVesselsPartGraphNode represents a drawing node of a GuiVesselsPartGraph.
    /// This implementation uses a StackPanel to contain a reused but reduced GuiTreeNode 
    /// with an additional Label with short text only.
    /// By reusing a GuiTreeNode it will have the same context menu and icon.
    /// </summary>
    class GuiVesselsPartGraphNode : StackPanel
    {
        /// <summary>
        /// Get the KmlPart this node is representing.
        /// </summary>
        public KmlPart DataPart
        {
            get
            {
                return Icon.DataNode as KmlPart;
            }
        }

        /// <summary>
        /// Get the X koord in the PartGrid of the parent GuiVesselsPartGraph.
        /// Needs to be seen from parent GuiVesselsPartGraph.
        /// </summary>
        public int KoordX { get; private set; }

        /// <summary>
        /// Get the Y koord in the PartGrid of the parent GuiVesselsPartGraph.
        /// Needs to be seen from parent GuiVesselsPartGraph.
        /// </summary>
        public int KoordY { get; private set; }

        /// <summary>
        /// Get a list of all lines connected to this node, so they can be highlighted on MouseEnter.
        /// This list will be managed by the parent GuiVesselsPartGraph.
        /// </summary>
        public List<Line> Lines { get; private set; }

        private GuiTreeNode Icon { get; set; }
        private Label Text { get; set; }

        private static Brush HighlightBrush { get; set; }

        static GuiVesselsPartGraphNode()
        {
            HighlightBrush = new SolidColorBrush(SystemColors.HighlightColor);
            HighlightBrush.Opacity = 0.3;
        }

        /// <summary>
        /// Creates a GuiVesselsPartGraphNode containing the given KmlPart at the given PartGrid koordinates.
        /// Also needs a master GuiTabsManager to switch view on double click on this node.
        /// </summary>
        /// <param name="part">The KmlPart this node will represent</param>
        /// <param name="koordX">The X koord in parents PartGrid</param>
        /// <param name="koordY">The Y koord in parents PartGrid</param>
        public GuiVesselsPartGraphNode(KmlPart part, int koordX, int koordY)
        {
            KoordX = koordX;
            KoordY = koordY;
            Lines = new List<Line>();

            if (part != null)
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal;

                Icon = new GuiTreeNode(part, true, false, true, false, true, false, false);
                Icon.ToolTip = part.ToString();
                Icon.Margin = new System.Windows.Thickness(-16, 0, 0, 0);
                // GuiTreeNode gets selected and drawn highlighted after using its context menu
                // we don't want this in the part graph
                Icon.Selected += Icon_Selected;
                Children.Add(Icon);

                Text = new Label();
                Text.DataContext = part;
                if (part.Parent is KmlVessel)
                {
                    Text.Content = "[" + (part.Parent as KmlVessel).Parts.IndexOf(part) + "]";
                }
                else
                {
                    Text.Content = "[x]";
                }
                Text.ToolTip = part.ToString();
                // TODO GuiVesselsPartGraphNode.GuiVesselsPartGraphNode(): Label.ContextMenu does not really work
                Text.ContextMenu = Icon.ContextMenu;
                Text.Margin = new System.Windows.Thickness(-6, -6, 0, 0);
                Children.Add(Text);

                MouseEnter += Node_MouseEnter;
                MouseLeave += Node_MouseLeave;
                Icon.MouseDoubleClick += Node_MouseDoubleClick;
                // TODO GuiVesselsPartGraphNode.GuiVesselsPartGraphNode(): Label.MouseDoubleClick does not really work
                Text.MouseDoubleClick += Node_MouseDoubleClick;
            }
        }

        /// <summary>
        /// Creates a dummy GuiVesselsPartGraphNode containing no part 
        /// but only blocking the space in parent PartGrid.
        /// </summary>
        /// <param name="koordX">The X koord in parents PartGrid</param>
        /// <param name="koordY">The Y koord in parents PartGrid</param>
        public GuiVesselsPartGraphNode(int koordX, int koordY)
            : this(null, koordX, koordY)
        {
        }

        /// <summary>
        /// The Child GuiTreeNode may change and some data is copied,
        /// so there is need for update.
        /// </summary>
        public void UpdateFromGuiTreeNode()
        {
            Icon.ToolTip = DataPart.ToString();
            if (DataPart.Parent is KmlVessel)
            {
                Text.Content = "[" + (DataPart.Parent as KmlVessel).Parts.IndexOf(DataPart) + "]";
            }
            Text.ToolTip = DataPart.ToString();
            Text.ContextMenu = Icon.ContextMenu;
        }

        private void Icon_Selected(object sender, RoutedEventArgs e)
        {
            (sender as GuiTreeNode).IsSelected = false;
        }

        private void Node_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            KmlPart part = null;
            if(sender is GuiTreeNode)
            {
                part = ((GuiTreeNode)sender).DataNode as KmlPart;
            }
            else if (sender is Label)
            {
                part = ((Label)sender).DataContext as KmlPart;
            }
            if (part != null)
            {
                GuiTabsManager.GetCurrent().Select(part);
            }
        }

        private void Node_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Background = null;
            foreach (Line l in Lines)
            {
                l.StrokeThickness = 1;
            }
        }

        private void Node_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Background = HighlightBrush;
            foreach (Line l in Lines)
            {
                l.StrokeThickness = 3;
            }
        }
    }
}
