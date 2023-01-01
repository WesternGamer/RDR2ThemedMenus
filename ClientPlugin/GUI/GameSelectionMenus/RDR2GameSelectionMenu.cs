using ParallelTasks;
using Rdr2ThemedMenus.GUI.GuiControls;
using Sandbox;
using Sandbox.Engine.Networking;
using Sandbox.Game;
using Sandbox.Game.Gui;
using Sandbox.Game.Localization;
using Sandbox.Game.Screens;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using System;
using System.IO;
using System.Linq;
using System.Text;
using VRage;
using VRage.Game;
using VRage.GameServices;
using VRage.Utils;
using VRageMath;
using VRageRender;

namespace Rdr2ThemedMenus.GUI.GameSelectionMenus
{
    internal class RDR2GameSelectionMenu : RDR2ButtonsMenuBase
    {
        private ImageButton continueButton = null;

        private ImageButton loadButton = null;

        private ImageButton newButton = null;

        private ImageButton joinButton = null;

        bool m_parallelLoadIsRunning;

        public RDR2GameSelectionMenu()
        {
            m_drawEvenWithoutFocus = true;
            CanBeHidden = true;
        }

        public override void RecreateControls(bool constructor)
        {
            base.RecreateControls(constructor);

            MyObjectBuilder_LastSession lastSession = MyLocalCache.GetLastSession();
            if (lastSession != null && (lastSession.Path == null || MyPlatformGameSettings.GAME_SAVES_TO_CLOUD || Directory.Exists(lastSession.Path)) && (!lastSession.IsLobby || MyGameService.LobbyDiscovery.ContinueToLobbySupported))
            {
                continueButton = new ImageButton(new Vector2(0.05f, 0.2f),
                new Vector2(0.28125f, 0.6f),
                MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                MyTexts.Get(MyCommonTexts.ScreenMenuButtonContinueGame).ToString(),
                _ => ContinueGame(),
                GetThumbnail(lastSession) ?? Path.Combine(Plugin.Instance.ContentDirectory, @"Textures\RDR2GameSelectionMenu\saves_continuegame.dds"));
                Controls.Add(continueButton);
            }
            

            loadButton = new ImageButton(new Vector2(0.359375f, 0.2f),
                new Vector2(0.28125f, 0.6f),
                MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                MyTexts.Get(MyCommonTexts.ScreenMenuButtonLoadGame).ToString(),
                _ => MyGuiSandbox.AddScreen(new MyGuiScreenLoadSandbox()),
                Path.Combine(Plugin.Instance.ContentDirectory, @"Textures\RDR2GameSelectionMenu\saves_loadgame.dds"));
            Controls.Add(loadButton);

            newButton = new ImageButton(new Vector2(0.66875f, 0.2f),
                new Vector2(0.28125f, 0.275f),
                MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                MyTexts.Get(MyCommonTexts.ScreenMenuButtonCampaign).ToString(),
                _ => MyGuiSandbox.AddScreen(new MyGuiScreenNewGame()),
                Path.Combine(Plugin.Instance.ContentDirectory, @"Textures\RDR2GameSelectionMenu\saves_newgame.dds"));
            Controls.Add(newButton);

            joinButton = new ImageButton(new Vector2(0.66875f, 0.5259f),
                new Vector2(0.28125f, 0.275f),
                MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
                MyTexts.Get(MyCommonTexts.ScreenMenuButtonJoinGame).ToString(),
                _ => OnJoinGameClick(),
                Path.Combine(Plugin.Instance.ContentDirectory, @"Textures\RDR2GameSelectionMenu\saves_joingame.dds"));
            Controls.Add(joinButton);
        }

        public override string GetFriendlyName()
        {
            return "RDR2GameSelectionMenu";
        }

