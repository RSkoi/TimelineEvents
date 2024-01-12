using System;
using System.Collections.Generic;
using System.Reflection;
using RuntimeUnityEditor.Core.Utils;
using Timeline;

namespace RSkoi_TimelineEvents.Core
{
    public static class TimelineReflection
    {
        public static List<KeyValuePair<float, Keyframe>> GetPrivateTimelineSelectedFrames()
        {
            return (List<KeyValuePair<float, Keyframe>>)TimelineEvents._timeline.GetPrivate("_selectedKeyframes");
        }

        public static List<KeyValuePair<float, Keyframe>> GetPrivateTimelineCopiedFrames()
        {
            return (List<KeyValuePair<float, Keyframe>>)TimelineEvents._timeline.GetPrivate("_copiedKeyframes");
        }

        public static List<KeyValuePair<float, Keyframe>> GetPrivateTimelineCutFrames()
        {
            return (List<KeyValuePair<float, Keyframe>>)TimelineEvents._timeline.GetPrivate("_cutKeyframes");
        }

        public static List<Interpolable> GetPrivateTimelineSelectedInterpolables()
        {
            return (List<Interpolable>)TimelineEvents._timeline.GetPrivate("_selectedInterpolables");
        }

        public static object CallPrivateTypes(this object self, string name, Type[] types, params object[] p)
        {
            return self.GetType().GetMethod(
                name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy,
                null,
                types,
                null).Invoke(self, p);
        }
    }
}
