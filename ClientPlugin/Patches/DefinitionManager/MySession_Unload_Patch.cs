using HarmonyLib;
using Sandbox.Definitions;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Audio;

namespace Rdr2ThemedMenus.Patches.DefinitionManager
{
    [HarmonyPatch(typeof(MySession), nameof(MySession.Unload))]
    internal class MySession_Unload_Patch
    {
        private static void Postfix()
        {
            Plugin.Instance.Log.Info("Reloading audio system to allow injected custom audio definitions to play.");
            MyAudio.Static.ReloadData(PluginAudioExtensions.GetSoundDataFromDefinitions(), PluginAudioExtensions.GetEffectData());  
        }
    }
}
