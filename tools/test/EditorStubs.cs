using System;
using System.Collections.Generic;
using UnityEngine;
namespace UnityEngine.SceneManagement { public struct Scene { } }
namespace UnityEngine { public enum UIOrientation { Portrait } }
namespace UnityEngine.Rendering { public enum GraphicsDeviceType { OpenGLES3 } }
namespace UnityEditor
{
    public sealed class MenuItem : Attribute { public MenuItem(string s) { } }
    public sealed class InitializeOnLoadAttribute : Attribute { }
    public class EditorStyles { public static GUIStyle boldLabel; }
    public static class GUILayout { public static void Label(string s, GUIStyle st) { } public static bool Button(string s) { return false; } public static void Space(float f) { } }
    public enum MessageType { None, Info, Warning }
    public static class EditorGUILayout
    {
        public static void HelpBox(string s, MessageType t) { } public static string TextField(string l, string v) { return v; }
        public static void LabelField(string s) { } public static Vector2 BeginScrollView(Vector2 v) { return v; } public static void EndScrollView() { }
    }
    public static class EditorGUI { public class DisabledScope : IDisposable { public DisabledScope(bool b) { } public void Dispose() { } } }
    public static class EditorApplication { public static event Action update; public static Action delayCall; public static bool isCompiling, isUpdating; }
    public static class SessionState { public static bool GetBool(string k, bool d) { return d; } public static void SetBool(string k, bool v) { } }
    public class ScriptableObjectStub : UnityEngine.Object { }
    public class EditorWindow : ScriptableObjectStub { public Vector2 minSize; public static T GetWindow<T>(string t) where T : EditorWindow, new() { return new T(); } public void Show() { } public void Repaint() { } }
    public static class EditorUtility
    {
        public static bool DisplayDialog(string a, string b, string c) { return true; } public static bool DisplayDialog(string a, string b, string c, string d) { return true; }
        public static void RevealInFinder(string p) { } public static void SetDirty(UnityEngine.Object o) { }
    }
    public static class AssetDatabase
    {
        public static void ImportAsset(string p, ImportAssetOptions o) { } public static void SaveAssets() { } public static void Refresh() { }
        public static UnityEngine.Object[] LoadAllAssetsAtPath(string p) { return new UnityEngine.Object[0]; }
    }
    public enum ImportAssetOptions { ForceUpdate }
    public class SerializedProperty { public int arraySize; public bool isArray; public UnityEngine.Object objectReferenceValue; public SerializedProperty GetArrayElementAtIndex(int i) { return this; } }
    public class SerializedObject { public SerializedObject(UnityEngine.Object o) { } public SerializedProperty FindProperty(string n) { return new SerializedProperty(); } public bool ApplyModifiedPropertiesWithoutUndo() { return true; } }
    public enum BuildTarget { Android, StandaloneWindows64 }
    public enum BuildTargetGroup { Android }
    public enum AndroidArchitecture { ARM64 }
    public enum AndroidSdkVersions { AndroidApiLevel29 }
    public enum ScriptingImplementation { IL2CPP }
    public enum ManagedStrippingLevel { Low }
    public enum BuildOptions { None, AutoRunPlayer }
    public static class EditorUserBuildSettings { public static BuildTarget activeBuildTarget; public static bool buildAppBundle; public static bool SwitchActiveBuildTarget(BuildTargetGroup g, BuildTarget t) { return true; } }
    public static class PlayerSettings
    {
        public static string companyName, productName; public static UIOrientation defaultInterfaceOrientation;
        public static void SetApplicationIdentifier(Build.NamedBuildTarget t, string id) { }
        public static void SetScriptingBackend(Build.NamedBuildTarget t, ScriptingImplementation s) { }
        public static void SetManagedStrippingLevel(Build.NamedBuildTarget t, ManagedStrippingLevel l) { }
        public static void SetUseDefaultGraphicsAPIs(BuildTarget t, bool b) { }
        public static void SetGraphicsAPIs(BuildTarget t, UnityEngine.Rendering.GraphicsDeviceType[] a) { }
        public static Dictionary<int, string> Defines = new Dictionary<int, string>();
        public static string GetScriptingDefineSymbols(Build.NamedBuildTarget t) { string s; return Defines.TryGetValue(t.id, out s) ? s : ""; }
        public static void SetScriptingDefineSymbols(Build.NamedBuildTarget t, string d) { Defines[t.id] = d; }
        public static class Android { public static AndroidArchitecture targetArchitectures; public static AndroidSdkVersions minSdkVersion; }
    }
    public struct EditorBuildSettingsScene { public EditorBuildSettingsScene(string p, bool e) { } }
    public static class EditorBuildSettings { public static EditorBuildSettingsScene[] scenes; }
    public struct BuildPlayerOptions { public string[] scenes; public string locationPathName; public BuildTarget target; public BuildOptions options; }
    public static class BuildPipeline { public static Build.Reporting.BuildReport BuildPlayer(BuildPlayerOptions o) { return null; } }
}
namespace UnityEditor.Build
{
    public struct NamedBuildTarget { public int id; public static NamedBuildTarget Android = new NamedBuildTarget { id = 1 }; public static NamedBuildTarget Standalone = new NamedBuildTarget { id = 2 }; }
}
namespace UnityEditor.Build.Reporting
{
    public enum BuildResult { Succeeded }
    public class BuildSummary { public BuildResult result; public string outputPath; public ulong totalSize; }
    public class BuildReport { public BuildSummary summary; }
}
namespace UnityEditor.PackageManager
{
    public enum StatusCode { InProgress, Success, Failure }
    public class Error { public string message; }
    public class PackageInfo { public string version; }
    public static class Client { public static Requests.AddRequest Add(string id) { return new Requests.AddRequest(); } public static object Resolve() { return null; } }
}
namespace UnityEditor.PackageManager.Requests
{
    public class AddRequest { public bool IsCompleted = true; public StatusCode Status = StatusCode.Success; public PackageInfo Result = new PackageInfo(); public Error Error; }
}
namespace UnityEditor.SceneManagement
{
    public enum NewSceneSetup { EmptyScene } public enum NewSceneMode { Single }
    public static class EditorSceneManager
    {
        public static bool SaveCurrentModifiedScenesIfUserWantsTo() { return true; }
        public static UnityEngine.SceneManagement.Scene NewScene(NewSceneSetup s, NewSceneMode m) { return default(UnityEngine.SceneManagement.Scene); }
        public static bool SaveScene(UnityEngine.SceneManagement.Scene s, string p) { return true; }
    }
}
namespace UnityEditor.Android { public interface IPostGenerateGradleAndroidProject { int callbackOrder { get; } void OnPostGenerateGradleAndroidProject(string path); } }
