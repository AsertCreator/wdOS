using System;
using IL2CPU.API.Attribs;

namespace wdOS.Core.Foundation.Threading
{
    [Plug(Target = typeof(Delegate))]
    public static unsafe class DelegatePlug
    {
        // shit, now code can resident only in first 2 gib of ram
        public static int GetHashCode(Delegate th, 
            [FieldAccess(Name = "System.IntPtr System.Delegate._methodPtr")] ref IntPtr addr)
        {
            return (int)addr.ToPointer();
        }
    }
}
