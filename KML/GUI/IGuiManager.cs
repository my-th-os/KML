namespace KML
{
    /// <summary>
    /// This interface defines standard funktions every GuiManager must have,
    /// like methods to react on pressed buttons from toolbar etc.
    /// </summary>
    interface IGuiManager
    {
        /// <summary>
        /// Focus the standard control. Also possibly select first item in a list, 
        /// if there is one and none is selected.
        /// </summary>
        void Focus();

        /// <summary>
        /// Toolbar navigation next was clicked.
        /// Implementing classes should react to this.
        /// </summary>
        void Next();

        /// <summary>
        /// Toolbar navigation previous was clicked.
        /// Implementing classes should react to this.
        /// </summary>
        void Previous();

        /// <summary>
        /// Some key was pressed.
        /// Implementing classes should react to this.
        /// </summary>
        void CommandExec(string Command);

        /// <summary>
        /// Select should be called from within other GuiManagers
        /// and wants this manager to get avtive and go to given item.
        /// </summary>
        /// <param name="item">The KmlItem to select</param>
        /// <returns>Whether item was found or not</returns>
        bool Select(KmlItem item);

        /// <summary>
        /// Get the selected KmlItem. Will be needed to check if
        /// views have to be refreshed.
        /// </summary>
        /// <returns>The currently selected KmlItem</returns>
        KmlItem GetSelectedItem();
    }
}
