#if UNITY_EDITOR && UNITY_ANDROID
using System;
using System.IO;
using System.Xml;
using UnityEditor.Android;
using UnityEngine;

namespace ARFlashcards.EditorTools
{
    /// <summary>
    /// Android 11+ hides other apps from yours unless the manifest has a &lt;queries&gt; entry. Without it
    /// the "Speak" button (Android text-to-speech) cannot find the phone's voice engine.
    /// This adds the entry to the generated manifest; if anything goes wrong it only logs a warning.
    /// </summary>
    public sealed class AndroidManifestPostprocessor : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder { get { return 100; } }

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            try
            {
                string manifestPath = Path.Combine(path, "src", "main", "AndroidManifest.xml");
                if (!File.Exists(manifestPath)) return;
                var doc = new XmlDocument();
                doc.Load(manifestPath);
                XmlElement manifest = doc.DocumentElement;
                if (manifest == null) return;
                foreach (XmlNode n in manifest.ChildNodes)
                    if (n.Name == "queries" && n.OuterXml.Contains("TTS_SERVICE")) return;

                const string androidNs = "http://schemas.android.com/apk/res/android";
                XmlElement queries = doc.CreateElement("queries");
                XmlElement intent = doc.CreateElement("intent");
                XmlElement action = doc.CreateElement("action");
                XmlAttribute name = doc.CreateAttribute("android", "name", androidNs);
                name.Value = "android.intent.action.TTS_SERVICE";
                action.Attributes.Append(name);
                intent.AppendChild(action);
                queries.AppendChild(intent);
                manifest.AppendChild(queries);
                doc.Save(manifestPath);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ARFlashcards] Could not add TTS <queries> to the manifest (voice may not work): " + e.Message);
            }
        }
    }
}
#endif
