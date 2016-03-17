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
        static GuiIcons Icons = new GuiIcons16();

        private DlgMessage(string message, string title, Image image)
        {
            InitializeComponent();

            Owner = Application.Current.MainWindow;
            if (title != null)
            {
                Title = title;
            }
            if (image != null)
            {
                Icon = image.Source;
            }
            TextMessage.Text = message;

            CalcNeededSize();
        }

        private DlgMessage(string message, string title)
            : this (message, title, null)
        {
        }

        private DlgMessage(string message)
            : this (message, null, null)
        {
        }

        /// <summary>
        /// Show a dialog window with given title and message.
        /// </summary>
        /// <param name="message">The message to show</param>
        /// <param name="title">The window title</param>
        /// <returns>True if "Ok" was clicked, false otherwise</returns>
        public static bool? Show(string message, string title) 
        {
            DlgMessage dlg = new DlgMessage(message, title);
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

        /// <summary>
        /// Show a dialog window with given message.
        /// </summary>
        /// <param name="message">The Syntax.Message to show</param>
        /// <returns>True if "Ok" was clicked, false otherwise</returns>
        public static bool? Show(Syntax.Message message)
        {
            string title = null;
            Image img = null;
            if (message is Syntax.ErrorMessage)
            {
                title = "KML Error";
                img = Icons.Error;
            }
            else if (message is Syntax.WarningMessage)
            {
                title = "KML Error";
                img = Icons.Warning;
            }
            DlgMessage dlg = new DlgMessage(message.ToString(true), title, img);
            return dlg.ShowDialog();
        }

        /// <summary>
        /// Show a dialog with a summary of all given messages.
        /// </summary>
        /// <param name="messages">A list of Syntax.Messages to show</param>
        /// <returns>True if "Ok" was clicked, false otherwise</returns>
        public static bool? Show(List<Syntax.Message> messages)
        {
            string title = null;
            Image img = null;
            StringBuilder str = new StringBuilder();
            if (messages.Any(x => x is Syntax.ErrorMessage))
            {
                title = "KML Error";
                img = Icons.Error;
            }
            else if (messages.Any(x => x is Syntax.WarningMessage))
            {
                title = "KML Warning";
                img = Icons.Warning;
            }
            if (Syntax.Messages.Count > 0)
            {
                str.Append(Syntax.Messages[0].ToString(true));
            }
            for (int i = 1; i < Syntax.Messages.Count; i++)
            {
                str.AppendLine();
                str.AppendLine();
                str.Append(Syntax.Messages[i].ToString(true));
            }
            DlgMessage dlg = new DlgMessage(str.ToString(), title, img);
            return dlg.ShowDialog();
        }

        /// <summary>
        /// Show a dialog with a summary of all given messages.
        /// The list will be cleared afterwards.
        /// </summary>
        /// <param name="messages">A list of Syntax.Messages to show</param>
        /// <returns>True if "Ok" was clicked, false otherwise</returns>
        public static bool? ShowAndClear(List<Syntax.Message> messages)
        {
            bool? result = Show(messages);
            messages.Clear();
            return result;
        }

        private void CalcNeededSize()
        {
            // Readout real border width and height from main window (theme, fontsize, whatever)
            double borderWidth = Application.Current.MainWindow.Width - (Application.Current.MainWindow.Content as Grid).ActualWidth; //16.0;
            double borderHeight = Application.Current.MainWindow.Height - (Application.Current.MainWindow.Content as Grid).ActualHeight; // 39.0;

            // Recalculate the needed size depending on content text,
            // pretending to have unlimited space
            TextMessage.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            TextMessage.Arrange(new Rect(TextMessage.DesiredSize));

            // Limit size to owner, wrap and scroll if needed
            if (TextMessage.DesiredSize.Width + borderWidth > Owner.ActualWidth)
            {
                Width = Owner.ActualWidth;
                TextMessage.TextWrapping = TextWrapping.Wrap;

                // Recalculate with word wrap
                TextMessage.Measure(new Size(Width - borderWidth, Double.PositiveInfinity));
                TextMessage.Arrange(new Rect(TextMessage.DesiredSize));
            }
            else
            {
                Width = TextMessage.DesiredSize.Width + borderWidth;
            }

            if (TextMessage.ActualHeight + borderHeight + ButtonOk.Height > Owner.ActualHeight)
            {
                Height = Owner.ActualHeight;
            }
            else
            {
                Height = TextMessage.ActualHeight + borderHeight + ButtonOk.Height;
            }
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
