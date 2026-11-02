using System;
namespace UnityEngine
{
    public class AndroidJavaProxy { public AndroidJavaProxy(string iface) { } }
    public class AndroidJavaObject : IDisposable
    {
        public AndroidJavaObject(string cls, params object[] a) { }
        public T Call<T>(string m, params object[] a) { return default(T); }
        public T GetStatic<T>(string f) { return default(T); }
        public void Dispose() { }
    }
    public class AndroidJavaClass : AndroidJavaObject { public AndroidJavaClass(string c) : base(c) { } }
}
namespace UnityEngine.Android { public static class Permission { public const string Camera = "android.permission.CAMERA"; public static bool HasUserAuthorizedPermission(string p) { return true; } } }
