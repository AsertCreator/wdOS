using Cosmos.Core;
using Cosmos.Core.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using wdOS.Pillow;

namespace wdOS.Platform
{
    internal unsafe static class RuntimeManager
    {
        internal const int FileStdOut = -1;
        internal const int FileStdErr = -2;
        internal const int FileStdIn = -3;
        internal static bool initialized = false;
        internal unsafe static void Initialize()
        {
            if (!initialized)
            {
                ExecutionEngine.InstrinsicHandlers.Add(delegate(ref EEFunctionExecutionContext ctx) // IOWrite
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
                ExecutionEngine.InstrinsicHandlers.Add(delegate (ref EEFunctionExecutionContext ctx) // ConsoleWrite
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
                initialized = true;
            }
        }
        internal static int Write(byte[] bytes, int index, int fd)
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
        internal static int Execute(string path, string cmd)
        {
            Process process = new Process();
            int result = int.MinValue;
            process.IsRunning = true;
            PlatformManager.AllProcesses.Add(process);

            try
            {
                if (!FileSystemManager.FileExists(path)) return result;

                byte[] bytes = FileSystemManager.ReadBytesFile(path);
                EEExecutable executable = ExecutionEngine.Load(bytes);

                var funcres = executable.Execute(cmd);

                if (funcres.IsExceptionUnwinding)
                {
                    OnProcessCrash(funcres);
                    return result;
                }

                if (funcres.ReturnedValue.ObjectType == ExecutionEngine.ObjectTypeInteger)
                    result = (int)funcres.ReturnedValue.ObjectValue;
                else
                    result = 0;

                process.IsRunning = false;
            }
            catch
            {
                process.IsRunning = false;
                OnProcessCrash(null);
            }
            return result;
        }
        internal static int GetRuntimeVersion() => 10;
        internal static void OnProcessCrash(EEFunctionResult res)
        {
            // todo: process crash handling
        }
    }
    internal unsafe class Process
    {
        internal int PID = PlatformManager.AllocPID();
        internal string BinaryPath;
        internal string ConsoleArguments;
        internal string CurrentDirectory;
        internal EEExecutable ExecutableFile;
        internal Process Executor;
        internal bool IsRunning = false;
    }
}
