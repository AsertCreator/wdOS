using Cosmos.HAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform.Foundation
{
    public static class ServiceManager
    {
        public static List<Service> Services = new();
        public static PIT.PITTimer ServiceTimer;
        private static bool initialized = false;
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
                            service.Tick();
                        }
                    }
                    catch { }
                }, 
                500000000, true);
                initialized = true;
                Global.PIT.RegisterTimer(ServiceTimer);
            }
        }
        public abstract class Service
        {
            public abstract string Name { get; }
            public abstract string Description { get; }
            public abstract void Tick();
        }
    }
}
