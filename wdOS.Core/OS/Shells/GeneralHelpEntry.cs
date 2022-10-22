namespace wdOS.Core.OS.Shells
{
    internal class GeneralHelpEntry : HelpEntry
    {
        internal override string Name => CurrentName;
        internal override string Description => CurrentDesc;
        internal string CurrentName;
        internal string CurrentDesc;
        internal GeneralHelpEntry(string name, string desc)
        {
            CurrentName = name;
            CurrentDesc = desc;
        }
    }
}
