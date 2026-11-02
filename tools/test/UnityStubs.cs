// Minimal stand-ins for the Unity / Vuforia API surface used by the project, so the scripts can be
// compiled and unit-tested outside Unity (mcs / mono). They mirror real signatures; they do not render.
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine
{
    public sealed class RuntimeInitializeOnLoadMethodAttribute : Attribute { public RuntimeInitializeOnLoadMethodAttribute() { } public RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType t) { } }
    public enum RuntimeInitializeLoadType { AfterSceneLoad, SubsystemRegistration }
    public sealed class DefaultExecutionOrder : Attribute { public DefaultExecutionOrder(int o) { } }
    public sealed class TooltipAttribute : Attribute { public TooltipAttribute(string s) { } }

    public struct Vector2
    {
        public float x, y;
        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 zero { get { return new Vector2(0, 0); } }
        public static Vector2 one { get { return new Vector2(1, 1); } }
        public float magnitude { get { return (float)Math.Sqrt(x * x + y * y); } } public float sqrMagnitude { get { return x * x + y * y; } }
        public static Vector2 operator *(Vector2 a, float s) { return new Vector2(a.x * s, a.y * s); }
        public static Vector2 operator +(Vector2 a, Vector2 b) { return new Vector2(a.x + b.x, a.y + b.y); }
        public static Vector2 operator -(Vector2 a, Vector2 b) { return new Vector2(a.x - b.x, a.y - b.y); }
    }
    public struct Vector4 { public float x, y, z, w; public Vector4(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; } }
    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero { get { return new Vector3(0, 0, 0); } }
        public static Vector3 one { get { return new Vector3(1, 1, 1); } }
        public static Vector3 up { get { return new Vector3(0, 1, 0); } }
        public static Vector3 right { get { return new Vector3(1, 0, 0); } }
        public float magnitude { get { return (float)Math.Sqrt(x * x + y * y + z * z); } }
        public Vector3 normalized { get { float m = magnitude; return m > 1e-6f ? new Vector3(x / m, y / m, z / m) : zero; } }
        public static Vector3 operator +(Vector3 a, Vector3 b) { return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z); }
        public static Vector3 operator -(Vector3 a, Vector3 b) { return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z); }
        public static Vector3 operator *(Vector3 a, float s) { return new Vector3(a.x * s, a.y * s, a.z * s); }
        public static Vector3 operator *(float s, Vector3 a) { return a * s; }
        public static Vector3 Cross(Vector3 a, Vector3 b) { return new Vector3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x); }
        public static float Dot(Vector3 a, Vector3 b) { return a.x * b.x + a.y * b.y + a.z * b.z; }
    }
    public struct Rect { public float x, y, width, height; public bool Contains(Vector2 p) { return p.x >= x && p.x < x + width && p.y >= y && p.y < y + height; } public Rect(float x, float y, float w, float h) { this.x = x; this.y = y; width = w; height = h; }
        public float xMin { get { return x; } } public float yMin { get { return y; } } public float xMax { get { return x + width; } } public float yMax { get { return y + height; } } }
    public struct Quaternion
    {
        public float x, y, z, w;
        public Quaternion(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
        public static Quaternion identity { get { return new Quaternion(0, 0, 0, 1); } }
        public static Quaternion AngleAxis(float deg, Vector3 axis)
        {
            axis = axis.normalized; float r = deg * (float)Math.PI / 180f * 0.5f; float s = (float)Math.Sin(r);
            return new Quaternion(axis.x * s, axis.y * s, axis.z * s, (float)Math.Cos(r));
        }
        public static Quaternion Euler(float x, float y, float z) { return AngleAxis(y, new Vector3(0, 1, 0)) * AngleAxis(x, new Vector3(1, 0, 0)) * AngleAxis(z, new Vector3(0, 0, 1)); }
        public static Quaternion Euler(Vector3 v) { return Euler(v.x, v.y, v.z); }
        public static Quaternion FromToRotation(Vector3 a, Vector3 b)
        {
            a = a.normalized; b = b.normalized; float d = Vector3.Dot(a, b);
            if (d > 0.999999f) return identity;
            if (d < -0.999999f) return AngleAxis(180f, Math.Abs(a.x) < 0.9f ? new Vector3(1, 0, 0) : new Vector3(0, 0, 1));
            Vector3 c = Vector3.Cross(a, b);
            var q = new Quaternion(c.x, c.y, c.z, 1f + d); float m = (float)Math.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
            return new Quaternion(q.x / m, q.y / m, q.z / m, q.w / m);
        }
        public static Quaternion operator *(Quaternion a, Quaternion b)
        {
            return new Quaternion(a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y, a.w * b.y - a.x * b.z + a.y * b.w + a.z * b.x,
                                  a.w * b.z + a.x * b.y - a.y * b.x + a.z * b.w, a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z);
        }
    }
    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b) { this.r = r; this.g = g; this.b = b; a = 1; }
        public Color(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static Color white { get { return new Color(1, 1, 1, 1); } }
        public static Color gray { get { return new Color(.5f, .5f, .5f, 1); } }
        public static implicit operator Color32(Color c) { return new Color32((byte)(c.r * 255), (byte)(c.g * 255), (byte)(c.b * 255), (byte)(c.a * 255)); }
    }
    public struct Color32 { public byte r, g, b, a; public Color32(byte r, byte g, byte b, byte a) { this.r = r; this.g = g; this.b = b; this.a = a; } }
    public static class Mathf
    {
        public const float PI = 3.14159265f;
        public static float Sin(float f) { return (float)Math.Sin(f); } public static float Cos(float f) { return (float)Math.Cos(f); }
        public static float Sqrt(float f) { return (float)Math.Sqrt(f); } public static float Exp(float f) { return (float)Math.Exp(f); }
        public static float Abs(float f) { return Math.Abs(f); } public static float Clamp01(float f) { return f < 0 ? 0 : f > 1 ? 1 : f; }
        public static float Clamp(float f, float a, float b) { return f < a ? a : f > b ? b : f; }
        public static int Max(int a, int b) { return Math.Max(a, b); }
        public static float Min(float a, float b) { return Math.Min(a, b); } public static float Max(float a, float b) { return Math.Max(a, b); }
        public static float Min(float a, float b, float c) { return Math.Min(Math.Min(a, b), c); }
        public static int RoundToInt(float f) { return (int)Math.Round(f); }
        public static float Lerp(float a, float b, float t) { return a + (b - a) * Clamp01(t); }
    }
    public static class Time { public static float time, deltaTime = 0.016f, unscaledTime, realtimeSinceStartup; }
    public static class Screen { public static int width = 1080, height = 1920; public static Rect safeArea = new Rect(0, 0, 1080, 1920); public static int sleepTimeout; }
    public static class SleepTimeout { public const int NeverSleep = -1; }
    public static class Application { public static int targetFrameRate; public static void OpenURL(string u) { } }
    public static class Debug { public static void Log(object o) { } public static void LogWarning(object o) { Console.WriteLine("WARN " + o); } }
    public static class Random { static System.Random r = new System.Random(7); public static float value { get { return (float)r.NextDouble(); } } }
    public class WaitForSeconds { public WaitForSeconds(float s) { } }
    public enum PrimitiveType { Sphere, Capsule, Cylinder, Cube }
    public enum TextureFormat { RGBA32 }
    public enum FilterMode { Bilinear }
    public enum TextureWrapMode { Clamp }
    public enum CameraClearFlags { SolidColor }
    public enum LightType { Directional }
    public enum LightShadows { None }
    public enum FontStyle { Normal, Bold }
    public enum TextAnchor { UpperLeft, MiddleLeft, MiddleCenter, MiddleRight }
    public enum TextClipping { Overflow, Clip }
    public enum ScaleMode { StretchToFill, ScaleToFit }
    public enum EventType { MouseDown, MouseUp, MouseDrag, ScrollWheel, Repaint, Layout, Used }

    public class Object
    {
        public string name = "";
        public static void Destroy(Object o) { var c = o as Component; if (c != null) c.gameObject.components.Remove(c); var g = o as GameObject; if (g != null) g.destroyed = true; }
        public static T Instantiate<T>(T o) where T : Object { return o; }
        public static T FindFirstObjectByType<T>() where T : Object { return null; }
        public static implicit operator bool(Object o) { return !ReferenceEquals(o, null); }
        public static bool operator ==(Object a, Object b) { return ReferenceEquals(a, b); }
        public static bool operator !=(Object a, Object b) { return !ReferenceEquals(a, b); }
        public override bool Equals(object o) { return ReferenceEquals(this, o); }
        public override int GetHashCode() { return base.GetHashCode(); }
    }
    public class Component : Object
    {
        public GameObject gameObject;
        public Transform transform { get { return gameObject.transform; } }
        public T GetComponent<T>() where T : class { return gameObject.GetComponent<T>(); }
        public string tag { get { return gameObject.tag; } set { gameObject.tag = value; } }
    }
    public class Behaviour : Component { public bool enabled = true; }
    public class MonoBehaviour : Behaviour
    {
        public Coroutine StartCoroutine(IEnumerator e) { return null; }
        public void Invoke(string m, float t) { }
        public void CancelInvoke() { }
    }
    public class Coroutine { }
    public class Transform : Component
    {
        public Transform parent; public List<Transform> children = new List<Transform>();
        public Vector3 localPosition = Vector3.zero, localScale = Vector3.one; public Quaternion localRotation = Quaternion.identity;
        public Quaternion rotation { get { return localRotation; } set { localRotation = value; } }
        public void SetParent(Transform p, bool worldPositionStays) { if (parent != null) parent.children.Remove(this); parent = p; if (p != null) p.children.Add(this); }
        public void SetParent(Transform p) { SetParent(p, true); }
    }
    public class GameObject : Object
    {
        public List<Component> components = new List<Component>(); public bool destroyed; public string tag = "Untagged"; public int layer; bool active = true;
        public Transform transform { get; private set; }
        public bool activeSelf { get { return active; } }
        public GameObject() : this("GameObject") { }
        public GameObject(string n, params Type[] types)
        {
            name = n;
            transform = new Transform(); transform.gameObject = this; components.Add(transform);
            foreach (var t in types) if (t != typeof(Transform)) AddComponent(t);
        }
        public void SetActive(bool a) { active = a; }
        public Component AddComponent(Type t)
        {
            var c = (Component)Activator.CreateInstance(t); c.gameObject = this; components.Add(c);
            // Unity calls Awake then OnEnable immediately when a component is added to an active object
            foreach (var n in new[] { "Awake", "OnEnable" })
            { var m = t.GetMethod(n, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance); if (m != null) m.Invoke(c, null); }
            return c;
        }
        public T AddComponent<T>() where T : Component { return (T)AddComponent(typeof(T)); }
        public T GetComponent<T>() where T : class { foreach (var c in components) { var r = c as T; if (r != null) return r; } return null; }
        public static GameObject CreatePrimitive(PrimitiveType t)
        {
            var g = new GameObject(t.ToString()); g.AddComponent<MeshFilter>(); g.AddComponent<MeshRenderer>(); g.AddComponent<Collider>();
            g.GetComponent<MeshFilter>().sharedMesh = new Mesh { name = t.ToString() }; return g;
        }
    }
    public class Collider : Component { }
    public class Mesh : Object
    {
        public Vector3[] vertices; public int[] triangles;
        public void RecalculateNormals() { } public void RecalculateBounds() { }
    }
    public class MeshFilter : Component { public Mesh sharedMesh; }
    public class Renderer : Component
    {
        public Material sharedMaterial; public Rendering.ShadowCastingMode shadowCastingMode; public bool receiveShadows;
    }
    public class MeshRenderer : Renderer { }
    public class Shader : Object { public static Shader Find(string n) { return null; } }
    public class Material : Object
    {
        public Dictionary<string, Color> colors = new Dictionary<string, Color>();
        public Material(Material m) { if (m != null) name = m.name; } public Material(Shader s) { }
        public bool HasProperty(string p) { return true; }
        public void SetColor(string p, Color c) { colors[p] = c; } public void SetFloat(string p, float f) { }
    }
    public class Texture : Object { }
    public class Texture2D : Texture
    {
        public int width = 2, height = 2;
        public Texture2D(int w, int h) { width = w; height = h; }
        public Texture2D(int w, int h, TextureFormat f, bool mip) { width = w; height = h; }
        public FilterMode filterMode; public TextureWrapMode wrapMode;
        public bool LoadImage(byte[] b, bool nonReadable) { width = 64; height = 64; return b != null && b.Length > 8; }
        public void SetPixels32(Color32[] p) { } public void Apply(bool a, bool b) { }
    }
    public class TextAsset : Object { public byte[] bytes = new byte[100]; }
    public class AudioClip : Object { public static AudioClip Create(string n, int s, int c, int r, bool st) { return new AudioClip(); } public bool SetData(float[] d, int o) { return true; } }
    public class AudioSource : Behaviour { public bool playOnAwake; public float spatialBlend; public void PlayOneShot(AudioClip c, float v) { } }
    public class AudioListener : Behaviour { }
    public class Font : Object { public static Font CreateDynamicFontFromOSFont(string[] n, int s) { return new Font(); } }
    public class Light : Behaviour { public LightType type; public float intensity; public LightShadows shadows; }
    public static class RenderSettings { public static Rendering.AmbientMode ambientMode; public static Color ambientLight; }
    public class Camera : Behaviour
    {
        public static Camera main; public float nearClipPlane, farClipPlane; public CameraClearFlags clearFlags; public Color backgroundColor;
    }
    public static class Resources
    {
        public static T Load<T>(string path) where T : Object
        {
            string root = Environment.GetEnvironmentVariable("ARF_RES") ?? "";
            if (typeof(T) == typeof(TextAsset)) { string f = System.IO.Path.Combine(root, path + ".bytes"); return System.IO.File.Exists(f) ? (T)(object)new TextAsset { bytes = System.IO.File.ReadAllBytes(f) } : null; }
            return null;
        }
        public static T GetBuiltinResource<T>(string n) where T : Object { return (T)(object)new Font(); }
    }
    public struct Touch { public Vector2 position, deltaPosition; }
    public static class Input
    {
        public static int touchCount; public static Vector2 mousePosition; public static Vector2 mouseScrollDelta;
        public static Touch GetTouch(int i) { return new Touch(); } public static bool GetMouseButton(int i) { return false; } public static float GetAxis(string a) { return 0; }
    }
    public static class SystemInfoStub { }
}
namespace UnityEngine.Rendering { public enum ShadowCastingMode { Off } public enum AmbientMode { Flat } }

