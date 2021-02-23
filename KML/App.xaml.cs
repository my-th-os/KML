using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace KML
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Application entry point, App is instanciated by Program.Main()
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                GuiIcons ico = new GuiIcons16();
                string txt = "Sorry, an unhandled exception occurred!\n\n" +
                    "Please report follwing details on the KSP forum thread or via GitHub issue:\n\n" + e.Exception.ToString();
                DlgMessage.Show(txt, "KML Exception", ico.Error);
            }
            catch
            {
                // maybe something breaks the DlgMessage, e.g. there is no main window (to center onto), then fall back to MessageBox
                MessageBox.Show(e.Exception.ToString(), "KML Exception");
            }
            e.Handled = true;
        }
    }
}
