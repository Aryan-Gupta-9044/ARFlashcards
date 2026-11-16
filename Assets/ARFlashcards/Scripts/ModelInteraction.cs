using System;
using UnityEngine;

namespace ARFlashcards
{
    /// <summary>
    /// On every animal model: pop-in animation, slow auto-spin, drag = rotate, pinch / wheel / +- buttons = scale.
    /// </summary>
    public sealed class ModelInteraction : MonoBehaviour
    {
        public float baseScale = 0.1f;      // metres per model unit
        public float spinSpeed = 24f;       // degrees / second when idle
        public float minFactor = 0.5f, maxFactor = 2.5f;

        /// <summary>Raised by the +/- buttons; every visible model reacts.</summary>
        public static event Action<float> ZoomRequested;
        public static void RequestZoom(float factor) { if (ZoomRequested != null) ZoomRequested(factor); }
        public static event Action ResetRequested;
        public static void RequestReset() { if (ResetRequested != null) ResetRequested(); }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { ZoomRequested = null; ResetRequested = null; }

        float yaw, factor = 1f, popT;
        float idleTimer;

        void OnEnable() { popT = 0f; ZoomRequested += OnZoom; ResetRequested += OnReset; Apply(0f); }
        void OnDisable() { ZoomRequested -= OnZoom; ResetRequested -= OnReset; }

        void OnZoom(float f) { factor = Mathf.Clamp(factor * f, minFactor, maxFactor); }
        void OnReset() { yaw = 0f; factor = 1f; idleTimer = 1.2f; Apply(EaseOutBack(popT)); }

        void Update()
        {
            popT = Mathf.Min(1f, popT + Time.deltaTime / 0.4f);

            Vector2 drag = PointerInput.ConsumeDrag();
            if (drag.sqrMagnitude > 0.01f) { yaw -= drag.x * 0.35f; idleTimer = 1.2f; }
            else if (idleTimer > 0f) idleTimer -= Time.deltaTime;
            else yaw += spinSpeed * Time.deltaTime;

            float z = PointerInput.ConsumeZoom();
            if (Mathf.Abs(z - 1f) > 0.0001f) factor = Mathf.Clamp(factor * z, minFactor, maxFactor);

            Apply(EaseOutBack(popT));
        }

        void Apply(float pop)
        {
            transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            transform.localScale = Vector3.one * (baseScale * factor * Mathf.Max(pop, 0.0001f));
        }

        static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f, c3 = c1 + 1f;
            float u = t - 1f;
            return 1f + c3 * u * u * u + c1 * u * u;
        }
    }
}
