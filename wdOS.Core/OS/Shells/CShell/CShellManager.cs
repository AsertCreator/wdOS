using Cosmos.Core;
using Cosmos.HAL;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using System;
using System.Collections.Generic;
using System.Drawing;
using wdOS.Core.OS.Foundation;
using Sys = Cosmos.System;

namespace wdOS.Core.OS.Shells.CShell
{
    internal class CShellManager : Shell, IPackage
    {
        internal static CShellManager Instance;
        internal static PCScreenFont Font;
        internal static Canvas FSC;
        internal static class DefaultColors
        {
            internal static Pen WhitePen;
            internal static Pen BlackPen;
            internal static Pen Dock0;
            internal static Pen Dock1;
            internal static Pen Background;
        }
        internal static int ScreenWidth;
        internal static int ScreenHeight;
        internal static int WindowCount;
        internal static int WindowWidth;
        internal static int WindowTitleBarHeight;
        internal static bool Running;
        internal static List<Window> AllWindows;
        internal static string DockText;
        internal static int DockYPos;
        internal static ulong Framecount;
        internal static ulong FPS;
        internal static PIT Timer = new();
        internal const int CursorSize = 10;
        internal const int CursorBorderSize = 2;
        internal const int DockBorderSize = 2;
        internal const int DockSize = 35;
        internal override string Name => "CShell";
        internal override int MajorVersion => Kernel.BuildConstants.VersionMajor;
        internal override int MinorVersion => Kernel.BuildConstants.VersionMinor;
        internal override int PatchVersion => Kernel.BuildConstants.VersionPatch;
        string IPackage.Name => Name.ToLower();
        string IPackage.Description => "wdOS built-in graphical shell";
        string[] IPackage.Files => new string[] { "wdOS.Core.bin" };
        int IPackage.MajorVersion => MajorVersion;
        int IPackage.MinorVersion => MinorVersion;
        int IPackage.PatchVersion => PatchVersion;
        PackageDatabase.PackageType IPackage.Type => PackageDatabase.PackageType.SystemShell;
        internal override void BeforeRun()
        {
            try
            {
                Instance = this;
                SetMode(800, 600);
                Running = true;
                AllWindows = new();
                Font = PCScreenFont.Default;
                WindowTitleBarHeight = Font.Height + 10;
                DockYPos = ScreenHeight - DockSize;
                DefaultColors.Dock0 = new(Color.Gray);
                DefaultColors.Dock1 = new(Color.DarkGray);
                DefaultColors.WhitePen = new(Color.White);
                DefaultColors.BlackPen = new(Color.Black);
                DefaultColors.Background = new(Color.Wheat);
                Kernel.Log("Ready to render! Starting...");
                PIT.PITTimer timer = new(() =>
                {
                    Kernel.SweepTrash();
                    DockText =
                        $"{Kernel.StringTime}, " +
                        $"memory used: {Kernel.UsedRAM} ({Math.Round((Kernel.UsedRAM + 0.0) / (Kernel.TotalRAM + 0.0) * 100)}), " +
                        $"total memory: {Kernel.TotalRAM}, " +
                        $"fps: {FPS}, framecount: {Framecount}";
                    FPS = Framecount;
                    Framecount = 0;
                }, 1000000000, true);
                Timer.RegisterTimer(timer);
                Kernel.SweepTrash();
            }
            catch { }
        }
        internal override void Run()
        {
            HandleInterface();
            Kernel.Log("Shutting down...");
            FSC.Disable();
            Kernel.ShutdownPC(false);
        }
        internal static void SetMode(int width, int height)
        {
            try
            {
                ScreenWidth = width;
                ScreenHeight = height;
                Sys.MouseManager.ScreenWidth = (uint)ScreenWidth;
                Sys.MouseManager.ScreenHeight = (uint)ScreenHeight;
                FSC = FullScreenCanvas.GetFullScreenCanvas(new Mode(ScreenWidth, ScreenHeight, (ColorDepth)32));
                Kernel.Log($"SystemCanvas with res of {ScreenWidth}x{ScreenHeight}x32 created!");
            }
            catch
            {
                Kernel.Log($"Unable to create SystemCanvas!");
                Console.Clear();
                Console.WriteLine("Can't set SystemCanvas to new mode!");
                while (true) { }
            }
        }
        internal static void HandleInterface()
        {
            while (Running)
            {
                Framecount++;
                // Screen Content Layer
                {
                    if (WindowCount == 0) FSC.DrawFilledRectangle(DefaultColors.Background, 0, 0, ScreenWidth, ScreenHeight);
                    else FSC.DrawFilledRectangle(DefaultColors.BlackPen, 0, 0, ScreenWidth, WindowTitleBarHeight);
                    for (int i = 0; i < WindowCount; i++) AllWindows[i].RenderWindow();
                    FSC.DrawFilledRectangle(DefaultColors.Dock0, 0, DockYPos, ScreenWidth, DockSize);
                    FSC.DrawString(DockText, Font, DefaultColors.WhitePen, 10, ScreenHeight - 15);
                }
                // Screen Interaction Layer
                {
                    // handle mouse
                    if (Sys.MouseManager.MouseState == Sys.MouseState.None)
                        FSC.DrawFilledRectangle(DefaultColors.WhitePen, (int)Sys.MouseManager.X, (int)Sys.MouseManager.Y,
                            CursorSize, CursorSize);
                    else FSC.DrawFilledRectangle(DefaultColors.BlackPen, (int)Sys.MouseManager.X, (int)Sys.MouseManager.Y,
                        CursorSize, CursorSize);
                    // handle keyboard
                    if (Sys.KeyboardManager.KeyAvailable)
                    {
                        var key = Sys.KeyboardManager.ReadKey();
                        switch (key.KeyChar)
                        {
                            case 'e': // create window
                                Random random = new();
                                CreateWindow(new Window()
                                {
                                    Title = $"Window {random.Next()}",
                                    Back = new Pen(Color.FromArgb(random.Next(255), random.Next(255), random.Next(255)))
                                });
                                break;
                            case 'q':
                                Instance.IsRunning = false;
                                break;
                        }
                    }
                }
                FSC.Display(); // display canvas
            }
        }
        internal static void CreateWindow(Window win)
        {
            if (WindowCount < 2)
            {
                AllWindows.Add(win);
                WindowCount++;
                WindowWidth = ScreenWidth / WindowCount;
                for (int i = 0; i < WindowCount; i++)
                    AllWindows[i].RebuildLocationCache(i, WindowCount);
                Kernel.SweepTrash();
            }
        }
        internal static void CloseWindow(int win)
        {
            if (win < WindowCount)
            {
                var wind = AllWindows[win];
                AllWindows.RemoveAt(win);
                GCImplementation.Free(wind);
                WindowCount--;
            }
        }
    }
}
