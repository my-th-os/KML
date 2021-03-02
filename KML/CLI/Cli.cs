using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
        private enum Action { None, Version, Export, ImportReplace, ImportBefore, ImportAfter, Delete };

        private Mode mode = Mode.Info;
        private bool repair = false;
        private bool select = false;
        private bool multiselect = false;
        private List<string> selectors = new List<string>();
        private List<string> filenames = new List<string>();
        private bool quiet = false;
        private Action action = Action.None;
        private string actionfilename = "";
        private bool filechanged = false;

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
                            case "version":
                                // this is not an error but it forces the info page
                                error = true;
                                action = Action.Version;
                                break;
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
                            case "multiselect":
                                if (split.Length > 1) selectorstr = split[1];
                                select = true;
                                multiselect = true;
                                break;
                            case "export":
                                if (action != Action.None) error = true;
                                if (split.Length > 1)
                                    actionfilename = split[1];
                                else
                                    quiet = true;
                                action = Action.Export;
                                break;
                            case "import-replace":
                                if (action != Action.None || split.Length <= 1) 
                                    error = true;
                                else
                                    actionfilename = split[1];
                                action = Action.ImportReplace;
                                break;
                            case "import-before":
                                if (action != Action.None || split.Length <= 1)
                                    error = true;
                                else
                                    actionfilename = split[1];
                                action = Action.ImportBefore;
                                break;
                            case "import-after":
                                if (action != Action.None || split.Length <= 1)
                                    error = true;
                                else
                                    actionfilename = split[1];
                                action = Action.ImportAfter;
                                break;
                            case "delete":
                                if (action != Action.None) error = true;
                                action = Action.Delete;
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
                            case 'm':
                                if (split.Length > 1) selectorstr = split[1];
                                select = true;
                                multiselect = true;
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

            if ((action == Action.Export || ActionIsImport(action)) && 
                (!select || multiselect || selectorstr.Length == 0 || mode == Mode.Warnings))
            {
                error = true;
            }
            if (action == Action.Delete && 
                (!select || selectorstr.Length == 0 || mode == Mode.Warnings))
            {
                error = true;
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
                if (action == Action.Version)
                {
                    UpdateChecker.CheckUpdateCli();
                    Console.WriteLine();
                    return;
                }
                Console.WriteLine("Use: KML [Opt] <save-file>");
                Console.WriteLine("Opt: --tree             | -t : List tree");
                Console.WriteLine("     --vessels          | -v : List vessels");
                Console.WriteLine("     --kerbals          | -k : List kerbals");
                Console.WriteLine("     --warnings         | -w : Show warnings");
                Console.WriteLine("     --repair           | -r : Repair docking problems, includes -w");
                Console.WriteLine("     --select           | -s : Show numbers, select one by -s=<Sel>");
                Console.WriteLine("     --multiselect      | -m : Select all occurences by tag/name, includes -s");
                Console.WriteLine("     --version               : Show version and check online for updates");
                Console.WriteLine("     Actions on selection, need -s=<Sel> or -m=<Sel>, only one of:");
                Console.WriteLine("     --export=<file>         : Export selection, no -m, defaults <file> to stdout");
                Console.WriteLine("     --import-replace=<file> : Import file to replace selection, no -m");
                Console.WriteLine("     --import-before=<file>  : Import file as new before selection, no -m");
                Console.WriteLine("     --import-after=<file>   : Import file as new after selection, no -m");
                Console.WriteLine("     --delete                : Delete selection, -m is allowed");
                Console.WriteLine("Sel: < number | tag-start | name-start >[/Sel]");
                Console.WriteLine("     Only in tree you can select by tag or go deep into hierarchy");
                Console.WriteLine();
            }
            else
            {
                foreach (var filename in filenames)
                {
                    ExecuteFile(filename);
                    WriteLine();
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
                WriteLineColor(e.ToString(), ConsoleColor.Red, true);
                return 1;
            }
        }

        private void ExecuteFile(string filename)
        {
            filechanged = false;

            bool isFileFound = System.IO.File.Exists(filename);
            bool isFileCraft = System.IO.Path.GetExtension(filename) == ".craft";
            bool isFileSave = System.IO.Path.GetExtension(filename) == ".sfs";

            if (!isFileFound)
            {
                WriteLineColor("File not found \"" + filename + "\"", ConsoleColor.Red, true);
            }
            else if (isFileCraft)
            {
                WriteLineColor("No CLI support for craft file \"" + filename + "\"", ConsoleColor.Red, true);
            }
            else if (!isFileSave)
            {
                WriteLineColor("Unknown type of file \"" + filename + "\"", ConsoleColor.Red, true);
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
                            ProcessWarnings();
                            break;
                    }
                    if (filechanged) SaveFile(filename, roots);
                }
                catch (System.IO.IOException e)
                {
                    WriteLineColor("File Error: " + e.Message, ConsoleColor.Red, true);
                }
            }
        }

        private void SaveFile(string filename, List<KmlItem> roots)
        {
            WriteLine();
            // data was changed due to repair, save now (backup is built-in)
            string backupname = KmlItem.WriteFile(filename, roots);
            if (backupname.Length > 0 && System.IO.File.Exists(backupname))
            {
                WriteLineColor("(backup) " + backupname, ConsoleColor.Green);
            }
            WriteLineColor("(saving) " + filename, ConsoleColor.Green);
        }

        private void WriteLine(string line = "", bool force = false)
        {
            if (quiet && !force) return;
            Console.WriteLine(line);
        }

        private void WriteLineColor(string line, ConsoleColor color, bool force = false)
        {
            if (quiet && ! force) return;
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

            string selector = "";
            if (selectors.Count > 0)
            {
                selector = selectors[0];
                selectors.RemoveAt(0);
            }
            else if (nodes.Count > 0)
            {
                WriteLine();
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
                        WriteLine();
                        WriteLine((select ? selectorprefix + i.ToString() + ": " : "") + nodes[i].ToString());
                        foundselection = true;
                        // only delete or list attribs on the finally selected node
                        if (selectors.Count == 0)
                        {
                            if (action == Action.Export)
                            {
                                ExportNode(nodes[i]);
                                break;
                            }
                            else if (ActionIsImport(action))
                            {
                                ImportNode(nodes[i]);
                                break;
                            }
                            else if (action == Action.Delete)
                            {
                                DeleteNode(nodes[i]);
                                // stop going deeper
                                if (multiselect) continue; else break;
                            }
                            WriteLine();
                            ListAttributes(nodes[i]);
                        }
                        ListTree(nodes[i].Children, selectorprefix + i.ToString() + "/");
                        if (!multiselect) break;
                    }
                }
                else
                {
                    WriteLine((select ? selectorprefix + i.ToString() + ": " : "") + nodes[i].ToString());
                }
            }
            if (nodes.Count == 0)
            {
                WriteLine();
                WriteLine("(no child nodes)");
            }
            else if (selector.Length > 0 && !foundselection)
            {
                WriteLine();
                WriteLine("(no match)");
            }
            // push back the selector, to be used recursively
            if (selector.Length > 0)
            {
                selectors.Insert(0, selector);
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

            string selector = "";
            if (selectors.Count > 0)
            {
                selector = selectors[0];
                selectors.RemoveAt(0);
            }
            else if (vessels.Count > 0)
            { 
                WriteLine();
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
                        WriteLine();
                        WriteLine((select ? i.ToString() + ": " : "") + vessels[i].ToString());
                        foundselection = true;
                        // only delete on the finally selected node
                        if (selectors.Count == 0 && action == Action.Export)
                        {
                            ExportNode(vessels[i]);
                            break;
                        }
                        else if (selectors.Count == 0 && ActionIsImport(action))
                        {
                            ImportNode(vessels[i]);
                            break;
                        }
                        else if (selectors.Count == 0 && action == Action.Delete)
                        {
                            DeleteNode(vessels[i]);
                            // stop going deeper
                            if (multiselect) continue; else break;
                        }
                        ListParts(vessels[i]);
                        if (!multiselect) break;
                    }
                }
                else
                {
                    WriteLine((select ? i.ToString() + ": " : "") + vessels[i].ToString());
                }
            }
            if (vessels.Count == 0)
            {
                WriteLine();
                WriteLine("(none)");
            }
            else if (selector.Length > 0 && !foundselection)
            {
                WriteLine();
                WriteLine("(no match)");
            }
            // push back the selector, to be used recursively
            if (selector.Length > 0)
            {
                selectors.Insert(0, selector);
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

            string selector = "";
            if (selectors.Count > 0)
            {
                selector = selectors[0];
                selectors.RemoveAt(0);
            }
            else if (kerbals.Count > 0)
            {
                WriteLine();
            }
            for (int i = 0; i < kerbals.Count; i++)
            {
                if (selector.Length > 0)
                {
                    if (selector != i.ToString() && !kerbals[i].Name.StartsWith(selector))
                    {
                        continue;
                    }
                    else
                    {
                        WriteLine();
                        WriteLine((select ? i.ToString() + ": " : "") + kerbals[i].ToString());
                        foundselection = true;
                        // only delete on the finally selected node
                        if (selectors.Count == 0 && action == Action.Export)
                        {
                            ExportNode(kerbals[i]);
                            break;
                        }
                        else if (selectors.Count == 0 && ActionIsImport(action))
                        {
                            ImportNode(kerbals[i]);
                            break;
                        }
                        else if (selectors.Count == 0 && action == Action.Delete)
                        {
                            DeleteNode(kerbals[i]);
                            // stop going deeper
                            if (multiselect) continue; else break;
                        }
                        WriteLine();
                        ListAttributes(kerbals[i]);
                        if (!multiselect) break;
                    }
                }
                else
                {
                    WriteLine((select ? i.ToString() + ": " : "") + kerbals[i].ToString());
                }
            }
            if (kerbals.Count == 0)
            {
                WriteLine();
                WriteLine("(none)");
            }
            else if (selector.Length > 0 && !foundselection)
            {
                WriteLine();
                WriteLine("(no match)");
            }
            // push back the selector, to be used recursively
            if (selector.Length > 0)
            {
                selectors.Insert(0, selector);
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
            else if (vessel.Parts.Count > 0)
            {
                WriteLine();
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
                        WriteLine();
                        WriteLine(part.ToString());
                        foundselection = true;
                        // only delete on the finally selected node
                        if (selectors.Count == 0 && action == Action.Export)
                        {
                            ExportNode(part);
                            break;
                        }
                        else if (selectors.Count == 0 && ActionIsImport(action))
                        {
                            ImportNode(part);
                            break;
                        }
                        else if (selectors.Count == 0 && action == Action.Delete)
                        {
                            DeleteNode(part);
                            // stop going deeper
                            if (multiselect) continue; else break;
                        }
                        WriteLine();
                        ListAttributes(part);
                        if (!multiselect) break;
                    }
                }
                else
                {
                    WriteLine(part.ToString());
                }
            }
            if (vessel.Parts.Count == 0)
            {
                WriteLine();
                WriteLine("(no parts)");
            }
            else if (selector.Length > 0 && !foundselection)
            {
                WriteLine();
                WriteLine("(no match)");
            }
            // push back the selector, to be used recursively
            if (selector.Length > 0)
            {
                selectors.Insert(0, selector);
            }
        }

        private void ListAttributes(KmlNode node)
        {
            foreach (var attrib in node.Attribs)
            {
                WriteLine(attrib.ToString());
            }
            if (node.Attribs.Count == 0)
            {
                WriteLine("(no attributes)");
            }
        }

        private void ProcessWarnings()
        {
            // make a copy, in case of repairs the original list changes
            List<Tuple<string, KmlItem>> warnings = new List<Tuple<string, KmlItem>>();

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
                WriteLine();
                WriteLineColor((select ? i.ToString() + ": " : "") + warning.Item1, ConsoleColor.Yellow);
                if (repair && warning.Item2 is KmlPartDock)
                {
                    // this is now a dock source that needed repair in the first iteration
                    KmlPartDock dock = (KmlPartDock)warning.Item2;
                    // repair only once
                    if (dock.NeedsRepair)
                    {
                        dock.Repair();
                        filechanged = true;
                    }
                    // show repaired info in any case
                    WriteLineColor("(repaired) " + dock.ToString(), ConsoleColor.Green);
                }
            }

            if (warnings.Count == 0)
            {
                WriteLine();
                WriteLine("(none)");
            }
            else if(selectors.Count > 0 && !foundselection)
            {
                WriteLine();
                WriteLine("(no match)");
            }
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

        private void ExportNode(KmlNode node)
        {
            var sr = new StringWriter();
            KmlItem.WriteItem(sr, node, 0);
            sr.Flush();
            if (actionfilename.Length > 0)
            {
                try
                {
                    File.WriteAllText(actionfilename, sr.GetStringBuilder().ToString());
                    WriteLineColor("(export) " + actionfilename, ConsoleColor.Green);
                }
                catch (Exception e)
                {
                    WriteLineColor("(export failed) " + e.Message, ConsoleColor.Red);
                }
            }
            else
            {
                Console.Write(sr.GetStringBuilder().ToString());
            }
        }

        private bool ActionIsImport(Action action)
        {
            return action == Action.ImportReplace || action == Action.ImportBefore || action == Action.ImportAfter;
        }

        private void ImportNode(KmlNode node)
        {
            if (actionfilename.Length == 0 || !File.Exists(actionfilename))
            {
                WriteLineColor("(import file not found) " + actionfilename, ConsoleColor.Red);
            }
            else if (node.Parent == null)
            {
                // that would make no sense anyway, the import file would be a full save
                WriteLineColor("(import on root node denied)", ConsoleColor.Red);
            }
            else
            {
                List<KmlItem> input = KmlItem.ParseFile(actionfilename);
                if (input.Count == 0)
                {
                    WriteLineColor("(import data empty)", ConsoleColor.Red);
                }
                else if (action == Action.ImportReplace && input.Count > 1)
                {
                    WriteLineColor("(import contains more than one node)", ConsoleColor.Red);
                }
                else if (!input.All(x => x is KmlNode))
                {
                    WriteLineColor("(import data format error)", ConsoleColor.Red);
                }
                else if (action == Action.ImportBefore)
                {
                    foreach (var item in input)
                    {
                        node.Parent.InsertBefore(node, item);
                        WriteLineColor("(import) " + item.ToString(), ConsoleColor.Green);
                    }
                    filechanged = true;
                }
                else if (action == Action.ImportAfter)
                {
                    // insert after node each time would reverse order
                    KmlItem last = node;
                    foreach (var item in input)
                    {
                        node.Parent.InsertAfter(last, item);
                        last = item;
                        WriteLineColor("(import) " + item.ToString(), ConsoleColor.Green);
                    }
                    filechanged = true;
                }
                else if (action == Action.ImportReplace)
                {
                    // Only special case to react on changed attribs is renaming assigned kerbals,
                    // because that can be easily done in GUI and CLI should be able to do this as well.
                    // Other problems caused by uninformed usage falls back on the user and is not handled in the GUI as well.

                    // already checked that input.Count == 1
                    if (node is KmlKerbal && input[0] is KmlKerbal)
                    {
                        KmlKerbal oldKerbal = (KmlKerbal)node;
                        KmlKerbal newKerbal = (KmlKerbal)input[0];

                        // rename the old one, so it will invoke Name_Changed event
                        KmlAttrib name = oldKerbal.GetAttrib("name");
                        if (name != null)
                        {
                            name.Value = newKerbal.Name;
                        }
                    }
                    node.Parent.InsertBefore(node, input[0]);
                    // raw deletion does not changes internals (assigned kerbals etc.)
                    node.CanBeDeleted = true;
                    if (node.DeleteRaw())
                    {
                        WriteLineColor("(import) " + input[0].ToString(), ConsoleColor.Green);
                        filechanged = true;
                    }
                    else
                    {
                        WriteLineColor("(import failed)", ConsoleColor.Red);
                    }
                }
            }
        }

        private void DeleteNode(KmlNode node)
        {
            if (!node.CanBeDeleted || node.Parent == null)
            {
                WriteLineColor("(deletion denied)", ConsoleColor.Red);
                return;
            }
            else if (node is KmlKerbal)
            {
                if ((node as KmlKerbal).AssignedPart != null)
                {
                    WriteLineColor("(removed from assigned crew part)", ConsoleColor.Green);
                }
            }
            else if (node is KmlVessel)
            {
                if ((node as KmlVessel).AssignedCrew.Count > 0)
                {
                    WriteLineColor("(crew sent home to astronaut complex)", ConsoleColor.Green);
                }
            }
            else if (node is KmlPart)
            {
                if (node.Parent is KmlVessel)
                {
                    WriteLineColor("(removed from vessel structure)", ConsoleColor.Green);
                    WriteLineColor("(updated attachment indices)", ConsoleColor.Green);
                    foreach (KmlKerbal kerbal in (node.Parent as KmlVessel).AssignedCrew)
                    {
                        if (kerbal.AssignedPart == node)
                        {
                            WriteLineColor("(crew sent home to astronaut complex)", ConsoleColor.Green);
                            break;
                        }
                    }
                }
            }
            // we are currently iterating over some list, keep the contained items constant
            node.Parent.InsertBefore(node, new KmlGhostNode("(deleted)"));
            if (node.Delete())
            {
                filechanged = true;
                WriteLineColor("(deleted)", ConsoleColor.Green);
            }
            else
            {
                WriteLineColor("(deletion failed)", ConsoleColor.Red);
            }
        }
    }
}
