using System;
using System.Collections.Generic;

namespace wdOS.Core
{
    internal interface IHelpEntry
    {
        internal string Name { get; }
        internal string Description { get; }
        internal static void ShowHelpMenu(List<IHelpEntry> entries)
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
