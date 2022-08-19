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
        internal static Application CurrentlyAccessing;
        internal static void SWI(ref INTs.IRQContext context)
        {
            try
            {

            }
            catch
            {
                //CurrentlyAccessing.Info = new(e.Message);
            }
        }
    }
    internal enum SystemCall
    {
        
    }
}
