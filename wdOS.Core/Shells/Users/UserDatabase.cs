using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Core.Shells.Users
{
    internal static class UserDatabase
    {
        internal static List<User> AllUsers = new()
        {
            new User()
            {
                IsAdmin = true,
                IsRemovable = false,
                IsEncrypted = false,
                IsTemporary = false,
                Username = "root",
                Password = "",
                Profile = FileSystem.PrivateDir + "\\RootProfile",
                MaxLoginCount = -1
            },
            new User()
            {
                IsAdmin = false,
                IsRemovable = false,
                IsEncrypted = false,
                IsTemporary = false,
                Username = "nobody",
                Password = "",
                Profile = FileSystem.PrivateDir + "\\GuestProfile",
                MaxLoginCount = -1
            },
        };
        internal static void CreateUser(User asu, string username, string password = "", bool isadmin = false)
        {
            User user = new User();
            user.Username = username;
            user.Password = password;
            user.IsEnabled = true;
            user.IsRemovable = true;
            user.IsAdmin = asu.IsAdmin && isadmin;
            user.MaxLoginCount = -1;
            AllUsers.Add(user);
        }
        internal static void DeleteUser(User asu, User who) { if (asu.IsAdmin && asu.IsEnabled && who.IsRemovable) { AllUsers.Remove(who); } }
        internal static void MakeAdmin(User asu, User who) => who.IsAdmin = who.IsAdmin || (asu.IsAdmin && asu.IsEnabled);
        internal static string SaveDatabase()
        {
            throw new NotImplementedException();
        }
    }
    internal class User
    {
        internal static byte NextID;
        internal byte Id = NextID++;
        internal byte EFKey;
        internal byte ESKey;
        internal string Username;
        internal string Password;
        internal string Computer;
        internal string Profile;
        internal bool IsAdmin;
        internal bool IsEnabled;
        internal bool IsTemporary;
        internal bool IsEncrypted;
        internal bool IsRemovable;
        internal int MaxLoginCount;
        internal int CurrentLoginCount;
    }
}
