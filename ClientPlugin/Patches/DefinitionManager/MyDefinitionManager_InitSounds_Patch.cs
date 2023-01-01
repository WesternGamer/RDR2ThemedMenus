using HarmonyLib;
using Sandbox.Definitions;
using System.Collections.Generic;
using System.Reflection;
using VRage.Game;
using VRage.ObjectBuilders;
using System.IO;
using VRage.Audio;

namespace Rdr2ThemedMenus.Patches.DefinitionManager
{
    //[HarmonyPatch(typeof(MyDefinitionManager), "InitSounds")]
    internal class MyDefinitionManager_InitSounds_Patch
    {
        private static MethodInfo initdefinition = AccessTools.Method(typeof(MyDefinitionManager), "InitDefinition").MakeGenericMethod(typeof(MyAudioDefinition));

        private static bool Prefix(MyModContext context, Dictionary<MyDefinitionId, MyAudioDefinition> output, MyObjectBuilder_AudioDefinition[] classes, bool failOnDebug)
        {
            MyObjectBuilder_Definitions definitions;

            MyObjectBuilderSerializer.DeserializeXML(Path.Combine(Plugin.Instance.ContentDirectory, @"Data\GuiSounds.xml"), out definitions);

            Plugin.Instance.Log.Info($"Injecting custom audio definitions.");

            foreach (MyObjectBuilder_AudioDefinition myObjectBuilder_AudioDefinition in definitions.Sounds)
            {
                Plugin.Instance.Log.Debug($"Injecting audio definition: {myObjectBuilder_AudioDefinition.Id.SubtypeName}");
                output[myObjectBuilder_AudioDefinition.Id] = (MyAudioDefinition)initdefinition.Invoke(null, new object[] { context, myObjectBuilder_AudioDefinition });
            }

            return true;
        }
    }
}
