using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using KKAPI.Utilities;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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

        internal const string EVENT_FILE_FILTERS = "TXT (*.txt)|*.txt|CS (*.cs)|*.cs|XML (*.xml)|*.xml|All files (*.*)|*.*";

        public static ManualLogSource logger;

        internal static TimelineEvents _instance;
        internal static Timeline.Timeline _timeline;
        internal static bool _isPlaying = false;
        internal static bool _interpolableEnabled = true;

        private void Awake()
        {
            _instance = this;
            _timeline = Singleton<Timeline.Timeline>.Instance;

            logger = Logger;

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
    }
}
