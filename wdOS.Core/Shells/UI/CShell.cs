using Cosmos.System;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using System.Collections.Generic;
using System.Drawing;

namespace wdOS.Core.Shells.UI
{
    internal class CShell : Shell
    {
        internal static PCScreenFont Font = PCScreenFont.Default;
        internal static Canvas FSC;
        internal static Color WhiteColor = Color.White;
        internal static Color Background = Color.Wheat;
        internal static Pen WhitePen = new(Color.White, 1);
        internal static Pen BlackPen = new(Color.Black, 1);
        internal static int CursorSize = 10;
        internal static int CursorBorderSize = 2;
        internal static Pen Dock0 = new(Color.Gray, 1);
        internal static Pen Dock1 = new(Color.DarkGray, 1);
        internal static int Framecount;
        internal static int DockBorderSize = 2;
        internal static int DockSize = 35;
        internal static int DockYPos = 35;
        internal static int ScreenWidth;
        internal static int ScreenHeight;
        internal static int MaxWindowCount = 3;
        internal static int FocusedWindow = 0;
        internal static int WindowCount = 0;
        internal static int GarbageCleanTimer;
        internal static int WindowTitleBarHeight;
        internal static bool Running = true;
        internal static List<Window> AllWindows = new();
        internal override string Name => "CShell";
        internal override int MajorVersion => Kernel.VersionMajor;
        internal override int MinorVersion => Kernel.VersionMinor;
        internal override int PatchVersion => Kernel.VersionPatch;
        internal override void Run()
        {
            try
            {
                FSC = FullScreenCanvas.GetFullScreenCanvas();
                Kernel.Log("Canvas created!");
                FSC.Mode = new Mode(800, 600, ColorDepth.ColorDepth32);
                Kernel.Log("Mode 800x600x32 created!");
                ScreenWidth = FSC.Mode.Columns;
                ScreenHeight = FSC.Mode.Rows;
                WindowTitleBarHeight = Font.Height + 10;
                MouseManager.ScreenWidth = (uint)ScreenWidth;
                MouseManager.ScreenHeight = (uint)ScreenHeight;
                DockYPos = ScreenHeight - DockSize;
                Kernel.Log("Starting rendering");
                RenderScreen();
            }
            catch { }
            Kernel.Log("Shutting down...");
            FSC.Disable();
            Kernel.ShutdownPC(false);
        }
        internal static void RenderScreen()
        {
            while (Running)
            {
                // draw windows 
                if (WindowCount == 0) FSC.Clear();
                else FSC.DrawFilledRectangle(Dock1, 0, 0, ScreenWidth, WindowTitleBarHeight);
                for (int i = 0; i < WindowCount; i++) AllWindows[i].RenderWindow();
                // handle mouse events
                HandleMouse((int)MouseManager.X, (int)MouseManager.X, MouseManager.MouseState);
                // handle keyboard events
                HandleKeyboard();
                // display canvas
                FSC.Display();
                // clean up scheduling
                GarbageCleanTimer++;
                Framecount++;
                if (GarbageCleanTimer > 50) { CleanUp(); }
            }
        }
        internal static void HandleMouse(int x, int y, MouseState state)
        {
            // focused window
            int windowwidth = ScreenWidth / WindowCount;
            FSC.DrawFilledRectangle(
                BlackPen, FocusedWindow * windowwidth,
                windowwidth, ScreenWidth, WindowTitleBarHeight);
            FSC.DrawFilledRectangle(Dock0, 0, DockYPos, ScreenWidth, DockSize);
            // draw cursor
            FSC.DrawFilledRectangle(BlackPen, x, y, CursorSize, CursorSize);
            if (state == MouseState.None)
                FSC.DrawFilledRectangle(WhitePen, x + 1, y + 1,
                    CursorSize - CursorBorderSize, CursorSize - CursorBorderSize);
            else FSC.DrawFilledRectangle(BlackPen, x, y, CursorSize, CursorSize);
            // select focused window
            if (state == MouseState.Left)
            {
                Kernel.Log($"Mouse clicked at {x} {y}");
                if (WindowCount != 0)
                {
                    for (int i = 0; i < WindowCount; i++)
                        if (Collide(
                            i * windowwidth, 0,
                            i * windowwidth + windowwidth,
                            WindowTitleBarHeight))
                        { FocusedWindow = i; return; }
                }
            }
        }
        internal static void HandleKeyboard()
        {
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
                    case 'q': // close window
                        if (KeyboardManager.AltPressed)
                        {
                            CloseWindow(FocusedWindow);
                            break;
                        }
                        else { goto default; }
                    default:
                        break;
                }
            }
        }
        internal static void CreateWindow(Window win)
        {
            if (AllWindows.Count != MaxWindowCount)
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
                CleanUp();
                Kernel.Log($"Done!");
            }
        }
        internal static void CloseWindow(int win)
        {
            if (win < WindowCount)
            {
                AllWindows.RemoveAt(win);
                WindowCount--;
                CleanUp();
                Kernel.Log($"Closed window number #{win}");
            }
        }
        internal static void CleanUp()
        {
            Kernel.Log($"Objects collected and sweeped: {Cosmos.Core.Memory.Heap.Collect()}");
            GarbageCleanTimer = 0;
        }
        internal static bool Collide(int x1, int y1, int x2, int y2)
        {
            int mx = (int)MouseManager.X, my = (int)MouseManager.Y;
            return mx > x1 && mx < x2 && my > y1 && my < y2;
        }
        internal override void BeforeRun() { }
    }
}
