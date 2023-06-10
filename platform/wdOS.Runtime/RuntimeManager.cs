using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using wdOS.Pillow;

namespace wdOS.Platform
{
    public unsafe static class RuntimeManager
    {
        public const int FileStdOut = -1;
        public const int FileStdErr = -2;
        public const int FileStdIn = -3;
        public static List<IRuntimeComponent> AllComponents = new();
        public static bool initialized = false;
        public unsafe static void Initialize()
        {
            if (!initialized)
            {
                ExecutionEngine.InstrinsicHandlers.Add(delegate(ref EEFunctionExecutionContext ctx) // IOWrite, icall 0
                {
                    var raw_write = ctx.FunctionStack.Pop();
                    var raw_index = ctx.FunctionStack.Pop();
                    var raw_descr = ctx.FunctionStack.Pop();

                    if (raw_write.ObjectType != ExecutionEngine.ObjectTypeCompound || raw_write.ObjectType != ExecutionEngine.ObjectTypeString) 
                    { 
                        ctx.FunctionStack.Push(new EEObject(ExecutionEngine.ObjectTypeUndefined));
                        return;
                    }
                    if (raw_index.ObjectType != ExecutionEngine.ObjectTypeInteger) 
                    {
                        ctx.FunctionStack.Push(new EEObject(ExecutionEngine.ObjectTypeUndefined));
                        return;
                    }
                    if (raw_descr.ObjectType != ExecutionEngine.ObjectTypeInteger) 
                    {
                        ctx.FunctionStack.Push(new EEObject(ExecutionEngine.ObjectTypeUndefined));
                        return;
                    }

                    var result = 0;

                    if (raw_write.ObjectType != ExecutionEngine.ObjectTypeString)
                    {
                        var write = (CustomDictionary)raw_write.ObjectValue;
                        var index = (int)raw_index.ObjectValue;
                        var descr = (int)raw_descr.ObjectValue;

                        result = Write(write.ToByteArray(), index, descr);
                    }
                    else
                    {
                        var write = ((string)raw_write.ObjectValue).ToCharArray();
                        var index = (int)raw_index.ObjectValue;
                        var descr = (int)raw_descr.ObjectValue;
                        byte[] bytes = new byte[write.Length];

                        for (int i = 0; i < write.Length; i++) bytes[i] = (byte)write[i];

                        result = Write(bytes, index, descr);
                    }

                    ctx.FunctionStack.Push(new EEObject(ExecutionEngine.ObjectTypeInteger) { ObjectValue = result });
                    return;
                });
                ExecutionEngine.InstrinsicHandlers.Add(delegate (ref EEFunctionExecutionContext ctx) // ConsoleWrite, icall 1
                {
                    var raw_write = ctx.FunctionStack.Pop();

                    if (raw_write.ObjectType != ExecutionEngine.ObjectTypeString)
                    {
                        ctx.FunctionStack.Push(new EEObject(ExecutionEngine.ObjectTypeUndefined));
                        return;
                    }

                    var write = (string)raw_write.ObjectValue;

                    Console.Write(write);

                    ctx.FunctionStack.Push(new EEObject(ExecutionEngine.ObjectTypeUndefined));
                    return;
                });
                ExecutionEngine.InstrinsicHandlers.Add(delegate (ref EEFunctionExecutionContext ctx) // ConsoleReadKey, icall 2
                {
                    ctx.FunctionStack.Push(new EEObject(ExecutionEngine.ObjectTypeUndefined));
                    // not implemented
                    return;
                });
                ExecutionEngine.InstrinsicHandlers.Add(delegate (ref EEFunctionExecutionContext ctx) // ConvertToString, icall 3
                {
                    var obj = ctx.FunctionStack.Pop();

                    ctx.FunctionStack.Push(new EEObject(ExecutionEngine.ObjectTypeString) { ObjectValue = obj.ToString() });

                    return;
                });
                ExecutionEngine.InstrinsicHandlers.Add(delegate (ref EEFunctionExecutionContext ctx) // IsTypeOf, icall 4
                {
                    var type = ctx.FunctionStack.Pop();
                    var obj = ctx.FunctionStack.Pop();

                    if (type.ObjectType != ExecutionEngine.ObjectTypeInteger)
                    {
                        ctx.FunctionStack.Push(new EEObject(ExecutionEngine.ObjectTypeUndefined));
                        return;
                    }

                    int actualtype = (int)type.ObjectValue;

                    ctx.FunctionStack.Push(new EEObject(ExecutionEngine.ObjectTypeBoolean) 
                    { 
                        ObjectValue = obj.ObjectType == actualtype ? 1 : 0
                    });

                    return;
                });
                initialized = true;
            }
        }
        public static int Write(byte[] bytes, int index, int fd)
        {
            switch (fd)
            {
                case FileStdOut:
                    for (int i = 0; i < bytes.Length; i++)
                        Console.Write((char)bytes[i]);
                    return bytes.Length;
                case FileStdErr:
                    for (int i = 0; i < bytes.Length; i++)
                        Console.Write((char)bytes[i]);
                    return bytes.Length;
            }
            return -1;
        }
        public static int GetRuntimeVersion() => 10;
    }
}
