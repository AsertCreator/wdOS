using Cosmos.Core;
using Cosmos.Core.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform
{
    // won't work yet
    internal unsafe static class ProcessRuntime
    {
        internal static KernelFunctionTable[] FunctionTables;
        internal const int FileStdOut = -1;
        internal const int FileStdErr = -2;
        internal const int FileStdIn = -3;
        internal static bool initialized = false;
        internal unsafe static void Initialize()
        {
            if (!initialized)
            {
                // it's in form of tables because it's easier to make new features
                FunctionTables = new KernelFunctionTable[]
                {
                    new KernelFunctionTable()
                    {
                        FunctionTableName = "Bootstrapper",
                        FunctionTableID = 0xFF01,
                        Functions = new()
                        {
                            // Bootstrapper.GetTotalRAM
                            [0] = x => Bootstrapper.GetTotalRAM(),
                            // Bootstrapper.GetUsedRAM
                            [1] = x => Bootstrapper.GetUsedRAM()
                        }
                    },
                    new KernelFunctionTable()
                    {
                        FunctionTableName = "PlatformLogger",
                        FunctionTableID = 0xFF02,
                        Functions = new()
                        {
                            // PlatformLogger.Log
                            [0] = x =>
                            {
                                LogInfo* info = (LogInfo*)x;
                                PlatformLogger.Log(
                                    Utilities.FromCString(info->String),
                                    Utilities.FromCString(info->Component),
                                    (LogLevel)info->LogLevel);
                                return 0;
                            },
                            // PlatformLogger.GetSystemLog
                            [1] = x =>
                            {
                                return (uint)Utilities.ToCStringU(PlatformLogger.GetSystemLog().ToString());
                            },
                        }
                    },
                    new KernelFunctionTable()
                    {
                        FunctionTableName = "ProcessRuntime",
                        FunctionTableID = 0xFF03,
                        Functions = new()
                        {
                            // ProcessRuntime.Execute
                            [0] = x =>
                            {
                                ExecuteInfo* info = (ExecuteInfo*)x;
                                string path = Utilities.FromCString(info->ExecutablePath);
                                string cmd = Utilities.FromCString(info->ConsoleArguments);
                                return (uint)Execute(path, cmd);
                            },
                            // ProcessRuntime.Alloc
                            [1] = x => (uint)Alloc(x),
                            // ProcessRuntime.Free
                            [2] = x => { Free((void*)x); return 0; },
                            // ProcessRuntime.GetConsoleArguemnts
                            [3] = x => (uint)GetConsoleArguments(),
                        }
                    },
                    new KernelFunctionTable()
                    {
                        FunctionTableName = "UserConsole",
                        FunctionTableID = 0xFF04,
                        Functions = new()
                        {
                            // ProcessRuntime.GetCursorX
                            [0] = x => UserConsole.GetCursorX(),
                            // ProcessRuntime.GetCursorY
                            [1] = x => UserConsole.GetCursorY(),
                            // ProcessRuntime.GetConsoleForeground
                            [2] = x => UserConsole.GetConsoleForeground(),
                            // ProcessRuntime.GetConsoleBackground
                            [3] = x => UserConsole.GetConsoleBackground(),
                            // ProcessRuntime.SetConsoleForeground
                            [4] = x => { UserConsole.SetConsoleForeground(x); return 0; },
                            // ProcessRuntime.SetConsoleBackground
                            [5] = x => { UserConsole.SetConsoleBackground(x); return 0; },
                            // ProcessRuntime.SetCursorX
                            [6] = x => { UserConsole.SetCursorX(x); return 0; },
                            // ProcessRuntime.SetCursorY
                            [7] = x => { UserConsole.SetCursorY(x); return 0; },
                            // ProcessRuntime.Write
                            [8] = x => { UserConsole.Write((char*)x); return 0; },
                            // ProcessRuntime.Read
                            [9] = x => UserConsole.Read((char*)x)
                        }
                    },
                };
                INTs.SetIntHandler(0xF0, HandleSWI);
                initialized = true;
            }
        }
        internal static KernelFunctionTable RetrieveFunctionTable(ulong id)
        {
            for (int i = 0; i < FunctionTables.Length; i++)
            {
                var table = FunctionTables[i];
                if (table.FunctionTableID == id) return table;
            }
            return null;
        }
        internal unsafe static void HandleSWI(ref INTs.IRQContext ctx)
        {
            uint eax = ctx.EAX;
            uint ecx = ctx.ECX;
            uint result = unchecked((uint)-1);

            ushort tbid = (ushort)(eax & 0xFFFF0000);
            ushort func = (ushort)(eax & 0x0000FFFF << 16);

            var table = RetrieveFunctionTable(tbid);

            if (table == null)
            {
                ctx.EAX = unchecked(int.MaxValue); // will mean "wrong table"
                return;
            }

            if (!table.Functions.TryGetValue(func, out var entry))
            {
                ctx.EAX = unchecked((uint)int.MinValue); // will mean "wrong function"
                return;
            }
            else result = entry(ecx); // actually invoke

            ctx.EAX = result;
            // wow, thats slow
        }
        internal static void* Alloc(uint sz)
        {
            return PlatformManager.CurrentProcess.Alloc(sz);
        }
        internal static void Free(void* addr)
        {
            PlatformManager.CurrentProcess.Free(addr);
        }
        internal static char* GetConsoleArguments()
        {
            return Utilities.ToCStringU(PlatformManager.CurrentProcess.ConsoleArguments);
        }
        internal static int Execute(string path, string cmd)
        {
            if (!FileSystemManager.FileExists(path)) return 1;
            byte[] bytes = FileSystemManager.ReadBytesFile(path);
            ElfFile file = new(bytes, path);
            StartElfFile(file, cmd, true);
            return 0;
        }
        internal unsafe static void MapElfFile(ElfFile file)
        {
            _ = file;
            // todo: mapping
        }
        internal static unsafe int StartElfFile(ElfFile file, string args, bool saveprev)
        {
            MapElfFile(file);
            file.CurrentProcess = new()
            {
                ExecutableFile = file,
                BinaryPath = file.BinaryPath,
                CurrentDirectory = "0:/",
                ConsoleArguments = args,
                IsRunning = true,
                Executor = PlatformManager.CurrentProcess
            };
            int result = Utilities.Call(file.FileHeader->ElfPointerEntry);
            file.CurrentProcess.IsRunning = false;
            PlatformManager.CurrentProcess = file.CurrentProcess.Executor;
            return result;
        }
    }
    internal sealed class KernelFunctionTable
    {
        internal string FunctionTableName;
        internal ushort FunctionTableID;
        internal Dictionary<ushort, Func<uint, uint>> Functions;
    }
    internal sealed unsafe class ElfFile
    {
        internal ElfHeader32* FileHeader;
        internal ElfProgHeader32* ProgHeader;
        internal ElfSectHeader32* SectHeader;
        internal Process CurrentProcess;
        internal byte* RawFile;
        internal string BinaryPath;
        internal byte[] reference;
        internal ElfFile(byte[] bytes, string path)
        {
            BinaryPath = path;
            fixed (byte* ptr = &bytes[0]) RawFile = ptr;
            reference = bytes;
            FileHeader = (ElfHeader32*)RawFile;
            SectHeader = (ElfSectHeader32*)(RawFile + FileHeader->ElfPointerSectionTable);
            ProgHeader = (ElfProgHeader32*)(RawFile + FileHeader->ElfPointerProgTable);
        }
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct ElfProgHeader32
    {
        internal uint ProgType;
        internal uint ProgOffset;
        internal uint ProgVirtAddr;
        internal uint ProgPhysAddr;
        internal uint ProgFileSize;
        internal uint ProgLoadSize;
        internal uint ProgFlags;
        internal uint ProgAlign;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct ElfSectHeader32
    {
        internal uint SectName;
        internal uint SectType;
        internal uint SectFlags;
        internal uint SectAddress;
        internal uint SectOffset;
        internal uint SectSize;
        internal uint SectLink;
        internal uint SectInfo;
        internal uint SectAlign;
        internal uint SectEntSize;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct ElfHeader32
    {
        internal uint ElfMagic;
        internal byte ElfBitWidth;
        internal byte ElfEndianness;
        internal byte ElfVersion;
        internal byte ElfABIType;
        internal byte ElfABIVersion;
        internal readonly uint ElfUnused0;
        internal readonly ushort ElfUnused1;
        internal readonly byte ElfUnused2;
        internal byte ElfFileType;
        internal byte ElfFileArch;
        internal uint ElfVersion2;
        internal uint ElfPointerEntry;
        internal uint ElfPointerProgTable;
        internal uint ElfPointerSectionTable;
        internal uint ElfFlags;
        internal ushort ElfHeaderSize;
        internal ushort ElfProgTableEntrySize;
        internal ushort ElfProgTableEntryCount;
        internal ushort ElfSectionTableEntrySize;
        internal ushort ElfSectionTableEntryCount;
        internal ushort ElfSectionNameTableID;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct LogInfo
    {
        internal char* String;
        internal char* Component;
        internal byte LogLevel;
    }
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct ExecuteInfo
    {
        internal char* ExecutablePath;
        internal char* ConsoleArguments;
    }
    internal unsafe class Process
    {
        internal const int SubsystemEmbedded = 0;
        internal const int SubsystemNative = 1;
        internal const int SubsystemScript = 2;
        internal int PID = PlatformManager.AllocPID();
        internal string BinaryPath;
        internal string ConsoleArguments;
        internal string CurrentDirectory;
        internal ElfFile ExecutableFile;
        internal Process Executor;
        internal List<ProcessUsedMemory> UsedMemory = new();
        internal int SubsystemType = SubsystemEmbedded;
        internal bool IsRunning = false;
        internal uint GetUsedMemory()
        {
            uint c = 0;
            for (int i = 0; i < UsedMemory.Count; i++)
                c += UsedMemory[i].MemorySize;
            return c;
        }
        internal void Dispose()
        {
            for (int i = 0; i < UsedMemory.Capacity; i++)
            {
                var mem = UsedMemory[i];
                UsedMemory.Remove(mem);
                Heap.Free(mem.UsedMemory);
                GCImplementation.Free(mem);
            }
        }
        internal void* Alloc(uint sz)
        {
            void* ptr = Heap.Alloc(sz);
            UsedMemory.Add(new()
            {
                UsedMemory = ptr,
                MemorySize = sz
            });
            return ptr;
        }
        internal bool Free(void* addr)
        {
            for (int i = 0; i < UsedMemory.Count; i++)
            {
                ProcessUsedMemory mem = UsedMemory[i];
                if (mem.UsedMemory == addr)
                {
                    Heap.Free(mem.UsedMemory);
                    UsedMemory.Remove(mem);
                    GCImplementation.Free(mem);
                    return true;
                }
            }
            return false;
        }
    }
    internal unsafe struct ProcessUsedMemory
    {
        internal void* UsedMemory;
        internal uint MemorySize;
    }
}
