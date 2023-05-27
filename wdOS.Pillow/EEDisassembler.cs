using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Pillow
{
    public static class EEDisassembler
    {
        public static int GetInstructionSize(byte opcode) => opcode switch
        {
            0x00 => 1,
            0x01 => 1,
            0x02 => 1,
            0x03 => 1,
            0x04 => 1,
            0x05 => 1,
            0x06 => 9,
            0x07 => 1,
            0x08 => 1,
            0x09 => 1,
            0x0A => 1,
            0x0B => 5,
            0x0C => 1,
            0x0D => 1,
            0x0E => 5,
            0x0F => 1,
            0x10 => 1,
            0x11 => 1,
            0x12 => 1,
            0x13 => 1,
            0x14 => 1,
            0x15 => 1,
            0x16 => 1,
            0x17 => 1,
            0x18 => 5,
            0x19 => 5,
            0x1A => 5,
            0x1B => 5,
            0x1C => 5,
            0x1D => 5,
            0x1E => 5,
            0x1F => 1,
            0x20 => 1,
            0x22 => 1,
            0x23 => 5,
            0x24 => 3,
            0x25 => 2,
            0x26 => 1,
            0x27 => 1,
            0x28 => 1,
            _ => 1,
        };
        public static string DisassembleInstruction(byte[] inst) => inst[0] switch
        {
            0x00 => "nop",
            0x01 => "ret",
            0x02 => "add",
            0x03 => "sub",
            0x04 => "mul",
            0x05 => "div",
            0x06 => $"pushint {BitConverter.ToInt64(inst.AsSpan()[1..9])}",
            0x07 => "pushfalse",
            0x08 => "pushtrue",
            0x09 => "pushnull",
            0x0A => "pushundf",
            0x0B => $"pushstr {BitConverter.ToInt32(inst.AsSpan()[1..5])}",
            0x0C => "pusharg",
            0x0D => "pushobj",
            0x0E => $"pushfunc {BitConverter.ToInt32(inst.AsSpan()[1..5])}",
            0x0F => "setfield",
            0x10 => "getfield",
            0x11 => "setlocal",
            0x12 => "getlocal",
            0x13 => "pusheh",
            0x14 => "popeh",
            0x15 => "throw",
            0x16 => "deletefield",
            0x17 => "call",
            0x18 => $"br {BitConverter.ToInt32(inst.AsSpan()[1..5])}",
            0x19 => $"breq {BitConverter.ToInt32(inst.AsSpan()[1..5])}",
            0x1A => $"brneq {BitConverter.ToInt32(inst.AsSpan()[1..5])}",
            0x1B => $"brbt {BitConverter.ToInt32(inst.AsSpan()[1..5])}",
            0x1C => $"brlt {BitConverter.ToInt32(inst.AsSpan()[1..5])}",
            0x1D => $"brez {BitConverter.ToInt32(inst.AsSpan()[1..5])}",
            0x1E => $"brnez {BitConverter.ToInt32(inst.AsSpan()[1..5])}",
            0x1F => "dup",
            0x20 => "pop",
            0x22 => "swap",
            0x23 => $"pushint.i {BitConverter.ToInt32(inst.AsSpan()[1..5])}",
            0x24 => $"pushint.s {BitConverter.ToInt16(inst.AsSpan()[1..3])}",
            0x25 => $"pushint.b {inst[1]}",
            0x26 => "setglobal",
            0x27 => "getglobal",
            0x28 => "getglobal",
            _ => $".byte {inst[0]}",
        };
        public static string DisassembleFunction(EEFunction eef)
        {
            StringBuilder stringBuilder = new();
            var insts = eef.RawInstructions;

            stringBuilder.AppendLine($".maxlocal {eef.LocalCount}");
            stringBuilder.AppendLine($".maxarg {eef.ArgumentCount}");
            stringBuilder.AppendLine($".attribute {(byte)eef.Attribute}");
            stringBuilder.AppendLine($".attributeaux {eef.AttributeAux}");

            stringBuilder.AppendLine();

            for (int i = 0; i < insts.Length; i++)
            {
                switch (insts[i])
                {
                    case 0x00: stringBuilder.AppendLine("nop"); break;
                    case 0x01: stringBuilder.AppendLine("ret"); break;
                    case 0x02: stringBuilder.AppendLine("add"); break;
                    case 0x03: stringBuilder.AppendLine("sub"); break;
                    case 0x04: stringBuilder.AppendLine("mul"); break;
                    case 0x05: stringBuilder.AppendLine("div"); break;
                    case 0x06: stringBuilder.AppendLine($"pushint {BitConverter.ToInt64(insts.AsSpan()[(i + 1)..(i + 9)])}"); i += 8; break;
                    case 0x07: stringBuilder.AppendLine("pushfalse"); break;
                    case 0x08: stringBuilder.AppendLine("pushtrue"); break;
                    case 0x09: stringBuilder.AppendLine("pushnull"); break;
                    case 0x0A: stringBuilder.AppendLine("pushundf"); break;
                    case 0x0B: stringBuilder.AppendLine($"pushstr {BitConverter.ToInt32(insts.AsSpan()[(i + 1)..(i + 5)])}"); i += 4; break;
                    case 0x0C: stringBuilder.AppendLine("pusharg"); break;
                    case 0x0D: stringBuilder.AppendLine("pushobj"); break;
                    case 0x0E: stringBuilder.AppendLine($"pushfunc {BitConverter.ToInt32(insts.AsSpan()[(i + 1)..(i + 5)])}"); i += 4; break;
                    case 0x0F: stringBuilder.AppendLine("setfield"); break;
                    case 0x10: stringBuilder.AppendLine("getfield"); break;
                    case 0x11: stringBuilder.AppendLine("setlocal"); break;
                    case 0x12: stringBuilder.AppendLine("getlocal"); break;
                    case 0x13: stringBuilder.AppendLine("pusheh"); break;
                    case 0x14: stringBuilder.AppendLine("popeh"); break;
                    case 0x15: stringBuilder.AppendLine("throw"); break;
                    case 0x16: stringBuilder.AppendLine("deletefield"); break;
                    case 0x17: stringBuilder.AppendLine("call"); break;
                    case 0x18: stringBuilder.AppendLine($"br {BitConverter.ToInt32(insts.AsSpan()[(i + 1)..(i + 5)])}"); i += 4; break;
                    case 0x19: stringBuilder.AppendLine($"breq {BitConverter.ToInt32(insts.AsSpan()[(i + 1)..(i + 5)])}"); i += 4; break;
                    case 0x1A: stringBuilder.AppendLine($"brneq {BitConverter.ToInt32(insts.AsSpan()[(i + 1)..(i + 5)])}"); i += 4; break;
                    case 0x1B: stringBuilder.AppendLine($"brbt {BitConverter.ToInt32(insts.AsSpan()[(i + 1)..(i + 5)])}"); i += 4; break;
                    case 0x1C: stringBuilder.AppendLine($"brlt {BitConverter.ToInt32(insts.AsSpan()[(i + 1)..(i + 5)])}"); i += 4; break;
                    case 0x1D: stringBuilder.AppendLine($"brez {BitConverter.ToInt32(insts.AsSpan()[(i + 1)..(i + 5)])}"); i += 4; break;
                    case 0x1E: stringBuilder.AppendLine($"brnez {BitConverter.ToInt32(insts.AsSpan()[(i + 1)..(i + 5)])}"); i += 4; break;
                    case 0x1F: stringBuilder.AppendLine("dup"); break;
                    case 0x20: stringBuilder.AppendLine("pop"); break;
                    case 0x22: stringBuilder.AppendLine("swap"); break;
                    case 0x23: stringBuilder.AppendLine($"pushint.i {BitConverter.ToInt32(insts.AsSpan()[(i + 1)..(i + 5)])}"); i += 4; break;
                    case 0x24: stringBuilder.AppendLine($"pushint.s {BitConverter.ToInt16(insts.AsSpan()[(i + 1)..(i + 3)])}"); i += 2; break;
                    case 0x25: stringBuilder.AppendLine($"pushint.b {insts.AsSpan()[i + 1]}"); break;
                    case 0x26: stringBuilder.AppendLine("setglobal"); break;
                    case 0x27: stringBuilder.AppendLine("getglobal"); break;
                    case 0x28: stringBuilder.AppendLine("getglobal"); break;
                    default: stringBuilder.AppendLine($".byte {insts[i]}"); break;
                }
            }

            return stringBuilder.ToString();
        }
    }
}
