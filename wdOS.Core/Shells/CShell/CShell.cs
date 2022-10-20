using Cosmos.HAL;
using Cosmos.System;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using System;
using System.Collections.Generic;
using System.Drawing;
using wdOS.Core.Shells.Users;

namespace wdOS.Core.Shells.CShell
{
    internal class CShell : Shell
    {
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
        internal static int Framecount;
        internal static int ScreenWidth;
        internal static int ScreenHeight;
        internal static int WindowCount;
        internal static int WindowWidth;
        internal static int WindowTitleBarHeight;
        internal static bool Running;
        internal static List<Window> AllWindows;
        internal static string DockText;
        internal static int DockYPos;
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
                SetMode(800, 600);
                UpdateUICache();
                Kernel.Log("Ready to render! Starting...");
                PIT.PITTimer timer = new(() => { }, 1000000000, true);
                Cosmos.HAL.Global.PIT.RegisterTimer(timer);
            }
            catch { }
        }
        internal override void Run()
        {
            try { RenderScreen(); }
            catch { }
            Kernel.Log("Shutting down...");
            FSC.Disable();
            Kernel.ShutdownPC(false);
        }
        internal void UpdateUICache()
        {
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
        }
        internal void SetMode(int width, int height)
        {
            ScreenWidth = width;
            ScreenHeight = height;
            MouseManager.ScreenWidth = (uint)ScreenWidth;
            MouseManager.ScreenHeight = (uint)ScreenHeight;
            FSC = FullScreenCanvas.GetFullScreenCanvas(new Mode(ScreenWidth, ScreenHeight, ColorDepth.ColorDepth32));
            Kernel.Log($"Mode {ScreenWidth}x{ScreenHeight}x32 created!");
        }
        internal void RenderScreen()
        {
            while (Running)
            {
                HandleSCL(); // Screen Content Layer
                HandleSIL(); // Screen Interaction Layer
                FSC.Display(); // display canvas
                if (Framecount % 10 == 0) { CleanUp(); DockText = 
                        $"{Kernel.StringTime}, " +
                        $"memory used: {Kernel.UsedRAM} ({Math.Round((Kernel.UsedRAM + 0.0) / (Kernel.TotalRAM + 0.0) * 100)}), " +
                        $"total memory: {Kernel.TotalRAM}"; }
                Framecount++; // clean up scheduling
            }
        }
        internal void HandleSCL()
        {
            if (WindowCount == 0) FSC.DrawFilledRectangle(DefaultColors.Background, 0, 0, ScreenWidth, ScreenHeight);
            else FSC.DrawFilledRectangle(DefaultColors.BlackPen, 0, 0, ScreenWidth, WindowTitleBarHeight);
            for (int i = 0; i < WindowCount; i++) AllWindows[i].RenderWindow();
            FSC.DrawFilledRectangle(DefaultColors.Dock0, 0, DockYPos, ScreenWidth, DockSize);
            FSC.DrawString(DockText, Font, DefaultColors.WhitePen, 10, ScreenHeight - 15);
        }
        internal void HandleSIL()
        {
            // handle mouse
            HandleSILMouse((int)MouseManager.X, (int)MouseManager.Y, MouseManager.MouseState);
            if (KeyboardManager.KeyAvailable)
            {
                var key = KeyboardManager.ReadKey();
                Kernel.Log($"Keyboard clicked \'{key.KeyChar}\'");
                switch (key.KeyChar)
                {
                    case 'e': // create window
                        System.Random random = new();
                        CreateWindow(new Window()
                        {
                            Title = $"Window {random.Next()}",
                            Back = new Pen(Color.FromArgb(random.Next(255), random.Next(255), random.Next(255)))
                        });
                        break;
                    case 'q':
                        IsRunning = false;
                        break;
                }
            }
        }
        internal void HandleSILMouse(int x, int y, MouseState state)
        {
            FSC.DrawFilledRectangle(DefaultColors.BlackPen, x, y, CursorSize, CursorSize);
            if (state == MouseState.None)
                FSC.DrawFilledRectangle(DefaultColors.WhitePen, x + 1, y + 1,
                    CursorSize - CursorBorderSize, CursorSize - CursorBorderSize);
            else FSC.DrawFilledRectangle(DefaultColors.BlackPen, x, y, CursorSize, CursorSize);
            // select focused window
            if (state == MouseState.Left)
            {
                Kernel.Log($"Mouse clicked at {x} {y}");
            }
        }
        internal void CreateWindow(Window win)
        {
            if (WindowCount < 2)
            {
                Kernel.Log($"Creating new window...");
                AllWindows.Add(win);
                WindowCount++;
                for (int i = 0; i < WindowCount; i++)
                {
                    Window winw = AllWindows[i];
                    winw.SizeX = ScreenWidth / WindowCount;
                    winw.SizeY = ScreenHeight;
                    winw.LocationX = 0;
                    winw.LocationY = 0;
                    winw.RebuildLocationCache(i, WindowCount);
                }
                WindowWidth = ScreenWidth / WindowCount;
                CleanUp();
                Kernel.Log($"Done!");
            }
        }
        internal void CloseWindow(int win)
        {
            if (win < WindowCount)
            {
                AllWindows.RemoveAt(win);
                WindowCount--;
                Kernel.Log($"Closed window number #{win}");
            }
        }
        internal void CleanUp() => Kernel.Log($"Objects collected and sweeped: {Cosmos.Core.Memory.Heap.Collect()}");
        internal bool Collide(int x1, int y1, int x2, int y2)
        {
            int mx = (int)MouseManager.X, my = (int)MouseManager.Y;
            return mx > x1 && mx < x2 && my > y1 && my < y2;
        }
    }
}
