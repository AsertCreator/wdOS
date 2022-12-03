namespace wdOS.Core.OS.Shells.TShell
{
    internal class PingCommand : ConsoleCommand
    {
        internal override string Name => "ping";
        internal override string Description => "pings web server and determines its reachability";
        internal override int Execute(string[] args)
        {
            string url = args[0];

            return 0;
        }
    }
}
