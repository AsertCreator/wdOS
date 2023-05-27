using Cosmos.Core.Memory;
using Cosmos.HAL;
using Cosmos.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform
{
    internal static class ServiceManager
    {
        internal static List<Service> Services = new();
        internal static PIT.PITTimer ServiceTimer;
        private static bool initialized = false;
        private static uint nextsid = 0;
        internal static void Initialize()
        {
            if (!initialized)
            {
                ServiceTimer = new PIT.PITTimer(() =>
                {
                    try
                    {
                        for (int i = 0; i < Services.Count; i++)
                        {
                            var service = Services[i];
                            if (!service.IsEnabled)
                                if (!service.Tick(service))
                                    DisableService(service.SID);
                        }
                    }
                    catch { }
                },
                500000000, true);
                Cosmos.HAL.Global.PIT.RegisterTimer(ServiceTimer);

                CreateManagedService("PeriodicGC", "Service, that periodically collects memory garbage", () =>
                {
                    Heap.Collect();
                    return true;
                });

                initialized = true;
            }
        }
        internal static uint AllocSID() => nextsid++;
        internal unsafe static uint CreateManagedService(string name, string desc, Func<bool> callback)
        {
            Service serv = new()
            {
                Name = name,
                Description = desc,
                AuxObject = callback,
                Tick = x => ((Func<bool>)x.AuxObject)()
            };
            Services.Add(serv);
            return serv.SID;
        }
        internal static Service GetServiceBySID(uint sid)
        {
            for (int i = 0; i < Services.Count; i++)
            {
                var service = Services[i];
                if (service.SID == sid) return service;
            }
            return null;
        }
        internal static bool EnableService(uint sid)
        {
            var service = GetServiceBySID(sid);
            if (service == null) return false;
            service.IsEnabled = true;
            return true;
        }
        internal static bool DisableService(uint sid)
        {
            var service = GetServiceBySID(sid);
            if (service == null) return false;
            service.IsEnabled = false;
            return true;
        }
    }
    internal unsafe sealed class Service
    {
        internal string Name;
        internal string Description;
        internal bool IsEnabled = true;
        internal object AuxObject = null;
        internal uint SID = ServiceManager.AllocSID();
        internal Func<Service, bool> Tick;
    }
}
