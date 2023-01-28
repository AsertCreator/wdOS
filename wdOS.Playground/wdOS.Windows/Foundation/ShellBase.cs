namespace wdOS.Core.Foundation
{
    internal abstract class ShellBase
    {
        internal bool IsRunning = true;
        internal abstract string Name { get; }
        internal abstract int MajorVersion { get; }
        internal abstract int MinorVersion { get; }
        internal abstract int PatchVersion { get; }
        internal abstract void Run();
        internal abstract void BeforeRun();
    }
}
