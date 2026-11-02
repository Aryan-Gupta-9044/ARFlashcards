using UnityEngine;

namespace ARFlashcards
{
    /// <summary>Synthesised UI sounds (no audio assets needed). Drop real animal sounds into
    /// Resources/ARFlashcards/Sounds/&lt;id&gt;.wav (or .mp3 / .ogg) and they play when that card appears.</summary>
    public sealed class AudioFx : MonoBehaviour
    {
        AudioSource src;
        AudioClip chime, ding, buzz, click;

        void Awake()
        {
            src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            chime = Make("chime", 0.55f, new[] { 784f, 1175f }, 5f);
            ding = Make("ding", 0.7f, new[] { 659f, 988f, 1319f }, 4f);
            buzz = Make("buzz", 0.22f, new[] { 150f, 160f }, 9f);
            click = Make("click", 0.06f, new[] { 1400f }, 30f);
        }

        public void PlayCardFound(string cardId)
        {
            var real = Resources.Load<AudioClip>("ARFlashcards/Sounds/" + cardId);
            src.PlayOneShot(real != null ? real : chime, 0.8f);
        }

        public void PlayCorrect() { src.PlayOneShot(ding, 0.8f); }
        public void PlayWrong() { src.PlayOneShot(buzz, 0.6f); }
        public void PlayClick() { src.PlayOneShot(click, 0.5f); }

        static AudioClip Make(string name, float seconds, float[] freqs, float decay)
        {
            const int rate = 22050;
            int n = (int)(seconds * rate);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / rate;
                float env = Mathf.Exp(-decay * t) * Mathf.Min(1f, i / 120f);
                float s = 0f;
                for (int k = 0; k < freqs.Length; k++) s += Mathf.Sin(2f * Mathf.PI * freqs[k] * t) / freqs.Length;
                data[i] = s * env * 0.6f;
            }
            var clip = AudioClip.Create(name, n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
