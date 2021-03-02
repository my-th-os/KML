using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace KML
{
    /// <summary>
    /// A DlgSearch is used to search for text input in the KML structure
    /// </summary>
    public partial class DlgSearch : Window
    {
        // Preserve state between multiple windows,
        // hiding instead of closing causes more problems than it helps
        private static double _width = 0;
        private static double _height = 0;
        private static string _searchString = "";
        private static bool _checkNodeTag = true;
        private static bool _checkNodeText = true;
        private static bool _checkAttribName = true;
        private static bool _checkAttribValue = true;
        private static KmlItem _selectedItem;
        private static int _searchContinued = 0;
        private static List<KmlItem> _searchResults = new List<KmlItem>();

        private DispatcherTimer SearchTimer { get; set; }
        
        private DlgSearch(string presetText)
        {
            InitializeComponent();

            SearchTimer = new DispatcherTimer();
            SearchTimer.Interval = TimeSpan.FromSeconds(1.0);
            SearchTimer.Tick += SearchTimer_Tick;

            DlgHelper.Initialize(this);

            if (presetText != null)
            {
                TextBoxInput.Text = presetText;
            }
            else
            {
                TextBoxInput.Text = _searchString;
            }
            // Select all
            TextBoxInput.SelectAll();
            TextBoxInput.Focus();

            CheckNodeTag.IsChecked = _checkNodeTag;
            CheckNodeText.IsChecked = _checkNodeText;
            CheckAttribName.IsChecked = _checkAttribName;
            CheckAttribValue.IsChecked = _checkAttribValue;

            if (TextBoxInput.Text.Length > 2)
            {
                Search();
                foreach (TreeViewItem item in Tree.Items)
                {
                    if (item.DataContext == _selectedItem)
                    {
                        item.IsSelected = true;
                    }
                    foreach (TreeViewItem sub in item.Items)
                    {
                        if (sub.DataContext == _selectedItem)
                        {
                            sub.IsSelected = true;
                        }
                    }
                }
            }
            

            if (_width > 0 && _height > 0)
            {
                Width = _width;
                Height = _height;
            }

            // Scroll to selected item
            if (Tree.SelectedItem is TreeViewItem)
                (Tree.SelectedItem as TreeViewItem).BringIntoView();

            // We need to measure the ActualHeight of TextBoxInput,
            // because it reads 0.0 if it's not set, unlesse Arrage is called.
            // And it's not set on purpose to use system default.
            //TextBoxInput.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            //TextBoxInput.Arrange(new Rect(TextBoxInput.DesiredSize));

            //DlgHelper.CalcNeededSize(this, TextMessage, ButtonOk.Height + TextBoxInput.ActualHeight);
        }

        /// <summary>
        /// Show a search dialog.
        /// </summary>
        /// <param name="left">The left coordinate for the dialog</param>
        /// <param name="top">The top coordinate for the dialog</param>
        /// <param name="selectedItem">Out: Returns the selected KmlItem if "Ok" was clicked, null otherwise</param>
        /// <param name="presetText">The preset text to search for</param>
        /// <returns>True if "Ok" was clicked, false otherwise</returns>
        public static bool Show(double left, double top, out KmlItem selectedItem, string presetText = null)
        {
            DlgSearch dlg = new DlgSearch(presetText);
            dlg.Left = left;
            dlg.Top = top;
            return dlg.Show(out selectedItem);
        }

        /// <summary>
        /// Show a search dialog.
        /// </summary>
        /// <param name="position">The left top position for the dialog </param>
        /// <param name="selectedItem">Out: Returns the selected KmlItem if "Ok" was clicked, null otherwise</param>
        /// <param name="presetText">The preset text to search for</param>
        /// <returns>True if "Ok" was clicked, false otherwise</returns>
        public static bool Show(Point position, out KmlItem selectedItem, string presetText = null)
        {
            return Show(position.X, position.Y, out selectedItem, presetText);
        }

        /// <summary>
        /// Show a search dialog.
        /// </summary>
        /// <param name="elementToShowBelow">A FrameworkElement to get the position from where the dialog will be placed under</param>
        /// <param name="selectedItem">Out: Returns the selected KmlItem if "Ok" was clicked, null otherwise</param>
        /// <param name="presetText">The preset text to search for</param>
        /// <returns>True if "Ok" was clicked, false otherwise</returns>
        public static bool Show(FrameworkElement elementToShowBelow, out KmlItem selectedItem, string presetText = null)
        {
            DlgSearch dlg = new DlgSearch(presetText);
            if (elementToShowBelow != null)
            {
	            // Get the screen position of the button
                GeneralTransform transform = elementToShowBelow.TransformToAncestor(dlg.Owner);
                // Matrix is Identity on my PC but scaled on my notebook, maybe DPI settings
                Matrix m = PresentationSource.FromVisual(dlg.Owner).CompositionTarget.TransformFromDevice;
                Point rootPoint = transform.Transform(new Point(-9, elementToShowBelow.ActualHeight + 3));
                // Now get the point of the button in the screen
                Point point = dlg.Owner.PointToScreen(rootPoint);
                point = m.Transform(point);
                dlg.Left = point.X;
                dlg.Top = point.Y;
            }
            return dlg.Show(out selectedItem);
        }

        private bool Show(out KmlItem selectedItem)
        {
            bool? result = ShowDialog();
            selectedItem = null;
            if (result == true)
            {
                _searchString = TextBoxInput.Text;
                _checkNodeTag = CheckNodeTag.IsChecked == true;
                _checkNodeText = CheckNodeText.IsChecked == true;
                _checkAttribName = CheckAttribName.IsChecked == true;
                _checkAttribValue = CheckAttribValue.IsChecked == true;
                if (Tree.SelectedItem is TreeViewItem && (Tree.SelectedItem as TreeViewItem).DataContext is KmlItem)
                {
                    _selectedItem = (Tree.SelectedItem as TreeViewItem).DataContext as KmlItem;
                    selectedItem = _selectedItem;
                }
            }
            _width = ActualWidth;
            _height = ActualHeight;
            return result == true;
        }

        private void Search(bool continued = false)
        {
            const int SEARCHLIMIT = 100;

            List<KmlItem> list;
            // TODO DlgSearch.Search(): loading the list on every continuation isn't the best choice
            // but it's ok, because that is fast, only loading items into tree is slow
            list = GuiTabsManager.GetCurrent().TreeManager.Search(TextBoxInput.Text,
                CheckNodeTag.IsChecked == true, CheckNodeText.IsChecked == true,
                CheckAttribName.IsChecked == true, CheckAttribValue.IsChecked == true);

            _searchResults.Clear();
            _searchResults.AddRange(list);

            int maxItemCount = SEARCHLIMIT;
            KmlNode parentNode = null;
            TreeViewItem parentItem = null;
            if (continued)
            {
                _searchContinued++;
                maxItemCount = SEARCHLIMIT * (_searchContinued + 1);
                parentItem = (TreeViewItem)Tree.Items[Tree.Items.Count - 1];
                parentNode = list[maxItemCount - SEARCHLIMIT - 1].Parent;
            }
            else
            {
                _searchContinued = 0;
                Tree.Items.Clear();
            }
            for (int i = maxItemCount - SEARCHLIMIT; i < list.Count; i++)
            {
                KmlItem item = list[i];
                // if (Tree.Items.Count >= maxParentCount)
                if (i >= maxItemCount)
                {
                    TreeViewItem dummy = new TreeViewItem();
                    dummy.Header = "...";
                    TreeViewItem toBeContinued = new TreeViewItem();
                    toBeContinued.Header = "... " + (list.Count - maxItemCount) + " more items ...";
                    toBeContinued.Expanded += ToBeContinued_Expanded;
                    toBeContinued.Items.Add(dummy);
                    Tree.Items.Add(toBeContinued);
                    return;
                }
                if (item is KmlNode || item is KmlAttrib)
                {
                    if (item.Parent != null && item.Parent != parentNode)
                    {
                        parentNode = item.Parent;
                        parentItem = new TreeViewItem();
                        parentItem.Header = item.Parent.PathToString(@"\");
                        Tree.Items.Add(parentItem);
                        parentItem.IsExpanded = true;
                    }
                    TreeViewItem node;
                    if (item is KmlNode)
                    {
                        node = new GuiTreeNode((KmlNode)item, true, true, true, false, false, true, false);
                    }
                    else
                    {
                        node = new TreeViewItem();
                        node.DataContext = item;
                        node.Header = item.ToString();
                    }
                    node.Margin = new Thickness(-16, 0, 0, 0);
                    if (parentNode == null)
                    {
                        Tree.Items.Add(node);
                    }
                    else
                    {
                        parentItem.Items.Add(node);
                    }
                    node.IsSelected = Tree.SelectedItem == null;
                }
            }
        }

        /// <summary>
        /// Get next result from last search.
        /// </summary>
        /// <returns>The next KmlItem from search</returns>
        public static KmlItem SearchNext()
        {
            int i = _searchResults.IndexOf(_selectedItem);
            if (i >= 0 && i < _searchResults.Count - 1)
            {
                _selectedItem = _searchResults[i + 1];
            }
            return _selectedItem;
        }

        /// <summary>
        /// Get previous result from last search.
        /// </summary>
        /// <returns>The previous KmlItem from search</returns>
        public static KmlItem SearchPrevious()
        {
            int i = _searchResults.IndexOf(_selectedItem);
            if (i > 0 && i < _searchResults.Count)
            {
                _selectedItem = _searchResults[i - 1];
            }
            return _selectedItem;
        }

        /// <summary>
        /// Resets search results.
        /// </summary>
        public static void SearchReset()
        {
            _searchResults.Clear();
            _selectedItem = null;
        }

        private void ToBeContinued_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem toBeContinued = (sender as TreeViewItem);
            // toBeContinued.Expanded -= ToBeContinued_Expanded;
            // toBeContinued.Items.Clear();
            Tree.Items.Remove(toBeContinued);
            Search(true);
        }

        void SearchTimer_Tick(object sender, EventArgs e)
        {
            SearchTimer.Stop();
            if (TextBoxInput.Text.Length > 2)
            {
                Search();
            }
            else
            {
                Tree.Items.Clear();
            }
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ButtonCancel.Width = ButtonOk.Width = (Content as Grid).ActualWidth / 2.0;
            Tree.Height = ActualHeight - PanelTop.ActualHeight - ButtonOk.ActualHeight;
        }

        private void TextBoxInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.Visibility == System.Windows.Visibility.Visible)
            {
                if (SearchTimer.IsEnabled)
                {
                    // Restart and wait another second
                    SearchTimer.Stop();
                }
                SearchTimer.Start();
            }
        }

        private void Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ButtonOk.IsEnabled = Tree.SelectedItem is TreeViewItem &&  (Tree.SelectedItem as TreeViewItem).DataContext is KmlItem;
        }

        private void Check_Click(object sender, RoutedEventArgs e)
        {
            TextBoxInput_TextChanged(sender, new TextChangedEventArgs(e.RoutedEvent, UndoAction.None));
        }

        private void Tree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Tree.SelectedItem is GuiTreeNode)
            {
                ButtonOk_Click(ButtonOk, new RoutedEventArgs());
            }
        }
    }
}
