using System;
using System.Runtime.InteropServices;

namespace KML
{
    /// <summary>
    /// Main program, WPF app or CLI
    /// </summary>
    public static class Program
    {
        private static Cli cli;

        /// <summary>
        /// Main entry point, no XAML involved so far
        /// </summary>
        /// <param name="args">CLI arguments</param>
        /// <returns></returns>
        [STAThread]
        public static int Main(string[] args)
        {
            // watch out for different index of first real argument:
            // cli = new Cli(Environment.GetCommandLineArgs(), 1);
            cli = new Cli(args, 0);
            if (cli.Requested)
            {
                return cli.ExecuteCatch();
            }
            else
            {
                FreeConsole();
                var app = new App();
                return app.Run();
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int FreeConsole();
    }
}
