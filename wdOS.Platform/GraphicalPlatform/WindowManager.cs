using Cosmos.System.Graphics.Fonts;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wdOS.Platform.GraphicalPlatform
{
    internal unsafe static class WindowManager
    {
        internal static Desktop CurrentDesktop { get; private set; }
        internal static void Run()
        {
            while (true)
            {
                RenderDesktop();
                GraphicsManager.Swap();
            }
        }
        internal static void RenderWindow(Window win)
        {
        }
        internal static void RenderDesktop()
        {
            if (CurrentDesktop.OffsetRect.Left != 0 ||
                CurrentDesktop.OffsetRect.Right != 0 ||
                CurrentDesktop.OffsetRect.Top != 0 ||
                CurrentDesktop.OffsetRect.Bottom != 0)
                GraphicsManager.Clear(CurrentDesktop.BackColor);
            GraphicsManager.FillRectangle(Color.White, CurrentDesktop.OffsetRect);
            GraphicsManager.DrawText(Color.Black, "Starting up wdOS Graphical Platform", 10, 10, PCScreenFont.Default);
        }
        internal static Desktop CreateDesktop() => new();
        internal static bool SwitchDesktop(Desktop desk)
        {
            if (UserManager.CurrentUser != desk.Owner) return false;
            CurrentDesktop = desk;
            return true;
        }
    }
    internal class Desktop
    {
        internal Color BackColor = Color.Black;
        internal Rect OffsetRect = new();
        internal List<Window> OpenWindows = new();
        internal UserManager.User Owner = UserManager.CurrentUser;
    }
    internal class Window
    {
        internal string Title = "";
        internal Rect WindowRect = new();
        internal Color WindowColor = Color.Black;
        internal List<Control> Controls = new();
    }
    internal class Control
    {

    }
    internal struct Rect
    {
        internal int Top;
        internal int Left;
        internal int Bottom;
        internal int Right;
    }
}
