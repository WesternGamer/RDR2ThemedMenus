using HarmonyLib;
using Sandbox;
using Sandbox.Graphics;
using Sandbox.Graphics.GUI;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VRage.Input;
using VRageMath;

namespace Rdr2ThemedMenus.GUI
{
    internal static class CustomGuiTools
    {
        private static List<MyGuiScreenBase> m_screens = (List<MyGuiScreenBase>)AccessTools.Field(typeof(MyScreenManager), "m_screens").GetValue(null);

        private static MethodInfo GetIndexOfLastNonTopScreen = AccessTools.Method(typeof(MyScreenManager), "GetIndexOfLastNonTopScreen");

        private static MethodInfo NotifyScreenAdded = AccessTools.Method(typeof(MyScreenManager), "NotifyScreenAdded");

        //Same as the original implementation but allows squares
        public static void DrawRectangle(Vector2 topLeftPosition, Vector2 size, Color color)
        {
            Vector2 screenSizeFromNormalizedSize = GetScreenSizeFromNormalizedSize(size);
            screenSizeFromNormalizedSize = new Vector2((int)screenSizeFromNormalizedSize.X, (int)screenSizeFromNormalizedSize.Y);
            Vector2 screenCoordinateFromNormalizedCoordinate = MyGuiManager.GetScreenCoordinateFromNormalizedCoordinate(topLeftPosition);
            screenCoordinateFromNormalizedCoordinate = new Vector2((int)screenCoordinateFromNormalizedCoordinate.X, (int)screenCoordinateFromNormalizedCoordinate.Y);
            MyGuiManager.DrawSpriteBatch("Textures\\GUI\\Blank.dds", (int)screenCoordinateFromNormalizedCoordinate.X, (int)screenCoordinateFromNormalizedCoordinate.Y, (int)screenSizeFromNormalizedSize.X, (int)screenSizeFromNormalizedSize.Y, color);
        }

        //Same as the original implementation but allows squares
        public static Vector2I GetScreenSizeFromNormalizedSize(Vector2 normalizedSize)
        {
            return new Vector2I((int)(MySandboxGame.ScreenSize.Y * normalizedSize.X), (int)(MySandboxGame.ScreenSize.Y * normalizedSize.Y));
        }

        public static void AddScreenDelayed(MyGuiScreenBase screen, int delayMs)
        {
            Task.Factory.StartNew(() =>
            {
                MyGuiScreenBase myGuiScreenBase = ((m_screens.Count <= 0) ? null : m_screens[m_screens.Count - 1]);
                MyGuiScreenBase screenToAdd = screen;
                screen.Closed += delegate (MyGuiScreenBase sender, bool isUnloading)
                {
                    MyScreenManager.RemoveScreen(sender);
                };
                MyScreenManager.GetScreenWithFocus()?.HideTooltips();
                MyGuiScreenBase previousScreen = null;
                if (screenToAdd.CanHideOthers)
                {
                    do
                    {
                        previousScreen = MyScreenManager.GetPreviousScreen(previousScreen, (MyGuiScreenBase x) => x.CanBeHidden, (MyGuiScreenBase x) => x.CanHideOthers);
                    }
                    while (previousScreen != null && previousScreen != null && previousScreen.State == MyGuiScreenState.CLOSED);
                }
                if (previousScreen != null && previousScreen.State != MyGuiScreenState.CLOSING)
                {
                    previousScreen.HideScreen();
                }
                Thread.Sleep(delayMs);
                if (!screenToAdd.IsLoaded)
                {
                    screenToAdd.State = MyGuiScreenState.OPENING;
                    screenToAdd.LoadData();
                    screenToAdd.LoadContent();
                }
                if (screenToAdd.IsAlwaysFirst())
                {
                    m_screens.Insert(0, screenToAdd);
                }
                else if (screenToAdd.IsTopMostScreen())
                {
                    m_screens.Add(screenToAdd);
                }
                else
                {
                    m_screens.Insert((int)GetIndexOfLastNonTopScreen.Invoke(null, null), screenToAdd);
                }
                NotifyScreenAdded.Invoke(null, new object[] { screenToAdd });

                MyGuiScreenBase myGuiScreenBase4 = ((m_screens.Count <= 0) ? null : m_screens[m_screens.Count - 1]);
                if (myGuiScreenBase != myGuiScreenBase4)
                {
                    myGuiScreenBase?.OnScreenOrderChanged(myGuiScreenBase, myGuiScreenBase4);
                    myGuiScreenBase4?.OnScreenOrderChanged(myGuiScreenBase, myGuiScreenBase4);
                }
            });
        }
    }
}
