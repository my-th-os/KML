using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KML
{
    /// <summary>
    /// A DlgMessage is used to just display a message where you only can click "Ok".
    /// </summary>
    public partial class DlgMessage : Window
    {
        private DlgMessage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Show a dialog window with given title and message.
        /// </summary>
        /// <param name="message">The message to show</param>
        /// <param name="title">The window title</param>
        /// <returns>True if "Ok" was clicked, false otherwise</returns>
        public static bool? Show(string message, string title) 
        {
            DlgMessage dlg = new DlgMessage();
            dlg.Owner = Application.Current.MainWindow;
            if (title != null)
            {
                dlg.Title = title;
            }
            dlg.TextMessage.Text = message;
            return dlg.ShowDialog();
        }

        /// <summary>
        /// Show a dialog window with given message.
        /// </summary>
        /// <param name="message">The message to show</param>
        /// <returns>True if "Ok" was clicked, false otherwise</returns>
        public static bool? Show(string message)
        {
            return Show(message, null);
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
