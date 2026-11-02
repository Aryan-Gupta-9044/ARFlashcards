using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARFlashcards
{
    /// <summary>
    /// The whole app in one component. It is added to the scene by the setup wizard, and also
    /// auto-created on Play if a scene lacks it, so pressing Play can never give an empty screen.
    ///
    /// AR mode : Vuforia tracks the five printed cards (image targets created at runtime from the
    ///           card images, so no Target Manager database is needed - only a license key).
    /// 3D mode : if Vuforia cannot start (no license, no camera, Editor) or the user taps the mode
    ///           button, the animals are shown on a stage in front of the camera instead.
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public sealed class ARFlashcardsApp : MonoBehaviour
    {
        [Tooltip("Printed width of one card in metres (the A4 sheet prints them at 90 mm).")]
        public float cardWidthMeters = 0.09f;
        [Tooltip("Seconds to wait for Vuforia before falling back to the 3D viewer.")]
        public float vuforiaTimeout = 12f;

        enum Mode { Starting, AR, Viewer }

        Mode mode = Mode.Starting;
        bool userForcedViewer;
        bool targetsReady;
        string vuforiaProblem;      // human-readable reason AR is unavailable
        float startTime;

        Camera cam;
        FlashcardSession session;
        UiController ui;
        AudioFx audioFx;
        readonly List<TrackedCard> tracked = new List<TrackedCard>();

        Transform stage;
        GameObject viewerModel;
        string viewerModelId;

        static ARFlashcardsApp instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { instance = null; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoStart()
        {
            if (FindFirstObjectByType<ARFlashcardsApp>() == null)
                new GameObject("ARFlashcards (auto)").AddComponent<ARFlashcardsApp>();
        }

        // ------------------------------------------------------------------ lifecycle
        void Awake()
        {
            if (instance != null && instance != this) { Destroy(gameObject); return; }   // only one app per scene
            instance = this;
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            startTime = Time.realtimeSinceStartup;

            SetupCamera();
            SetupLight();

            session = new FlashcardSession();
            audioFx = gameObject.AddComponent<AudioFx>();
            ui = gameObject.AddComponent<UiController>();
            ui.Init(session, audioFx);
            ui.ModeToggleClicked += OnModeToggle;
            ui.CardPicked += OnCardPicked;
            ui.SpeakClicked += OnSpeak;
            ui.SetStatus("Starting camera...", StatusKind.Info);
            ui.SetViewerMode(false);

            TextToSpeechBridge.Init();
            BuildStage();

            try { VuforiaBridge.Hook(OnVuforiaInit, OnVuforiaStarted); }
            catch (System.Exception e) { Problem("Vuforia is not available: " + e.Message); }

            StartCoroutine(Watchdog());
        }

        void OnDestroy()
        {
            if (instance != this) return;
            instance = null;
            VuforiaBridge.Unhook();
        }

        // ------------------------------------------------------------------ scene plumbing
        void SetupCamera()
        {
            cam = Camera.main;
            if (cam == null) cam = FindFirstObjectByType<Camera>();
            if (cam == null)
            {
                var go = new GameObject("AR Camera", typeof(Camera), typeof(AudioListener));
                go.tag = "MainCamera";
                cam = go.GetComponent<Camera>();
            }
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 50f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.07f, 0.14f, 1f);
            try { VuforiaBridge.EnsureBehaviour(cam); }
            catch (System.Exception e) { Debug.LogWarning("[ARFlashcards] Could not add VuforiaBehaviour: " + e.Message); }
        }

        void SetupLight()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.62f, 0.64f, 0.70f);
            if (FindFirstObjectByType<Light>() != null) return;
            var go = new GameObject("Directional Light");
            var l = go.AddComponent<Light>();
            l.type = LightType.Directional;
            l.intensity = 1.1f;
            l.shadows = LightShadows.None;
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        void BuildStage()
        {
            var go = new GameObject("Viewer Stage");
            stage = go.transform;
            stage.SetParent(cam.transform, false);
            stage.localPosition = new Vector3(0f, 0.04f, 0.62f);
            // pedestal
            var ped = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ped.name = "Pedestal";
            var col = ped.GetComponent<Collider>(); if (col != null) Destroy(col);
            ped.transform.SetParent(stage, false);
            ped.transform.localPosition = new Vector3(0f, -0.145f, 0f);
            ped.transform.localScale = new Vector3(0.46f, 0.008f, 0.46f);
            var r = ped.GetComponent<Renderer>();
            r.sharedMaterial = MatLib.Get(new Color(0.16f, 0.2f, 0.34f));
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            stage.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------------ Vuforia
        void OnVuforiaInit(string error)
        {
            if (error == null) return;                      // ok; wait for OnVuforiaStarted
            if (error == VuforiaBridge.PackageMissing)
            {
                Problem("Vuforia package not installed - use menu AR Flashcards > Setup Wizard.");
                return;
            }
            string e = error.ToUpperInvariant();
            if (e.Contains("LICENSE")) Problem("Vuforia license key missing or invalid.");
            else if (e.Contains("PERMISSION")) Problem("Camera permission denied. Enable it in phone Settings.");
            else if (e.Contains("CAMERA")) Problem("Camera could not be opened (" + error + ").");
            else Problem("Vuforia error: " + error);
        }

        void OnVuforiaStarted() { TryCreateTargets(); }

        void TryCreateTargets()
        {
            if (targetsReady) return;
            try
            {
                if (!VuforiaBridge.BehaviourReady) return;
                var created = new List<TrackedCard>();
                foreach (var card in CardLibrary.All)
                {
                    var tex = Loc.LoadPng("ARFlashcards/Cards/" + card.Id);
                    if (tex == null) throw new System.Exception("card image missing for " + card.Id);
                    GameObject target = VuforiaBridge.CreateImageTarget(tex, cardWidthMeters, card.Id);

                    var model = AnimalFactory.Build(card.Id);
                    model.transform.SetParent(target.transform, false);
                    var inter = model.AddComponent<ModelInteraction>();
                    inter.baseScale = cardWidthMeters * 0.85f;

                    var tc = target.AddComponent<TrackedCard>();
                    tc.Bind(card, model);
                    tc.TrackingChanged += OnTrackingChanged;
                    created.Add(tc);
                }
                tracked.AddRange(created);
                targetsReady = true;
                if (!userForcedViewer) EnterAR();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[ARFlashcards] Target creation not ready yet: " + e.Message);
            }
        }

        void OnTrackingChanged(TrackedCard tc, bool isTracked)
        {
            if (mode != Mode.AR) return;
            if (isTracked)
            {
                session.Show(tc.card);
                audioFx.PlayCardFound(tc.card.Id);
                ui.SetStatus("Tracking: " + tc.card.Name(Lang.English), StatusKind.Tracking);
            }
            else
            {
                session.Hide(tc.card);
                if (session.Current == null) ui.SetStatus("Scanning... point the camera at a flashcard", StatusKind.Scanning);
            }
        }

        IEnumerator Watchdog()
        {
            while (mode == Mode.Starting)
            {
                if (!targetsReady) TryCreateTargets();
                if (mode != Mode.Starting) yield break;
                if (Time.realtimeSinceStartup - startTime > vuforiaTimeout)
                {
                    Problem(vuforiaProblem ?? "Vuforia did not start. Check the license key and camera.");
                    yield break;
                }
                yield return new WaitForSeconds(0.4f);
            }
        }

        void Problem(string text)
        {
            vuforiaProblem = text;
            Debug.LogWarning("[ARFlashcards] " + text);
            if (mode == Mode.AR) return;                     // already working; keep going
            EnterViewer(true);
        }

        // ------------------------------------------------------------------ modes
        void EnterAR()
        {
            mode = Mode.AR;
            stage.gameObject.SetActive(false);
            session.ClearVisible();
            foreach (var t in tracked) if (t != null && t.IsTracked) session.Show(t.card);
            ui.SetViewerMode(false);
            ui.SetStatus(session.Current != null ? "Tracking: " + session.Current.Name(Lang.English)
                         : "Scanning... point the camera at a flashcard", session.Current != null ? StatusKind.Tracking : StatusKind.Scanning);
        }

        void EnterViewer(bool automatic)
        {
            mode = Mode.Viewer;
            stage.gameObject.SetActive(true);
            ui.SetViewerMode(true);
            if (automatic && vuforiaProblem != null)
            {
                ui.SetStatus(vuforiaProblem + " Showing 3D viewer.", StatusKind.Error);
                ui.Toast("AR unavailable - using 3D viewer");
            }
            else ui.SetStatus("3D viewer - tap an animal, drag to rotate, pinch to zoom", StatusKind.Info);
            ShowInViewer(session.Current != null ? session.Current : CardLibrary.All[0]);
        }

        void OnModeToggle()
        {
            if (mode == Mode.Viewer)
            {
                if (!targetsReady) { TryCreateTargets(); }
                if (targetsReady) { userForcedViewer = false; EnterAR(); }
                else ui.Toast(vuforiaProblem ?? "AR camera is not ready yet");
            }
            else
            {
                userForcedViewer = true;
                vuforiaProblem = null;
                EnterViewer(false);
            }
        }

        void OnCardPicked(Card c)
        {
            if (mode != Mode.Viewer) return;
            ShowInViewer(c);
        }

        void ShowInViewer(Card c)
        {
            if (viewerModel == null || viewerModelId != c.Id)
            {
                if (viewerModel != null) Destroy(viewerModel);
                viewerModel = AnimalFactory.Build(c.Id);
                viewerModelId = c.Id;
                viewerModel.transform.SetParent(stage, false);
                viewerModel.transform.localPosition = new Vector3(0f, -0.14f, 0f);
                var inter = viewerModel.AddComponent<ModelInteraction>();
                inter.baseScale = 0.26f;
                audioFx.PlayCardFound(c.Id);
            }
            session.ClearVisible();
            session.Show(c);
        }

        // ------------------------------------------------------------------ speech
        void OnSpeak(Card c)
        {
            var r = TextToSpeechBridge.Speak(c.Speech(Loc.Current), Loc.Current);
            if (r == SpeakResult.NotReady) ui.Toast("Voice engine is starting - try again");
            else if (r == SpeakResult.LanguageMissing) ui.Toast(Loc.LangName(Loc.Current) + " voice not installed on this phone");
            else if (r == SpeakResult.NotAvailable) ui.Toast("Voice works on the Android phone");
        }

        void Update()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (mode == Mode.Starting && Time.realtimeSinceStartup - startTime > 3f
                && !UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
                vuforiaProblem = "Camera permission denied. Enable it in phone Settings.";
#endif
        }
    }
}
