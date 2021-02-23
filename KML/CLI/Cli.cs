using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KML
{
    /// <summary>
    /// Command Line Interface, checks request for CLI on construction
    /// </summary>
    public class Cli
    {
        /// <summary>
        /// Determines if a CLI session was requested by known CLI arguments
        /// </summary>
        public bool Requested { get; private set; }

        private enum Mode { Info, Vessels, Kerbals, Warnings };

        private Mode mode = Mode.Info;
        private bool repair = false;
        private List<string> filenames = new List<string>();

        /// <summary>
        /// Creates a CLI instance
        /// </summary>
        public Cli(string[] args, int startarg)
        {
            Requested = false;
            mode = Mode.Warnings;
            filenames.Clear();

            for (int i = startarg; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg[0] == '-')
                {
                    string a = arg.ToLower();
                    if ((a == "-v") || (a == "-vessels") || (a == "--vessels"))
                    {
                        mode = Mode.Vessels;
                    }
                    else if ((a == "-k") || (a == "-kerbals") || (a == "--kerbals"))
                    {
                        mode = Mode.Kerbals;
                    }
                    else if ((a == "-w") || (a == "-warnings") || (a == "--warnings"))
                    {
                        mode = Mode.Warnings;
                    }
                    else if ((a == "-r") || (a == "-repair") || (a == "--repair"))
                    {
                        mode = Mode.Warnings;
                        repair = true;
                    }
                    else
                    {
                        mode = Mode.Info;
                    }
                    Requested = true;
                }
                else
                {
                    filenames.Add(arg);
                }
            }

            if (filenames.Count == 0)
            {
                mode = Mode.Info;
            }
        }

        /// <summary>
        /// Executes the CLI commands
        /// </summary>
        public void Execute()
        {
            if (mode == Mode.Info)
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string version = assembly.GetName().Version.ToString();
                while (version.EndsWith(".0"))
                    version = version.Substring(0, version.Length - 2);
                string copyright = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location).LegalCopyright.Replace("Copyright ", "");

                Console.WriteLine("KML - Kerbal Markup Lister - Version " + version + " - " + copyright);
                Console.WriteLine("Usage: KML [ --vessels | -v | --kerbals | -k | --warnings | -w | --repair | -r ] <save-file>");
            }
            else
            {
                foreach (var filename in filenames)
                {
                    ExecuteFile(filename);
                    Console.WriteLine();
                }
            }
        }

        /// <summary>
        /// Execute wrapped by try/catch in case there is no outer exception handler 
        /// see App.Application_DispatcherUnhandledException
        /// </summary>
        public int ExecuteCatch()
        {
            try
            {
                Execute();
                return 0;
            }
            catch (Exception e)
            {
                WriteLineColor(e.ToString(), ConsoleColor.Red);
                return 1;
            }
        }

        private void ExecuteFile(string filename)
        {
            bool isFileFound = System.IO.File.Exists(filename);
            bool isFileCraft = System.IO.Path.GetExtension(filename) == ".craft";
            bool isFileSave = System.IO.Path.GetExtension(filename) == ".sfs";

            if (!isFileFound)
            {
                WriteLineColor("File not found \"" + filename + "\"", ConsoleColor.Red);
            }
            else if (isFileCraft)
            {
                WriteLineColor("No CLI support for craft file \"" + filename + "\"", ConsoleColor.Red);
            }
            else if (!isFileSave)
            {
                WriteLineColor("Unknown type of file \"" + filename + "\"", ConsoleColor.Red);
            }
            else
            {
                Syntax.Messages.Clear();

                try
                {
                    var KmlRoots = KmlItem.ParseFile(filename);
                    WriteLineColor(mode.ToString() + " in \"" + filename + "\"", ConsoleColor.DarkCyan);

                    switch (mode)
                    {
                        case Mode.Vessels:
                            ListVessels(KmlRoots);
                            break;
                        case Mode.Kerbals:
                            ListKerbals(KmlRoots);
                            break;
                        case Mode.Warnings:
                            if (ProcessWarnings(KmlRoots))
                            {
                                Console.WriteLine();
                                // data was changed due to repair, save now (backup is built-in)
                                string backupname = KmlItem.WriteFile(filename, KmlRoots);
                                if (backupname.Length > 0 && System.IO.File.Exists(backupname))
                                {
                                    WriteLineColor("(backup) " + backupname, ConsoleColor.Green);
                                }
                                WriteLineColor("(saving) " + filename, ConsoleColor.Green);
                            }
                            break;
                    }
                }
                catch(System.IO.IOException e)
                {
                    WriteLineColor("File Error: " + e.Message, ConsoleColor.Red);
                }
            }
        }

        private void WriteLineColor(string line, ConsoleColor color)
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(line);
            Console.ForegroundColor = old;
        }

        private void ListVessels(List<KmlItem> KmlRoots)
        {
            List<KmlVessel> vessels = new List<KmlVessel>();
            foreach (KmlVessel vessel in GetFlatList<KmlVessel>(KmlRoots))
            {
                if (vessel.Origin == KmlVessel.VesselOrigin.Flightstate)
                {
                    vessels.Add(vessel);
                }
            }
            // Sort the list
            vessels = vessels.OrderBy(x => x.Name).ToList();

            Console.WriteLine();
            foreach (KmlVessel vessel in vessels)
            {
                Console.WriteLine(vessel.ToString());
            }
            if (vessels.Count == 0)
            {
                Console.WriteLine("(none)");
            }
        }

        private void ListKerbals(List<KmlItem> KmlRoots)
        {
            List<KmlKerbal> kerbals = new List<KmlKerbal>();
            foreach (KmlKerbal kerbal in GetFlatList<KmlKerbal>(KmlRoots))
            {
                if (kerbal.Origin == KmlKerbal.KerbalOrigin.Roster)
                {
                    kerbals.Add(kerbal);
                }
            }
            // Sort the list
            kerbals = kerbals.OrderBy(x => x.Name).ToList();

            Console.WriteLine();
            foreach (KmlKerbal kerbal in kerbals)
            {
                Console.WriteLine(kerbal.ToString());
            }
            if (kerbals.Count == 0)
            {
                Console.WriteLine("(none)");
            }
        }

        private bool ProcessWarnings(List<KmlItem> KmlRoots)
        {
            // make a copy, in case of repairs the original list changes
            List<Tuple<string, KmlItem>> warnings = new List<Tuple<string, KmlItem>>();

            // need to save file?
            bool repaired = false;

            foreach (Syntax.Message msg in new List<Syntax.Message>(Syntax.Messages))
            {
                // dock may need repair only once, but may appear multiple times in warnings
                // here we only keep sources if we do plan to repair and show a repaired message in any case
                KmlItem source = null;
                if (msg.Source is KmlPartDock)
                {
                    KmlPartDock dock = (KmlPartDock)msg.Source;
                    if (dock.NeedsRepair)
                    {
                        source = msg.Source;
                    }
                }
                warnings.Add(new Tuple<string, KmlItem>(msg.ToString(), source));
            }
            foreach (var warning in warnings)
            {
                Console.WriteLine();
                WriteLineColor(warning.Item1, ConsoleColor.Yellow);
                if (repair && warning.Item2 is KmlPartDock)
                {
                    // this is now a dock source that needed repair in the first iteration
                    KmlPartDock dock = (KmlPartDock)warning.Item2;
                    // repair only once
                    if (dock.NeedsRepair)
                    {
                        dock.Repair();
                        repaired = true;
                    }
                    // show repaired info in any case
                    WriteLineColor("(repaired) " + dock.ToString(), ConsoleColor.Green);
                }
            }

            if (warnings.Count == 0)
            {
                Console.WriteLine();
                Console.WriteLine("(none)");
            }
            return repaired;
        }

        private List<T> GetFlatList<T>(List<KmlItem> source) where T : KmlItem
        {
            List<T> target = new List<T>();
            foreach (KmlItem item in source)
            {
                if (item is T)
                {
                    target.Add((T)item);
                }
                if (item is KmlNode)
                {
                    target.AddRange(GetFlatList<T>(((KmlNode)item).AllItems));
                }
            }
            return target;
        }
    }
}
