// Compiled only with -define:ARF_VUFORIA: stand-in for the Vuforia Engine API surface we use.
using System;
namespace Vuforia
{
    public enum VuforiaInitError { NONE, LICENSE_ERROR }
    public enum Status { NO_POSE, LIMITED, EXTENDED_TRACKED, TRACKED }
    public struct TargetStatus { public Status Status; }
    public class ObserverBehaviour : UnityEngine.Behaviour { public event Action<ObserverBehaviour, TargetStatus> OnTargetStatusChanged;
        public void Fire(Status s) { if (OnTargetStatusChanged != null) OnTargetStatusChanged(this, new TargetStatus { Status = s }); } }
    public class ImageTargetBehaviour : ObserverBehaviour { }
    public class ObserverFactory { public static System.Collections.Generic.List<ImageTargetBehaviour> Created = new System.Collections.Generic.List<ImageTargetBehaviour>();
        public ImageTargetBehaviour CreateImageTarget(UnityEngine.Texture2D t, float w, string n)
        { var g = new UnityEngine.GameObject(n); var b = g.AddComponent<ImageTargetBehaviour>(); Created.Add(b); return b; } }
    public class VuforiaBehaviour : UnityEngine.Behaviour { public static VuforiaBehaviour Instance = new VuforiaBehaviour(); public ObserverFactory ObserverFactory = new ObserverFactory(); }
    public class VuforiaApplication { public static VuforiaApplication Instance = new VuforiaApplication();
        public event Action<VuforiaInitError> OnVuforiaInitialized; public event Action OnVuforiaStarted;
        public void RaiseInit(VuforiaInitError e) { if (OnVuforiaInitialized != null) OnVuforiaInitialized(e); }
        public void RaiseStarted() { if (OnVuforiaStarted != null) OnVuforiaStarted(); } }
}
