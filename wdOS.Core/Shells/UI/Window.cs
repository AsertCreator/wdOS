using Cosmos.System.Graphics;

namespace wdOS.Core.Shells.UI
{
    internal class Window
    {
        internal string Title = "Empty Window";
        internal Pen Back = CShell.DefaultColors.WhitePen;
        internal int SizeX;
        internal int SizeY;
        internal int SizeY2;
        internal int LocationX;
        internal int LocationY;
        internal int TextLocationX0;
        internal int TextLocationX1;
        internal void RenderWindow()
        {
            CShell.FSC.DrawFilledRectangle(Back, LocationX, CShell.WindowTitleBarHeight, SizeX, SizeY2);
            CShell.FSC.DrawString(Title, CShell.Font, CShell.DefaultColors.WhitePen, TextLocationX0, 5);
        }
        internal void RebuildLocationCache(int tile, int tilecount)
        {
            SizeX = CShell.ScreenWidth / tilecount;
            LocationX = tile * SizeX;
            TextLocationX0 = LocationX + 5;
            TextLocationX1 = LocationX + SizeX - 5 - ((CShell.Font.Width + 5) * 5) + 10;
            SizeY2 = SizeY - CShell.WindowTitleBarHeight;
            Kernel.Log($"Cache rebuilt!");
        }
    }
}