        private void OnJoinGameClick()
        {
            if (!MyGameService.IsOnline)
            {
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, messageCaption: MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), messageText: new StringBuilder().AppendFormat(MyTexts.GetString(MyGameService.IsActive ? MyCommonTexts.SteamIsOfflinePleaseRestart : MyCommonTexts.ErrorJoinSessionNoUser), MySession.GameServiceName)));
                return;
            }

            MyGameService.Service.RequestPermissions(Permissions.Multiplayer, attemptResolution: true, delegate (PermissionResult granted)
            {
                if (granted == PermissionResult.Error)
                {
                    MyGuiSandbox.Show(MyCommonTexts.XBoxPermission_MultiplayerError, default(MyStringId), MyMessageBoxStyleEnum.Info);
                    return;
                }

                MyGameService.Service.RequestPermissions(Permissions.UGC, attemptResolution: true, delegate (PermissionResult ugcGranted)
                {
                    if (ugcGranted == PermissionResult.Error)
                    {
                        MySandboxGame.Static.Invoke(delegate
                        {
                            MyGuiSandbox.Show(MyCommonTexts.XBoxPermission_MultiplayerError, default(MyStringId), MyMessageBoxStyleEnum.Info);
                        }, "New Game screen");
                        return;
                    }

                    MyGameService.Service.RequestPermissions(Permissions.CrossMultiplayer, attemptResolution: true, delegate (PermissionResult crossGranted)
                    {
                        MyGuiScreenJoinGame myGuiScreenJoinGame = new MyGuiScreenJoinGame(crossGranted == PermissionResult.Granted);
                        //myGuiScreenJoinGame.Closed += joinGameScreen_Closed;
                        MyGuiSandbox.AddScreen(myGuiScreenJoinGame);
                    });
                });
            });
        }

        private void joinGameScreen_Closed(MyGuiScreenBase source, bool isUnloading)
        {
            if (source.Cancelled)
            {
                base.State = MyGuiScreenState.OPENING;
                source.Closed -= joinGameScreen_Closed;
            }
        }

        private string GetThumbnail(MyObjectBuilder_LastSession session)
        {
            string text = session?.Path;
            if (text == null)
            {
                return null;
            }
            if (Directory.Exists(text + MyGuiScreenLoadSandbox.CONST_BACKUP))
            {
                string[] directories = Directory.GetDirectories(text + MyGuiScreenLoadSandbox.CONST_BACKUP);
                if (directories.Any())
                {
                    string text2 = Path.Combine(directories.Last(), MyTextConstants.SESSION_THUMB_NAME_AND_EXTENSION);
                    if (File.Exists(text2) && new FileInfo(text2).Length > 0)
                    {
                        return text2;
                    }
                }
            }
            string text3 = Path.Combine(text, MyTextConstants.SESSION_THUMB_NAME_AND_EXTENSION);
            if (File.Exists(text3) && new FileInfo(text3).Length > 0)
            {
                return text3;
            }
            if (MyPlatformGameSettings.GAME_SAVES_TO_CLOUD)
            {
                byte[] array = MyGameService.LoadFromCloud(MyCloudHelper.Combine(MyCloudHelper.LocalToCloudWorldPath(text + "/"), MyTextConstants.SESSION_THUMB_NAME_AND_EXTENSION));
                if (array != null)
                {
                    try
                    {
                        string text4 = Path.Combine(text, MyTextConstants.SESSION_THUMB_NAME_AND_EXTENSION);
                        Directory.CreateDirectory(text);
                        File.WriteAllBytes(text4, array);
                        MyRenderProxy.UnloadTexture(text4);
                        return text4;
                    }
                    catch
                    {
                    }
                }
            }
            return null;
        }

        private void ContinueGame()
        {
            MyObjectBuilder_LastSession mySession = MyLocalCache.GetLastSession();
            if (mySession == null)
            {
                return;
            }
            if (mySession.IsOnline)
            {
                if (mySession.IsLobby)
                {
                    MyJoinGameHelper.JoinGame(ulong.Parse(mySession.ServerIP));
                    return;
                }
                MyGameService.Service.RequestPermissions(Permissions.Multiplayer, attemptResolution: true, delegate (PermissionResult granted)
                {
                    switch (granted)
                    {
                        case PermissionResult.Granted:
                            MyGameService.Service.RequestPermissions(Permissions.CrossMultiplayer, attemptResolution: true, delegate (PermissionResult crossGranted)
                            {
                                switch (crossGranted)
                                {
                                    case PermissionResult.Granted:
                                        MyGameService.Service.RequestPermissions(Permissions.UGC, attemptResolution: true, delegate (PermissionResult ugcGranted)
                                        {
                                            switch (ugcGranted)
                                            {
                                                case PermissionResult.Granted:
                                                    JoinServer(mySession);
                                                    break;
                                                case PermissionResult.Error:
                                                    MySandboxGame.Static.Invoke(delegate
                                                    {
                                                        MyGuiSandbox.Show(MyCommonTexts.XBoxPermission_MultiplayerError, default(MyStringId), MyMessageBoxStyleEnum.Info);
                                                    }, "New Game screen");
                                                    break;
                                            }
                                        });
                                        break;
                                    case PermissionResult.Error:
                                        MySandboxGame.Static.Invoke(delegate
                                        {
                                            MyGuiSandbox.Show(MyCommonTexts.XBoxPermission_MultiplayerError, default(MyStringId), MyMessageBoxStyleEnum.Info);
                                        }, "New Game screen");
                                        break;
                                }
                            });
                            break;
                        case PermissionResult.Error:
                            MySandboxGame.Static.Invoke(delegate
                            {
                                MyGuiSandbox.Show(MyCommonTexts.XBoxPermission_MultiplayerError, default(MyStringId), MyMessageBoxStyleEnum.Info);
                            }, "New Game screen");
                            break;
                    }
                });
            }
            else if (!m_parallelLoadIsRunning)
            {
                m_parallelLoadIsRunning = true;
                MyGuiScreenProgress progressScreen = new MyGuiScreenProgress(MyTexts.Get(MySpaceTexts.ProgressScreen_LoadingWorld));
                MyScreenManager.AddScreen(progressScreen);
                Parallel.StartBackground(delegate
                {
                    MySessionLoader.LoadLastSession();
                }, delegate
                {
                    progressScreen.CloseScreen();
                    m_parallelLoadIsRunning = false;
                });
            }
        }

        private void JoinServer(MyObjectBuilder_LastSession mySession)
        {
            try
            {
                MyGuiScreenProgress prog = new MyGuiScreenProgress(MyTexts.Get(MyCommonTexts.DialogTextCheckServerStatus));
                MyGuiSandbox.AddScreen(prog);
                MyGameService.OnPingServerResponded += OnPingSuccess;
                MyGameService.OnPingServerFailedToRespond += OnPingFailure;
                MyGameService.PingServer(mySession.GetConnectionString());
                void OnPingFailure(object sender, object data)
                {
                    MyGuiSandbox.RemoveScreen(prog);
                    MySandboxGame.Static.ServerFailedToRespond(sender, data);
                    MyGameService.OnPingServerResponded -= OnPingSuccess;
                    MyGameService.OnPingServerFailedToRespond -= OnPingFailure;
                }
                void OnPingSuccess(object sender, MyGameServerItem item)
                {
                    MyGuiSandbox.RemoveScreen(prog);
                    MySandboxGame.Static.ServerResponded(sender, item);
                    MyGameService.OnPingServerResponded -= OnPingSuccess;
                    MyGameService.OnPingServerFailedToRespond -= OnPingFailure;
                }
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLine(ex);
                MyGuiSandbox.Show(MyTexts.Get(MyCommonTexts.MultiplayerJoinIPError), MyCommonTexts.MessageBoxCaptionError);
            }
        }

    }
}
