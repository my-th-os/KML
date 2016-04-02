using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Effects;

namespace KML
{
    /// <summary>
    /// A GuiTreeNode is a specialized ListViewItem to display
    /// and contain a KmlKerbal.
    /// </summary>
    class GuiKerbalsNode : ListViewItem
    {
        /// <summary>
        /// Get the contained KmlKerbal from this GuiKerbalsNode.
        /// </summary>
        public KmlKerbal DataKerbal
        {
            get
            {
                return (KmlKerbal)DataContext;
            }
            private set
            {
                DataContext = value;
            }
        }

        private static GuiIcons Icons16 = new GuiIcons16();
        private static GuiIcons Icons48 = new GuiIcons48();

        /// <summary>
        /// Creates a GuiKerbalsNode containing the given DataKerbal.
        /// To have nice icons in the tree, a GuiIcons can be
        /// provided.
        /// </summary>
        /// <param name="dataKerbal">The KmlKerbal to contain</param>
        public GuiKerbalsNode(KmlKerbal dataKerbal)
        {
            DataKerbal = dataKerbal;

            AssignTemplate();
            BuildContextMenu();

            // Get notified when KmlNode ToString() changes
            DataKerbal.ToStringChanged += DataKerbal_ToStringChanged;
        }

        private void AssignTemplate()
        {
            // Fit an Image and a TextBlock into a Stackpanel,
            // have the Stackpanel as node Header

            StackPanel pan = new StackPanel();
            pan.Orientation = Orientation.Horizontal;
            pan.Children.Add(GenerateImage(DataKerbal));
            pan.Children.Add(GenerateTraitImage(DataKerbal));
            ProgressBar prog = GenerateProgressBar(DataKerbal.Brave);
            prog.Margin = new Thickness(-16, 4, 0, 0);
            prog.ToolTip = "brave";
            pan.Children.Add(prog);
            prog = GenerateProgressBar(DataKerbal.Dumb);
            prog.Margin = new Thickness(-32, 36, 3, 0);
            prog.ToolTip = "dumb";
            pan.Children.Add(prog);
            pan.Children.Add(GenerateText(DataKerbal));
            Content = pan;
        }

        private void BuildContextMenu()
        {
            // Copy that from a GuiTreeNode
            GuiTreeNode dummy = new GuiTreeNode(DataKerbal, true, true, true, false, true, false);
            ContextMenu = dummy.ContextMenu;
            // To avoid follwing error output (uncritical), we need to have a parent for the TreeViewItem, so we also make a dummy
            // System.Windows.Data Error: 4 : Cannot find source for binding with reference 'RelativeSource FindAncestor, AncestorType='System.Windows.Controls.ItemsControl', AncestorLevel='1''. BindingExpression:Path=HorizontalContentAlignment; DataItem=null; target element is 'GuiTreeNode' (Name=''); target property is 'HorizontalContentAlignment' (type 'HorizontalAlignment')
            // System.Windows.Data Error: 4 : Cannot find source for binding with reference 'RelativeSource FindAncestor, AncestorType='System.Windows.Controls.ItemsControl', AncestorLevel='1''. BindingExpression:Path=VerticalContentAlignment; DataItem=null; target element is 'GuiTreeNode' (Name=''); target property is 'VerticalContentAlignment' (type 'VerticalAlignment')
            TreeView dummyTree = new TreeView();
            dummyTree.Items.Add(dummy);
        }

        private Image GenerateImage(KmlKerbal kerbal)
        {
            Image image = new Image();
            image.Height = 48;
            if (kerbal.Type.ToLower() == "applicant")
            {
                image.Source = Icons48.KerbalApplicant.Source;
            }
            else if (kerbal.Type.ToLower() == "tourist")
            {
                image.Source = Icons48.KerbalTorist.Source;
            }
            else
            {
                image.Source = Icons48.Kerbal.Source;
            }
            image.Margin = new Thickness(0, 0, 3, 0);

            return image;
        }

        private Image GenerateTraitImage(KmlKerbal kerbal)
        {
            Image image = new Image();
            image.Height = 16;
            if (kerbal.Trait.ToLower() == "pilot")
            {
                image.Source = Icons16.KerbalPilot.Source;
            }
            else if (kerbal.Trait.ToLower() == "engineer")
            {
                image.Source = Icons16.KerbalEngineer.Source;
            }
            else if (kerbal.Trait.ToLower() == "scientist")
            {
                image.Source = Icons16.KerbalScience.Source;
            }
            else if (kerbal.Trait.ToLower() == "tourist")
            {
                image.Source = Icons16.KerbalCamera.Source;
            }
            else
            {
                image.Source = Icons16.Ghost.Source;
            }
            image.Margin = new Thickness(0, -24, 0, 0);

            return image;
        }

        private ProgressBar GenerateProgressBar(double ratio)
        {
            ProgressBar prog = new ProgressBar();
            prog.Maximum = 1.0;
            prog.Value = ratio;
            prog.Height = 8;
            prog.Width = 32;
            //prog.Margin = new Thickness(0, 0, 0, 0);

            DropShadowEffect eff = new DropShadowEffect();
            eff.ShadowDepth = 1;
            prog.Effect = eff;

            return prog;
        }

        private TextBlock GenerateText(KmlKerbal kerbal)
        {
            TextBlock text = new TextBlock();
            text.Inlines.Add(new Bold(new Run(kerbal.Name)));
            text.Inlines.Add(new Run("\n" + kerbal.Type + "\n" + kerbal.Trait));
            text.Margin = new Thickness(3, 0, 0, 0);
            return text;
        }

        private void DataKerbal_ToStringChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            AssignTemplate();
        }
    }
}
