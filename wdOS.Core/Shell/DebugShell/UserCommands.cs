using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wdOS.Core.Foundation;
using static wdOS.Core.Foundation.SystemDatabase;

namespace wdOS.Core.Shell.DebugShell
{
    public static class UserCommands
    {
        public static void AddCommands()
        {
            TShellManager.AllCommands.Add(new SwitchUserCommand());
            TShellManager.AllCommands.Add(new AddUserCommand());
            TShellManager.AllCommands.Add(new WhoAmICommand());
            TShellManager.AllCommands.Add(new ListUserCommand());
        }
        public class SwitchUserCommand : ConsoleCommand
        {
            public override string Name => "su";
            public override string Description => "switches user";
            public override int Execute(string[] args)
            {
                if (args.Length != 1)
                {
                    Console.WriteLine("su: invalid count of arguments");
                    return 1;
                }
                string username = args[0];
                UserBase user = FindByName(username);
                if (user != null)
                {
                    CurrentUser = user;
                    return 0;
                }
                Console.WriteLine("su: no such user");
                return 1;
            }
        }
        public class AddUserCommand : ConsoleCommand
        {
            public override string Name => "adduser";
            public override string Description => "adds new user";
            public override int Execute(string[] args)
            {
                if (args.Length != 1)
                {
                    Console.WriteLine("adduser: invalid count of arguments");
                    return 1;
                }
                string username = args[0];
                if (FindByName(username) != null)
                {
                    Console.WriteLine("adduser: user with that name already exists");
                    return 1;
                }
                CreateUser(new CustomUser(username, "users", "", 0));
                return 1;
            }
        }
        public class WhoAmICommand : ConsoleCommand
        {
            public override string Name => "whoami";
            public override string Description => "shows current user info";
            public override int Execute(string[] args)
            {
                if (args.Length != 0)
                {
                    Console.WriteLine("whoami: invalid count of arguments");
                    return 1;
                }
                Console.WriteLine(CurrentUser.UserName);
                return 1;
            }
        }
        public class ListUserCommand : ConsoleCommand
        {
            public override string Name => "lsuser";
            public override string Description => "shows all users info";
            public override int Execute(string[] args)
            {
                if (args.Length != 0)
                {
                    Console.WriteLine("listuser: invalid count of arguments");
                    return 1;
                }
                List<IHelpEntry> entries = new List<IHelpEntry>();
                foreach (var user in AvailableUsers)
                {
                    if(!user.IsHidden)
                        entries.Add(new GeneralHelpEntry(user.UserName, 
                            $"group: {user.UserGroup}, disabled: {user.IsDisabled}, root: {user.IsRoot}"));
                }
                IHelpEntry.ShowHelpMenu(entries);
                return 1;
            }
        }
    }
}
