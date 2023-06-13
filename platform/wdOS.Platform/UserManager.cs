using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform
{
    public delegate void EnumerateUsersCallback(User user);
    public delegate bool FindUsersPredicate(User user);
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
        public static User CurrentUser;
        public static User EveryoneUser;
        public static bool initialized = false;
        private static List<User> AvailableUsers;
        public static void Initialize()
        {
            if (!initialized)
            {
                LastLoginTime = DateTime.Now - new TimeSpan(10, 0, 0);
                NoLoginMaxTime = new TimeSpan(0, 10, 0);

                EveryoneUser = new("everyone", "users", "", 0)
                {
                    IsHidden = true,
                    IsDisabled = true
                };

                AvailableUsers = new()
                {
                    new RootUser("root"),
                    EveryoneUser
                };
                CurrentUser = AvailableUsers[0];

                PlatformManager.Log("set up basic user information!", "usermanager");
                initialized = true;
            }
        }
        public static int Login(string username, string key, bool force = false)
        {
            if (force) PlatformManager.Log("using a force login on user " + username, "usermanager");
            if (force && !CurrentUser.IsRoot) throw new InsufficientPrivilegesException();

            for (int i = 0; i < AvailableUsers.Count; i++)
            {
                User user = AvailableUsers[i];
                if (user.Username == username && user != EveryoneUser)
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
        public static int GetUserCount(bool counthidden)
        {
            int count = 0;
            EnumerateUsers(x => count++, counthidden);
            return count;
        }
        public static void OnUserReplication()
        {
            // todo: user replication
        }
        public static void ReplicateUsers()
        {
            // todo: user replication
        }
        public static void EnumerateUsers(EnumerateUsersCallback callback, bool showhidden = false)
        {
            for (int i = 0; i < AvailableUsers.Count; i++)
            {
                var user = AvailableUsers[i];
                if (user.IsHidden && !showhidden) continue;
                callback(user);
            }
        }
        public static User[] FindRootUsers() => FindUsersByPredicate(x => x.IsRoot);
        public static User[] FindRegularUsers() => FindUsersByPredicate(x => !x.IsRoot);
        public static User[] FindPasswordUsers() => FindUsersByPredicate(x => x.UserLockType == UserLockTypePass);
        public static User[] FindNonPasswordUsers() => FindUsersByPredicate(x => x.UserLockType == UserLockTypeNone);
        public static User[] FindSystemUsers() => FindUsersByPredicate(x => x.IsSystem);
		public static User[] FindNonSystemUsers() => FindUsersByPredicate(x => !x.IsSystem);
		public static User[] FindRegularRootUsers() => FindUsersByPredicate(x => x.IsRegularRoot);
        public static User[] FindUsersByPredicate(FindUsersPredicate predicate)
        {
            List<User> users = new List<User>();
            for (int i = 0; i < AvailableUsers.Count; i++)
            {
                var user = AvailableUsers[i];
                if (predicate(user)) users.Add(user);
            }
            return users.ToArray();
        }
        public static User FindByName(string username)
        {
            for (int i = 0; i < AvailableUsers.Count; i++)
            {
                var user = AvailableUsers[i];
                if (user.Username == username) return user;
            }
            return null;
        }
        public static bool CreateUser(User user)
        {
            if (!CurrentUser.IsAbleToManageUsers) throw new InsufficientPrivilegesException();

            user.Username = user.Username.Trim();

            if (FindByName(user.Username) != null) return false;

            PlatformManager.Log("adding new user. Name: " + user.Username, "usermanager");
            AvailableUsers.Add(user);
            user.SetDisabledState(false);
            FileSystemManager.CreateDirectory(GetUserProfile(user));

            for (int i = 0; i < ProfileSubfolders.Length; i++)
                FileSystemManager.CreateDirectory(GetUserProfile(user) + ProfileSubfolders[i]);

            if (!UpdateUserDatabase())
            {
                BroadcastManager.SendBroadcast(UserManager.EveryoneUser, "User Database",
                    "User Manager notifies you, that it wasn't able to save User Database, " +
                    "please save your work for PC maintainer to repair it");
            }

            PlatformManager.Log("created new user: " + user.Username, "usermanager");
            return true;
        }
        public static bool RemoveUser(string username)
        {
            if (!CurrentUser.IsAbleToManageUsers) throw new InsufficientPrivilegesException();

            var user = FindByName(username);
            if (user == null) return false;

            PlatformManager.Log("removing user. Name: " + username, "usermanager");

            AvailableUsers.Remove(user);

            PlatformManager.Log("removed user: " + user.Username, "usermanager");
            return true;
        }
        public static bool UpdateUserDatabase()
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
        public static string GetUserProfile(User user) => "0:\\PrivateUsers\\" + user.Username + '\\';
	}
	public class RootUser : User
	{
		public RootUser() : base("root", "root", "", 0)
		{
			IsDisabled = false;
			privs.IsRoot = true;
			IsReplicatedOverNetwork = false;
		}
		public RootUser(string username) : base(username, "root", "", 0)
		{
			IsDisabled = false;
			privs.IsRoot = true;
			IsReplicatedOverNetwork = false;
		}
	}
	public class EveryoneUser : User
	{
		public EveryoneUser() : base("everyone", "world", "", 0)
		{
			IsDisabled = false;
			privs.IsSystem = true;
			IsReplicatedOverNetwork = false;
		}
	}
	public class User
	{
		internal string Username;
		internal string UserGroup;
		internal string UserKey;
		internal int UserLockType;
		internal bool IsHidden;
		internal bool IsDisabled;
		internal bool IsLocal = true;
		public string UserName => Username;
		public bool IsRoot => privs.IsRoot | privs.IsSystem;
		public bool IsSystem => privs.IsSystem;
		public bool IsRegularRoot => !privs.IsSystem && privs.IsRoot;
		public bool IsAbleToManageHardware => privs.IsAbleToManageHardware || IsRoot;
		public bool IsAbleToManageServices => privs.IsAbleToManageServices || IsRoot;
		public bool IsAbleToUseProtectedFS => privs.IsAbleToUseProtectedFS || IsRoot;
		public bool IsAbleToManageUsers => privs.IsAbleToManageUsers || IsRoot;
		public bool IsAbleToShutdown => privs.IsAbleToShutdown || IsRoot;
		public bool IsReplicated => IsReplicatedOverNetwork;
		protected bool IsReplicatedOverNetwork = false;
		protected UserPrivileges privs;
		public User(string userName, string userGroup, string userKey, int userLockType)
		{
			Username = userName.Replace("/", "_").Replace("\\", "_").Replace(";", "_").Replace(" ", "_");
			UserGroup = userGroup;
			UserKey = userKey;
			UserLockType = userLockType;
			IsHidden = false;
			IsDisabled = true;
			privs = new();
		}
		public void SetUserGroup(string group)
		{
			if (!UserManager.CurrentUser.IsAbleToManageUsers) throw new InsufficientPrivilegesException();
			UserGroup = group;
		}
		public void SetUserLock(int locktype, string key)
		{
			if (UserManager.CurrentUser != this || !UserManager.CurrentUser.IsAbleToManageUsers) throw new InsufficientPrivilegesException();
			UserLockType = locktype;
			UserKey = key;
		}
		public void SetHiddenState(bool state)
		{
			if (!UserManager.CurrentUser.IsAbleToManageUsers) throw new InsufficientPrivilegesException();
			IsHidden = state;
		}
		public void SetDisabledState(bool state)
		{
			if (!UserManager.CurrentUser.IsAbleToManageUsers) throw new InsufficientPrivilegesException();
			IsDisabled = state;
		}
		public void SetReplicatableState(bool state)
		{
			if (!UserManager.CurrentUser.IsAbleToManageUsers) throw new InsufficientPrivilegesException();
			IsReplicatedOverNetwork = state;
		}
		public void MakeRoot()
		{
			if (!UserManager.CurrentUser.IsRoot) throw new InsufficientPrivilegesException();
			privs.IsRoot = true;
		}
		public bool MakeRegular()
		{
			if (!UserManager.CurrentUser.IsRoot) throw new InsufficientPrivilegesException();
			if (UserManager.FindRegularRootUsers().Length == 1) return false;
			privs.IsRoot = false;

			return true;
		}
	}
	public sealed class UserPrivileges
	{
		public bool IsRoot;
		public bool IsSystem;
		public bool IsAbleToManageHardware;
		public bool IsAbleToManageServices;
		public bool IsAbleToUseProtectedFS;
		public bool IsAbleToManageUsers;
		public bool IsAbleToShutdown;
	}
	public sealed class InsufficientPrivilegesException : Exception { }
}
