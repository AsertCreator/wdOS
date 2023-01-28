using System;

namespace wdOS.Core.Shell.TShell
{
    internal class PingCommand : ConsoleCommand
    {
        internal override string Name => "ping";
        internal override string Description => "pings web server and determines its reachability";
        internal override int Execute(string[] args)
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
            return 0;
        }
    }
}
