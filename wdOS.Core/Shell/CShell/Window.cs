using System.Drawing;
using wdOS.Core.Foundation;

namespace wdOS.Core.Shell.CShell
{
    internal class Window
    {
        internal string Title = "Empty Window";
        internal Color Back = CShellManager.DefaultTheme.WhiteColor;
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
            CShellManager.FSC.DrawString(Title, CShellManager.Font, CShellManager.DefaultTheme.WhiteColor, TextLocationX0, 5);
        }
        internal void RebuildLocationCache(int tile, int tilecount)
        {
            SizeX = CShellManager.WindowWidth;
            SizeY = CShellManager.ScreenHeight;
            LocationY = 0;
            LocationX = tile * SizeX;
            TextLocationX0 = LocationX + 5;
            TextLocationX1 = LocationX + SizeX - 5 - (CShellManager.Font.Width + 5) * 5 + 10;
            SizeY2 = SizeY - CShellManager.WindowTitleBarHeight;
            Kernel.Log("Cache rebuilt!");
        }
    }
}
