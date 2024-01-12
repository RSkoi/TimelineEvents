using System;

namespace RSkoi_TimelineEvents.Core
{
    public class TimelineEventType
    {
        public string EventValue { get; set; }
        public string Hash { get; set; }
        public float Time { get; set; }
        public bool Executed { get; set; }

        public TimelineEventType() { }

        public TimelineEventType(TimelineEventType e)
        {
            this.EventValue = e.EventValue;
            this.Hash = e.Hash;
            this.Time = e.Time;
            this.Executed = e.Executed;
        }

        public TimelineEventType(float time, bool executed)
        {
            this.EventValue = GetDefaultValue();
            this.Time = time;
            this.Executed = executed;
        }

        public TimelineEventType(string value, string hash, float time, bool executed)
        {
            this.EventValue = value;
            this.Hash = hash;
            this.Time = time;
            this.Executed = executed;
        }

        public new string ToString()
        {
            return $"EventType[value: \"{EventValue?.Substring(0, Math.Min(EventValue.Length, 25))}\"," +
                $" hash: {Hash}, time: {Time}, executed: {Executed}]";
        }

        private static string GetDefaultValue()
        {
            return $"Debug.Log(\"Event {TimelineEvents.hashedEvents.Count}\");";
        }
    }
}
