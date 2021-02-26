using System;
using System.Collections.Generic;
using System.Linq;

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

        private enum Mode { Info, Tree, Vessels, Kerbals, Warnings };

        private Mode mode = Mode.Info;
        private bool repair = false;
        private bool select = false;
        private List<string> selectors = new List<string>();
        private List<string> filenames = new List<string>();

        /// <summary>
        /// Creates a CLI instance
        /// </summary>
        public Cli(string[] args, int startarg)
        {
            Requested = false;
            mode = Mode.Warnings;
            filenames.Clear();
            bool error = false;
            string selectorstr = "";

            for (int i = startarg; i < args.Length; i++)
            {
                var arg = args[i];
                bool isOpts = arg.Length > 0 && arg[0] == '-';
                bool isWord = isOpts && arg.Length > 1 && arg[1] == '-';

                string clean = "";
                if (isWord)
                {
                    clean = arg.Substring(2, arg.Length - 2);
                }
                else if (isOpts)
                {
                    clean = arg.Substring(1, arg.Length - 1);
                }

                char[] sep = {'='};
                var split = clean.Split(sep, 2);
                clean = split[0];

                if (isOpts)
                {
                    if (isWord)
                    {
                        switch (clean)
                        {
                            case "tree":
                                mode = Mode.Tree;
                                break;
                            case "vessels":
                                mode = Mode.Vessels;
                                break;
                            case "kerbals":
                                mode = Mode.Kerbals;
                                break;
                            case "warnings":
                                mode = Mode.Warnings;
                                break;
                            case "repair":
                                mode = Mode.Warnings;
                                repair = true;
                                break;
                            case "select":
                                if (split.Length > 1) selectorstr = split[1];
                                select = true;
                                break;
                            default:
                                error = true;
                                break;
                        }
                    }
                    else foreach(char c in clean)
                    {
                        switch (c)
                        {
                            case 't':
                                mode = Mode.Tree;
                                break;
                            case 'v':
                                mode = Mode.Vessels;
                                break;
                            case 'k':
                                mode = Mode.Kerbals;
                                break;
                            case 'w':
                                mode = Mode.Warnings;
                                break;
                            case 'r':
                                mode = Mode.Warnings;
                                repair = true;
                                break;
                            case 's':
                                if (split.Length > 1) selectorstr = split[1];
                                select = true;
                                break;
                            default:
                                error = true;
                                break;
                        }
                    }
                    if (!select && split.Length > 1)
                    {
                        error = true;
                    }
                    Requested = true;
                }
                else
                {
                    filenames.Add(arg);
                }
            }

            if (select && selectorstr.Length > 0)
            {
                selectors = new List<string>(selectorstr.Split('/'));
            }

            if (error || filenames.Count == 0)
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

                Console.WriteLine("KML: Kerbal Markup Lister " + version + " " + copyright);
                // Console.WriteLine("Use: KML [ --vessels | -v | --kerbals | -k | --warnings | -w | --repair | -r ] <save-file>");
                Console.WriteLine("Use: KML [Opt] <save-file>");
                Console.WriteLine("Opt: --tree     | -t : List tree");
                Console.WriteLine("     --vessels  | -v : List vessels");
                Console.WriteLine("     --kerbals  | -k : List kerbals");
                Console.WriteLine("     --warnings | -w : Show warnings");
                Console.WriteLine("     --select   | -s : Show numbers, select one by -s=<Sel>");
                Console.WriteLine("     --repair   | -r : Repair docking problems, includes -w");
                Console.WriteLine("Sel: < number | tag-start | name-start >[/Sel]");
                Console.WriteLine("     Only in tree you can select by tag or go deep into hierarchy");
                Console.WriteLine();
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
                    var roots = KmlItem.ParseFile(filename);
                    WriteLineColor(mode.ToString() + " in \"" + filename + "\"", ConsoleColor.DarkCyan);

                    switch (mode)
                    {
                        case Mode.Tree:
                            ListTree(roots);
                            break;
                        case Mode.Vessels:
                            ListVessels(roots);
                            break;
                        case Mode.Kerbals:
                            ListKerbals(roots);
                            break;
                        case Mode.Warnings:
                            if (ProcessWarnings())
                            {
                                Console.WriteLine();
                                // data was changed due to repair, save now (backup is built-in)
                                string backupname = KmlItem.WriteFile(filename, roots);
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

        private void ListTree(List<KmlItem> roots)
        {
            List<KmlNode> nodes = new List<KmlNode>();

            // Check if any of these roots is not a node
            // If so, pack all roots into a new ghost root
            if (roots.Any(x => !(x is KmlNode)))
            {
                KmlGhostNode root = new KmlGhostNode("[root]");
                root.AddRange(roots);
                nodes.Add(root);
            }
            else foreach (KmlItem item in roots)
            {
                item.CanBeDeleted = false;
                nodes.Add((KmlNode)item);
            }

            ListTree(nodes, "");
        }

        private void ListTree(List<KmlNode> nodes, string selectorprefix)
        {
            bool foundselection = false;

            Console.WriteLine();
            string selector = "";
            if (selectors.Count > 0)
            {
                selector = selectors[0];
                selectors.RemoveAt(0);
            }
            for (int i = 0; i < nodes.Count; i++)
            {
                if (selector.Length > 0)
                {
                    if (selector != i.ToString() && !nodes[i].Tag.StartsWith(selector) && !nodes[i].Name.StartsWith(selector))
                    {
                        continue;
                    }
                    else
                    {
                        Console.WriteLine((select ? selectorprefix + i.ToString() + ": " : "") + nodes[i].ToString());
                        foundselection = true;
                        // only list attribs on the finally selected node
                        if (selectors.Count == 0)
                        {
                            Console.WriteLine();
                            ListAttributes(nodes[i]);
                        }
                        ListTree(nodes[i].Children, selectorprefix + i.ToString() + "/");
                        break;
                    }
                }
                else
                {
                    Console.WriteLine((select ? selectorprefix + i.ToString() + ": " : "") + nodes[i].ToString());
                }
            }
            if (nodes.Count == 0)
            {
                Console.WriteLine("(no child nodes)");
            }
            else if (selector.Length > 0 && !foundselection)
            {
                Console.WriteLine("(no match)");
            }
        }

        private void ListVessels(List<KmlItem> roots)
        {
            bool foundselection = false;

            List<KmlVessel> vessels = new List<KmlVessel>();
            foreach (KmlVessel vessel in GetFlatList<KmlVessel>(roots))
            {
                if (vessel.Origin == KmlVessel.VesselOrigin.Flightstate)
                {
                    vessels.Add(vessel);
                }
            }
            // Sort the list
            vessels = vessels.OrderBy(x => x.Name).ToList();

            Console.WriteLine();
            string selector = "";
            if (selectors.Count > 0)
            {
                selector = selectors[0];
                selectors.RemoveAt(0);
            }
            for (int i = 0; i < vessels.Count; i++)
            {
                if (selector.Length > 0)
                {
                    if (selector != i.ToString() && !vessels[i].Name.StartsWith(selector))
                    {
                        continue;
                    }
                    else
                    {
                        Console.WriteLine((select ? i.ToString() + ": " : "") + vessels[i].ToString());
                        foundselection = true;
                        Console.WriteLine();
                        ListParts(vessels[i]);
                        break;
                    }
                }
                else
                {
                    Console.WriteLine((select ? i.ToString() + ": " : "") + vessels[i].ToString());
                }
            }
            if (vessels.Count == 0)
            {
                Console.WriteLine("(none)");
            }
            else if (selector.Length > 0 && !foundselection)
            {
                Console.WriteLine("(no match)");
            }
        }

        private void ListKerbals(List<KmlItem> roots)
        {
            bool foundselection = false;

            List<KmlKerbal> kerbals = new List<KmlKerbal>();
            foreach (KmlKerbal kerbal in GetFlatList<KmlKerbal>(roots))
            {
                if (kerbal.Origin == KmlKerbal.KerbalOrigin.Roster)
                {
                    kerbals.Add(kerbal);
                }
            }
            // Sort the list
            kerbals = kerbals.OrderBy(x => x.Name).ToList();

            Console.WriteLine();
            for (int i = 0; i < kerbals.Count; i++)
            {
                if (selectors.Count > 0)
                {
                    if (selectors[0] != i.ToString() && !kerbals[i].Name.StartsWith(selectors[0]))
                    {
                        continue;
                    }
                    else
                    {
                        Console.WriteLine((select ? i.ToString() + ": " : "") + kerbals[i].ToString());
                        foundselection = true;
                        Console.WriteLine();
                        ListAttributes(kerbals[i]);
                        break;
                    }
                }
                else
                {
                    Console.WriteLine((select ? i.ToString() + ": " : "") + kerbals[i].ToString());
                }
            }
            if (kerbals.Count == 0)
            {
                Console.WriteLine("(none)");
            }
            else if (selectors.Count > 0 && !foundselection)
            {
                Console.WriteLine("(no match)");
            }
        }

        private void ListParts(KmlVessel vessel)
        {
            bool foundselection = false;

            string selector = "";
            if (selectors.Count > 0)
            {
                selector = selectors[0];
                selectors.RemoveAt(0);
            }
            foreach (var part in vessel.Parts)
            {
                if (selector.Length > 0)
                {
                    if (!part.ToString().Replace(", root", "").StartsWith("PART [" + selector + "]") && !part.Name.StartsWith(selector))
                    {
                        continue;
                    }
                    else
                    {
                        Console.WriteLine(part.ToString());
                        foundselection = true;
                        Console.WriteLine();
                        ListAttributes(part);
                        break;
                    }
                }
                else
                {
                    Console.WriteLine(part.ToString());
                }
            }
            if (vessel.Parts.Count == 0)
            {
                Console.WriteLine("(no parts)");
            }
            else if (selector.Length > 0 && !foundselection)
            {
                Console.WriteLine("(no match)");
            }
        }

        private void ListAttributes(KmlNode node)
        {
            foreach (var attrib in node.Attribs)
            {
                Console.WriteLine(attrib.ToString());
            }
            if (node.Attribs.Count == 0)
            {
                Console.WriteLine("(no attributes)");
            }
        }

        private bool ProcessWarnings()
        {
            // make a copy, in case of repairs the original list changes
            List<Tuple<string, KmlItem>> warnings = new List<Tuple<string, KmlItem>>();

            // need to save file?
            bool repaired = false;

            bool foundselection = false;

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
            for (int i = 0; i < warnings.Count; i++)
            {
                if (selectors.Count > 0)
                {
                    if (selectors[0] != i.ToString())
                    {
                        continue;
                    }
                    else
                    {
                        foundselection = true;
                    }
                }

                var warning = warnings[i];
                Console.WriteLine();
                WriteLineColor((select ? i.ToString() + ": " : "") + warning.Item1, ConsoleColor.Yellow);
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
            else if(selectors.Count > 0 && !foundselection)
            {
                Console.WriteLine();
                Console.WriteLine("(no match)");
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
