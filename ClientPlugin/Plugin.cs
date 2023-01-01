using System;
using System.IO;
using Rdr2ThemedMenus.GUI;
using HarmonyLib;
using Sandbox.Graphics.GUI;
using Rdr2ThemedMenus.Config;
using Rdr2ThemedMenus.Logging;
using Rdr2ThemedMenus.Patches;
using VRage.FileSystem;
using VRage.Plugins;
using Sandbox.Game;
using Rdr2ThemedMenus.GUI.MainMenuScreens;
using Sandbox.Definitions;
using VRage.Audio;
using System.Linq;
using VRage.Collections;
using VRage.Data.Audio;
using System.Collections.Generic;
using VRage.Game;
using VRage.ObjectBuilders;
using Rdr2ThemedMenus.Patches.DefinitionManager;
using Sandbox.Engine.Utils;
using VRage;
using Sandbox;
using Sandbox.Game.GUI;
using VRage.UserInterface.Media;

namespace Rdr2ThemedMenus
{
    // ReSharper disable once UnusedType.Global
    public class Plugin : IPlugin, IDisposable
    {
        public const string Name = "Rdr2ThemedMenus";
        public static Plugin Instance { get; private set; }

        public long Tick { get; private set; }

        public readonly string ContentDirectory = Path.Combine(MyFileSystem.ContentPath, "RDR2ThemedMenus");

        public IPluginLogger Log => Logger;
        private static readonly IPluginLogger Logger = new PluginLogger(Name);

        public IPluginConfig Config => config?.Data;
        private PersistentConfig<PluginConfig> config;
        private static readonly string ConfigFileName = $"{Name}.cfg";

        private static bool initialized;
        private static bool failed;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public void Init(object gameInstance)
        {
            Instance = this;

            Log.Info("Loading");

            if (Directory.Exists(Path.Combine(MyFileSystem.UserDataPath, "Storage/PluginData")));
            {
                Directory.CreateDirectory(Path.Combine(MyFileSystem.UserDataPath, "Storage/PluginData"));
            }

            var configPath = Path.Combine(MyFileSystem.UserDataPath, "Storage/PluginData", ConfigFileName);
            config = PersistentConfig<PluginConfig>.Load(Log, configPath);

            if (!PatchHelpers.HarmonyPatchAll(Log, new Harmony(Name)))
            {
                failed = true;
                return;
            }

            Log.Debug("Successfully loaded");
        }

        public void Dispose()
        {
            try
            {
                // TODO: Save state and close resources here, called when the game exists (not guaranteed!)
                // IMPORTANT: Do NOT call harmony.UnpatchAll() here! It may break other plugins.
            }
            catch (Exception ex)
            {
                Log.Critical(ex, "Dispose failed");
            }

            Instance = null;
        }

        public void Update()
        {
            EnsureInitialized();
            try
            {
                if (!failed)
                {
                    CustomUpdate();
                    Tick++;
                }
            }
            catch (Exception ex)
            {
                Log.Critical(ex, "Update failed");
                failed = true;
            }
        }

        private void EnsureInitialized()
        {
            if (initialized || failed)
                return;

            Log.Info("Initializing");
            try
            {
                Initialize();
            }
            catch (Exception ex)
            {
                Log.Critical(ex, "Failed to initialize plugin");
                failed = true;
                return;
            }

            Log.Debug("Successfully initialized");
            initialized = true;
        }

        private void Initialize()
        {
            List<string> files = new List<string>();
            files.Add(Path.Combine(Plugin.Instance.ContentDirectory, @"Data\GuiSounds.xml"));
            InjectCustomDefinitions(files);
            MyPerGameSettings.GUI.MainMenu = typeof(RDR2MainMenu);
            MyPerGameSettings.BasicGameInfo.GameName = "Red Dead Redemption 2";
            MyPerGameSettings.BasicGameInfo.ApplicationName = "Red Dead Redemption 2";
            MyPerGameSettings.BasicGameInfo.GameAcronym = "RDR2";
        }

        private void CustomUpdate()
        {
            // TODO: Put your update code here. It is called on every simulation frame!
        }


        // ReSharper disable once UnusedMember.Global
        public void OpenConfigDialog()
        {
            MyGuiSandbox.AddScreen(new MyPluginConfigDialog());
        }

        private void InjectCustomDefinitions(List<string> files)
        {
            List<Tuple<MyObjectBuilder_Definitions, string>> customDefinitions = new List<Tuple<MyObjectBuilder_Definitions, string>>();

            foreach (string definitionFile in files)
            {
                MyObjectBuilder_Definitions definitions;

                MyObjectBuilderSerializer.DeserializeXML(definitionFile, out definitions);

                Tuple<MyObjectBuilder_Definitions, string> definitionList = new Tuple<MyObjectBuilder_Definitions, string>(definitions, definitionFile);

                customDefinitions.Add(definitionList);
            }

            MyDefinitionManager_Injections.CustomDefinitions = customDefinitions;
            MyDefinitionManager.Static.PreloadDefinitions();
            MyAudio.ReloadData(PluginAudioExtensions.GetSoundDataFromDefinitions(), PluginAudioExtensions.GetEffectData());
        }
    }
}