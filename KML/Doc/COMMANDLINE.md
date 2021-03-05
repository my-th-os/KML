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

#### Basics
What most users need, is to check for warnings (change path to KML.exe and *.sfs to your situation)

```
$ ./KML.exe saves/test/persistent.sfs --warnings
```

and then repair docking and contract problems automatically.

```
$ ./KML.exe saves/test/persistent.sfs --repair
```

To see the content of your file, you can list your vessels with `--vessels` or `-v` and your kerbals with `--kerbals` or `-k`.
There you will get an alphabetically ordered list, so it's easier to search.

When you need a proper search function, it is best to combine KML with other tools from your OS, like `grep`.

```
$ ./KML.exe saves/test/persistent.sfs -k | grep "Jeb"
KERBAL (Jebediah Kerman, Crew, Pilot, Available)
```


#### Navigating the KML tree
KML's main pupropse is to deal with the semi-structured text in the save file, that can be best represented in a tree.
This is similar to dealing with files and folders on your hard disk.

If you inspect the tree with `-t`, KML will only list the single root node, always called 'GAME'.

```
$ ./KML.exe saves/test/persistent.sfs -t
Tree in "saves/test/persistent.sfs"

GAME
```

Maybe you want to know, what is inside that 'GAME'? Then you must select that node.
To do so, you have to add `-s` and this will enable numbering the nodes, so that you can select one.

```
$ ./KML.exe saves/test/persistent.sfs -t -s
Tree in "saves/test/persistent.sfs"

0: GAME
```

It is still not selected, so let's now do that by its number `-s=0`.
Then KML will show three sections:
- the path to the selection, each level in its own line
- a list of the attributes within that node
- a list of all contained nodes with a new numbering scheme to select

```
$ ./KML.exe saves/test/persistent.sfs -ts=0
Tree in "saves/test/persistent.sfs"

0: GAME

version = 1.11.1
...
versionCreated = 1.11.0.3045 (WindowsPlayer x64)

0/0: CometNames
...
0/22: FLIGHTSTATE
0/23: LoaderInfo
0/24: ROSTER
0/25: MESSAGESYSTEM
```

The ordering here is according to the appearance in the file, this is not sorted alphabetically.

Once you know the content of 'GAME', you can navigate deeper into the hierarchy.
To select the 'ROSTER', you can use its numbering scheme `-s=0/24`.
You can also select the node by its tag `-s=0/ROSTER` or even `-s=0/ROS` (and be lucky that there is no 'ROSCOSMOS' node earlier).

Under the 'ROSTER' there is a node with the 'KERBAL' tag for Jeb, Bill, Bob and co.
Would be handy to also select by the name, that KML usually displays in parentheses, and you can do that.

```
$ ./KML.exe saves/test/persistent.sfs -ts=0/ROSTER/Jeb
Tree in "saves/test/persistent.sfs"

0: GAME

0/24: ROSTER

0/24/0: KERBAL (Jebediah Kerman, Crew, Pilot, Available)

name = Jebediah Kerman
...
```

Please keep in mind, that every time KSP writes a new save, the numbers change.
Also the numbers in the output of `-ts` are different from `-vs` or `-ks`, because the latter are sorted.
Even a search for 'Jeb' might bring up a 'Jebeny Kerman' next time.
So don't memorize and don't use them in batch scripts of any sorts.
Always explore the current state anew.

#### Editing
Well, probably you do not only want to explore the content, but you have some changes to do
(fix a bug caused by KSP, restore only a single vessel from an older save, do some cheating, ...). 
This is very easy to do in the GUI, but in the CLI you need to be a bit more specific.

First, like in the previous section, you always need to hunt down the node, that you want to work with.
Best practice is to first get that selection statement correct.

```
$ ./KML.exe saves/test/persistent.sfs "-ts=0/FLIGHTSTATE/Wobbly Junk"
```

Make sure the output really is the object that you want.
Then keep that command-line and only append an action like `--delete`.

```
$ ./KML.exe saves/test/persistent.sfs "-ts=0/FLIGHTSTATE/Wobbly Junk" --delete
Tree in "saves/test/persistent.sfs"

0: GAME

0/22: FLIGHTSTATE

0/22/43: VESSEL (Wobbly Junk, Ship, ORBITING)
(crew sent home to astronaut complex)
(deleted)

(backup) saves/test/zKMLBACKUP20210305012649-persistent.sfs
(saving) saves/test/persistent.sfs
```

But deleting is a very harsh way to change things, you mostly want to edit some text.
In the GUI you can change text in-place. Using the CLI we finally fall back to the text editor of your choice.
But instead of dealing with that huge textfile that the save is, KML can break it down into smaller pieces.

After checking your select statement, you can export that subset of the save to another file.

```
$ ./KML.exe saves/test/persistent.sfs -ks=Jebediah --export=jeb.sfs
Kerbals in "saves/test/persistent.sfs"

24: KERBAL (Jebediah Kerman, Crew, Pilot, Available)
(export) jeb.sfs
```

This export is *only* a few hundred lines, because it includes all the data that is contained within Jeb's 'KERBAL' node, including child nodes.
But the content that you care about is probably always pretty much on top of the file (otherwise you would have selected deeper in).

After you changed the exported "jeb.sfs" with a text editor (let's say you changed the kerbal's name), you can import that to replace its previous state.

```
$ ./KML.exe saves/test/persistent.sfs -ks=Jebediah --import-replace=jeb.sfs
```

Alternatively, you can import that file as new content added after the selection.

```
$ ./KML.exe saves/test/persistent.sfs -ks=Jebediah --import-after=jeb.sfs
Kerbals in "saves/test/persistent.sfs"

24: KERBAL (Jebediah Kerman, Crew, Pilot, Available)
(import) KERBAL (Jebediah Kerman II, Crew, Pilot, Available)

(backup) saves/test/zKMLBACKUP20210305030703-persistent.sfs
(saving) saves/test/persistent.sfs
```

Congratulations, you successfully cloned your favorite kerbal!
