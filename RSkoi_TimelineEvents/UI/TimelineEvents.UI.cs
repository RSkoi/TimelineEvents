using RSkoi_TimelineEvents.Core;

using KKAPI.Utilities;
using UILib;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Events;

namespace RSkoi_TimelineEvents.UI
{
    public class TimelineEventsUI
    {
        public static Text keyframeValueText;
        public static Button loadFromFileButton;

        internal static void Init()
        {
            TimelineEvents._timeline.GetFieldValue("_ui", out object timelineCanvas);

            keyframeValueText = ((Canvas)timelineCanvas).transform.Find("Keyframe Window/Main Container/Main Fields/Value/Background/Text").gameObject.GetComponent<Text>();

            GameObject timelineEventsUIFramekeyContainer = new("timelineEventsUIFramekeyContainer");
            timelineEventsUIFramekeyContainer.transform.SetParent(((Canvas)timelineCanvas).transform.Find("Keyframe Window/Main Container/Main Fields/Use Current"));
            timelineEventsUIFramekeyContainer.transform.localPosition = Vector3.zero;
            loadFromFileButton = CreateButton("TimelineEventsButton_LoadFile", timelineEventsUIFramekeyContainer.transform, new(-130f, 45f, 0f), new(150f, 30f), "Load from file", TimelineEvents.LoadFromFile);

            TimelineCompatibility.AddInterpolableModelStatic<TimelineEventType, string>(
                TimelineEvents.INTERPOLABLE_OWNER,
                TimelineEvents.INTERPOLABLE_ID,
                "",
                TimelineEvents.INTERPOLABLE_NAME,
                TimelineEvents._instance.EventDelegateBefore,
                TimelineEvents._instance.EventDelegateAfter,
                TimelineEvents._instance.EventIsCompatible,
                TimelineEvents._instance.EventGetValue,
                TimelineEvents._instance.EventDeserializeValueXML,
                TimelineEvents._instance.EventSerializeValueXML,
                null,
                null,
                TimelineEvents._instance.EventCheckIntegrity,
                false,
                null,
                null
            );
        }

        private static Button CreateButton(string objName, Transform parent, Vector3 pos, Vector2 size, string label, UnityAction onClickAction)
        {
            Button button = UIUtility.CreateButton(objName, parent, label);
            ((RectTransform)button.transform).sizeDelta = size;
            button.transform.localPosition = pos;
            button.onClick.AddListener(onClickAction);
            return button;
        }
    }
}
