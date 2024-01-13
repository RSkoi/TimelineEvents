using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace RSkoi_TimelineEvents
{
    public partial class TimelineEvents
    {
        internal static GameObject _warningPrefab;
        internal static bool _warningInit = false;

        private static GameObject _warningContainer;
        private static Text _warningText;
        private static Text _warningCountdownText;
        private static Button _warningContinueButton;
        private static Button _warningDeleteScriptsButton;
        private static Toggle _warningToggle;
        private static Scrollbar _warningScrollbar;

        private static bool _warningEnabled = true;

        private static float _warningScrollDifference = .05f;
        private static float _warningSecToWait = 5f;
        private static float _warningSecLeftToWait;
        private static bool _warningWaitTimeOver = false;

        internal static void LoadWarningResource()
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RSkoi_TimelineEvents.Resources.timelineevents.unity3d");
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            _warningPrefab = AssetBundle.LoadFromMemory(buffer).LoadAsset<GameObject>("TimelineEventsWarningCanvas");
            stream.Close();
        }

        internal static void InstantiateWarning()
        {
            _warningContainer = Instantiate(_warningPrefab);
            _warningContainer.SetActive(false);
        }

        internal static void InitWarning()
        {
            if (_warningInit)
                return;

            _warningText = GetTimelineEventsComponent<Text>("TimelineEventsWarningPanel/Scroll View/Viewport/Content/Text");
            _warningCountdownText = GetTimelineEventsComponent<Text>("TimelineEventsWarningPanel/CountdownLabel");
            _warningContinueButton = GetTimelineEventsComponent<Button>("TimelineEventsWarningPanel/ContinueButton");
            _warningDeleteScriptsButton = GetTimelineEventsComponent<Button>("TimelineEventsWarningPanel/RemoveContinueButton");
            _warningToggle = GetTimelineEventsComponent<Toggle>("TimelineEventsWarningPanel/WarningAcceptToggle");
            _warningScrollbar = GetTimelineEventsComponent<Scrollbar>("TimelineEventsWarningPanel/Scroll View/Scrollbar Vertical");

            _warningContinueButton.onClick.AddListener(WarningAcceptedEventClicked);
            _warningDeleteScriptsButton.onClick.AddListener(RemoveScriptsClicked);

            _warningInit = true;
        }

        public static void WarningUpdate()
        {
            if (_warningInit && _warningContainer.activeSelf)
            {
                _warningCountdownText.text = _warningSecLeftToWait.ToString("0");
                _warningSecLeftToWait = Mathf.Clamp(_warningSecLeftToWait - Time.deltaTime, 0f, _warningSecLeftToWait);
                _warningContinueButton.interactable = ScrolledToBottom() && _warningWaitTimeOver && _warningToggle.isOn;
            }
        }

        private void StartWarning()
        {
            _warningWaitTimeOver = false;
            _warningSecLeftToWait = _warningSecToWait;
            _warningContinueButton.interactable = false;
            _warningScrollbar.value = 1f;
            _warningToggle.isOn = false;

            _warningContainer.SetActive(true);
            StartCoroutine(WaitForSeconds(_warningSecToWait));
        }

        private IEnumerator WaitForSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            _warningWaitTimeOver = true;
        }

        private static bool ScrolledToBottom()
        {
            if (_warningScrollbar.value <= _warningScrollDifference)
                return true;
            return false;
        }

        private static void RemoveScriptsClicked()
        {
            WarningClearAllScripts();
            _warningContainer.SetActive(false);
        }

        private static void WarningAcceptedEventClicked()
        {
            WarningContinue();
            _warningContainer.SetActive(false);
        }

        private static T GetTimelineEventsComponent<T>(string transformPath)
        {
            return _warningContainer.transform
                    .Find(transformPath)
                    .gameObject
                    .GetComponent<T>();
        }
    }
}
