using Cosmos.Core;
using Cosmos.HAL;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using System;
using System.Collections.Generic;
using System.Drawing;
using wdOS.Core.Foundation;
using wdOS.Core.Shell;
using Sys = Cosmos.System;

namespace wdOS.Core.Shell.CShell
{
    internal class CShellManager : ShellBase
    {
        internal static CShellManager Instance;
        internal static PCScreenFont Font;
        internal static Canvas FSC;
        internal static class DefaultTheme
        {
            internal static Color WhiteColor;
            internal static Color BlackColor;
            internal static Color Dock0;
            internal static Color Dock1;
            internal static Color Background;
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
        internal bool RunInTerminalMode;
        internal const int CursorSize = 10;
        internal const int CursorBorderSize = 2;
        internal const int DockBorderSize = 2;
        internal const int DockSize = 35;
        internal override string Name => "CShell";
        internal override int MajorVersion => Kernel.BuildConstants.VersionMajor;
        internal override int MinorVersion => Kernel.BuildConstants.VersionMinor;
        internal override int PatchVersion => Kernel.BuildConstants.VersionPatch;
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
                DefaultTheme.Dock0 = Color.Gray;
                DefaultTheme.Dock1 = Color.DarkGray;
                DefaultTheme.WhiteColor = Color.White;
                DefaultTheme.BlackColor = Color.Black;
                DefaultTheme.Background = Color.Wheat;
                Kernel.Log("Ready to render! Starting...");
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
                Kernel.Log("SystemCanvas with res of " + ScreenWidth + "x" + ScreenHeight + "x32 created!");
            }
            catch
            {
                Kernel.Log("Unable to create SystemCanvas!");
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
                                    Back = Color.FromArgb(random.Next(255), random.Next(255), random.Next(255))
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
