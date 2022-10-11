using Cosmos.Core;
using Cosmos.System.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace wdOS.Core.Shells.CShell
{
    internal class SystemCanvas : Canvas
    {
        internal static List<Mode> Modes = new() { CurrentMode };
        internal static Mode CurrentMode = new Mode(ModeSizeX, ModeSizeY, ColorDepth.ColorDepth32);
        internal static Color[] Buffer = new Color[ModeSize];
        internal static uint Nothing;
        internal static int ModeSize = ModeSizeX * ModeSizeY;
        internal static int ModeSizeX = 800;
        internal static int ModeSizeY = 600;
        public override List<Mode> AvailableModes => Modes;
        public override Mode DefaultGraphicMode => Modes[0];
        public override Mode Mode { get => CurrentMode; set => Nothing = 0; }
        public SystemCanvas()
        {
            if (!VBE.IsAvailable())
            {
                Console.WriteLine("No compatible display adapters are found!");
                return;
            }
        }
        public override void Clear(int color)
        {
            Color clearColor = Color.FromArgb(color);
            for (int i = 0; i < ModeSize; i++) Buffer[i] = clearColor;
        }
        public override void Disable() { /* nope */ }
        public override void Display()
        {

        }
        public override void DrawArray(Color[] colors, int x, int y, int width, int height)
        {
            throw new NotImplementedException();
        }
        public override void DrawPoint(Pen pen, int x, int y)
        {
            throw new NotImplementedException();
        }
        public override void DrawPoint(Pen pen, float x, float y)
        {
            throw new NotImplementedException();
        }
        public override Color GetPointColor(int x, int y)
        {
            throw new NotImplementedException();
        }
        public override string Name()
        {
            throw new NotImplementedException();
        }
    }
}
