using HarmonyLib;
using Rdr2ThemedMenus.GUI.GameSelectionMenus;
using Rdr2ThemedMenus.GUI.GuiControls;
using Rdr2ThemedMenus.PluginCompatibility;
using Sandbox;
using Sandbox.Definitions;
using Sandbox.Engine.Networking;
using Sandbox.Game;
using Sandbox.Game.Gui;
using Sandbox.Game.Screens;
using Sandbox.Game.World;
using Sandbox.Graphics;
using Sandbox.Graphics.GUI;
using System;
using System.IO;
using VRage;
using VRage.Input;
using VRage.Utils;
using VRageMath;

namespace Rdr2ThemedMenus.GUI.MainMenuScreens
{
    internal class RDR2MainMenu : MyGuiScreenBase
    {
        private MainMenuVideoPlayer backgroundScreen;

        private ButtonPrompt buttonPrompt;

        private Type optionsMenuType = AccessTools.TypeByName("SpaceEngineers.Game.GUI.MyGuiScreenOptionsSpace");

        public RDR2MainMenu()
        : this(pauseGame: false)
        {
        }

        public RDR2MainMenu(bool pauseGame)
        {
            //MyDefinitionManager.Static.PreloadDefinitions(); //TODO add to Session Loader code
            //Plugin.Instance.Log.Info("Reloading audio system to allow injected custom audio definitions to play.");
            //MyAudio.Static.ReloadData(PluginAudioExtensions.GetSoundDataFromDefinitions(), PluginAudioExtensions.GetEffectData());
            m_closeOnEsc = false;
            m_position = Vector2.Zero;
            RecreateControls(true); 
            m_drawEvenWithoutFocus = true;
            CanBeHidden = true;
            if (!pauseGame && MyGuiScreenGamePlay.Static == null)
            {
                MyGuiSandbox.AddScreen(backgroundScreen = new MainMenuVideoPlayer(Path.Combine(Plugin.Instance.ContentDirectory, @"Videos\bayouNwa01_crop.wmv"), 800));
            }
            MyInput.Static.IsJoystickLastUsed = MySandboxGame.Config.ControllerDefaultOnStart || MyPlatformGameSettings.CONTROLLER_DEFAULT_ON_START;
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);
            
            if (MyGuiScreenGamePlay.Static == null)
            {
                CreateMainMenuControls();
            }
            else
            {
                CreatePauseMenuControls();
            }
        }

        private void CreateMainMenuControls()
        {

            Button playBtn = new Button(MyKeys.Enter, null, MyTexts.Get(MyCommonTexts.JoinGame_TabTitle_Lobbies).ToString(), _ => CustomGuiTools.AddScreenDelayed(new RDR2GameSelectionMenu(), 2000));

            Button settingsBtn = new Button(MyKeys.Z, null, MyTexts.Get(MyCommonTexts.ScreenMenuButtonOptions).ToString(), _ => OnClickOptions());

            Button characterBtn = new Button(MyKeys.LeftShift, null, MyTexts.Get(MyCommonTexts.ScreenMenuButtonInventory).ToString(), _ => OnClickInventory());

            Button pluginsBtn = new Button(MyKeys.Space, null, "Plugins", _ => PluginLoader.OpenPluginsMenu());

            Button exitBtn = new Button(MyKeys.Escape, 
                null,
                MyTexts.Get(MyCommonTexts.ScreenMenuButtonExitToWindows).ToString(), 
                onClick: _ => CustomGuiTools.AddScreenDelayed(
                    MyGuiSandbox.CreateMessageBox(
                        MyMessageBoxStyleEnum.Error, 
                        MyMessageBoxButtonsType.YES_NO, 
                        MyTexts.Get(MyCommonTexts.MessageBoxTextAreYouSureYouWantToExit), 
                        MyTexts.Get(MyCommonTexts.MessageBoxCaptionExit), 
                        null, 
                        null, 
                        null, 
                        null, 
                        OnExitToWindowsMessageBoxCallback),
                    2000));

            buttonPrompt = new ButtonPrompt(this, new MyGuiControlBase[] { playBtn, settingsBtn, characterBtn, pluginsBtn, exitBtn });
        }

        private void CreatePauseMenuControls()
        {

        }

        public override bool Draw()
        {
            
            if (MyGuiScreenGamePlay.Static == null)
            {
                DrawMainMenu();
            }
            else
            {
                DrawPauseMenu();
            }
            
            return base.Draw();
        }

