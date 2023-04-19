using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform.Foundation
{
    public static class UserManager
    {
        public static string[] ProfileSubfolders =
            { "Workspace", "Desktop", "Downloads", "Music", "Documents" };
        public const int UserLockTypeNone = 0;
        public const int UserLockTypePass = 1;
        public const int UserLoginResultLoggedInto = 0;
        public const int UserLoginResultInvalidUsername = 1;
        public const int UserLoginResultInvalidPassword = 2;
        public const int UserLoginResultNotAvailable = 3;
        public static DateTime LastLoginTime;
        public static TimeSpan NoLoginMaxTime;
        public static List<User> AvailableUsers;
        public static User CurrentUser;
        private static bool initialized = false;
        public static void Initialize()
        {
            if (!initialized)
            {
                LastLoginTime = DateTime.Now - new TimeSpan(10, 0, 0);
                NoLoginMaxTime = new TimeSpan(0, 10, 0);

                AvailableUsers = new List<User>
                {
                    new RootUser("root")
                };
                CurrentUser = AvailableUsers[0];

                PlatformLogger.Log("Set up basic user information!");
                initialized = true;
            }
        }
        public static int Login(string username, string key, bool force = false)
        {
            if (force) PlatformLogger.Log($"Using a force login on user {username}");
            if (force && !CurrentUser.IsRoot) throw new InsufficientPrivilegesException();

            for (int i = 0; i < AvailableUsers.Count; i++)
            {
                User user = AvailableUsers[i];
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
        public static User FindByName(string username)
        {
            foreach (var user in AvailableUsers)
            {
                if (user.UserName == username) return user;
            }
            return null;
        }
        public static void CreateUser(User user)
        {
            if (!CurrentUser.IsAbleToManageUsers) throw new InsufficientPrivilegesException();

            PlatformLogger.Log("Adding new user. Name: " + user.UserName);
            AvailableUsers.Add(user);
            user.SetDisabledState(false);
            FileSystemManager.CreateDirectory(GetUserProfile(user));

            for (int i = 0; i < ProfileSubfolders.Length; i++)
                FileSystemManager.CreateDirectory(GetUserProfile(user) + ProfileSubfolders[i]);

            // FileSystem.WriteStringFile(GetUserProfile(user) + "userinfo.dat",
            //     $"user.name=\"{user.UserName}\";" +
            //     $"user.pass=\"{user.UserKey}\";" +
            //     $"user.locktype={user.UserLockType};" +
            //     $"user.group=\"{user.UserGroup}\";" +
            //     $"user.state.isroot={user.IsRoot};" +
            //     $"user.state.ishidden={user.IsHidden};" +
            //     $"user.state.isdisabled={user.IsDisabled}");

            PlatformLogger.Log($"Created new user: {user.UserName} - {user.UserGroup}");
        }
        public static bool RemoveUser(string username)
        {
            if (!CurrentUser.IsAbleToManageUsers) throw new InsufficientPrivilegesException();

            var user = FindByName(username);
            if (user == null) return false;

            PlatformLogger.Log("Removing user. Name: " + username);

            AvailableUsers.Remove(user);

            PlatformLogger.Log($"Removed user: {user.UserName} - {user.UserGroup}");
            return true;
        }
        public static string GetUserProfile(User user) => $"0:\\PrivateUsers\\{user.UserName}\\";
        public class RootUser : User
        {
            public RootUser() : base("root", "root", "", 0)
            {
                IsDisabled = false;
                privs.IsRoot = true;
            }
            public RootUser(string username) : base(username, "root", "", 0)
            {
                IsDisabled = false;
                privs.IsRoot = true;
            }
        }
        public class User
        {
            public string UserName { get; protected set; }
            public string UserGroup { get; protected set; }
            public string UserKey { get; protected set; }
            public int UserLockType { get; protected set; }
            public bool IsHidden { get; protected set; }
            public bool IsDisabled { get; protected set; }
            public bool IsRoot => privs.IsRoot;
            public bool IsAbleToManageHardware => privs.IsAbleToManageHardware;
            public bool IsAbleToManageServices => privs.IsAbleToManageServices;
            public bool IsAbleToUseProtectedFS => privs.IsAbleToUseProtectedFS;
            public bool IsAbleToManageUsers => privs.IsAbleToManageUsers;
            public bool IsAbleToShutdown => privs.IsAbleToShutdown;
            protected UserPrivileges privs;
            public User(string userName, string userGroup, string userKey, int userLockType)
            {
                UserName = userName.Replace("/", "_").Replace("\\", "_").Replace(";", "_").Replace(" ", "_");
                UserGroup = userGroup;
                UserKey = userKey;
                UserLockType = userLockType;
                IsHidden = false;
                IsDisabled = true;
                privs = new();
            }
            public void SetUserGroup(string group)
            {
                if (!CurrentUser.privs.IsAbleToManageUsers) throw new InsufficientPrivilegesException();
                UserGroup = group;
            }
            public void SetUserLock(int locktype, string key)
            {
                if (CurrentUser != this) throw new InsufficientPrivilegesException();
                UserLockType = locktype;
                UserKey = key;
            }
            public void SetHiddenState(bool state)
            {
                if (!CurrentUser.privs.IsAbleToManageUsers) throw new InsufficientPrivilegesException();
                IsHidden = state;
            }
            public void SetDisabledState(bool state)
            {
                if (!CurrentUser.privs.IsAbleToManageUsers) throw new InsufficientPrivilegesException();
                IsDisabled = state;
            }
        }
        public class UserPrivileges
        {
            public bool IsRoot;
            public bool IsAbleToManageHardware;
            public bool IsAbleToManageServices;
            public bool IsAbleToUseProtectedFS;
            public bool IsAbleToManageUsers;
            public bool IsAbleToShutdown;
        }
    }
    public class InsufficientPrivilegesException : Exception { }
}
