using System.Windows;
using System.Windows.Controls;

namespace KML
{
    /// <summary>
    /// The GuiWarningsManager manages one Listview to display possible warnings and errors.
    /// </summary>
    class GuiWarningsManager : IGuiManager
    {
        /// <summary>
        /// Get true if the warnings list is empty, false if there are warnings.
        /// </summary>
        public bool IsEmpty { get { return WarningsList.Items.Count == 0; } }

        private GuiTabsManager Master { get; set; }
        private ListView WarningsList { get; set; }

        /// <summary>
        /// Creates a GuiWarningsManager to link and manage the given ListView.
        /// </summary>
        /// <param name="master">The master GuiTabsManager</param>
        /// <param name="warningsList">The ListView to manage</param>
        public GuiWarningsManager(GuiTabsManager master, ListView warningsList)
        {
            Master = master;
            WarningsList = warningsList;
        }

        /// <summary>
        /// Initialize before every loading of the KMl tree.
        /// </summary>
        public void BeforeTreeLoad()
        {
            Syntax.Messages.Clear();
        }

        /// <summary>
        /// Load all warnings from KML tree. Tree needs to be loaded first.
        /// </summary>
        public void AfterTreeLoad()
        {
            WarningsList.Items.Clear();
            foreach (Syntax.Message msg in Syntax.Messages)
            {
                GuiWarningsNode node = new GuiWarningsNode(msg);
                node.MouseDoubleClick += WarningsNode_MouseDoubleClick;
                WarningsList.Items.Add(node);
            }
            // All current messages are shown in the list, 
            // so they are handled and can be cleared from the list.
            // Future messages will be shown in a DlgMessage.
            Syntax.Messages.Clear();
        }

        /// <summary>
        /// Focus the standard control. Also select first item in the list, 
        /// if there is one and none is selected.
        /// </summary>
        public void Focus()
        {
            if(WarningsList.SelectedIndex < 0)
            {
                Next();
            }
            else
            {
                Application.Current.MainWindow.UpdateLayout();
                (WarningsList.SelectedItem as ListViewItem).Focus();
            }
        }

        /// <summary>
        /// Selects next warning in the list.
        /// </summary>
        public void Next()
        {
            int selectIndex = WarningsList.SelectedIndex + 1;
            if (selectIndex < WarningsList.Items.Count)
            {
                WarningsList.SelectedIndex = selectIndex;
                Focus();
            }
        }

        /// <summary>
        /// Selects previous warning in the list.
        /// </summary>
        public void Previous()
        {
            int selectIndex = WarningsList.SelectedIndex - 1;
            if (selectIndex >= 0)
            {
                WarningsList.SelectedIndex = selectIndex;
                Focus();
            }
        }

        /// <summary>
        /// Some key was pressed.
        /// </summary>
        public void CommandExec(string Command)
        {
            if (WarningsList.SelectedItem is GuiWarningsNode)
                (WarningsList.SelectedItem as GuiWarningsNode).CommandExec(Command);
        }

        /// <summary>
        /// Select should be called from within other GuiManagers
        /// and wants this manager to get avtive and go to given item.
        /// </summary>
        /// <param name="item">The KmlItem to select</param>
        /// <returns>Whether item was found or not</returns>
        public bool Select(KmlItem item)
        {
            foreach(GuiWarningsNode node in WarningsList.Items)
            {
                if(node.DataMessage.Source == item)
                {
                    // Force a refreh, by causing SelectionChanged to invoke
                    WarningsList.SelectedItem = null;
                    WarningsList.SelectedItem = node;
                    WarningsList.ScrollIntoView(node);
                    Focus();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get the selected KmlItem. Will be needed to check if
        /// views have to be refreshed.
        /// </summary>
        /// <returns>The currently selected KmlItem</returns>
        public KmlItem GetSelectedItem()
        {
            if (WarningsList.SelectedItem is GuiWarningsNode)
            {
                return (WarningsList.SelectedItem as GuiWarningsNode).DataMessage.Source;
            }
            return null;
        }

        private void WarningsNode_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Master.Select((sender as GuiWarningsNode).DataMessage.Source);
        }
    }
}