        private void DrawMainMenu()
        {
            MyGuiManager.DrawSpriteBatch(Path.Combine(Plugin.Instance.ContentDirectory, @"Textures\MainMenu\landing_page_pc_lower.dds"), new Vector2(0.5f, 1f), new Vector2(1f, 0.2735f), new Color(255, 255, 255, m_transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM, true);
            MyGuiManager.DrawSpriteBatch(Path.Combine(Plugin.Instance.ContentDirectory, @"Textures\MainMenu\landing_page_pc_upper.dds"), new Vector2(0.5f, 0f), new Vector2(1f, 0.4241f), new Color(255, 255, 255, m_transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP, true);
            MyGuiManager.DrawSpriteBatch(Path.Combine(Plugin.Instance.ContentDirectory, @"Textures\MainMenu\logo_sp.dds"), new Vector2(0.5f, 0.0712f), new Vector2(0.4989f, 0.3129f), new Color(255, 255, 255, m_transitionAlpha), MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_TOP, true);

        }

        private void DrawPauseMenu()
        {

        }

        public override string GetFriendlyName()
        {
            return "RDR2MainMenu";
        }

        public override void CloseScreenNow(bool isUnloading = false)
        {
            base.CloseScreenNow(isUnloading);
            if (backgroundScreen != null)
            {
                backgroundScreen.CloseScreenNow(isUnloading);
            }
            backgroundScreen = null;
        }

        public override bool HideScreen()
        {
            if (backgroundScreen != null)
            {
                backgroundScreen.HideScreen();
            }
            
            return base.HideScreen();
        }

        public override bool UnhideScreen()
        {
            if (backgroundScreen != null)
            {
                backgroundScreen.UnhideScreen();
            }
            
            return base.UnhideScreen();
        }

        public override int GetTransitionClosingTime()
        {
            return 800;
        }

        public override int GetTransitionOpeningTime()
        {
            return 800;
        }

        private void OnExitToWindowsMessageBoxCallback(MyGuiScreenMessageBox.ResultEnum callbackReturn)
        {
            if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
            {
                MySandboxGame.Config.ControllerDefaultOnStart = MyInput.Static.IsJoystickLastUsed;
                MySandboxGame.Config.Save();
                MySandboxGame.Log.WriteLine("Application closed by user");
                MyScreenManager.CloseAllScreensNowExcept(null);
                MySandboxGame.ExitThreadSafe();
            }
        }

        private void OnClickInventory()
        {
            if (MyGameService.IsActive)
            {
                if (MyGameService.Service.GetInstallStatus(out var _))
                {
                    if (MySession.Static == null)
                    {
                        MyGuiScreenLoadInventory inventory = MyGuiSandbox.CreateScreen<MyGuiScreenLoadInventory>(Array.Empty<object>());
                        MyGuiScreenLoading screen = new MyGuiScreenLoading(inventory, null);
                        MyGuiScreenLoadInventory myGuiScreenLoadInventory = inventory;
                        myGuiScreenLoadInventory.OnLoadingAction = (Action)Delegate.Combine(myGuiScreenLoadInventory.OnLoadingAction, (Action)delegate
                        {
                            MySessionLoader.LoadInventoryScene();
                            MySandboxGame.IsUpdateReady = true;
                            inventory.Initialize(inGame: false, null);
                        });
                        CustomGuiTools.AddScreenDelayed(screen, 2000);
                    }
                    else
                    {
                        CustomGuiTools.AddScreenDelayed(MyGuiSandbox.CreateScreen<MyGuiScreenLoadInventory>(new object[2] { false, null }), 2000);
                    }
                }
                else
                {
                    CustomGuiTools.AddScreenDelayed(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, messageCaption: MyTexts.Get(MyCommonTexts.MessageBoxCaptionInfo), messageText: MyTexts.Get(MyCommonTexts.InventoryScreen_InstallInProgress)), 2000);
                }
            }
            else
            {
                CustomGuiTools.AddScreenDelayed(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, messageCaption: MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), messageText: MyTexts.Get(MyCommonTexts.SteamIsOfflinePleaseRestart)), 2000);
            }
        }

        private void OnClickOptions()
        {
            CustomGuiTools.AddScreenDelayed(MyGuiSandbox.CreateScreen(optionsMenuType, new object[] { MyPlatformGameSettings.LIMITED_MAIN_MENU }), 2000);
        }

        public override void LoadContent()
        {
            base.LoadContent();
            
        }
    }
}
