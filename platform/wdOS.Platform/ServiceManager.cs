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
    public static class ServiceManager
    {
        public static List<Service> Services = new();
        public static List<Action> EachSecondActions = new();
        public static PIT.PITTimer ServiceTimer;
        private static bool initialized = false;
        private static uint osscilations = 0;
        private static uint nextsid = 0;
        public static void Initialize()
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

                CreateManagedService("PeriodicGC", "Service, that periodically collects memory garbage. This is a critical service.", () =>
                {
                    Heap.Collect();
                    return true;
                });

				CreateManagedService("EachSecond", "Service, that calls certain methods within kernel each second. This is a critical service.", () =>
				{
                    osscilations++;
                    if (osscilations % 2 == 0) 
                        for (int i = 0; i < EachSecondActions.Count; i++) 
                            EachSecondActions[i]();
					return true;
				});

				initialized = true;
            }
        }
        public static uint AllocSID() => nextsid++;
        public unsafe static uint CreateManagedService(string name, string desc, Func<bool> callback)
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
        public static Service GetServiceBySID(uint sid)
        {
            for (int i = 0; i < Services.Count; i++)
            {
                var service = Services[i];
                if (service.SID == sid) return service;
            }
            return null;
        }
        public static bool EnableService(uint sid)
        {
            var service = GetServiceBySID(sid);
            if (service == null) return false;
            service.IsEnabled = true;
            return true;
        }
        public static bool DisableService(uint sid)
        {
            var service = GetServiceBySID(sid);
            if (service == null) return false;
            service.IsEnabled = false;
            return true;
        }
    }
    public unsafe sealed class Service
    {
        public string Name;
        public string Description;
        public bool IsEnabled = true;
        public object AuxObject = null;
        public uint SID = ServiceManager.AllocSID();
        public Func<Service, bool> Tick;
    }
}
