using IL2CPU.API.Attribs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XSharp;
using XSharp.Assembler;

namespace wdOS.Platform.Plugs
{
    public class HardwareManagerFRPC : AssemblerMethod
    {
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            XS.EnableInterrupts();
            XS.Push(0);
            XS.Push(0);
            XS.LoadIdt(XSRegisters.ESP, true);
            XS.Int3();
            // as idt's size is now 0, cpu will triple fault and reset

            XS.DisableInterrupts();
            XS.Return();
            // but we still may survive after that
        }
    }
}
