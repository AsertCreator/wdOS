using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Pillow
{
    public static class ExecutionEngine
    {
        public const int ObjectTypeInteger = 0; // long
        public const int ObjectTypeDecimal = 1; // double
        public const int ObjectTypeBoolean = 2; // long
        public const int ObjectTypeString = 3; // string
        public const int ObjectTypeCompound = 4; // CustomDictionary
        public const int ObjectTypeFunction = 5; // EEFunction
        public const int ObjectTypeUndefined = 7; // null
        public const int ObjectTypeNull = 8; // null
        public static Dictionary<string, EEObject> AllGlobalVariables { get; set; } = new();
        public static List<InstrinsicHandler> InstrinsicHandlers { get; set; } = new();
        public static List<EEObject> AllObjects { get; set; } = new();
        public static List<string> AllStringLiterals { get; set; } = new();
        public static List<EEFunction> AllFunctions { get; set; } = new();
        public static List<EEThread> RunningThreads { get; set; } = new();
        public static Version GetPillowVersion() => new Version(0, 1, 0);
        public static int GetExecutableVersion() => 1;
        public static void StartScheduling()
        {
            List<EEThread> ended = new();
            while (RunningThreads.Count != 0)
            {
                ended.Clear();
                for (int i = 0; i < RunningThreads.Count; i++)
                {
                    var thread = RunningThreads[i];
                    if (thread.Contexts.Count != 0)
                    {
                        var ctx = thread.Contexts.Peek();
                        thread.Step(ref ctx, thread.DumpStack);
                        if (!ctx.IsRunning)
                        {
                            if (thread.Contexts.Count == 1)
                            {
                                if (!ctx.FunctionResult.IsExceptionUnwinding)
                                {
                                    thread.Result = thread.Contexts.Peek().FunctionResult;
                                    ended.Add(thread);
                                }
                                else
                                {
                                    // todo: exception handling in scheduler
                                }
                            }
                            else
                            {
                                if (!ctx.FunctionResult.IsExceptionUnwinding)
                                {
                                    thread.Contexts.Pop(); // discard current context, it has ended
                                    ctx = thread.Contexts.Pop();
                                    ctx.FunctionStack.Push(ctx.FunctionResult.ReturnedValue);
                                }
                                else
                                {
                                    // todo: exception handling in scheduler
                                }
                            }
                        }
                    }
                    else
                    {
                        ended.Add(thread);
                    }
                }
                for (int i = 0; i < ended.Count; i++)
                {
                    RunningThreads.Remove(ended[i]);
                }
            }
        }
        public static EEExecutable Load(byte[] bytes)
        {
            EEExecutable ex = new();
            BinaryReader reader = new(new MemoryStream(bytes), Encoding.ASCII);
            var magic = reader.ReadUInt32();

            if (magic != EEExecutable.ExecutableMagic)
                throw new Exception("Invalid file magic");

            var vers = reader.ReadUInt32();

            if (vers <= GetExecutableVersion())
            {
                var strc = reader.ReadUInt32();
                var glvr = reader.ReadUInt32();
                var func = reader.ReadUInt32();
                var entr = reader.ReadInt32();

                for (int i = 0; i < strc; i++)
                    ex.AllStringLiterals.Add(new(reader.ReadChars(reader.ReadInt32())));

                for (int i = 0; i < func; i++)
                {
                    EEFunction funct = new()
                    {
                        Attribute = (EEFunctionAttribute)reader.ReadByte(),
                        AttributeAux = reader.ReadInt32(),
                        ArgumentCount = reader.ReadInt16(),
                        LocalCount = reader.ReadInt16(),
                        RawInstructions = reader.ReadBytes(reader.ReadInt32())
                    };
                    ex.AllFunctions.Add(funct);
                }

                ex.Entrypoint = ex.AllFunctions[entr];
                ex.GlobalVariableCount = (int)glvr;
                ex.RuntimeVersion = (int)vers;

                return ex;
            }
            throw new Exception("attempted to load plex binary, which runtime version is higher than max supported. " +
                $"binary version: {vers}, max supported verison: {GetExecutableVersion()}");
        }
        public static int CollectGarbage()
        {
            int res = 0;
            for (int i = 0; i < AllObjects.Count; i++)
            {
                var obj = AllObjects[i];
                if (obj.References <= 0)
                {
                    res++;
                    AllObjects.Remove(obj);
                }
            }
            return res;
        }
        public static byte[] Save(EEExecutable exec)
        {
            List<byte> bytes = new();

            bytes.AddRange(BitConverter.GetBytes(EEExecutable.ExecutableMagic));

            bytes.AddRange(BitConverter.GetBytes(GetExecutableVersion()));
            bytes.AddRange(BitConverter.GetBytes(exec.AllStringLiterals.Count));
            bytes.AddRange(BitConverter.GetBytes(exec.GlobalVariableCount));
            bytes.AddRange(BitConverter.GetBytes(exec.AllFunctions.Count));
            bytes.AddRange(BitConverter.GetBytes(exec.AllFunctions.IndexOf(exec.Entrypoint)));

            for (int i = 0; i < exec.AllStringLiterals.Count; i++)
            {
                string str = exec.AllStringLiterals[i];
                bytes.AddRange(BitConverter.GetBytes(str.Length));
                var chs = str.ToCharArray();
                for (int j = 0; j < str.Length; j++)
                    bytes.Add((byte)chs[j]);
            }

            for (int i = 0; i < exec.AllFunctions.Count; i++)
            {
                EEFunction funct = exec.AllFunctions[i];
                bytes.Add((byte)funct.Attribute);
                bytes.AddRange(BitConverter.GetBytes(funct.AttributeAux));
                bytes.AddRange(BitConverter.GetBytes(funct.ArgumentCount));
                bytes.AddRange(BitConverter.GetBytes(funct.LocalCount));
                bytes.AddRange(BitConverter.GetBytes(funct.RawInstructions.Length));
                bytes.AddRange(funct.RawInstructions);
            }

            return bytes.ToArray();
        }
    }
    public enum EEFunctionAttribute { Instrinsic = 1 }
}