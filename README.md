## KML - Kerbal Markup Lister v0.9.4 - 2023-04-21
### A persistence file editor for Kerbal Space Program
KSP Forum: http://forum.kerbalspaceprogram.com/index.php?/topic/133971-win-kml-persistence-file-editor/

Do you face a problem with broken docking ports or need just a little fuel-cheating? Do you got tired of editing save games in a text editor with long loading time and so much scrolling to compare different parts and vessels? So did I and decided to make a more helpful external editor to display the XML-like structure (the "KML" - Kerbal Markup Language) in a tree view and pick out vessels, kerbals and parts to be displayed in more eye-candy way.

The current version is almost feature-complete and functional, and already helps with quite a few problems (or some cheating if you choose to do so).

Any feedback, more testing, bug reports and suggestions are very welcome.

### Sceenshots
![](https://github.com/my-th-os/KML/blob/master/KML/Doc/KML-Tree.png?raw=true)
![](https://github.com/my-th-os/KML/blob/master/KML/Doc/KML-Vessels.png?raw=true)
![](https://github.com/my-th-os/KML/blob/master/KML/Doc/KML-Kerbals.png?raw=true)

### New Features
- CLI selected warning like `-ws=0` shows source node - for darthgently
- CLI shows all sources nodes for each warning with multiselect `-wm=*`
- GUI and CLI always name 2 nodes in warning message, even if it is about an attribute

### Features 
- open SFS and CRAFT files
- save files edited or exactly as loaded
- backup of overwritten files
- display file structure in a "KML" tree
- display all vessels in a sorted and filtered list
- display all kerbals in a sorted and filtered list
- display vessel's part attachment structure
- display warnings about structural problems
- search for nodes, attributes and their content 
- adding, inserting and deleting nodes and attributes
- change position of nodes (except parts) and attributes
- copy & paste of nodes and attributes
- deletion of parts will fix part indices and connections 
- deletion of kerbals and vessels will fix crew assignment 
- support renaming kerbals when assigned 
- send kerbal home to astronaut complex 
- send vessel to low kerbin orbit 
- repair broken docking ports and grappling devices
- repair broken ID references in contracts
- refill part resources
- change vessel flag in all its parts - for Enceos
- list all GameData dirs used by vessel parts - for TheCardinal
- CLI mode for basic editing and repairs
- tested with KSP 1.0 to 1.12 (newest KML version should still work with at least all these KSP versions) 

### TODO
- identify add-on part connections (only a little KAS support for now)
- more testing (uncommon vessel builds I didn't think of?)
- support multiple docking ports per part
- documentation
- a lot of source code TODOs

### Install
##### Windows
- download [KML_Windows.zip](https://github.com/my-th-os/KML/releases) and extract anywhere you want
- run the KML.exe and open your save file
- or choose "open with" on your save file and select KML.exe (command-line arguments supported)

##### Linux, Mac
- need to have [Mono](https://www.mono-project.com/) installed
- download [KML_Mono.zip](https://github.com/my-th-os/KML/releases) and extract anywhere you want
- run the KML.exe command-line from the terminal of your choice

### Instructions, Hints, FAQ
- Don't find your vessel in the tree (ordered by in-game creation)? Switch to the "vessels" tab to have a sorted list and the possibility to filter by type (debris, ship, etc.).
- Don't find your kerbal in the tree? There's also a sorted and filtered list in the "kerbals" tab.
- Want to see how the parts of your vessel are connected? Have a look on the graph displayed in the "vessels" tab.
- What do the colored lines in that part graph mean?
  - Dark green line is a node attachment in top/down direction
  - Light green is a node attachment in front/back or left/right direction
  - Yellow line is a surface attachment
  - Blue line is a docking connection
- If parts are arranged in a way you can't see where the connections go, mouse over the part and they are drawn thicker.
- After you found your vessel, kerbal or part in the list you want to see it in the tree? Just make a double click.
- Sometimes there appears a warnings tab, what do these warnings mean? Some KML elements are more heavily inspected than others, like vessel parts and especially docking or grappling devices. If something is not as expected there is a warning generated.
- There is a warning but my save game works fine! If the problem is related to stock parts please send me feedback about this problem. If KML has warnings about add-on parts not connected (e.g. KAS): That's ok for now, KML does not support that add-on and can't understand what it has written to the save file. KML just expects all parts to be somehow connected and gives a warning otherwise.
- Don't fear about saving a file with warnings, unknown add-ons, missing part-connections, etc. KML will save the file as it was read and only apply the changes you made. All unknown data will be kept as it was.

### Command Line Interface
When you download the [KML_Mono.zip](https://github.com/my-th-os/KML/releases),
you will get no graphical user interface (GUI), but only a command line interface (CLI).
The benefit of this version is that it runs on Windows, Linux and Mac. The latter need to have
[Mono](https://www.mono-project.com/) installed.

The full Windows version [KML_Windows.zip](https://github.com/my-th-os/KML/releases)
includes the GUI and a CLI. To access the CLI, you just call it with any dashed argument.

```
$ ./KML.exe --help
KML: Kerbal Markup Lister 0.9 © 2021 Oliver Pola (Mythos)
Use: KML [Opt] <save-file>
Opt: --tree             | -t : List tree
     --vessels          | -v : List vessels
     --kerbals          | -k : List kerbals
     --warnings         | -w : Show warnings
     --repair           | -r : Repair docking and contract problems, includes -w
     --select           | -s : Show numbers, select one by -s=<Sel>
     --multiselect      | -m : Select all occurences by tag/name, includes -s
     --version               : Show version and check online for updates
     Actions on selection, need -s=<Sel> or -m=<Sel>, only one of:
     --export=<file>         : Export selection, no -m, defaults <file> to stdout
     --import-replace=<file> : Import file to replace selection, no -m
     --import-before=<file>  : Import file as new before selection, no -m
     --import-after=<file>   : Import file as new after selection, no -m
     --delete                : Delete selection, -m is allowed
Sel: < number | tag-start | name-start >[/Sel]
     Only in tree you can select by tag or go deep into hierarchy
```

What most users need, is to check for warnings (change path to KML.exe and *.sfs to your situation)

```
$ ./KML.exe saves/test/persistent.sfs --warnings
```

and then repair docking and contract problems automatically.

```
$ ./KML.exe saves/test/persistent.sfs --repair
```

To learn more, check the [COMMANDLINE.md](https://github.com/my-th-os/KML/blob/master/KML/Doc/COMMANDLINE.md) introduction.

### Developers
- Mythos (initiator, maintainer)
- pamidur (contributor)