using Cosmos.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Core
{
    internal static class API
    {
        internal static List<Endpoint> AllEndpoints = new() 
        { 
            
        };
        internal static Application CurrentlyAccessing;
        internal static void SWI(ref INTs.IRQContext context)
        {
            try
            {
                if (context.EAX < AllEndpoints.Count)
                {

                }
            }
            catch(Exception e)
            {
                //CurrentlyAccessing.Info = new(e.Message);
            }
        }
        internal abstract class Endpoint
        {
            public abstract string Name { get; }
            public abstract void SWI(ref INTs.IRQContext context);
        }
        internal class ConsoleEndpoint : Endpoint
        {
            public override string Name => "ConsoleEndpoint";
            public override void SWI(ref INTs.IRQContext context)
            {
                switch (context.EBX)
                {
                    case 0:
                        break;
                    default:
                        throw new Exception();
                }
            }
        }
    }
}
