# wdOS
[Cosmos](https://github.com/CosmosOS/Cosmos)-based operating system for x86 PCs. 
Don't even try to ask me about name, its basically means wednesday. Just an idea 
to create an OS came at wednesday

### Development process
~Look at projects~. Currently system is at version 0.6.0 I think its a post-alpha. 

Almost implemented features:
- User System
- TShell, basically optimized terminal
- CShell, basically unoptimized graphical shell
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

**A**: No, currently SDK is in development


**Q**: Can I install system to drive?

**A**: Somewhat. Kernel itself currently is not installable, beacuse Cosmos's implementation of CDFS is
broken and systems based on it aren't able to read CDROM