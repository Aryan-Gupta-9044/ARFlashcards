#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;

namespace ARFlashcards.EditorTools
{
    /// <summary>
    /// Keeps the scripting define ARF_VUFORIA in sync with whether the Vuforia package is installed.
    /// Installed   -> define is added  -> the AR code in VuforiaBridge / TrackedCard is compiled.
    /// Not present -> define is removed -> the project still compiles and runs in 3D-viewer mode.
    /// Nothing for the user to do: this runs by itself after every script reload.
    /// </summary>
    [InitializeOnLoad]
    public static class VuforiaDefineManager
    {
        public const string Define = "ARF_VUFORIA";

        static VuforiaDefineManager()
        {
            EditorApplication.delayCall += Sync;
        }

        /// <summary>True when the Vuforia Engine assembly is loaded in the editor.</summary>
        public static bool VuforiaPresent
        {
            get { return FindType("Vuforia.VuforiaBehaviour") != null; }
        }

        public static Type FindType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t = null;
                try { t = asm.GetType(fullName, false); } catch (Exception) { }
                if (t != null) return t;
            }
            return null;
        }

        public static bool DefineSet
        {
            get { return Has(NamedBuildTarget.Standalone) || Has(NamedBuildTarget.Android); }
        }

        static bool Has(NamedBuildTarget t)
        {
            string s = PlayerSettings.GetScriptingDefineSymbols(t);
            return new List<string>(s.Split(';')).Contains(Define);
        }

        [MenuItem("AR Flashcards/Refresh Vuforia detection")]
        public static void Sync()
        {
            bool want = VuforiaPresent;
            Apply(NamedBuildTarget.Standalone, want);
            Apply(NamedBuildTarget.Android, want);
        }

        static void Apply(NamedBuildTarget target, bool want)
        {
            try
            {
                string current = PlayerSettings.GetScriptingDefineSymbols(target);
                var list = new List<string>();
                foreach (var d in current.Split(';')) if (d.Trim().Length > 0) list.Add(d.Trim());
                bool has = list.Contains(Define);
                if (want == has) return;
                if (want) list.Add(Define); else list.Remove(Define);
                PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", list.ToArray()));
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning("[ARFlashcards] Could not update scripting defines: " + e.Message);
            }
        }
    }
}
#endif
