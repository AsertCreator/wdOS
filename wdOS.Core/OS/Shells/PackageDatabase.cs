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
        internal static void ScanUpgradables(string newlist)
        {
            LogMessage($"Scanning upgradable packages...", LogType.Info);
            try
            {
                if (!Protected)
                {
                    string[] packages = newlist.Split(':');
                    foreach (var package in packages)
                    {
                        string[] data = package.Split('@');
                        string name = data[0];
                        string vers = data[1];
                        var pack = FindPackageByName(name);
                        var vint = ToVersion(vers);
                        if (pack.MajorVersion > vint.Item1 || pack.MinorVersion > vint.Item2 || pack.PatchVersion > vint.Item3)
                            UpgradablePackages.Add(name);
                        GCImplementation.Free(data);
                        GCImplementation.Free(vers);
                        GCImplementation.Free(vint);
                    }
                    GCImplementation.Free(packages);
                    GCImplementation.Free(newlist);
                    LogMessage($"Found {UpgradablePackages.Count} upgradable packages!", LogType.Info);
                    return;
                }
                LogMessage("Database is protected", LogType.Warning);
            }
            catch
            {
                LogMessage($"Unable to find upgradable packages!", LogType.Error);
            }
        }
        internal static void InstallPackage(string packagedef)
        {
            LogMessage($"Installing package \"{packagedef}\"...", LogType.Info);
            try
            {
                if (!Protected)
                {
                    string[] data = packagedef.Split('@');
                    string[] files = Utilities.SkipArray(packagedef.Split('$'), 1);
                    string name = data[0];
                    string vers = data[1];
                    var vint = ToVersion(vers);
                    Packages.Add(new GeneralPackage(name, "General Package", files, vint));
                    GCImplementation.Free(data);
                    GCImplementation.Free(vers);
                    LogMessage($"Successfully installed package \"{packagedef}\"!", LogType.Info);
                    return;
                }
                LogMessage("Database is protected", LogType.Warning);
            }
            catch
            {
                LogMessage($"Unable to install package \"{packagedef}\"!", LogType.Error);
            }
        }
        internal static void RemovePackage(string packagename)
        {
            LogMessage($"Removing package \"{packagename}\"...", LogType.Info);
            try
            {
                if (!Protected)
                {
                    var package = FindPackageByName(packagename);
                    Packages.Remove(package);
                    UpgradablePackages.Remove(packagename);
                    GCImplementation.Free(packagename);
                    LogMessage($"Successfully removed package \"{packagename}\"!", LogType.Info);
                }
                LogMessage("Database is protected", LogType.Warning);
            }
            catch
            {
                LogMessage($"Unable to remove package \"{packagename}\"!", LogType.Error);
            }
        }
        internal static void StartDatabase(string defs)
        {
            LogMessage($"Unocking and parsing database...", LogType.Info);
            try
            {
                if (Protected)
                {
                    // todo: implement database
                    LogMessage("Successfully unlocked and parsed database!", LogType.Info);
                    return;
                }
                LogMessage("Database is already unlocked!", LogType.Error);
            }
            catch
            {
                LogMessage($"Unable to unlock package database!", LogType.Error);
            }
        }
        internal static string ShutdownDatabase()
        {
            LogMessage($"Locking and serialiazing database...", LogType.Info);
            try
            {
                if (!Protected)
                {
                    string data = "";
                    foreach (var package in Packages)
                    {
                        string packagedef = $"{package.Name}@{package.MajorVersion}.{package.MinorVersion}.{package.PatchVersion}";
                        foreach (var file in package.Files) packagedef += $"${file}";
                        data += packagedef + ":";
                    }
                    LogMessage("Successully locked and serialized database!", LogType.Info);
                    return data[..^1];
                }
                LogMessage("Database is already locked!", LogType.Error);
                return "";
            }
            catch
            {
                LogMessage($"Unable to lock package database!", LogType.Error);
                return "";
            }
        }
        internal static void LogMessage(string message, LogType type)
        {
            Kernel.Log($"[{Utilities.GetLogTypeAsString(type)}] {message}");
            Log.Append($"[{DateTime.Now}] [{Utilities.GetLogTypeAsString(type)}] {message}\n");
        }
        internal static (int, int, int) ToVersion(string str)
        {
            try
            {
                string[] octets = str.Split('.');
                var vers = (int.Parse(octets[0]), int.Parse(octets[1]), int.Parse(octets[2]));
                GCImplementation.Free(octets);
                return vers;
            }
            catch { return (0, 0, 0); }
        }
        internal enum PackageType { SystemShell, UserProgram, UserBundleProgram }
    }
}
