using Cosmos.System.Graphics;
using System.Drawing;

namespace wdOS.Core.Shells.UI
{
    internal class Window
    {
        internal string Title = "Empty Window";
        internal Pen Back = CShell.WhitePen;
        internal int SizeX;
        internal int SizeY;
        internal int SizeY2;
        internal int LocationX;
        internal int LocationY;
        internal int TextLocationX0;
        internal int TextLocationX1;
        internal int TileID;
        internal bool IsAbleToClose = true;
        internal bool IsAbleToMinimize = true;
        internal bool IsAbleToMaximize = true;
        internal void RenderWindow()
        {
            CShell.FSC.DrawFilledRectangle(Back, LocationX, CShell.WindowTitleBarHeight, SizeX, SizeY2);
            CShell.FSC.DrawString(Title, CShell.Font, CShell.WhitePen, TextLocationX0, 5);
            CShell.FSC.DrawString("Close", CShell.Font, CShell.WhitePen, TextLocationX1, 5);
        }
        internal void RebuildLocationCache(int tile, int tilecount)
        {
            SizeX = CShell.ScreenWidth / tilecount;
            LocationX = tile * SizeX;
            TextLocationX0 = LocationX + 5;
            TextLocationX1 = LocationX + SizeX - 5 - (CShell.Font.Width + 5) * 5 + 10;
            SizeY2 = SizeY - CShell.WindowTitleBarHeight;
            Kernel.Log($"Cache rebuilt!");
        }
    }
}
