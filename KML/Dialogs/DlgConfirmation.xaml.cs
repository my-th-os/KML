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
    /// A DlgConfirmation is used to display a message and let the user decide between "Ok" and "Cancel".
    /// </summary>
    public partial class DlgConfirmation : Window
    {
        private DlgConfirmation(string message, string title, Image image)
        {
            InitializeComponent();

            if (title != null)
            {
                Title = title;
            }
            if (image != null)
            {
                Icon = image.Source;
            }
            TextMessage.Text = message;

            DlgHelper.Initialize(this);
            DlgHelper.CalcNeededSize(this, TextMessage, ButtonOk.Height);
        }

        private DlgConfirmation(string message, string title)
            : this (message, title, null)
        {
        }

        private DlgConfirmation(string message)
            : this (message, null, null)
        {
        }

        /// <summary>
        /// Show a dialog window with given title and message.
        /// </summary>
        /// <param name="message">The message to show</param>
        /// <param name="title">The window title</param>
        /// <returns>True if "Ok" was clicked, false otherwise</returns>
        public static bool Show(string message, string title)
        {
            DlgConfirmation dlg = new DlgConfirmation(message, title);
            return dlg.ShowDialog() == true;
        }

        /// <summary>
        /// Show a dialog window with given message.
        /// </summary>
        /// <param name="message">The message to show</param>
        /// <returns>True if "Ok" was clicked, false otherwise</returns>
        public static bool Show(string message)
        {
            return Show(message, null);
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
        }
    }
}
