using System;
using System.Collections.Generic;

namespace wdOS.Core.Shells
{
    internal abstract class HelpEntry
    {
        internal abstract string Name { get; }
        internal abstract string Description { get; }
        internal static void ShowHelpMenu(List<HelpEntry> entries)
        {
            int maxlength = 0;
            foreach (var entry in entries)
            {
                if (entry.Name.Length > maxlength)
                { maxlength = entry.Name.Length; }
            }
            foreach (var entry in entries)
            {
                int numberSpaces = maxlength - entry.Name.Length;
                Console.WriteLine($"{entry.Name}{new string(' ', numberSpaces + 1)}- {entry.Description}");
            }
            Console.WriteLine($"Total number of entries: {entries.Count}");
        }
    }
}
