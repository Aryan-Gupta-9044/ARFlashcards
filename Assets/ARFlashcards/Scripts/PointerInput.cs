using UnityEngine;

namespace ARFlashcards
{
    /// <summary>
    /// Drag / zoom input with NO package dependency. Dragging is read from Unity's IMGUI events
    /// (works with the old Input Manager, the new Input System, mouse and touch). Two-finger pinch is
    /// only read when the old Input Manager is active; the +/- buttons and the mouse wheel cover the rest.
    /// </summary>
    public static class PointerInput
    {
        static Vector2 pendingDrag;
        static float pendingScale = 1f;
        static bool dragging;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { pendingDrag = Vector2.zero; pendingScale = 1f; dragging = false; }

        /// <summary>Call at the END of OnGUI (after buttons had their chance to use the event).</summary>
        public static void FeedGui(Event e, Rect zone)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    dragging = zone.Contains(e.mousePosition);
                    break;
                case EventType.MouseDrag:
                    if (dragging) pendingDrag += e.delta;
                    break;
                case EventType.MouseUp:
                    dragging = false;
                    break;
                case EventType.ScrollWheel:
                    if (zone.Contains(e.mousePosition)) pendingScale *= 1f - Mathf.Clamp(e.delta.y, -3f, 3f) * 0.06f;
                    break;
            }
        }

        public static Vector2 ConsumeDrag() { Vector2 d = pendingDrag; pendingDrag = Vector2.zero; return d; }

        /// <summary>Multiplicative zoom since the last call (mouse wheel or two-finger pinch). 1 = none.</summary>
        public static float ConsumeZoom()
        {
            float z = pendingScale; pendingScale = 1f;
#if ENABLE_LEGACY_INPUT_MANAGER
            try
            {
                if (Input.touchCount == 2)
                {
                    Touch a = Input.GetTouch(0), b = Input.GetTouch(1);
                    float now = (a.position - b.position).magnitude;
                    float prev = ((a.position - a.deltaPosition) - (b.position - b.deltaPosition)).magnitude;
                    if (prev > 1f) z *= now / prev;
                }
            }
            catch (System.InvalidOperationException) { /* old Input Manager disabled at runtime */ }
#endif
            return z;
        }
    }
}
