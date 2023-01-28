using System;
using System.Linq;
using wdOS.Core.Foundation;

namespace wdOS.Core.Shell.TShell
{
    internal class FormatCommand : ConsoleCommand
    {
        internal override string Name => "format";
        internal override string Description => "format specific drive entirely";
        internal static string[] SupportedFilesystems = new string[] { "FAt32" };
        internal override int Execute(string[] args)
        {
            Console.WriteLine("You can't use format on Windows");
            return 0;
        }
    }
}
