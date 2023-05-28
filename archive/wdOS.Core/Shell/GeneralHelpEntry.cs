namespace wdOS.Core.Shell
{
    public class GeneralHelpEntry : IHelpEntry
    {
        string IHelpEntry.EntryName => CurrentName;
        string IHelpEntry.EntryDescription => CurrentDesc;
        public string CurrentName;
        public string CurrentDesc;
        public GeneralHelpEntry(string name, string desc)
        {
            CurrentName = name;
            CurrentDesc = desc;
        }
    }
}
