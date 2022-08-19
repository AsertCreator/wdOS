using IL2CPU.API.Attribs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XSharp;
using XSharp.Assembler;

namespace wdOS.Plugs
{
    [Plug(Target = typeof(object))]
    internal class ObjectPlug : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.Set(XSRegisters.EAX, XSRegisters.EBP, sourceDisplacement: 0x8);
            XS.Push(XSRegisters.EAX);
        }
    }
}
