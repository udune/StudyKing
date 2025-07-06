
public static class Constants
{
    public static class Firebase
    {
        public const string GOOGLE_WEB_CLIENT_ID = "222061272404-27lp0ocv653h3jci5vitlp1qq97otq3p.apps.googleusercontent.com";
        public const string UNITY_EDITOR_USER_ID = "9HyPrbDAf4Q1eLMhp9LVkxptBlx1";
    }

    public static class OpenAI
    {
        public const string API_URL = "https://api.openai.com/v1/chat/completions";
        public const string MODEL = "gpt-3.5-turbo";
        public const int MAX_TOKENS = 200;
        public const float TEMPERATURE = 0.7f;
    }

    public static class PlayerPrefs
    {
        public const string HAS_SIGNED_WITH_GOOGLE = "HasSignedWithGoogle";
        public const string HAS_SIGNED_WITH_APPLE = "HasSignedWithApple";
    }
    
    public static class GlobalDefine
    {
        public const string GOOGLE_PLAY_STORE = "https://play.google.com/store/apps/details?id=com.MinchanKim.StudyKing";
        public const string APPLE_PLAY_STORE = "";
    }

}
