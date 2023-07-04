using Cosmos.Core;
using Cosmos.Core.Memory;
using Cosmos.System.Graphics;
using PrismAPI.Graphics;
using PrismAPI.Graphics.Fonts;
using PrismAPI.Hardware.GPU;
using System;
using System.Collections.Generic;
using wdOS.Platform.Core;
using wdOS.Platform.Shell.Widgets;
using static wdOS.Platform.Core.BuildConstants;

namespace wdOS.Platform.Shell.UI
{
    public static class WindowManager
    {
        public static Font SystemFont;
        public static Display CanvasObject;
        public static ushort ScreenWidth = 800;
        public static ushort ScreenHeight = 600;
        public static Color BackgroundColor = Color.ClassicBlue;
        public static Color GrayColor = new Color(198, 198, 198);
        public static Color NearWhiteColor = new Color(222, 222, 222);
        public static UIDesktop CurrentDesktop;
		public static UIDesktop NotifyDesktop;
		public static List<UIDesktop> UserDesktops;
		public static List<WidgetBase> RegisteredWidgets = new();
		public static ulong Framecount;
        private static bool initialized = false;

        public static void Initialize()
        {
            if (!initialized)
            {
                try
				{
					SystemFont = Font.Fallback;
					CanvasObject = Display.GetDisplay(ScreenWidth, ScreenHeight);

                    RegisteredWidgets.Add(new LoginWidget());
					RegisteredWidgets.Add(new CalculatorWidget());

					SetupNotifyDesktop();
                    CurrentDesktop = NotifyDesktop;

                    Notify("todo: login screen in ui", -1);

					initialized = true;
                }
                catch
                {
                    if (CanvasObject == null || SystemFont == null)
					{
						Console.SetWindowSize(90, 30);

						Console.Clear();
                        Console.WriteLine("wdOS was unable to initialize graphical shell. Press any key to return to Debug Shell...");
                        Console.ReadKey();

						Console.WriteLine("To try run graphical shell again, execute \"shell\" command\n");

						DebugShellManager.RunDebugShell();
					}
                    else
                    {
						ShowMessage("wdOS was unable to initialize graphical shell. Press any key to return to Debug Shell...", Color.Black, Color.Blue);
						Console.ReadKey();
						CanvasObject.IsEnabled = false;
						Console.SetWindowSize(90, 30);

						Console.Clear();
						Console.WriteLine("To try run graphical shell again, execute \"shell\" command\n");

						DebugShellManager.RunDebugShell();
					}
                }
            }
        }
        public static WidgetBase FindWidgetByID(uint id)
        {
            for (int i = 0; i < RegisteredWidgets.Count; i++)
            {
                var widget = RegisteredWidgets[i];
                if (widget.WidgetID == id)
                    return widget;
            }
            return null;
        }
        public static UserSession CreateUISession()
        {
            UserSession session = new UserSession();
            session.InteractDesktop = null;
            session.User = UserManager.CurrentUser;
            return session;
        }
        public static unsafe void DestroyWidget(ref UIWindow widget)
        {
            for (int i = 0; i < widget.Controls.Count; i++)
            {
                var control = widget.Controls[i];
                GCImplementation.Free(control.Text);
				GCImplementation.Free(control.Size);
				GCImplementation.Free(control.Location);
				GCImplementation.Free(control);
			}

			Heap.Free(widget.ClientAreaBuffer.Internal);
			GCImplementation.Free(widget.WindowTitle);
			GCImplementation.Free(widget.ClientAreaBuffer);
			GCImplementation.Free(widget.Location);
			GCImplementation.Free(widget.Size);
			GCImplementation.Free(widget.Controls);
			GCImplementation.Free(widget);

			widget = null;
        }
        public static void DestroyDesktop(ref UIDesktop desktop)
        {
            for (int i = 0; i < desktop.Widgets.Count; i++)
            {
                var widget = desktop.Widgets[i];
                DestroyWidget(ref widget);
            }

            GCImplementation.Free(desktop.KeyBuffer);
			GCImplementation.Free(desktop.Name);
			GCImplementation.Free(desktop.BackgroundColor);
			GCImplementation.Free(desktop.DesktopAuxObject);
			GCImplementation.Free(desktop.Widgets);
			GCImplementation.Free(desktop);

            desktop = null;
		}
        public static void DestroyUISession(ref UserSession session)
        {
            DestroyDesktop(ref session.InteractDesktop);
            GCImplementation.Free(session);
            session = null;
        }
        public static void SetupInteractDesktop(UserSession session)
        {
            session.InteractDesktop = new()
            {
                BackgroundColor = Color.LightGray,
                DesktopOwner = session.User,
                DesktopAuxObject = null,
                Name = "Desktop of " + session.User.UserName
            };
        }
        public static void SetupNotifyDesktop()
        {
			NotifyDesktop = new()
			{
				DesktopAuxObject = null,
				DesktopOwner = UserManager.EveryoneUser,
				BackgroundColor = Color.LightGray,
                Name = "Notification Desktop"
			};
		}
        public static void Start()
        {
            while (true)
            {
                try
                {
                    CanvasObject.Clear(BackgroundColor);

                    CurrentDesktop.Render();

                    CanvasObject.DrawFilledRectangle((int)Cosmos.System.MouseManager.X, (int)Cosmos.System.MouseManager.Y, 8, 8, 0, Color.Black);
					CanvasObject.DrawFilledRectangle((int)Cosmos.System.MouseManager.X + 1, (int)Cosmos.System.MouseManager.Y + 1, 6, 6, 0, Color.White);

					CanvasObject.Update();

                    Heap.Collect();

                    Framecount++;
                }
                catch
				{
					ShowMessage("wdOS was unable to process certain information. Press any key to try again. Press R to shutdown shell", Color.Black, Color.Blue);
					var keyinfo = Console.ReadKey();

                    if (keyinfo.KeyChar == 'r')
					{
						CanvasObject.IsEnabled = false;
						Console.SetWindowSize(90, 30);

						Console.Clear();
						Console.WriteLine("To try run graphical shell again, execute \"shell\" command\n");

						DebugShellManager.RunDebugShell();
					}
				}
            }
        }
        public static void Notify(string message, int inputtype)
        {
            if (CurrentDesktop.DesktopID == NotifyDesktop.DesktopID)
                CurrentDesktop.OpenWidget(FindWidgetByID(LoginWidget.ID), (message, inputtype));
        }
        public static void ShowMessage(string message, Color back, Color fore)
        {
            CanvasObject.Clear(back);
            CanvasObject.DrawString(5, 5, message, SystemFont, fore);
            CanvasObject.Update();
        }
    }
    public class UserSession
    {
        public User User;
        public UIDesktop InteractDesktop;
    }
    public record struct Point(int X, int Y);
}
