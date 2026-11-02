using UnityEngine;

namespace ARFlashcards
{
    public enum SpeakResult { Ok, NotReady, LanguageMissing, NotAvailable }

    /// <summary>Reads the card aloud with Android's built-in text-to-speech (Android device only).</summary>
    public static class TextToSpeechBridge
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        static AndroidJavaObject tts;
        static volatile bool ready;

        sealed class InitListener : AndroidJavaProxy
        {
            public InitListener() : base("android.speech.tts.TextToSpeech$OnInitListener") { }
            public void onInit(int status) { ready = (status == 0); }
        }
#endif

        public static void Init()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (tts != null) return;
            try
            {
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    tts = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, new InitListener());
                }
            }
            catch (System.Exception e) { Debug.LogWarning("[ARFlashcards] TTS init failed: " + e.Message); tts = null; }
#endif
        }

        public static SpeakResult Speak(string text, Lang lang)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (tts == null) Init();
            if (tts == null || !ready) return SpeakResult.NotReady;
            try
            {
                string code = lang == Lang.Hindi ? "hi" : lang == Lang.Tamil ? "ta" : "en";
                int r;
                using (var locale = new AndroidJavaObject("java.util.Locale", code))
                    r = tts.Call<int>("setLanguage", locale);
                if (r < 0) return SpeakResult.LanguageMissing;      // -1 missing data, -2 not supported
                try { tts.Call<int>("speak", text, 0, null, "arf"); }
                catch (System.Exception) { tts.Call<int>("speak", text, 0, null); }
                return SpeakResult.Ok;
            }
            catch (System.Exception e) { Debug.LogWarning("[ARFlashcards] TTS speak failed: " + e.Message); return SpeakResult.NotAvailable; }
#else
            Debug.Log("[ARFlashcards] (voice only works on an Android device) " + text);
            return SpeakResult.NotAvailable;
#endif
        }

        public static void Stop()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try { if (tts != null && ready) tts.Call<int>("stop"); } catch (System.Exception) { }
#endif
        }
    }
}
