# wdOS
[Cosmos](https://github.com/CosmosOS/Cosmos)-based operating system for x86 PCs. 
Don't even try to ask me about name, its basically means wednesday. Just an idea 
to create an OS came at wednesday

### Development process
Currently system is at version 0.9.0, pre-beta. Also, system is currently in state 
of switching to almost new architecture, so most of its features will be rewritten. 
List below applies only to wdOS Core OS. wdOS Platform OS is somewhat seperate OS 
that should be able to run native apps on disk.

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
**Q**: How?

**A**: This OS uses [Cosmos](https://github.com/CosmosOS/Cosmos) which allows to write OSes on C#/F#/VB.NET


**Q**: Minimum system requirements?
| RAM            | Disk | CD-ROM | Video        | CPU                         |
|----------------|------|--------|--------------|-----------------------------|
| 256 MiB, maybe | any  | any    | internal gpu | any starting from core solo |


**Q**: Recomended system requirements?
| RAM     | Disk | CD-ROM | Video        | CPU                          |
|---------|------|--------|--------------|------------------------------|
| 512 MiB | any  | any    | internal gpu | any starting from core 2 duo |


**Q**: Can I make apps or drivers for this OS?

**A**: Somewhat. wdOS is now has somewhat working Pillow Runtime, so you can embed and run
primitive apps. Finally!


**Q**: Can I install system to drive?

**A**: Somewhat. Kernel itself currently is not installable, beacuse Cosmos's implementation of CDFS is
broken and systems based on it aren't able to read CDROM