namespace wdOS.Core.OS.Shells
{
    internal interface IPackage
    {
        internal string Name { get; }
        internal string Description { get; }
        internal string[] Files { get; }
        internal int MajorVersion { get; }
        internal int MinorVersion { get; }
        internal int PatchVersion { get; }
        internal PackageDatabase.PackageType Type { get; }
    }
}
