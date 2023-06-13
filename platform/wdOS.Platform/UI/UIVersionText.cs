using PrismAPI.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static wdOS.Platform.WindowManager;

namespace wdOS.Platform.UI
{
	public sealed class UIVersionText
	{
		public string[] Texts;
		public int BottomMargin;
		public int RightMargin;
		private bool initialized = false;
		public void Render()
		{
			if (!initialized)
			{
				Texts = new string[2];
				Texts[0] = "wdOS Platform, version: " +
					PlatformManager.BuildConstants.VersionMajor + "." +
					PlatformManager.BuildConstants.VersionMinor + "." +
					PlatformManager.BuildConstants.VersionPatch + ", stage: " +
					PlatformManager.BuildConstants.GetDevStageName(
						PlatformManager.BuildConstants.CurrentOSType);
				Texts[1] = "Built on Cosmos Project. Uses PrismAPI.";
				initialized = true;
			}
			for (int i = 0; i < Texts.Length; i++)
			{
				string text = Texts[i];
				ushort x = SystemFont.MeasureString(text);
				CanvasObject.DrawString(ScreenWidth - x - 5 - RightMargin, ScreenHeight - (Texts.Length - i) * SystemFont.Size - 5 - ScreenHeight, text, SystemFont, Color.White);
			}
		}
	}
}
