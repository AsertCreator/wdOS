namespace wdOS.Core.OS.Shells
{
    internal class GeneralHelpEntry : IHelpEntry
    {
        string IHelpEntry.EntryName => CurrentName;
        string IHelpEntry.EntryDescription => CurrentDesc;
        internal string CurrentName;
        internal string CurrentDesc;
        internal GeneralHelpEntry(string name, string desc)
        {
            CurrentName = name;
            CurrentDesc = desc;
        }
    }
}
