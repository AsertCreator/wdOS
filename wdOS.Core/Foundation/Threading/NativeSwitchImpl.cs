using System;
using System.Reflection;
using IL2CPU.API;
using IL2CPU.API.Attribs;
using XSharp;
using XSharp.Assembler;
using XSharp.Assembler.x86;

namespace wdOS.Core.Foundation.Threading
{
    [Plug(Target = typeof(Thread))]
    internal class NativeSwitchImpl : AssemblerMethod
    {
        internal static uint SwitcherESP;
        public override void AssembleNew(Assembler aAssembler, object aMethodInfo)
        {
            string label = LabelName.GetStaticFieldName(typeof(NativeSwitchImpl).GetRuntimeField("SwitcherESP"));
            XS.Set(label, XSRegisters.ESP, destinationIsIndirect: true);
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
                DestinationReg = XSRegisters.ESP,
                DestinationIsIndirect = true,
                DestinationDisplacement = 4
            };
        }
    }
}
