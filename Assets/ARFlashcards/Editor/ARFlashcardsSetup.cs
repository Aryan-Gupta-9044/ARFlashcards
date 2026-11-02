#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ARFlashcards.EditorTools
{
    /// <summary>
    /// AR Flashcards > Setup Wizard. Takes the project from "just opened" to "APK on the phone":
    /// install Vuforia, license key, scene, Android settings, build.
    /// This file does NOT reference the Vuforia API directly (reflection only), so it always compiles.
    /// </summary>
    [InitializeOnLoad]
    public sealed class ARFlashcardsSetup : EditorWindow
    {
        const string ScenePath = "Assets/Scenes/FlashcardScene.unity";
        const string VuforiaConfigPath = "Assets/Resources/VuforiaConfiguration.asset";
        const string PackageId = "com.aryan.arflashcards";
        const string VuforiaPackage = "com.ptc.vuforia.engine";
        const string RegistryUrl = "https://registry.packages.developer.vuforia.com/";

        string licenseInput = "";
        Vector2 scroll;

        static AddRequest addRequest;
        static string addFallback;
        static string installMessage = "";

        static ARFlashcardsSetup()
        {
            EditorApplication.delayCall += () =>
            {
                if (!File.Exists(ScenePath) && !SessionState.GetBool("ARF_WizardShown", false))
                {
                    SessionState.SetBool("ARF_WizardShown", true);
                    Open();
                }
            };
        }

        [MenuItem("AR Flashcards/Setup Wizard")]
        public static void Open()
        {
            var w = GetWindow<ARFlashcardsSetup>("AR Flashcards");
            w.minSize = new Vector2(460, 600);
            w.Show();
        }

        [MenuItem("AR Flashcards/One-Click Setup (scene + Android)")]
        public static void OneClick()
        {
            CreateScene();
            ApplyAndroidSettings();
            Open();
        }

        void OnEnable() { EditorApplication.update += Poll; }
        void OnDisable() { EditorApplication.update -= Poll; }

        void Poll()
        {
            if (addRequest == null || !addRequest.IsCompleted) { if (addRequest != null) Repaint(); return; }
            var r = addRequest; addRequest = null;
            if (r.Status == StatusCode.Success)
            {
                installMessage = "Vuforia installed (" + r.Result.version + "). Unity is recompiling - wait for the spinner (bottom-right) to finish.";
                Debug.Log("[ARFlashcards] " + installMessage);
            }
            else if (addFallback != null)
            {
                string id = addFallback; addFallback = null;
                installMessage = "Trying version " + id + " ...";
                addRequest = Client.Add(id);
            }
            else
            {
                installMessage = "Install failed: " + (r.Error != null ? r.Error.message : "unknown") +
                                 "\nSee docs/TROUBLESHOOTING.md (section: Vuforia will not install) for the manual 3-minute route.";
                Debug.LogWarning("[ARFlashcards] " + installMessage);
            }
            Repaint();
        }

        // ------------------------------------------------------------------ GUI
        void OnGUI()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);
            GUILayout.Label("AR Flashcards - Setup Wizard", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Work top to bottom. A tick means the step is done. You can press Play at any time - without Vuforia the app runs as a 3D viewer.", MessageType.None);

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                EditorGUILayout.HelpBox("Unity is compiling / importing - please wait...", MessageType.Info);

            // 1 install vuforia
            bool present = VuforiaDefineManager.VuforiaPresent;
            bool defined = VuforiaDefineManager.DefineSet;
            Header("1. Install Vuforia Engine", present && defined);
            if (!present)
            {
                EditorGUILayout.HelpBox("Adds the official Vuforia package registry and installs " + VuforiaPackage + ". Accept the Vuforia license/EULA if Unity asks.", MessageType.Warning);
                using (new EditorGUI.DisabledScope(addRequest != null))
                    if (GUILayout.Button("Install Vuforia Engine")) InstallVuforia();
            }
            else if (!defined)
            {
                EditorGUILayout.HelpBox("Vuforia found. Enabling AR code (a recompile follows)...", MessageType.Info);
                if (GUILayout.Button("Enable AR code now")) VuforiaDefineManager.Sync();
            }
            else EditorGUILayout.LabelField("Vuforia is installed and the AR code is enabled.");
            if (addRequest != null) EditorGUILayout.HelpBox("Installing... this can take 1-3 minutes.", MessageType.Info);
            if (!string.IsNullOrEmpty(installMessage)) EditorGUILayout.HelpBox(installMessage, MessageType.None);

            // 2 license
            string key = present ? ReadLicenseKey() : "";
            bool hasKey = !string.IsNullOrEmpty(key) && key.Length > 50;
            Header("2. Vuforia license key", hasKey);
            using (new EditorGUI.DisabledScope(!present))
            {
                if (!hasKey)
                {
                    EditorGUILayout.HelpBox("Free: developer.vuforia.com > Develop > License Manager > Get Development Key. Open it, copy the long key text and paste it below.", MessageType.Warning);
                    if (GUILayout.Button("Open Vuforia License Manager")) Application.OpenURL("https://developer.vuforia.com/vui/develop/licenses");
                    licenseInput = EditorGUILayout.TextField("Paste key", licenseInput);
                    using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(licenseInput)))
                        if (GUILayout.Button("Save key into project")) SaveLicenseKey(licenseInput.Trim());
                }
                else
                {
                    EditorGUILayout.LabelField("Key saved (" + key.Length + " characters).");
                    if (GUILayout.Button("Replace key")) { SaveLicenseKey(""); }
                }
            }

            // 3 scene
            bool sceneOk = File.Exists(ScenePath);
            Header("3. Scene", sceneOk);
            if (GUILayout.Button(sceneOk ? "Rebuild FlashcardScene" : "Create FlashcardScene")) CreateScene();
            if (sceneOk && present) EditorGUILayout.HelpBox("Installed Vuforia after creating the scene? Press 'Rebuild FlashcardScene' once so the AR Camera gets its Vuforia component.", MessageType.None);

            // 4 android
            bool android = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
            Header("4. Android settings", android);
            EditorGUILayout.HelpBox("Sets: Android platform, IL2CPP, ARM64, OpenGLES3, minimum Android 10 (Vuforia 11 requirement), portrait, package " + PackageId + ".", MessageType.None);
            if (GUILayout.Button("Apply Android settings")) ApplyAndroidSettings();

            // 5 build
            Header("5. Build", false);
            EditorGUILayout.HelpBox("Phone: Settings > Developer options > USB debugging ON, plug in the cable, press 'Build & Run'. Or build the APK and copy it to the phone.", MessageType.None);
            if (GUILayout.Button("Build APK  (Builds/ARFlashcards.apk)")) BuildApk(false);
            if (GUILayout.Button("Build & Run on connected phone")) BuildApk(true);

            GUILayout.Space(8);
            if (GUILayout.Button("Open project folder")) EditorUtility.RevealInFinder(Path.GetFullPath("Assets"));
            EditorGUILayout.EndScrollView();
        }

        static void Header(string text, bool ok)
        {
            GUILayout.Space(8);
            GUILayout.Label((ok ? "[x]  " : "[ ]  ") + text, EditorStyles.boldLabel);
        }

        // ------------------------------------------------------------------ install vuforia
        static void InstallVuforia()
        {
            try
            {
                EnsureRegistry();
                installMessage = "Registry added. Installing the package...";
                Client.Resolve();
                addFallback = VuforiaPackage + "@11.4.4";
                addRequest = Client.Add(VuforiaPackage);
            }
            catch (Exception e)
            {
                installMessage = "Could not prepare the install: " + e.Message;
                Debug.LogWarning("[ARFlashcards] " + installMessage);
            }
        }

        /// <summary>Adds the Vuforia scoped registry to Packages/manifest.json if it is not there yet.</summary>
        public static bool EnsureRegistry()
        {
            string path = "Packages/manifest.json";
            if (!File.Exists(path))
            {
                Directory.CreateDirectory("Packages");
                File.WriteAllText(path, "{\n  \"dependencies\": {\n  }\n}\n");
            }
            string json = File.ReadAllText(path);
            if (json.Contains("registry.packages.developer.vuforia.com")) return false;

            const string entry =
                "    {\n      \"name\": \"Vuforia\",\n      \"url\": \"" + RegistryUrl + "\",\n      \"scopes\": [\n        \"com.ptc.vuforia\"\n      ]\n    }";

            Match sr = Regex.Match(json, "\"scopedRegistries\"\\s*:\\s*\\[");
            if (sr.Success)
            {
                json = json.Insert(sr.Index + sr.Length, "\n" + entry + ",");
            }
            else
            {
                int brace = json.IndexOf('{');
                json = json.Insert(brace + 1, "\n  \"scopedRegistries\": [\n" + entry + "\n  ],");
            }
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
            Debug.Log("[ARFlashcards] Vuforia package registry added to Packages/manifest.json");
            return true;
        }

        // ------------------------------------------------------------------ license
        static object VuforiaSettings(out PropertyInfo licenseProp)
        {
            licenseProp = null;
            Type conf = VuforiaDefineManager.FindType("Vuforia.VuforiaConfiguration");
            if (conf == null) return null;
            PropertyInfo inst = conf.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            object c = inst != null ? inst.GetValue(null, null) : null;
            if (c == null) return null;
            PropertyInfo vp = conf.GetProperty("Vuforia");
            object v = vp != null ? vp.GetValue(c, null) : null;
            if (v == null) return null;
            licenseProp = v.GetType().GetProperty("LicenseKey");
            return licenseProp != null ? v : null;
        }

        static string ReadLicenseKey()
        {
            try
            {
                PropertyInfo lp;
                object v = VuforiaSettings(out lp);
                if (v != null) { string k = lp.GetValue(v, null) as string; return k ?? ""; }
            }
            catch (Exception) { }
            if (!File.Exists(VuforiaConfigPath)) return "";
            var m = Regex.Match(File.ReadAllText(VuforiaConfigPath), @"(?m)^\s*vuforiaLicenseKey:[ \t]*(\S*)\s*$");
            return m.Success ? m.Groups[1].Value : "";
        }

        static void SaveLicenseKey(string key)
        {
            try
            {
                PropertyInfo lp;
                object v = VuforiaSettings(out lp);
                if (v != null && lp.CanWrite)
                {
                    lp.SetValue(v, key, null);
                    Type conf = VuforiaDefineManager.FindType("Vuforia.VuforiaConfiguration");
                    object c = conf.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
                    EditorUtility.SetDirty((UnityEngine.Object)c);
                    AssetDatabase.SaveAssets();
                    Debug.Log("[ARFlashcards] Vuforia license key " + (key.Length > 0 ? "saved." : "cleared."));
                    return;
                }
            }
            catch (Exception e) { Debug.LogWarning("[ARFlashcards] Config API route failed (" + e.Message + "), editing the file instead."); }

            if (!File.Exists(VuforiaConfigPath))
            {
                EditorUtility.DisplayDialog("AR Flashcards",
                    "Vuforia configuration not found.\nSelect the AR Camera in the scene, click 'Open Vuforia Engine configuration' in the Inspector, paste the key in 'License Key', then continue.", "OK");
                return;
            }
            string text = File.ReadAllText(VuforiaConfigPath);
            File.WriteAllText(VuforiaConfigPath, Regex.Replace(text, @"(?m)^(\s*vuforiaLicenseKey:).*$", "$1 " + key));
            AssetDatabase.ImportAsset(VuforiaConfigPath, ImportAssetOptions.ForceUpdate);
            AssetDatabase.SaveAssets();
            Debug.Log("[ARFlashcards] Vuforia license key " + (key.Length > 0 ? "saved." : "cleared."));
        }

        // ------------------------------------------------------------------ scene
        public static void CreateScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory("Assets/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGO = new GameObject("AR Camera", typeof(Camera), typeof(AudioListener));
            camGO.tag = "MainCamera";
            var cam = camGO.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.07f, 0.14f, 1f);
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 50f;
            Type vb = VuforiaDefineManager.FindType("Vuforia.VuforiaBehaviour");
            if (vb != null) camGO.AddComponent(vb);

            var lightGO = new GameObject("Directional Light");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            new GameObject("ARFlashcards").AddComponent<ARFlashcardsApp>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("[ARFlashcards] Scene created: " + ScenePath + (vb == null ? " (Vuforia not installed yet - 3D viewer mode)" : ""));
        }

        // ------------------------------------------------------------------ android
        static void IncludeShaders()
        {
            try
            {
                var all = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
                if (all == null || all.Length == 0) return;
                var so = new SerializedObject(all[0]);
                var arr = so.FindProperty("m_AlwaysIncludedShaders");
                if (arr == null) return;
                foreach (string name in new[] { "Legacy Shaders/Diffuse", "Standard", "Sprites/Default", "Unlit/Color" })
                {
                    Shader sh = Shader.Find(name);
                    if (sh == null) continue;
                    bool has = false;
                    for (int i = 0; i < arr.arraySize; i++)
                        if (arr.GetArrayElementAtIndex(i).objectReferenceValue == sh) { has = true; break; }
                    if (has) continue;
                    arr.arraySize++;
                    arr.GetArrayElementAtIndex(arr.arraySize - 1).objectReferenceValue = sh;
                }
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            catch (Exception e) { Debug.LogWarning("[ARFlashcards] Could not add shaders to 'Always Included': " + e.Message); }
        }

        public static void ApplyAndroidSettings()
        {
            try
            {
                PlayerSettings.companyName = "Aryan";
                PlayerSettings.productName = "AR Flashcards";
                var android = NamedBuildTarget.Android;
                PlayerSettings.SetApplicationIdentifier(android, PackageId);
                PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
                PlayerSettings.SetManagedStrippingLevel(android, ManagedStrippingLevel.Low);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
                PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
                PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });
                PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
                EditorUserBuildSettings.buildAppBundle = false;
                IncludeShaders();

                if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                AssetDatabase.SaveAssets();
                Debug.Log("[ARFlashcards] Android settings applied.");
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog("AR Flashcards",
                    "Could not apply every Android setting:\n" + e.Message +
                    "\n\nIn Unity Hub > Installs > your Unity version > Add modules: install 'Android Build Support' with 'OpenJDK' and 'Android SDK & NDK Tools'.", "OK");
            }
        }

        // ------------------------------------------------------------------ build
        static void BuildApk(bool run)
        {
            if (!File.Exists(ScenePath)) CreateScene();
            ApplyAndroidSettings();
            if (!VuforiaDefineManager.VuforiaPresent &&
                !EditorUtility.DisplayDialog("AR Flashcards",
                    "Vuforia is not installed, so the app will only show the 3D viewer.\n\nBuild anyway?", "Build anyway", "Cancel"))
                return;
            if (VuforiaDefineManager.VuforiaPresent && string.IsNullOrEmpty(ReadLicenseKey()) &&
                !EditorUtility.DisplayDialog("AR Flashcards",
                    "No Vuforia license key is saved. The app will start in 3D viewer mode only.\n\nBuild anyway?", "Build anyway", "Cancel"))
                return;

            Directory.CreateDirectory("Builds");
            var opts = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/ARFlashcards.apk",
                target = BuildTarget.Android,
                options = run ? BuildOptions.AutoRunPlayer : BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(opts);
            var summary = report.summary;
            if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log("[ARFlashcards] APK built: " + summary.outputPath + " (" + (summary.totalSize / 1048576) + " MB)");
                EditorUtility.RevealInFinder(summary.outputPath);
            }
            else
            {
                EditorUtility.DisplayDialog("AR Flashcards", "Build " + summary.result + ". See the Console for the first error.", "OK");
            }
        }
    }
}
#endif
