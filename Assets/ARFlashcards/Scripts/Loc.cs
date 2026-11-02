using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARFlashcards
{
    /// <summary>
    /// Current language + loader for the pre-shaped Hindi / Tamil text images.
    /// Unity's built-in text cannot shape Indic scripts (conjuncts / vowel signs come out broken),
    /// so those strings are shipped as images (Resources/ARFlashcards/Text/*.bytes, made by tools/make_assets.py).
    /// </summary>
    public static class Loc
    {
        public static Lang Current = Lang.English;
        public static event Action Changed;

        static readonly Dictionary<string, Texture2D> cache = new Dictionary<string, Texture2D>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Current = Lang.English;
            Changed = null;
            cache.Clear();
        }

        public static void Cycle()
        {
            Current = (Lang)(((int)Current + 1) % 3);
            if (Changed != null) Changed();
        }

        public static string Code
        {
            get { return Current == Lang.English ? "EN" : Current == Lang.Hindi ? "HI" : "TA"; }
        }

        public static string LangName(Lang l)
        {
            return l == Lang.English ? "English" : l == Lang.Hindi ? "Hindi" : "Tamil";
        }

        /// <summary>kind = "name" or "fact". Returns null for English or when the image is missing.</summary>
        public static Texture2D TextImage(string cardId, Lang lang, string kind)
        {
            if (lang == Lang.English) return null;
            string code = lang == Lang.Hindi ? "hi" : "ta";
            return LoadPng("ARFlashcards/Text/" + cardId + "_" + code + "_" + kind);
        }

        /// <summary>Loads a PNG stored as Resources/&lt;path&gt;.bytes into an uncompressed, readable RGBA32 texture.</summary>
        public static Texture2D LoadPng(string resourcePath)
        {
            Texture2D t;
            if (cache.TryGetValue(resourcePath, out t)) return t;

            var asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null)
            {
                Debug.LogWarning("[ARFlashcards] Missing resource: " + resourcePath);
                cache[resourcePath] = null;
                return null;
            }
            t = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            t.filterMode = FilterMode.Bilinear;
            if (!t.LoadImage(asset.bytes, false))
            {
                Debug.LogWarning("[ARFlashcards] Could not decode image: " + resourcePath);
                UnityEngine.Object.Destroy(t);
                t = null;
            }
            cache[resourcePath] = t;
            return t;
        }
    }
}
