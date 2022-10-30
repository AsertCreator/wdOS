namespace wdOS.Core.OS.Shells
{
    internal class GeneralPackage : IPackage
    {
        string IPackage.Name => GeneralName;
        string IPackage.Description => GeneralDesc;
        string[] IPackage.Files => GeneralFiles;
        int IPackage.MajorVersion => GeneralMajorVersion;
        int IPackage.MinorVersion => GeneralMinorVersion;
        int IPackage.PatchVersion => GeneralPatchVersion;
        internal string GeneralName;
        internal string GeneralDesc;
        internal string[] GeneralFiles;
        internal int GeneralMajorVersion;
        internal int GeneralMinorVersion;
        internal int GeneralPatchVersion;
        internal GeneralPackage(string name, string desc, string[] files, (int, int, int) version)
        {
            GeneralName = name;
            GeneralDesc = desc;
            GeneralFiles = files;
            GeneralMajorVersion = version.Item1;
            GeneralMinorVersion = version.Item2;
            GeneralPatchVersion = version.Item3;
        }

        PackageDatabase.PackageType IPackage.Type => PackageDatabase.PackageType.UserProgram;
    }
}
