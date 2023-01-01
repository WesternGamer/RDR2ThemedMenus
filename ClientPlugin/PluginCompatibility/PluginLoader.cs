using HarmonyLib;
using Rdr2ThemedMenus.GUI;
using Sandbox.Graphics.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VRage;

namespace Rdr2ThemedMenus.PluginCompatibility
{
    internal static class PluginLoader
    {
        private static Type MyGuiScreenPluginConfigtype = AccessTools.TypeByName("avaness.PluginLoader.GUI.MyGuiScreenPluginConfig");

        private static bool HasError = (bool)AccessTools.Property(AccessTools.TypeByName("avaness.PluginLoader.PluginList"), "HasError").GetValue(AccessTools.Property(AccessTools.TypeByName("avaness.PluginLoader.Main"), "List").GetValue(AccessTools.Field(AccessTools.TypeByName("avaness.PluginLoader.Main"), "Instance").GetValue(null)));

        public static void OpenPluginsMenu()
        {
            if (HasError)
            {
                MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(buttonType: MyMessageBoxButtonsType.OK, messageText: new StringBuilder("An error occurred while downloading the plugin list.\nPlease send your game log to the developers of Plugin Loader."), messageCaption: MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), callback: (x) => CustomGuiTools.AddScreenDelayed(Activator.CreateInstance(MyGuiScreenPluginConfigtype, true) as MyGuiScreenBase, 2000)));
            }
            else
            {
                CustomGuiTools.AddScreenDelayed(Activator.CreateInstance(MyGuiScreenPluginConfigtype, true) as MyGuiScreenBase, 2000);
            }
        }
    }
}
