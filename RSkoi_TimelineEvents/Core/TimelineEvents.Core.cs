using RSkoi_TimelineEvents.Core;

using System;
using System.Text;
using System.IO;
using System.Xml;
using System.Security.Cryptography;
using System.Collections.Generic;
using RuntimeUnityEditor.Core.REPL;
using Studio;
using UnityEngine;
using Timeline;

namespace RSkoi_TimelineEvents
{
    public partial class TimelineEvents
    {
        public static Dictionary<string, TimelineEventType> hashedEvents = [];
        public static int loadingCounter = 0;

        private static bool _canPlay = true;
        private static string _stringifiedEvents = "";
        private static HashAlgorithm _hashAlgorithm = SHA256.Create();
        private static float _lastTime = -1f;

        public static void LoadFromFile()
        {
            KKAPI.Utilities.OpenFileDialog.Show(OnFileAccept, "Open text file", Application.dataPath, EVENT_FILE_FILTERS, "All files (*.*)|*.*");
            static void OnFileAccept(string[] strings)
            {
                if (strings == null || strings.Length == 0 || strings[0].IsNullOrEmpty())
                    return;

                string filePath = strings[0];
                string content = File.ReadAllText(filePath);
                ((TimelineEventType)GetKeyframesByType("SelectedKeyframes")[0].Value.value).EventValue = content;
                UpdateKeyframeValueTextPostfix();
            }
        }

        private void Execute(TimelineEventType e)
        {
            SendToREPL(e.EventValue);
            e.Executed = true;
        }

        private void SendToREPL(string input)
        {
            ReplWindow.Instance.Evaluate(input);
        }

        public TimelineEventType EventGetValue(ObjectCtrlInfo oci, string parameter)
        {
            if (hashedEvents.TryGetValue(parameter, out TimelineEventType e))
                return e;

            TimelineEventType newE = new(_lastTime, false);
            string key = GetHashString(newE.EventValue);
            newE.Hash = key;
            hashedEvents.Add(key, newE);

            return newE;
        }

        public Action<string, XmlTextWriter, TimelineEventType> EventSerializeValueXML
            = new ((string parameter, XmlTextWriter writer, TimelineEventType value) => WriteValue(writer, "value", value));

        public TimelineEventType EventDeserializeValueXML(string parameter, XmlNode node)
        {
            if (_stringifiedEvents.IsNullOrEmpty())
            {
                foreach (XmlNode script in node.SelectNodes("//EventValue"))
                    _stringifiedEvents += (_stringifiedEvents.IsNullOrEmpty() ? "" : "\n\n")
                        + $"<SCRIPT BEGIN>\n{script.InnerText}\n<SCRIPT END>";

                _warningText.text = _stringifiedEvents;
                _canPlay = false;

                StartWarning();
            }

            XmlNode curEventTypeNode = node.SelectNodes("//TimelineEventType")[loadingCounter++];
            TimelineEventType newE = new()
            {
                EventValue = curEventTypeNode.SelectSingleNode(".//EventValue").InnerText,
                Hash = curEventTypeNode.SelectSingleNode(".//Hash").InnerText,
                Time = float.Parse(curEventTypeNode.SelectSingleNode(".//Time").InnerText),
                Executed = bool.Parse(curEventTypeNode.SelectSingleNode(".//Executed").InnerText)
            };

            hashedEvents.Add(newE.Hash, newE);
            return newE;
        }

        public bool EventIsCompatible(ObjectCtrlInfo oci)
        {
            return true;
        }

        public bool EventCheckIntegrity(ObjectCtrlInfo oci, string parameter, TimelineEventType leftValue, TimelineEventType rightValue)
        {
            return true;
        }

        public void EventDelegateBefore(ObjectCtrlInfo oci, string parameter, TimelineEventType leftValue, TimelineEventType rightValue, float factor)
        {
            // do nuthin
        }

        public void EventDelegateAfter(ObjectCtrlInfo oci, string parameter, TimelineEventType leftValue, TimelineEventType rightValue, float factor)
        {
            // do nuthin
        }

        private static string GetHashString(string inputString)
        {
            StringBuilder sb = new ();
            foreach (byte b in _hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString)))
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }

        private static void WriteValue(XmlTextWriter self, string label, TimelineEventType value)
        {
            System.Xml.Serialization.XmlSerializer x = new(value.GetType());
            x.Serialize(self, value);
        }

        private static void WarningClearAllScripts()
        {
            hashedEvents.Clear();
            foreach (Interpolable i in TimelineReflection.GetPrivateTimelineInterpolables().Values)
                if (i.id == INTERPOLABLE_ID)
                {
                    i.keyframes.Clear();
                    break;
                }

            _canPlay = true;
        }

        private static void WarningContinue()
        {
            _canPlay = true;
        }
    }
}
