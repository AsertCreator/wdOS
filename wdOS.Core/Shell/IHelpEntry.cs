using System;
using System.Collections.Generic;

namespace wdOS.Core.Shell
{
    public interface IHelpEntry
    {
        public string EntryName { get; }
        public string EntryDescription { get; }
        public static void ShowHelpMenu(List<IHelpEntry> entries)
        {
            int maxlength = 0;
            foreach (var entry in entries)
            {
                if (entry.EntryName.Length > maxlength)
                { maxlength = entry.EntryName.Length; }
            }
            foreach (var entry in entries)
            {
                int numberSpaces = maxlength - entry.EntryName.Length;
                Console.WriteLine($"{entry.EntryName}{new string(' ', numberSpaces + 1)}- {entry.EntryDescription}");
            }
            Console.WriteLine($"total number of entries: {entries.Count}");
        }
    }
}
