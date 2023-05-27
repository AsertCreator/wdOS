using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Pillow
{
    public static class EEAssembler
    {
        // experimental, highly breakable
        public static EEFunction AssemblePillowIL(string str)
        {
            EEFunction func = new();
            string[] lines = str.Split('\n');
            List<byte> insts = new();
            short localcount = 0;
            short argcount = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].TrimStart().TrimEnd();
                int num = line.IndexOf("//");

                if (num != -1)
                    line = line[..num];

                string[] words = line.Split(' ');
                for (int j = 0; j < words.Length; j++)
                {
                    if (words[j] == ".maxlocal") localcount = short.Parse(words[++j]);
                    if (words[j] == ".maxarg") argcount = short.Parse(words[++j]);
                    if (words[j] == ".attribute") func.Attribute = (EEFunctionAttribute)byte.Parse(words[++j]);
                    if (words[j] == ".attributeaux") func.AttributeAux = int.Parse(words[++j]);
                    if (words[j] == ".byte") insts.Add(byte.Parse(words[++j]));
                    if (words[j] == "nop") insts.Add(0x00);
                    if (words[j] == "ret") insts.Add(0x01);
                    if (words[j] == "add") insts.Add(0x02);
                    if (words[j] == "sub") insts.Add(0x03);
                    if (words[j] == "mul") insts.Add(0x04);
                    if (words[j] == "div") insts.Add(0x05);
                    if (words[j] == "pushint")
                    {
                        insts.Add(0x06);
                        insts.AddRange(BitConverter.GetBytes(long.Parse(words[++j])));
                    }
                    if (words[j] == "pushint.i")
                    {
                        insts.Add(0x23);
                        insts.AddRange(BitConverter.GetBytes(int.Parse(words[++j])));
                    }
                    if (words[j] == "pushint.s")
                    {
                        insts.Add(0x24);
                        insts.AddRange(BitConverter.GetBytes(short.Parse(words[++j])));
                    }
                    if (words[j] == "pushint.b")
                    {
                        insts.Add(0x25);
                        insts.Add(byte.Parse(words[++j]));
                    }
                    if (words[j] == "pushfalse") insts.Add(0x07);
                    if (words[j] == "pushtrue") insts.Add(0x08);
                    if (words[j] == "pushnull") insts.Add(0x09);
                    if (words[j] == "pushundf") insts.Add(0x0A);
                    if (words[j] == "pushstr")
                    {
                        insts.Add(0x0B);
                        insts.AddRange(BitConverter.GetBytes(uint.Parse(words[++j])));
                    }
                    if (words[j] == "pusharg")
                    {
                        insts.Add(0x0C);
                        insts.AddRange(BitConverter.GetBytes(uint.Parse(words[++j])));
                    }
                    if (words[j] == "pushobj") insts.Add(0x0D);
                    if (words[j] == "pushfunc")
                    {
                        insts.Add(0x0E);
                        insts.AddRange(BitConverter.GetBytes(uint.Parse(words[++j])));
                    }
                    if (words[j] == "setfield") insts.Add(0x0F);
                    if (words[j] == "getfield") insts.Add(0x10);
                    if (words[j] == "setlocal") insts.Add(0x11);
                    if (words[j] == "getlocal") insts.Add(0x12);
                    if (words[j] == "pusheh") insts.Add(0x13);
                    if (words[j] == "popeh") insts.Add(0x14);
                    if (words[j] == "throw") insts.Add(0x15);
                    if (words[j] == "deletefield") insts.Add(0x16);
                    if (words[j] == "call") insts.Add(0x17);
                    if (words[j] == "br")
                    {
                        insts.Add(0x18);
                        insts.AddRange(BitConverter.GetBytes(uint.Parse(words[++j])));
                    }
                    if (words[j] == "breq")
                    {
                        insts.Add(0x19);
                        insts.AddRange(BitConverter.GetBytes(uint.Parse(words[++j])));
                    }
                    if (words[j] == "brneq")
                    {
                        insts.Add(0x1A);
                        insts.AddRange(BitConverter.GetBytes(uint.Parse(words[++j])));
                    }
                    if (words[j] == "brbt")
                    {
                        insts.Add(0x1B);
                        insts.AddRange(BitConverter.GetBytes(uint.Parse(words[++j])));
                    }
                    if (words[j] == "brlt")
                    {
                        insts.Add(0x1C);
                        insts.AddRange(BitConverter.GetBytes(uint.Parse(words[++j])));
                    }
                    if (words[j] == "brez")
                    {
                        insts.Add(0x1D);
                        insts.AddRange(BitConverter.GetBytes(uint.Parse(words[++j])));
                    }
                    if (words[j] == "brnez")
                    {
                        insts.Add(0x1E);
                        insts.AddRange(BitConverter.GetBytes(uint.Parse(words[++j])));
                    }
                    if (words[j] == "dup") insts.Add(0x1F);
                    if (words[j] == "pop") insts.Add(0x20);
                    if (words[j] == "swap") insts.Add(0x22);
                    if (words[j] == "setglobal") insts.Add(0x26);
                    if (words[j] == "getglobal") insts.Add(0x27);
                }
            }
            func.RawInstructions = insts.ToArray();
            func.ArgumentCount = argcount;
            func.LocalCount = localcount;
            return func;
        }
    }
}
