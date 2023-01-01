using HarmonyLib;
using Sandbox.Definitions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.ObjectBuilders;

namespace Rdr2ThemedMenus.Patches.DefinitionManager
{
    [HarmonyPatch(typeof(MyDefinitionManager), "GetDefinitionBuilders")]
    internal static class MyDefinitionManager_Injections
    {
        public static List<Tuple<MyObjectBuilder_Definitions, string>> CustomDefinitions = null;

        private static void Postfix(ref List<Tuple<MyObjectBuilder_Definitions, string>> __result)
        {
            MyObjectBuilder_Definitions definitions;

            MyObjectBuilderSerializer.DeserializeXML(Path.Combine(Plugin.Instance.ContentDirectory, @"Data\GuiSounds.xml"), out definitions);

            Tuple<MyObjectBuilder_Definitions, string> item = new Tuple<MyObjectBuilder_Definitions, string>(definitions, Path.Combine(Plugin.Instance.ContentDirectory, @"Data\GuiSounds.xml"));
            __result = __result.Concat(CustomDefinitions).ToList();
            __result.Sort((Tuple<MyObjectBuilder_Definitions, string> x, Tuple<MyObjectBuilder_Definitions, string> y) => x.Item2.CompareTo(y.Item2));
        }
    }
}