namespace UnityEngine
{
    public struct Matrix4x4 { public float sx; public static Matrix4x4 Scale(Vector3 v) { return new Matrix4x4 { sx = v.x }; } public static Matrix4x4 identity { get { return new Matrix4x4 { sx = 1 }; } } }
    public class RectOffset { public RectOffset(int l, int r, int t, int b) { } }
    public class GUIContent { public string text = ""; public static GUIContent none = new GUIContent(""); public GUIContent(string t) { text = t; } }
    public class GUIStyleState { public Texture2D background; public Color textColor; }
    public class GUIStyle
    {
        public GUIStyleState normal = new GUIStyleState(); public RectOffset border; public TextClipping clipping; public bool richText, wordWrap;
        public int fontSize; public FontStyle fontStyle; public TextAnchor alignment;
        public static GUIStyle none = new GUIStyle();
        public float CalcHeight(GUIContent c, float width)
        {   // rough: 0.55 em per glyph, 1.2 em line height
            float chars = c.text.Length * fontSize * 0.55f; int lines = Math.Max(1, (int)Math.Ceiling(chars / Math.Max(1f, width)));
            return lines * fontSize * 1.2f;
        }
    }
    public class Event
    {
        public static Event current = new Event();
        public EventType type = EventType.Repaint; public Vector2 mousePosition, delta;
    }
    public static class GUI
    {
        public static Matrix4x4 matrix = Matrix4x4.identity; public static Color color = Color.white;
        public static int boxes, buttons, labels, textures; public static Vector2? clickAt; public static List<string> labelLog = new List<string>();
        public static float lastBoxMaxX, lastBoxMaxY;
        public static void Reset() { boxes = buttons = labels = textures = 0; labelLog.Clear(); lastBoxMaxX = lastBoxMaxY = 0; }
        public static void Box(Rect r, GUIContent c, GUIStyle s) { boxes++; lastBoxMaxX = Math.Max(lastBoxMaxX, r.xMax); lastBoxMaxY = Math.Max(lastBoxMaxY, r.yMax); }
        public static void Label(Rect r, string t, GUIStyle s) { labels++; labelLog.Add(t); }
        public static void DrawTexture(Rect r, Texture t, ScaleMode m, bool a) { textures++; }
        public static bool Button(Rect r, GUIContent c, GUIStyle s)
        {
            buttons++;
            if (clickAt.HasValue && r.Contains(clickAt.Value)) { clickAt = null; return true; }
            return false;
        }
    }
}
namespace UnityEngine.Rendering { public static class GraphicsSettings { public static object currentRenderPipeline; } }
