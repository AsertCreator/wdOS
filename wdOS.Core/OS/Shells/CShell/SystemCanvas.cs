using Cosmos.HAL.Drivers;
using Cosmos.System.Graphics;
using System.Collections.Generic;
using System.Drawing;
using wdOS.Core.OS.Foundation;

namespace wdOS.Core.OS.Shells.CShell
{
    internal class SystemCanvas : Canvas
    {
        internal static List<Mode> Modes = new() { CurrentMode };
        internal static Mode CurrentMode;
        internal static VBEDriver Driver;
        internal static ushort ModeSizeX = 0;
        internal static ushort ModeSizeY = 0;
        public override List<Mode> AvailableModes => Modes;
        public override Mode DefaultGraphicMode => CurrentMode;
        public override Mode Mode { get => CurrentMode; set => Kernel.Log("Canvas mode access violation: " + value); }
        private SystemCanvas() { }
        public SystemCanvas(ushort sizex, ushort sizey)
        {
            Driver = new(sizex, sizey, 32);
            ModeSizeX = sizex;
            ModeSizeY = sizey;
            CurrentMode = new(ModeSizeX, ModeSizeY, ColorDepth.ColorDepth32);
            Driver.VBESet(sizex, sizey, 32, true);
        }
        public override void Clear(int color) => Driver.ClearVRAM((uint)color);
        public override void Clear(Color color) => Clear(color.ToArgb());
        public override void Disable() => Driver.DisableDisplay();
        public override void Display() => Driver.Swap();
        public override void DrawArray(Color[] colors, int x, int y, int width, int height)
        {
            int index = 0;
            for (int u = x; u < y + height; u++)
            {
                for (int c = y; c < x + width; c++)
                {
                    var off = (uint)(u * ModeSizeY * 4 + c * 4);
                    Driver.SetVRAM(off, colors[index].B);
                    Driver.SetVRAM(off + 1, colors[index].G);
                    Driver.SetVRAM(off + 2, colors[index].R);
                    Driver.SetVRAM(off + 3, 255);
                    index++;
                }
                index++;
            }
        }
        public override void DrawPoint(Pen pen, int x, int y)
        {
            var off = (uint)y * ModeSizeY * 4 + (uint)x * 4;
            Driver.SetVRAM(off, pen.Color.B);
            Driver.SetVRAM(off + 1, pen.Color.G);
            Driver.SetVRAM(off + 2, pen.Color.R);
            Driver.SetVRAM(off + 3, 255);
        }
        public override void DrawPoint(Pen pen, float x, float y)
        {
            var off = (uint)y * ModeSizeY * 4 + (uint)x * 4;
            Driver.SetVRAM(off, pen.Color.B);
            Driver.SetVRAM(off + 1, pen.Color.G);
            Driver.SetVRAM(off + 2, pen.Color.R);
            Driver.SetVRAM(off + 3, 255);
        }
        public override void DrawFilledRectangle(Pen pen, int x, int y, int width, int height)
        {
            var argb = pen.Color.ToArgb();
            var off = y * ModeSizeY * 4 + x * 4;
            for (int u = x; u < y + height; u++) Driver.ClearVRAM(off, width, argb);
        }
        public override void DrawSquare(Pen pen, int x, int y, int size) => DrawFilledRectangle(pen, x, y, size, size);
        public override Color GetPointColor(int x, int y) => Color.FromArgb((int)Driver.GetVRAM((uint)y * ModeSizeY * 4 + (uint)x * 4));
        public override string Name() => nameof(SystemCanvas) + " VBE";
    }
}
