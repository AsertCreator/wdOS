using wdOS.Core.Foundation;
using static wdOS.Core.Foundation.Kernel;

namespace wdOS.Core.Shell.Services
{
    public class PeriodicGC : ShellBase
    {
        public override string ShellName => "PeriodicGC";
        public override string ShellDesc => "service for automatic memory cleaning";
        public override int ShellMajorVersion => SystemDatabase.BuildConstants.VersionMajor;
        public override int ShellMinorVersion => SystemDatabase.BuildConstants.VersionMinor;
        public override int ShellPatchVersion => SystemDatabase.BuildConstants.VersionPatch;
        public override void ShellBeforeRun()
        {
            if (SystemDatabase.SystemSettings.EnablePeriodicGC)
                KernelLogger.Log($"Enabled Periodic GC: period {SystemDatabase.SystemSettings.ServicePeriod} ms");
        }
        public override void ShellRun()
        {
            if (SystemDatabase.SystemSettings.EnablePeriodicGC) SweepTrash();
        }
        public override void ShellAfterRun() { }
    }
}
