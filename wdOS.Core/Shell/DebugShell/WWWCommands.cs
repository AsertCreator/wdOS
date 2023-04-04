using System;

namespace wdOS.Core.Shell.DebugShell
{
    public static class WWWCommands
    {
        public static void AddCommands()
        {
            TShellManager.AllCommands.Add(new PingCommand());
        }
        public class PingCommand : ConsoleCommand
        {
            public override string Name => "ping";
            public override string Description => "pings web server and determines its reachability";
            public override int Execute(string[] args)
            {
                // string url = args[0];
                // NetworkManager.SendHTTPRequest(new HTTPRequest()
                // {
                //     pagelocation = url,
                //     requestmethod = "GET",
                //     requesttext = "",
                //     requesttype = "GET",
                //     serverdomain = url,
                //     serverhost = url
                // });
                throw new NotImplementedException();
            }
        }
    }
}
