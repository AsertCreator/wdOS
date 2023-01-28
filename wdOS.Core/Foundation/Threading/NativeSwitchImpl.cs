using System;
using System.Linq;
using System.Reflection;
using IL2CPU.API;
using IL2CPU.API.Attribs;
using XSharp;
using XSharp.Assembler;
using XSharp.Assembler.x86;

namespace wdOS.Core.Foundation.Threading
{
    [Plug(Target = typeof(Thread))]
    public class NativeSwitchImpl : AssemblerMethod
    {
        public static uint SwitcherESP;
        public static uint SwitcheeEIP;
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            string label = LabelName.GetStaticFieldName(typeof(NativeSwitchImpl).GetFields().Where(x => x.Name == "SwitcherESP").First());
            string desti = LabelName.GetStaticFieldName(typeof(NativeSwitchImpl).GetFields().Where(x => x.Name == "SwitcheeEIP").First());
            XS.Set(label, XSRegisters.ESP, destinationIsIndirect: true);
            XS.Set(XSRegisters.EAX, label, sourceIsIndirect: true, sourceDisplacement: 4);
            XS.Set(desti, XSRegisters.EAX, destinationIsIndirect: true);
            XS.Set(XSRegisters.EAX, label, sourceIsIndirect: true, sourceDisplacement: 40);
            XS.Set(XSRegisters.EBX, label, sourceIsIndirect: true, sourceDisplacement: 36);
            XS.Set(XSRegisters.ECX, label, sourceIsIndirect: true, sourceDisplacement: 32);
            XS.Set(XSRegisters.EDX, label, sourceIsIndirect: true, sourceDisplacement: 28);
            XS.Set(XSRegisters.ESP, label, sourceIsIndirect: true, sourceDisplacement: 24);
            XS.Set(XSRegisters.EBP, label, sourceIsIndirect: true, sourceDisplacement: 20);
            XS.Set(XSRegisters.ESI, label, sourceIsIndirect: true, sourceDisplacement: 16);
            XS.Set(XSRegisters.EDI, label, sourceIsIndirect: true, sourceDisplacement: 12);
            new Jump()
            {
                DestinationLabel = desti,
                DestinationIsIndirect = true
            };
        }
    }
}
