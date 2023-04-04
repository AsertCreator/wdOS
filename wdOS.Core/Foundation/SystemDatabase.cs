using Cosmos.HAL;
using Cosmos.System.FileSystem.VFS;
using Cosmos.System.Graphics.Fonts;
using Cosmos.System.Network.IPv4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wdOS.Core.Shell;

namespace wdOS.Core.Foundation
{
    public static class SystemDatabase
    {
        public const int UserLockTypeNone = 0;
        public const int UserLockTypePass = 1;
        public const int UserLoginResultLoggedInto = 0;
        public const int UserLoginResultInvalidUsername = 1;
        public const int UserLoginResultInvalidPassword = 2;
        public const int UserLoginResultNotAvailable = 3;
        public static List<UserBase> AvailableUsers;
        public static string[] ProfileSubfolders = 
            { "Workspace", "Desktop", "Downloads", "Music", "Documents" };
        public static UserBase CurrentUser;
        public static StringBuilder SystemLog = new();
        public static bool LoadUsersFromDisk = false;
        public static List<ShellBase> SSDShells = new();
        public static List<ShellBase> SSDService = new();
        public static void InitializeDatabase()
        {
            AvailableUsers = new List<UserBase>
            {
                new RootUser()
            };
            CurrentUser = AvailableUsers[0];

            KernelLogger.Log("Set up basic user information!");

            // won't enable until proven that it's working
            if (LoadUsersFromDisk)
            {
                var list = VFSManager.GetDirectoryListing("0:\\PrivateUsers\\");

                foreach (var user in list)
                {
                    KernelLogger.Log($"Found user profile: {user.mName}");
                    string userinfo = FileSystem.Normalize(user.mFullPath + "\\" + "userinfo.dat");
                    PropertyFile file = new(FileSystem.ReadStringFile(userinfo));
                    CustomUser userobj = new((string)file.Properties["user.name"], (string)file.Properties["user.group"],
                        (string)file.Properties["user.key"], (int)(long)file.Properties["user.locktype"])
                    {
                        IsRoot = (bool)file.Properties["user.state.isroot"],
                        IsHidden = (bool)file.Properties["user.state.ishidden"],
                        IsDisabled = (bool)file.Properties["user.state.isdisabled"]
                    };
                    AvailableUsers.Add(userobj);
                }
            }

            KernelLogger.Log("Enabled SystemDatabase with user database");
        }
        public static string GetTimeAsString() =>
            (RTC.Hour < 10 ? "0" + RTC.Hour : RTC.Hour) + ":" +
            (RTC.Minute < 10 ? "0" + RTC.Minute : RTC.Minute) + ":" +
            (RTC.Second < 10 ? "0" + RTC.Second : RTC.Second);
        public static string GetDateAsString() =>
            (RTC.DayOfTheMonth < 10 ? "0" + RTC.DayOfTheMonth : RTC.DayOfTheMonth) + "." +
            (RTC.Month < 10 ? "0" + RTC.Month : RTC.Month) + "." + RTC.Year;
        public static int Login(string username, string key, bool force = false)
        {
            if (force) KernelLogger.Log("Using a force login. It's completely unsafe");
            for (int i = 0; i < AvailableUsers.Count; i++)
            {
                UserBase user = AvailableUsers[i];
                if (user.UserName == username)
                {
                    if (user.IsDisabled) return UserLoginResultNotAvailable;
                    if (user.UserKey == key || user.UserLockType == UserLockTypeNone || force)
                    {
                        CurrentUser = user;
                        return UserLoginResultLoggedInto;
                    }
                    else return UserLoginResultInvalidPassword;
                }
            }
            return UserLoginResultInvalidUsername;
        }
        public static UserBase FindByName(string username)
        {
            foreach (var user in AvailableUsers)
            {
                if(user.UserName == username) return user;
            }
            return null;
        }
        public static void CreateUser(UserBase user)
        {
            AvailableUsers.Add(user);
            user.IsDisabled = false;
            FileSystem.CreateDirectory(GetUserProfile(user));

            for (int i = 0; i < ProfileSubfolders.Length; i++)
                FileSystem.CreateDirectory(GetUserProfile(user) + ProfileSubfolders[i]);

            FileSystem.WriteStringFile(GetUserProfile(user) + "userinfo.dat", 
                $"user.name=\"{user.UserName}\";" +
                $"user.pass=\"{user.UserKey}\";" +
                $"user.locktype={user.UserLockType};" +
                $"user.group=\"{user.UserGroup}\";" +
                $"user.state.isroot={user.IsRoot};" +
                $"user.state.ishidden={user.IsHidden};" +
                $"user.state.isdisabled={user.IsDisabled}");

            KernelLogger.Log($"Created new user: {user.UserName} - {user.UserGroup}");
        }
        public static string GetUserProfile(UserBase user) => $"0:\\PrivateUsers\\{user.UserName}\\";
        public static class SystemSettings
        {
            public static int CrashPowerOffTimeout = 5;
            public static int SystemTerminalFont = 0;
            public static int ServicePeriod = 1000;
            public static Address CustomAddress = null;
            public static Address RouterAddress = null;
            public static bool EnableAudio = false;
            public static bool EnableLogging = true;
            public static bool EnableNetwork = false;
            public static bool EnableVerbose = false;
            public static bool EnableServices = false;
            public static bool EnablePeriodicGC = true;
            public static bool CDROMBoot = true;
            public static Dictionary<int, PCScreenFont> TerminalFonts = new()
            {
                [0] = PCScreenFont.Default
            };
        }
        public static class BuildConstants
        {
            public const int VersionMajor = 0;
            public const int VersionMinor = 6;
            public const int VersionPatch = 0;
        }
        public abstract class UserBase
        {
            public string UserName;
            public string UserGroup;
            public string UserKey;
            public int UserLockType;
            public bool IsHidden;
            public bool IsDisabled;
            public bool IsRoot;
            public UserBase(string userName, string userGroup, string userKey, int userLockType)
            {
                UserName = userName.Replace("/", "_").Replace("\\", "_").Replace(";", "_").Replace(" ", "_");
                UserGroup = userGroup;
                UserKey = userKey;
                UserLockType = userLockType;
                IsHidden = false;
                IsDisabled = true;
                IsRoot = false;
            }
        }
        public class RootUser : UserBase
        {
            public RootUser() :
                base("root", "root", "", 0)
            {
                IsDisabled = false;
                IsRoot = true;
            }
            public RootUser(string username) :
                base(username, "root", "", 0)
            {
                IsDisabled = false;
                IsRoot = true;
            }
        }
        public class CustomUser : UserBase
        {
            public CustomUser(string userName, string userGroup, string userKey, int userLockType) :
                base(userName, userGroup, userKey, userLockType) 
            { 
            
            }
        }
    }
}
