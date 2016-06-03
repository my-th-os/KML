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
        private static GuiIcons Icons = new GuiIcons16();

        private DlgMessage(string message, string title, Image image)
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

        private DlgMessage(string message, string title)
            : this (message, title, null)
        {
        }

        private DlgMessage(string message)
            : this (message, null, null)
        {
        }

        /// <summary>
        /// Show a dialog window with given title, message and image.
        /// </summary>
        /// <param name="message">The message to show</param>
        /// <param name="title">The window title</param>
        /// <param name="image">The image for the window icon</param>
        /// <returns>True if "Ok" was clicked, false otherwise</returns>
        public static bool Show(string message, string title, Image image) 
        {
            DlgMessage dlg = new DlgMessage(message, title, image);
            return dlg.ShowDialog() == true;
        }

        /// <summary>
        /// Show a dialog window with given title and message.
        /// </summary>
        /// <param name="message">The message to show</param>
        /// <param name="title">The window title</param>
        /// <returns>True if "Ok" was clicked, false otherwise</returns>
        public static bool Show(string message, string title)
        {
            return Show(message, title, null);
        }

        /// <summary>
        /// Show a dialog window with given message.
        /// </summary>
        /// <param name="message">The message to show</param>
        /// <returns>True if "Ok" was clicked, false otherwise</returns>
        public static bool Show(string message)
        {
            return Show(message, null, null);
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
            else
            {
                // Usually Show is called when either a warning/error
                // or a successful message is expected.
                // Should be avoided to have no feedback.
                string txt = "Sorry, nothing happened or no feedback from action!\n\n" +
                    "Please report your steps and any details on the KSP forum thread or via GitHub issue.";
                title = "KML Warning";
                img = Icons.Warning;
                str.Append(txt);
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

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
