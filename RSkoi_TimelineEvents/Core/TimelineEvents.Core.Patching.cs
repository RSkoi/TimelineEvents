using RSkoi_TimelineEvents.Core;
using RSkoi_TimelineEvents.UI;
using RSkoi_TimelineEvents.SceneBehaviour;

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using HarmonyLib;
using KKAPI.Utilities;
using Timeline;
using UILib;
using RuntimeUnityEditor.Core.Utils;
using UnityEngine.UI;
using Studio;

namespace RSkoi_TimelineEvents
{
    public partial class TimelineEvents
    {
        [HarmonyPatch(typeof(Timeline.Timeline), "Play")]
        [HarmonyPostfix]
        private static void PlayPostfix()
        {
            _isPlaying = true;
        }

        [HarmonyPatch(typeof(Timeline.Timeline), "Pause")]
        [HarmonyPatch(typeof(Timeline.Timeline), "Stop")]
        [HarmonyPostfix]
        private static void PausePostfix(MethodBase __originalMethod)
        {
            _isPlaying = false;

            if (__originalMethod.Name.Equals("Stop"))
                foreach (TimelineEventType e in hashedEvents.Values)
                    e.Executed = false;
        }

        [HarmonyPatch(typeof(Timeline.Timeline), "UpdateCursor2")]
        [HarmonyPostfix]
        private static void UpdateCursor2Postfix()
        {
            if (_canPlay && _isPlaying)
                foreach (KeyValuePair<string, TimelineEventType> entry in hashedEvents)
                    if (_interpolableEnabled && !entry.Value.Executed && entry.Value.Time < TimelineCompatibility.GetPlaybackTime())
                        _instance.Execute(entry.Value);
        }

        [HarmonyPatch(typeof(Timeline.Timeline), "AddKeyframe")]
        [HarmonyPrefix]
        private static void AddKeyframePrefix(bool __runOriginal, Interpolable interpolable, float time)
        {
            if (interpolable.id != INTERPOLABLE_ID)
                return;

            if (!__runOriginal)
                return;

            _lastTime = time;
        }

        [HarmonyPatch(typeof(Timeline.Timeline), "DeleteSelectedKeyframes")]
        [HarmonyPrefix]
        private static bool DeleteKeyframesPrefix(bool __runOriginal)
        {
            if (!__runOriginal)
                return false;

            List<KeyValuePair<float, Keyframe>> selectedKeyframes = KeyframesContainTimelineEvents("SelectedKeyframes").Value;
            if (selectedKeyframes.Count == 0)
                return true;

            foreach (KeyValuePair<float, Keyframe> entry in selectedKeyframes)
                hashedEvents.Remove(((TimelineEventType)entry.Value.value).Hash);
            UIUtility.DisplayConfirmationDialog(result =>
            {
                if (result)
                    _timeline.CallPrivateTypes(
                        "DeleteKeyframes",
                        [typeof(KeyValuePair<float, Keyframe>[]), typeof(bool)],
                        GetKeyframesByType("SelectedKeyframes"), true);
            }, "Are you sure you want to delete the selected Keyframe(s)?");

            return false;
        }

        [HarmonyPatch(typeof(Timeline.Timeline), "RemoveInterpolables")]
        [HarmonyPrefix]
        private static bool RemoveInterpolablesPrefix(bool __runOriginal, IEnumerable<Interpolable> interpolables)
        {
            if (!__runOriginal)
                return false;

            foreach (Interpolable i in interpolables)
                if (i.id == INTERPOLABLE_ID && i.keyframes.Count > 0)
                    foreach (KeyValuePair<float, Keyframe> entry in i.keyframes)
                        hashedEvents.Remove(((TimelineEventType)entry.Value.value).Hash);

            return true;
        }

        [HarmonyPatch(typeof(Timeline.Timeline), "MoveKeyframe")]
        [HarmonyPrefix]
        private static void MoveKeyframePrefix(bool __runOriginal, Keyframe keyframe, float destinationTime)
        {
            if (keyframe.parent.id != INTERPOLABLE_ID)
                return;

            if (!__runOriginal)
                return;

            TimelineEventType e = (TimelineEventType)keyframe.value;
            string key = e.Hash;

            if (hashedEvents.ContainsKey(key))
                hashedEvents[key].Time = destinationTime;
        }

