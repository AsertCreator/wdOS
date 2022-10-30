using Cosmos.Core;

namespace wdOS.Core.OS.Foundation
{
    internal static class API
    {
        internal static INTs.IRQDelegate APIHandler;
        internal static void Setup()
        {
            APIHandler = delegate (ref INTs.IRQContext context)
            {
                switch (context.EAX)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                    case 4:
                        break;
                    case 5:
                        break;
                    case 6:
                        break;
                    case 7:
                        break;
                    case 8:
                        break;
                    case 9:
                        break;
                }
            };
            INTs.SetIntHandler(0xF0, APIHandler);
            CPU.UpdateIDT(true);
        }
    }
}
