using System.Drawing;
using Cosmos.System.Graphics;
using wdOS.Core.Foundation;

namespace wdOS.Core.Shell.CShell
{
    public class Window
    {
        public string Title = "Empty Window";
        public Pen Back = CShellManager.DefaultTheme.WhiteColor;
        public int SizeX;
        public int SizeY;
        public int SizeY2;
        public int LocationX;
        public int LocationY;
        public int TextLocationX0;
        public int TextLocationX1;
        public void RenderWindow()
        {
            CShellManager.FSC.DrawFilledRectangle(Back, LocationX, CShellManager.WindowTitleBarHeight, SizeX, SizeY2);
            CShellManager.FSC.DrawString(Title, CShellManager.Font, CShellManager.DefaultTheme.WhiteColor, TextLocationX0, 5);
        }
        public void RebuildLocationCache(int tile, int tilecount)
        {
            SizeX = CShellManager.WindowWidth / tilecount;
            SizeY = CShellManager.ScreenHeight;
            LocationY = 0;
            LocationX = tile * SizeX;
            TextLocationX0 = LocationX + 5;
            TextLocationX1 = LocationX + SizeX - 5 - (CShellManager.Font.Width + 5) * 5 + 10;
            SizeY2 = SizeY - CShellManager.WindowTitleBarHeight;
            KernelLogger.Log("Cache rebuilt!");
        }
    }
}
