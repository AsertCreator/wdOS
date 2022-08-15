using System;
using IL2CPU.API.Attribs;

namespace wdOS.Plugs
{
    [Plug(Target = typeof(MulticastDelegate))]
    public static unsafe class DelegatePlug
    {
        public static int GetHashCode(Delegate aThis, [FieldAccess(Name = "System.IntPtr System.Delegate._methodPtr")] ref IntPtr aAddress)
        {
            return (int)aAddress.ToPointer();
        }
    }
}
