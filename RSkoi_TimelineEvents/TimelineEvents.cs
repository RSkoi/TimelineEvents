using RSkoi_TimelineEvents.Core;

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using KKAPI.Utilities;

namespace RSkoi_TimelineEvents
{
    [BepInProcess("CharaStudio.exe")]
    [BepInDependency(KKAPI.KoikatuAPI.GUID, KKAPI.KoikatuAPI.VersionConst)]
    [BepInDependency(Timeline.Timeline.GUID, Timeline.Timeline.Version)]
    [BepInDependency(RuntimeUnityEditor.Core.RuntimeUnityEditorCore.GUID, RuntimeUnityEditor.Core.RuntimeUnityEditorCore.Version)]
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public partial class TimelineEvents : BaseUnityPlugin
    {
        internal const string PLUGIN_GUID = "RSkoi_TimelineEvents";
        internal const string PLUGIN_NAME = "RSkoi_TimelineEvents";
        internal const string PLUGIN_VERSION = "1.0.1";

        internal const string INTERPOLABLE_OWNER = "TimelineEvents";
        internal const string INTERPOLABLE_ID = "TimelineEventsInterpolables";
        internal const string INTERPOLABLE_NAME = "Events";

        internal const string EVENT_FILE_FILTERS = "TXT (*.txt)|*.txt|All files (*.*)|*.*";
        internal const string CUSTOM_DIRECTORY_NAME = "Timeline\\TimelineEvents";

        internal const string CACHE_FILENAME = "TimelineEventsCache.cache";

        public static ManualLogSource logger;

        internal static TimelineEvents _instance;
        internal static Timeline.Timeline _timeline;
        internal static bool _isPlaying = false;
        internal static bool _interpolableEnabled = true;

        internal static string _cachePath;
        internal static List<string> _cache = [];

        internal static ConfigEntry<bool> CarryOverCache { get; private set; }
        internal static ConfigEntry<bool> ForceWarning {  get; private set; }
        internal static ConfigEntry<KeyboardShortcut> DumpScriptsShortcut { get; private set; }

        private void Awake()
        {
            _instance = this;
            _timeline = Singleton<Timeline.Timeline>.Instance;

            logger = Logger;

            SetupConfig();

            _cachePath = $"{Paths.CachePath}\\{CACHE_FILENAME}";
            if (CarryOverCache.Value)
                LoadCache();

            LoadWarningResource();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(LoadedEvent);

            Harmony.CreateAndPatchAll(typeof(TimelineEvents));
        }

        private void Update()
        {
            if (_warningInit)
                WarningUpdate();

            if (DumpScriptsShortcut.Value.IsDown() && hashedEvents.Count > 0)
            {
                string path = $"{Paths.PluginPath}\\{CUSTOM_DIRECTORY_NAME}\\";
                Directory.CreateDirectory(path);

                string stringifiedHashedEvents = "";
                foreach (TimelineEventType e in hashedEvents.Values)
                    stringifiedHashedEvents += $"Keyframe: Hash {e.Hash}, Time {e.Time}, Executed {e.Executed}" +
                        $"\nScript:\n{e.EventValue}\n\n";

                string filePath = path + $"KeyframeDump_{DateTime.Now:yyyy_MM_dd_HH-mm-ss-ff}.txt";

                using FileStream fs = File.Create(filePath);
                using StreamWriter sr = new(fs);
                sr.WriteLine(stringifiedHashedEvents);

                System.Diagnostics.Process.Start(path);

                logger.LogMessage($"Dumped keyframes to {filePath}");
            }
        }

        private void LoadedEvent(Scene scene, LoadSceneMode loadMode)
        {
            loadingCounter = 0;
            hashedEvents.Clear();
            _stringifiedEvents = "";

            if (scene.buildIndex != 1)
                return;

            if (!TimelineCompatibility.IsTimelineAvailable())
            {
                logger.LogError("Timeline not available");
                return;
            }

            UI.TimelineEventsUI.Init();
            InstantiateWarning();
            InitWarning();
        }

        private void SetupConfig()
        {
            DumpScriptsShortcut = Config.Bind(
                "Keyboard Shortcuts",
                "Dump scripts to file",
                new KeyboardShortcut(KeyCode.D, KeyCode.LeftControl),
                new ConfigDescription("Dump all scripts within the current scene to a text file.",
                null,
                new ConfigurationManagerAttributes { Order = 1 }));

            ForceWarning = Config.Bind(
                "Config",
                "Force script warning on all scenes",
                true,
                "Force the script warning on all scenes, regardless if the warning has been dismissed before. " +
                "When unchecked/false, the script warning will only pop up once on each scene.");

            CarryOverCache = Config.Bind(
                "Config",
                "Carry over cache",
                false,
                "Whether TimelineEvents should carry over the cache across studio sessions. " +
                "When unchecked/false, the script warnings will pop up independently of previous sessions.");
        }

        public static void LoadCache()
        {
            if (File.Exists(_cachePath))
            {
                try
                {
                    string[] loadedCache = File.ReadAllLines(_cachePath);
                    _cache = [..loadedCache];
                }
                catch { logger.LogError($"Could not load cache from {_cachePath}"); }
            }
        }

        public static void SaveCache(string newHash = null)
        {
            if (!newHash.IsNullOrEmpty() && !_cache.Contains(newHash))
                _cache.Add(newHash);

            using FileStream fs = File.Create(_cachePath);
            using StreamWriter sr = new(fs);
            foreach (string line in _cache)
                sr.WriteLine(line);
        }
    }
}
