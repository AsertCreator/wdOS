# wdOS
[Cosmos](https://github.com/CosmosOS/Cosmos)-based operating system for x86 PCs. 
Shall be somewhat architectually and "what use it for"-ly similiar to Windows NT.
Bundled with Pillow Runtime and Weirdo Compiler which both make up Pillow Platform.
For more information, read `PILLOW.md` file.

### Development State
I've got this to version 0.10.0, pre-beta. Note, that system is currently has two
versions: wdOS Core and wdOS Platform. Latter one is newer and more stable. First
one is very buggy and definitely not modular. however, first one has cool things
that most of Cosmos-built OSes have. wdOS Core's development is stopped, now I work
on more stable and modular system, wdOS Platform. That feature list below is frozen 
and corresponds to wdOS Core. 

Almost implemented features:
- User System
- TShell, basically optimized terminal
- CShell, basically unoptimized graphical shell (removed; if you want use it, use 
commit history to get it back)
- Some nix commands
- System Installer

Will be implemented:
- Networking
- Audio Playing
- Error Handling
- Normal Interaction

Third-party features:
- MIV ([author](https://github.com/bartashevich))

### Not Asked Questions
**Q**: How's that possible?<br/>
**A**: This OS is built on [Cosmos](https://github.com/CosmosOS/Cosmos) which allows 
to write OSes on certain .NET languages. For more information about it, go to link 
above.


**Q**: What's are minimum system requirements?<br/>
**A**: There goes:

| RAM     | Disk | CD-ROM | Video          | CPU                         |
|---------|------|--------|----------------|-----------------------------|
| 256 MiB | IDE  | IDE    | VGA-compatible | Any starting from Pentium 4 |


**Q**: What's are recomended system requirements?<br/>
**A**: There goes:

| RAM     | Disk | CD-ROM | Video          | CPU                         |
|---------|------|--------|----------------|-----------------------------|
| 512 MiB | IDE  | IDE    | VBE-compatible | Any starting from Core Solo |


**Q**: Can I make apps or drivers for this OS?<br/>
**A**: Somewhat. wdOS now has somewhat working Pillow Runtime, so you can embed and run
primitive apps. Finally it works!


**Q**: Can I install system to drive?<br/>
**A**: Nope. Kernel itself currently is not installable, beacuse Cosmos's implementation 
of CDFS is broken and systems based on it aren't able to read CDROM.

### Credits

Cosmos Project - Cosmos Contributors (2005-2023)<br/>
ANTLR4 Runtime Libraries - © Copyright ANTLR (1992-2023)<br/>
PrismOS API Library - © Copyleft Terminal.cs and contributors (2021-2023)

> Personal names are hidden for privacy reasons

<b>© Copyright AsertCreator 2023</b>