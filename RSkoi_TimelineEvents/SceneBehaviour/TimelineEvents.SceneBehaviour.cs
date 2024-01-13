using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace RSkoi_TimelineEvents.SceneBehaviour
{
    public class TimelineEventsSceneBehaviour
    {
        private static readonly HashAlgorithm _hashAlgorithm = SHA256.Create();

        public static string lastScenePath = "";

        public static string GetCurrentSceneHash()
        {
            return GetCurrentSceneHash(lastScenePath);
        }

        public static string GetCurrentSceneHash(string sceneFilePath)
        {
            string inputString = "";
            try { inputString = $"{sceneFilePath.Split('/', '\\').Last()}-{new FileInfo(sceneFilePath).Length}"; }
            catch { TimelineEvents.logger.LogError("Could not determine hash of scene file"); }

            inputString = inputString.IsNullOrEmpty() ? $"INVALID {Guid.NewGuid()}" : inputString; 

            StringBuilder sb = new();
            foreach (byte b in _hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString)))
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }
    }
}
