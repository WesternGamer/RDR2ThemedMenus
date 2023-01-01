using HarmonyLib;
using Sandbox.Definitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VRage.Collections;
using VRage.Data.Audio;

namespace Rdr2ThemedMenus
{
    internal static class PluginAudioExtensions
    {
        private static MethodInfo getAudioEffectDefinitions = AccessTools.Method(typeof(MyDefinitionManager), "GetAudioEffectDefinitions");

        public static ListReader<MySoundData> GetSoundDataFromDefinitions()
        {
            return (from x in MyDefinitionManager.Static.GetSoundDefinitions()
                    where x.Enabled
                    select x.SoundData).ToList();
        }

        public static ListReader<MyAudioEffect> GetEffectData()
        {
            return (from x in (ListReader<MyAudioEffectDefinition>)getAudioEffectDefinitions.Invoke(MyDefinitionManager.Static, null)
                    select x.Effect).ToList();
        }
    }
}
