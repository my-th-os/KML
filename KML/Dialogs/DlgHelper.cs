using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace KML
{
    /// <summary>
    /// Container for some reused methods for dialogs
    /// </summary>
    public class DlgHelper
    {
        /// <summary>
        /// Common initialisation for all dialog windows
        /// </summary>
        /// <param name="window">The dialog window to initialize</param>
        public static void Initialize(Window window)
        {
            window.Owner = Application.Current.MainWindow;
        }

        /// <summary>
        /// Arrange window size to content of a TextBox, with some additional height for buttons etc.
        /// </summary>
        /// <param name="window">The dialog window to resize</param>
        /// <param name="textBox">The TextBox with content to fit into this dialog</param>
        /// <param name="additionalHeight">The height for additional buttons etc.</param>
        public static void CalcNeededSize(Window window, TextBox textBox, double additionalHeight)
        {
            // Readout real border width and height from main window (theme, fontsize, whatever)
            double borderWidth = Application.Current.MainWindow.Width - (Application.Current.MainWindow.Content as Grid).ActualWidth; //16.0;
            double borderHeight = Application.Current.MainWindow.Height - (Application.Current.MainWindow.Content as Grid).ActualHeight; // 39.0;

            // Recalculate the needed size depending on content text,
            // pretending to have unlimited space
            textBox.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            textBox.Arrange(new Rect(textBox.DesiredSize));

            // Limit size to owner, wrap and scroll if needed
            if (textBox.DesiredSize.Width + borderWidth > window.Owner.ActualWidth)
            {
                window.Width = window.Owner.ActualWidth;
                textBox.TextWrapping = TextWrapping.Wrap;

                // Recalculate with word wrap
                textBox.Measure(new Size(window.Width - borderWidth, Double.PositiveInfinity));
                textBox.Arrange(new Rect(textBox.DesiredSize));
            }
            else
            {
                window.Width = textBox.DesiredSize.Width + borderWidth;
            }

            if (textBox.ActualHeight + borderHeight + additionalHeight > window.Owner.ActualHeight)
            {
                window.Height = window.Owner.ActualHeight;
            }
            else if (textBox.Text == null || textBox.Text.Length == 0)
            {
                window.Height = borderHeight + additionalHeight;
            }
            else
            {
                window.Height = textBox.ActualHeight + borderHeight + additionalHeight;
            }
        }
    }
}
