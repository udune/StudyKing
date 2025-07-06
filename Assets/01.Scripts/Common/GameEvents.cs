using System;

namespace Common
{
    public static class GameEvents
    {
        public static event Action<long> OnStudyTimeUpdated;
        public static event Action<string> OnStudyCompleted;
        public static event Action OnUserSignedIn;
        public static event Action OnUserSignedOut;
        
        public static void TriggerStudyTimeUpdated(long time) => OnStudyTimeUpdated?.Invoke(time);
        public static void TriggerStudyCompleted(string subject) => OnStudyCompleted?.Invoke(subject);
        public static void TriggerUserSignedIn() => OnUserSignedIn?.Invoke();
        public static void TriggerUserSignedOut() => OnUserSignedOut?.Invoke();
    }
}