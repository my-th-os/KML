using System;

namespace KML
{
    /// <summary>
    /// Main program, Mono compatible, CLI only
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
            return cli.ExecuteCatch();
        }
    }
}
