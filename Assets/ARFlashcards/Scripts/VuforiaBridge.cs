using System;
using UnityEngine;
#if ARF_VUFORIA
using Vuforia;
#endif

namespace ARFlashcards
{
    /// <summary>
    /// The only place (with TrackedCard) that touches the Vuforia API. The real code is compiled only when
    /// the scripting define ARF_VUFORIA is set - the editor script VuforiaDefineManager sets it automatically as
    /// soon as the Vuforia package is installed. Until then the app still compiles and runs in 3D-viewer mode.
    /// </summary>
    public static class VuforiaBridge
    {
        public const string PackageMissing = "PACKAGE_MISSING";

#if ARF_VUFORIA
        static Action<string> onInit;
        static Action onStarted;
        static bool hooked;

        public static bool Compiled { get { return true; } }

        /// <summary>initCallback receives null when Vuforia initialised fine, otherwise the error name.</summary>
        public static void Hook(Action<string> initCallback, Action startedCallback)
        {
            if (hooked) return;
            onInit = initCallback; onStarted = startedCallback;
            VuforiaApplication.Instance.OnVuforiaInitialized += HandleInitialized;
            VuforiaApplication.Instance.OnVuforiaStarted += HandleStarted;
            hooked = true;
        }

        public static void Unhook()
        {
            if (!hooked) return;
            try
            {
                if (VuforiaApplication.Instance != null)
                {
                    VuforiaApplication.Instance.OnVuforiaInitialized -= HandleInitialized;
                    VuforiaApplication.Instance.OnVuforiaStarted -= HandleStarted;
                }
            }
            catch (Exception) { /* app is shutting down */ }
            hooked = false; onInit = null; onStarted = null;
        }

        static void HandleInitialized(VuforiaInitError error)
        {
            if (onInit != null) onInit(error == VuforiaInitError.NONE ? null : error.ToString());
        }

        static void HandleStarted()
        {
            if (onStarted != null) onStarted();
        }

        public static bool BehaviourReady { get { return VuforiaBehaviour.Instance != null; } }

        public static void EnsureBehaviour(Camera cam)
        {
            if (cam != null && cam.GetComponent<VuforiaBehaviour>() == null)
                cam.gameObject.AddComponent<VuforiaBehaviour>();
        }

        /// <summary>Runtime image target from a texture - no Target Manager database needed.</summary>
        public static GameObject CreateImageTarget(Texture2D texture, float widthMeters, string targetName)
        {
            var vb = VuforiaBehaviour.Instance;
            if (vb == null) throw new InvalidOperationException("VuforiaBehaviour not found on the camera");
            ImageTargetBehaviour itb = vb.ObserverFactory.CreateImageTarget(texture, widthMeters, targetName);
            if (itb == null) throw new InvalidOperationException("Vuforia returned no target for " + targetName);
            itb.enabled = true;
            return itb.gameObject;
        }
#else
        public static bool Compiled { get { return false; } }
        public static bool BehaviourReady { get { return false; } }

        public static void Hook(Action<string> initCallback, Action startedCallback)
        {
            if (initCallback != null) initCallback(PackageMissing);
        }
        public static void Unhook() { }
        public static void EnsureBehaviour(Camera cam) { }
        public static GameObject CreateImageTarget(Texture2D texture, float widthMeters, string targetName)
        {
            throw new InvalidOperationException("Vuforia package is not installed");
        }
#endif
    }
}
