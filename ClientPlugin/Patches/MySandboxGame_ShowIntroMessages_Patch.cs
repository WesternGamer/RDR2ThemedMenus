using HarmonyLib;
using Rdr2ThemedMenus.GUI.MainMenuScreens;
using Sandbox;
using Sandbox.Engine.Networking;
using Sandbox.Engine.Utils;
using Sandbox.Game.Screens;
using Sandbox.Graphics.GUI;
using VRage;

namespace Rdr2ThemedMenus.Patches
{
    [HarmonyPatch(typeof(MySandboxGame), nameof(MySandboxGame.ShowIntroMessages))]
    internal class MySandboxGame_ShowIntroMessages_Patch
    {
        private static bool Prefix()
        {
            RDR2MainMenu firstScreenOfType = MyScreenManager.GetFirstScreenOfType<RDR2MainMenu>();
            if (firstScreenOfType == null)
            {
                MyGuiSandbox.BackToMainMenu();
                firstScreenOfType = MyScreenManager.GetFirstScreenOfType<RDR2MainMenu>();
            }
            MyGuiScreenBase firstScreenOfType2 = MyScreenManager.GetFirstScreenOfType<MyGuiScreenGDPR>();
            if (MyFakes.ENABLE_GDPR_MESSAGE && (MySandboxGame.Config.GDPRConsentSent == false || !MySandboxGame.Config.GDPRConsentSent.HasValue))
            {
                if (firstScreenOfType2 == null)
                {
                    MyGuiSandbox.AddScreen(new MyGuiScreenGDPR());
                }
            }
            else
            {
                firstScreenOfType2?.CloseScreen();
            }
            firstScreenOfType2 = MyScreenManager.GetFirstScreenOfType<MyGuiScreenWelcomeScreen>();
            if (MySandboxGame.Config.WelcomScreenCurrentStatus == MyConfig.WelcomeScreenStatus.NotSeen)
            {
                if (firstScreenOfType2 == null)
                {
                    MyGuiSandbox.AddScreen(new MyGuiScreenWelcomeScreen());
                }
            }
            else
            {
                firstScreenOfType2?.CloseScreen();
            }
            if (MySandboxGame.ExperimentalOutOfMemoryCrash)
            {
                MySandboxGame.ExperimentalOutOfMemoryCrash = false;
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, messageCaption: MyTexts.Get(MyCommonTexts.MessageBoxCaptionWarning), messageText: MyTexts.Get(MyCommonTexts.ExperimentalOutOfMemoryCrashMessageBox), okButtonText: null, cancelButtonText: null, yesButtonText: null, noButtonText: null, callback: OnExperimentalOutOfMemoryCrashMessageBox));
            }
            return false;
        }

        private static void OnExperimentalOutOfMemoryCrashMessageBox(MyGuiScreenMessageBox.ResultEnum result)
        {
            if (result == MyGuiScreenMessageBox.ResultEnum.YES)
            {
                MySandboxGame.Config.ExperimentalMode = false;
                MySandboxGame.Config.Save();
            }
        }
    }
}
