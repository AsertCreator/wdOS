using Cosmos.Core;
using System;
using System.Collections.Generic;
using System.Text;
using wdOS.Core.OS.Foundation;

namespace wdOS.Core.OS.Shells
{
    internal static class PackageDatabase
    {
        internal static List<IPackage> Packages = new();
        internal static List<string> UpgradablePackages = new();
        internal static StringBuilder Log;
        internal static bool Protected = true;
        internal static IPackage FindPackageByName(string name)
        { foreach (var package in Packages) { if (package.Name.ToLower() == name.ToLower()) return package; } return null; }
        internal enum PackageType { SystemShell, UserProgram, UserBundleProgram }
    }
}