        [HarmonyPatch(typeof(Timeline.Timeline), "OpenKeyframeWindow")]
        [HarmonyPostfix]
        private static void OpenKeyframeWindowPostfix()
        {
            Button button = (Button)_timeline.GetPrivate("_keyframeUseCurrentValueButton");
            button.interactable = true;

            var selectedTimelineEvents = KeyframesContainTimelineEvents("SelectedKeyframes");
            if (selectedTimelineEvents.Key)
                button.interactable = false;

            var selected = GetKeyframesByType("SelectedKeyframes");
            TimelineEventsUI.loadFromFileButton.gameObject.SetActive(false);
            if (selectedTimelineEvents.Key && selected.Count == 1)
            {
                TimelineEventsUI.loadFromFileButton.gameObject.SetActive(true);
                string eventValue = ((TimelineEventType)selected[0].Value.value).EventValue;
                logger.LogMessage($"TimelineEvent Value:\n{eventValue}");
            }
        }

        [HarmonyPatch(typeof(Timeline.Timeline), "UpdateKeyframeValueText")]
        [HarmonyPostfix]
        private static void UpdateKeyframeValueTextPostfix()
        {
            var selectedTimelineEvents = KeyframesContainTimelineEvents("SelectedKeyframes");
            var selected = GetKeyframesByType("SelectedKeyframes");
            if (selectedTimelineEvents.Key && selected.Count == 1)
            {
                string eventValue = ((TimelineEventType)selected[0].Value.value).EventValue;
                TimelineEventsUI.keyframeValueText.text = eventValue.Substring(0, Math.Min(eventValue.Length, 190));
            }
        }

        [HarmonyPatch(typeof(Timeline.Timeline), "CopyKeyframes")]
        [HarmonyPatch(typeof(Timeline.Timeline), "CutKeyframes")]
        [HarmonyPrefix]
        private static bool CopyCutKeyframesPrefix(bool __runOriginal)
        {
            if (!__runOriginal)
                return false;

            if (KeyframesContainTimelineEvents("SelectedKeyframes").Key)
                return false;

            return true;
        }

        [HarmonyPatch(typeof(Timeline.Timeline), "UpdateInterpolablesViewTree")]
        [HarmonyPostfix]
        private static void UpdateInterpolablesViewTreePostfix()
        {
            List<Interpolable> currentlySelectedInterpolables = TimelineReflection.GetPrivateTimelineSelectedInterpolables();
            foreach (Interpolable selectedInterpolable in currentlySelectedInterpolables)
                if (selectedInterpolable.id == INTERPOLABLE_ID)
                    _interpolableEnabled = selectedInterpolable.enabled;
        }

        private static KeyValuePair<bool, List<KeyValuePair<float, Keyframe>>> KeyframesContainTimelineEvents(string method)
        {
            List<KeyValuePair<float, Keyframe>> keyframes = GetKeyframesByType(method);
            return KeyframesContainTimelineEvents(keyframes);
        }

        private static KeyValuePair<bool, List<KeyValuePair<float, Keyframe>>> KeyframesContainTimelineEvents(List<KeyValuePair<float, Keyframe>> keyframes)
        {
            List<KeyValuePair<float, Keyframe>> resKeyframes = keyframes.Where(entry => (entry.Value.value as TimelineEventType) != null).ToList();
            if (resKeyframes.Count != 0)
                return new(true, resKeyframes);

            return new(false, resKeyframes);
        }

        private static List<KeyValuePair<float, Keyframe>> GetKeyframesByType(string method) => method switch
        {
            "CopyKeyframes" => TimelineReflection.GetPrivateTimelineCopiedFrames(),
            "PasteKeyframes" => TimelineReflection.GetPrivateTimelineCopiedFrames(),
            "CutKeyframes" => TimelineReflection.GetPrivateTimelineCutFrames(),
            "SelectedKeyframes" => TimelineReflection.GetPrivateTimelineSelectedFrames(),
            _ => throw new ArgumentOutOfRangeException("method", $"Unexpected method name: {method}"),
        };

        [HarmonyPatch(typeof(SceneInfo), "Save", [typeof(string)])]
        [HarmonyPostfix]
        private static void SavePostfix(string _path)
        {
            SaveCache(TimelineEventsSceneBehaviour.GetCurrentSceneHash(_path));
        }

        [HarmonyPatch(typeof(SceneInfo), "Load", [typeof(string)])]
        [HarmonyPostfix]
        private static void LoadPostfix(string _path)
        {
            TimelineEventsSceneBehaviour.lastScenePath = _path;
        }
    }
}
