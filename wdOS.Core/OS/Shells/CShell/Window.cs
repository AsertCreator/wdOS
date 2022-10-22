using Cosmos.System.Graphics;
using wdOS.Core.OS.LowLevel;

namespace wdOS.Core.OS.Shells.CShell
{
    internal class Window
    {
        internal string Title = "Empty Window";
        internal Pen Back = CShellManager.DefaultColors.WhitePen;
        internal int SizeX;
        internal int SizeY;
        internal int SizeY2;
        internal int LocationX;
        internal int LocationY;
        internal int TextLocationX0;
        internal int TextLocationX1;
        internal void RenderWindow()
        {
            CShellManager.FSC.DrawFilledRectangle(Back, LocationX, CShellManager.WindowTitleBarHeight, SizeX, SizeY2);
            CShellManager.FSC.DrawString(Title, CShellManager.Font, CShellManager.DefaultColors.WhitePen, TextLocationX0, 5);
        }
        internal void RebuildLocationCache(int tile, int tilecount)
        {
            SizeX = CShellManager.ScreenWidth / tilecount;
            LocationX = tile * SizeX;
            TextLocationX0 = LocationX + 5;
            TextLocationX1 = LocationX + SizeX - 5 - (CShellManager.Font.Width + 5) * 5 + 10;
            SizeY2 = SizeY - CShellManager.WindowTitleBarHeight;
            Kernel.Log($"Cache rebuilt!");
        }
    }
}
