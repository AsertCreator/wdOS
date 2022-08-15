using Cosmos.Core;
using Cosmos.System.Graphics;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;
using System;
using System.IO;
using Sys = Cosmos.System;

namespace wdOS
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            try
            {
                Console.WriteLine("Starting system...");
                VFSManager.RegisterVFS(new CosmosVFS(), false);
                Log("Done. Booting into wdOS...");
                Sys.MouseManager.ScreenWidth = 800;
                Sys.MouseManager.ScreenWidth = 600;
                while (Sys.MouseManager.MouseState == Sys.MouseState.None)
                {
                    Console.WriteLine($"Mouse Position: X: {Sys.MouseManager.X}, Y: {Sys.MouseManager.Y}");
                }
            }
            catch (Exception e) { Panic(e); }
        }
        public unsafe static void Panic(Exception e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine($"!!! panic {e.Message}, apps loaded: {/*LoadedApps.Count*/0}!!!");
            Console.WriteLine("You need to restart computer to continue using wdOS");
            Console.WriteLine("Dumping kernel memory to disk...");
            try
            {
                uint end = CPU.GetEndOfKernel();
                byte[] dump = new byte[end];
                fixed (byte* ptr = &dump[0])
                { MemoryOperations.Copy((byte*)0, ptr, (int)end); }
                //WriteBytesFile("0:/KernelDump.dmp", dump);
            }
            catch
            {
                Console.WriteLine("Unable to dump kernel, skipping");
            }
            Console.Write("Dumping done! ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Have a nice day!");
            CPU.Halt();
        }
        public static void Log(string text) => Global.mDebugger.Send(text);
        protected override void Run() { }
        public static void PowerOff(bool restart = false)
        {
            Log("Starting shut down process...");
            Console.Clear();
            Console.WriteLine("Shutting down...");
            if (!restart)
                Sys.Power.Shutdown();
            else
                Sys.Power.Reboot();
        }/*
        public static byte[] StreamFullRead(Stream stream)
        {
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);
            return buffer;
        }
        public static void StreamFullWrite(Stream stream, byte[] data)
        {
            stream.Flush();
            stream.Write(data, 0, data.Length);
        }
        public static string ReadStringFile(string filepath)
        {
            if (VFSManager.DirectoryExists(filepath)) return null;
            if (!VFSManager.FileExists(filepath)) { VFSManager.CreateFile(filepath); }
            return new StreamReader(VFSManager.GetFileStream(filepath)).ReadToEnd();
        }
        public static void WriteStringFile(string filepath, string data)
        {
            if (VFSManager.DirectoryExists(filepath)) return;
            if (!VFSManager.FileExists(filepath)) { VFSManager.CreateFile(filepath); }
            new StreamWriter(VFSManager.GetFileStream(filepath)).WriteLine(data);
        }
        public static byte[] ReadBytesFile(string filepath)
        {
            if (VFSManager.DirectoryExists(filepath)) return null;
            if (!VFSManager.FileExists(filepath)) { VFSManager.CreateFile(filepath); }
            return StreamFullRead(VFSManager.GetFileStream(filepath));
        }
        public static void WriteBytesFile(string filepath, byte[] data)
        {
            if (VFSManager.DirectoryExists(filepath)) return;
            if (!VFSManager.FileExists(filepath)) { VFSManager.CreateFile(filepath); }
            StreamFullWrite(VFSManager.GetFileStream(filepath), data);
        }*/
    }
}