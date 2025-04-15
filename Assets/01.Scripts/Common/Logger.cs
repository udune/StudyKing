using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Common
{
    public static class Logger
    {
        [Conditional("DEV_VER")]
        public static void Log(string message)
        {
            Debug.Log($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
        }

        [Conditional("DEV_VER")]
        public static void LogWarning(string message)
        {
            Debug.LogWarning($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
        }

        public static void LogError(string message)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
        }
    }
}
