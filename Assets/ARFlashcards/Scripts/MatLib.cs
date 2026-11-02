using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ARFlashcards
{
    /// <summary>
    /// One material per colour. Picks a lit shader that fits the project's render pipeline:
    /// URP/Lit when URP is active, otherwise the Built-in "Legacy Shaders/Diffuse" (always included in builds).
    /// </summary>
    public static class MatLib
    {
        static Shader shader;
        static readonly Dictionary<int, Material> cache = new Dictionary<int, Material>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { shader = null; cache.Clear(); }

        static Shader Pick()
        {
            if (shader != null) return shader;
            bool srp = GraphicsSettings.currentRenderPipeline != null;
            string[] order = srp
                ? new[] { "Universal Render Pipeline/Lit", "Universal Render Pipeline/Simple Lit", "Universal Render Pipeline/Unlit", "HDRP/Lit",
                          "Legacy Shaders/Diffuse", "Standard", "Unlit/Color", "Sprites/Default" }
                : new[] { "Legacy Shaders/Diffuse", "Standard", "Unlit/Color", "Sprites/Default" };
            foreach (var n in order)
            {
                var s = Shader.Find(n);
                if (s != null) { shader = s; break; }
            }
            if (shader == null) Debug.LogWarning("[ARFlashcards] No usable shader found for the 3D models.");
            return shader;
        }

        public static Material Get(Color c)
        {
            Color32 k = c;
            int key = (k.r << 24) | (k.g << 16) | (k.b << 8) | k.a;
            Material m;
            if (cache.TryGetValue(key, out m) && m != null) return m;

            m = new Material(Pick());
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0f);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.28f);
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.28f);
            m.name = "ARF_" + k.r + "_" + k.g + "_" + k.b;
            cache[key] = m;
            return m;
        }
    }
}
