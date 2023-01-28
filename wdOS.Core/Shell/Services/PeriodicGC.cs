using wdOS.Core.Foundation;
using static wdOS.Core.Foundation.Kernel;

namespace wdOS.Core.Shell.Services
{
    internal class PeriodicGC : ServiceBase
    {
        internal override string Name => "PeriodicGC";
        internal override string Desc => "ServiceBase for automatic memory cleaning";
        internal override int MajorVersion => BuildConstants.VersionMajor;
        internal override int MinorVersion => BuildConstants.VersionMinor;
        internal override int PatchVersion => BuildConstants.VersionPatch;
        internal override void BeforeRun()
        {
            if (SystemSettings.EnablePeriodicGC)
                Log($"Enabled Periodic GC: period {SystemSettings.ServicePeriod} ms");
        }
        internal override void Run()
        {
            if (SystemSettings.EnablePeriodicGC)
            {
                SweepTrash();
            }
        }
    }
}
