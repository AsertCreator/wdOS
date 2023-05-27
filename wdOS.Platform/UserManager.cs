using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform
{
    internal static class UserManager
    {
        internal static string[] ProfileSubfolders =
            { "Workspace", "Desktop", "Downloads", "Music", "Documents" };
        internal const int UserLockTypeNone = 0;
        internal const int UserLockTypePass = 1;
        internal const int UserLoginResultLoggedInto = 0;
        internal const int UserLoginResultInvalidUsername = 1;
        internal const int UserLoginResultInvalidPassword = 2;
        internal const int UserLoginResultNotAvailable = 3;
        internal static DateTime LastLoginTime;
        internal static TimeSpan NoLoginMaxTime;
        internal static List<User> AvailableUsers;
        internal static User CurrentUser;
        internal static User EveryoneUser;
        internal static bool initialized = false;
        internal static void Initialize()
        {
            if (!initialized)
            {
                LastLoginTime = DateTime.Now - new TimeSpan(10, 0, 0);
                NoLoginMaxTime = new TimeSpan(0, 10, 0);

                EveryoneUser = new User("everyone", "everything", "", 0);
                AvailableUsers = new List<User>
                {
                    new RootUser("root"),
                    EveryoneUser
                };
                CurrentUser = AvailableUsers[0];

                PlatformManager.Log("set up basic user information!", "usermanager");
                initialized = true;
            }
        }
        internal static int Login(string username, string key, bool force = false)
        {
            if (force) PlatformManager.Log("using a force login on user " + username, "usermanager");
            if (force && !CurrentUser.IsRoot) throw new InsufficientPrivilegesException();

            for (int i = 0; i < AvailableUsers.Count; i++)
            {
                User user = AvailableUsers[i];
                if (user.UserName == username && user != EveryoneUser)
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
        internal static User FindByName(string username)
        {
            for (int i = 0; i < AvailableUsers.Count; i++)
            {
                var user = AvailableUsers[i];
                if (user.UserName == username) return user;
            }
            return null;
        }
        internal static bool CreateUser(User user)
        {
            if (!CurrentUser.IsAbleToManageUsers) throw new InsufficientPrivilegesException();

            user.UserName = user.UserName.Trim();

            if (FindByName(user.UserName) != null) return false;

            PlatformManager.Log("adding new user. Name: " + user.UserName, "usermanager");
            AvailableUsers.Add(user);
            user.SetDisabledState(false);
            FileSystemManager.CreateDirectory(GetUserProfile(user));

            for (int i = 0; i < ProfileSubfolders.Length; i++)
                FileSystemManager.CreateDirectory(GetUserProfile(user) + ProfileSubfolders[i]);

            if (!UpdateUserDatabase())
            {
                BroadcastManager.SendBroadcast(EveryoneUser, "User Database",
                    "User Manager notifies you, that it wasn't able to save User Database, " +
                    "please save your work for PC maintainer to reboot it");
            }

            PlatformManager.Log("created new user: " + user.UserName, "usermanager");
            return true;
        }
        internal static bool RemoveUser(string username)
        {
            if (!CurrentUser.IsAbleToManageUsers) throw new InsufficientPrivilegesException();

            var user = FindByName(username);
            if (user == null) return false;

            PlatformManager.Log("removing user. Name: " + username, "usermanager");

            AvailableUsers.Remove(user);

            PlatformManager.Log("removed user: " + user.UserName, "usermanager");
            return true;
        }
        internal static bool UpdateUserDatabase()
        {
            // FileSystem.WriteStringFile(GetUserProfile(user) + "userinfo.dat",
            //     $"user.name=\"{user.UserName}\";" +
            //     $"user.pass=\"{user.UserKey}\";" +
            //     $"user.locktype={user.UserLockType};" +
            //     $"user.group=\"{user.UserGroup}\";" +
            //     $"user.state.isroot={user.IsRoot};" +
            //     $"user.state.ishidden={user.IsHidden};" +
            //     $"user.state.isdisabled={user.IsDisabled}");
            return true;
        }
        internal static string GetUserProfile(User user) => "0:\\PrivateUsers\\" + user.UserName + '\\';
        internal class RootUser : User
        {
            internal RootUser() : base("root", "root", "", 0)
            {
                IsDisabled = false;
                privs.IsRoot = true;
            }
            internal RootUser(string username) : base(username, "root", "", 0)
            {
                IsDisabled = false;
                privs.IsRoot = true;
            }
        }
        internal class User
        {
            internal string UserName;
            internal string UserGroup;
            internal string UserKey;
            internal int UserLockType;
            internal bool IsHidden;
            internal bool IsDisabled;
            internal bool IsRoot => privs.IsRoot;
            internal bool IsAbleToManageHardware => privs.IsAbleToManageHardware;
            internal bool IsAbleToManageServices => privs.IsAbleToManageServices;
            internal bool IsAbleToUseProtectedFS => privs.IsAbleToUseProtectedFS;
            internal bool IsAbleToManageUsers => privs.IsAbleToManageUsers;
            internal bool IsAbleToShutdown => privs.IsAbleToShutdown;
            protected UserPrivileges privs;
            internal User(string userName, string userGroup, string userKey, int userLockType)
            {
                UserName = userName.Replace("/", "_").Replace("\\", "_").Replace(";", "_").Replace(" ", "_");
                UserGroup = userGroup;
                UserKey = userKey;
                UserLockType = userLockType;
                IsHidden = false;
                IsDisabled = true;
                privs = new();
            }
            internal void SetUserGroup(string group)
            {
                if (!CurrentUser.privs.IsAbleToManageUsers) throw new InsufficientPrivilegesException();
                UserGroup = group;
            }
            internal void SetUserLock(int locktype, string key)
            {
                if (CurrentUser != this) throw new InsufficientPrivilegesException();
                UserLockType = locktype;
                UserKey = key;
            }
            internal void SetHiddenState(bool state)
            {
                if (!CurrentUser.privs.IsAbleToManageUsers) throw new InsufficientPrivilegesException();
                IsHidden = state;
            }
            internal void SetDisabledState(bool state)
            {
                if (!CurrentUser.privs.IsAbleToManageUsers) throw new InsufficientPrivilegesException();
                IsDisabled = state;
            }
        }
        internal sealed class UserPrivileges
        {
            internal bool IsRoot;
            internal bool IsAbleToManageHardware;
            internal bool IsAbleToManageServices;
            internal bool IsAbleToUseProtectedFS;
            internal bool IsAbleToManageUsers;
            internal bool IsAbleToShutdown;
        }
    }
    internal sealed class InsufficientPrivilegesException : Exception { }
}
