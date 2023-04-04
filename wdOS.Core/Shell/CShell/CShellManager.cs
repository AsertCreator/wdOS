using Cosmos.Core;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using System;
using System.Collections.Generic;
using System.Drawing;
using wdOS.Core.Foundation;
using Sys = Cosmos.System;

namespace wdOS.Core.Shell.CShell
{
    public class CShellManager : ShellBase
    {
        public static CShellManager Instance;
        public static PCScreenFont Font;
        public static Canvas FSC;
        public static class DefaultTheme
        {
            public static Pen WhiteColor;
            public static Pen BlackColor;
            public static Pen Dock0;
            public static Pen Dock1;
            public static Pen Background;
        }
        public static int ScreenWidth;
        public static int ScreenHeight;
        public static int WindowCount;
        public static int WindowWidth;
        public static int WindowTitleBarHeight;
        public static bool Running;
        public static List<Window> AllWindows;
        public static string DockText;
        public static int DockYPos;
        public static ulong Framecount;
        public static ulong FPS;
        public const int CursorSize = 10;
        public const int CursorBorderSize = 2;
        public const int DockBorderSize = 2;
        public const int DockSize = 35;
        public override string ShellName => "CShell";
        public override string ShellDesc => "basically unoptimized shell";
        public override int ShellMajorVersion => SystemDatabase.BuildConstants.VersionMajor;
        public override int ShellMinorVersion => SystemDatabase.BuildConstants.VersionMinor;
        public override int ShellPatchVersion => SystemDatabase.BuildConstants.VersionPatch;
        public override void ShellBeforeRun()
        {
            try
            {
                Instance = this;

                Console.WriteLine("CShell is currently in process of being deprecated");

                SetMode(800, 600);
                Running = true;
                AllWindows = new();
                Font = PCScreenFont.Default;
                WindowTitleBarHeight = Font.Height + 10;
                DockYPos = ScreenHeight - DockSize;
                DefaultTheme.Dock0 = new(Color.Gray);
                DefaultTheme.Dock1 = new(Color.DarkGray);
                DefaultTheme.WhiteColor = new(Color.White);
                DefaultTheme.BlackColor = new(Color.Black);
                DefaultTheme.Background = new(Color.Wheat);
                KernelLogger.Log("Ready to render! Starting...");
                Kernel.SweepTrash();
            }
            catch { }
        }
        public override void ShellRun()
        {
            HandleInterface();
            KernelLogger.Log("Shutting down...");
            FSC.Disable();
            Kernel.ShutdownPC(false);
        }
        public static void SetMode(int width, int height)
        {
            try
            {
                ScreenWidth = width;
                ScreenHeight = height;
                Sys.MouseManager.ScreenWidth = (uint)ScreenWidth;
                Sys.MouseManager.ScreenHeight = (uint)ScreenHeight;
                FSC = FullScreenCanvas.GetFullScreenCanvas(new Mode(ScreenWidth, ScreenHeight, (ColorDepth)32));
                KernelLogger.Log("SystemCanvas with res of " + ScreenWidth + "x" + ScreenHeight + "x32 created!");
            }
            catch
            {
                KernelLogger.Log("Unable to create SystemCanvas!");
                Console.WriteLine("Can't set SystemCanvas to new mode!");
                while (true) { }
            }
        }
        public static void HandleInterface()
        {
            while (Running)
            {
                Framecount++;
                // Screen Content Layer
                {
                    if (WindowCount == 0) FSC.DrawFilledRectangle(DefaultTheme.Background, 0, 0, ScreenWidth, ScreenHeight);
                    else FSC.DrawFilledRectangle(DefaultTheme.BlackColor, 0, 0, ScreenWidth, WindowTitleBarHeight);
                    for (int i = 0; i < WindowCount; i++) AllWindows[i].RenderWindow();
                    FSC.DrawFilledRectangle(DefaultTheme.Dock0, 0, DockYPos, ScreenWidth, DockSize);
                    FSC.DrawString(DockText, Font, DefaultTheme.WhiteColor, 10, ScreenHeight - 15);
                    // handle mouse
                    if (Sys.MouseManager.MouseState == Sys.MouseState.None)
                        FSC.DrawFilledRectangle(DefaultTheme.WhiteColor, (int)Sys.MouseManager.X, (int)Sys.MouseManager.Y,
                            CursorSize, CursorSize);
                    else FSC.DrawFilledRectangle(DefaultTheme.BlackColor, (int)Sys.MouseManager.X, (int)Sys.MouseManager.Y,
                        CursorSize, CursorSize);
                }
                FSC.Display(); // display canvas
                // Screen Interaction Layer
                {
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
            }
        }
        public static void CreateWindow(Window win)
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
        public static void CloseWindow(int win)
        {
            if (win < WindowCount)
            {
                var wind = AllWindows[win];
                AllWindows.RemoveAt(win);
                GCImplementation.Free(wind);
                WindowCount--;
            }
        }

        public override void ShellAfterRun() { }
    }
}
