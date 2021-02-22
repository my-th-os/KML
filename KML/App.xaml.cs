using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace KML
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        private Cli cli;

        /// <summary>
        /// Main entry point when application starts, checks if CLI mode or GUI
        /// </summary>
        /// <param name="e">StartupEventArgs</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            cli = new Cli(Environment.GetCommandLineArgs());
            if (cli.Requested)
            {
                cli.Execute();
                Environment.Exit(0);
            }
            else
            {
                FreeConsole();
                var gui = new MainWindow();
                gui.Show();
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (cli != null && cli.Requested)
            {
                ConsoleColor old = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Exception.ToString());
                Console.ForegroundColor = old;
            }
            else
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
            }
            e.Handled = true;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int FreeConsole();
    }
}
