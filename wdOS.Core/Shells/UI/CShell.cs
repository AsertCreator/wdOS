using Cosmos.System;
using Cosmos.System.Graphics;
using Cosmos.System.Graphics.Fonts;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Serialization;

namespace wdOS.Core.Shells.UI
{
    internal class CShell : Shell
    {
        internal static PCScreenFont Font;
        internal static Canvas FSC;
        internal static class Pens
        {
            internal static Color WhiteColor;
            internal static Color Background;
            internal static Pen WhitePen;
            internal static Pen BlackPen;
            internal static Pen Dock0;
            internal static Pen Dock1;
        }
        internal static int Framecount;
        internal static int ScreenWidth;
        internal static int ScreenHeight;
        internal static int WindowCount;
        internal static int WindowWidth;
        internal static int WindowTitleBarHeight;
        internal static bool Running;
        internal static List<Window> AllWindows;
        internal static int DockYPos;
        internal const int CursorSize = 10;
        internal const int CursorBorderSize = 2;
        internal const int DockBorderSize = 2;
        internal const int DockSize = 35;
        internal const int MaxWindowCount = 3;
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
                Kernel.Log("Starting rendering");
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
        internal static void UpdateUICache()
        {
            Running = true;
            AllWindows = new();
            Font = PCScreenFont.Default;
            WindowTitleBarHeight = Font.Height + 10;
            DockYPos = ScreenHeight - DockSize;
            Pens.Dock0 = new(Color.Gray, 1);
            Pens.Dock1 = new(Color.DarkGray, 1);
            Pens.WhitePen = new(Color.White, 1);
            Pens.BlackPen = new(Color.Black, 1);
            Pens.WhiteColor = Color.White;
            Pens.Background = Color.Wheat;
        }
        internal static void SetMode(int width, int height)
        {
            FSC = FullScreenCanvas.GetFullScreenCanvas(
                new Mode(ScreenWidth, ScreenHeight, ColorDepth.ColorDepth32));
            Kernel.Log("Canvas created!");
            Kernel.Log($"Mode {ScreenWidth}x{ScreenHeight}x32 created!");
            ScreenWidth = FSC.Mode.Columns;
            ScreenHeight = FSC.Mode.Rows;
            MouseManager.ScreenWidth = (uint)ScreenWidth;
            MouseManager.ScreenHeight = (uint)ScreenHeight;
        }
        internal static void RenderScreen()
        {
            while (Running)
            {
                HandleSCL($"Time: {Kernel.GetStrTime()}, Framecount: {Framecount}"); // Screen Content Layer
                HandleSIL((int)MouseManager.X, (int)MouseManager.X, MouseManager.MouseState); // Screen Interaction Layer
                FSC.Display(); // display canvas
                if (Framecount % 150 == 0) { CleanUp(); }
                Framecount++; // clean up scheduling
            }
        }
        internal static void HandleSCL(string waterwark)
        {
            if (WindowCount == 0) FSC.Clear();
            else FSC.DrawFilledRectangle(Pens.Dock1, 0, 0, ScreenWidth, WindowTitleBarHeight);
            for (int i = 0; i < WindowCount; i++) AllWindows[i].RenderWindow();
            FSC.DrawString(waterwark, Font, Pens.WhitePen, WindowWidth - Font.Height * 2, 0);
            FSC.DrawFilledRectangle(Pens.Dock0, 0, DockYPos, ScreenWidth, DockSize);
        }
        internal static void HandleSIL(int x, int y, MouseState state)
        {
            // handle mouse
            HandleSILMouse(x, y, state);
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
                }
            }
        }
        internal static void HandleSILMouse(int x, int y, MouseState state)
        {
            FSC.DrawFilledRectangle(Pens.BlackPen, x, y, CursorSize, CursorSize);
            if (state == MouseState.None)
                FSC.DrawFilledRectangle(Pens.WhitePen, x + 1, y + 1,
                    CursorSize - CursorBorderSize, CursorSize - CursorBorderSize);
            else FSC.DrawFilledRectangle(Pens.BlackPen, x, y, CursorSize, CursorSize);
            // select focused window
            if (state == MouseState.Left)
            {
                Kernel.Log($"Mouse clicked at {x} {y}");
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
                WindowWidth = ScreenWidth / WindowCount;
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
                Kernel.Log($"Closed window number #{win}");
            }
        }
        internal static void CleanUp() => Kernel.Log($"Objects collected and sweeped: {Cosmos.Core.Memory.Heap.Collect()}");
        internal static bool Collide(int x1, int y1, int x2, int y2)
        {
            int mx = (int)MouseManager.X, my = (int)MouseManager.Y;
            return mx > x1 && mx < x2 && my > y1 && my < y2;
        }
    }
}
