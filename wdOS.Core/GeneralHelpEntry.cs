namespace wdOS.Core
{
    internal class GeneralHelpEntry : IHelpEntry
    {
        string IHelpEntry.Name => CurrentName;
        string IHelpEntry.Description => CurrentDesc;
        internal string CurrentName;
        internal string CurrentDesc;
        internal GeneralHelpEntry(string name, string desc)
        {
            CurrentName = name;
            CurrentDesc = desc;
        }
    }
}
