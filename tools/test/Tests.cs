using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using ARFlashcards;
using UnityEngine;

static class T
{
    static int pass, fail;
    public static void Ok(bool c, string name)
    {
        if (c) { pass++; Console.WriteLine("  PASS  " + name); }
        else { fail++; Console.WriteLine("  FAIL  " + name); }
    }
    public static int Done() { Console.WriteLine("\n" + pass + " passed, " + fail + " failed"); return fail == 0 ? 0 : 1; }
}

static class Program
{
    static object Field(object o, string n) { return o.GetType().GetField(n, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).GetValue(o); }

    static int Main(string[] args)
    {
        string exportDir = args.Length > 0 ? args[0] : null;

        Console.WriteLine("[CardLibrary & resources]");
        T.Ok(CardLibrary.All.Count == 5, "5 cards in the deck");
        var ids = new HashSet<string>();
        bool textOk = true;
        foreach (var c in CardLibrary.All)
        {
            ids.Add(c.Id);
            for (int i = 0; i < 3; i++) if (string.IsNullOrEmpty(c.Names[i]) || string.IsNullOrEmpty(c.Facts[i])) textOk = false;
        }
        T.Ok(ids.Count == 5, "card ids unique");
        T.Ok(textOk, "every card has EN/HI/TA name and fact");
        bool res = true;
        foreach (var c in CardLibrary.All)
        {
            if (Resources.Load<TextAsset>("ARFlashcards/Cards/" + c.Id) == null) { res = false; Console.WriteLine("   missing card " + c.Id); }
            foreach (var l in new[] { "hi", "ta" }) foreach (var k in new[] { "name", "fact" })
                if (Resources.Load<TextAsset>("ARFlashcards/Text/" + c.Id + "_" + l + "_" + k) == null) { res = false; Console.WriteLine("   missing text " + c.Id + l + k); }
        }
        T.Ok(res, "card images + Hindi/Tamil text images present in Resources");

        Console.WriteLine("[FlashcardSession]");
        var s = new FlashcardSession();
        int curEvents = 0, progEvents = 0; s.CurrentChanged += x => curEvents++; s.ProgressChanged += () => progEvents++;
        var lion = CardLibrary.Find("lion"); var dog = CardLibrary.Find("dog");
        T.Ok(s.Current == null && s.Scanned == 0 && s.Total == 5, "starts empty");
        s.Show(lion);
        T.Ok(s.Current == lion && s.Scanned == 1 && progEvents == 1, "show lion -> current + count 1");
        s.Hide(lion); s.Show(lion);
        T.Ok(s.Scanned == 1 && progEvents == 1, "re-scanning same card does not double count");
        s.Show(dog);
        T.Ok(s.Current == dog && s.Scanned == 2, "newest visible card wins");
        s.Hide(dog);
        T.Ok(s.Current == lion, "hiding newest falls back to older visible card");
        s.Hide(lion);
        T.Ok(s.Current == null, "all hidden -> no current");
        foreach (var c in CardLibrary.All) s.Show(c);
        T.Ok(s.Complete, "scanning all 5 marks session complete");
        s.ClearVisible(); s.ResetProgress();
        T.Ok(s.Current == null && s.Scanned == 0, "clear + reset");

        Console.WriteLine("[QuizRound]");
        var rng = new System.Random(42);
        int firstPos = 0, rounds = 300; bool uniq = true, hasAns = true, size3 = true;
        for (int i = 0; i < rounds; i++)
        {
            var q = QuizRound.Create(lion, CardLibrary.All, rng, 3);
            if (q.Options.Count != 3) size3 = false;
            if (!q.Options.Contains(lion)) hasAns = false;
            if (new HashSet<Card>(q.Options).Count != q.Options.Count) uniq = false;
            if (q.Options[0] == lion) firstPos++;
        }
        T.Ok(size3 && hasAns && uniq, "3 unique options, always containing the answer");
        T.Ok(firstPos > rounds * 0.2 && firstPos < rounds * 0.5, "answer position is shuffled (" + firstPos + "/" + rounds + " in slot 0)");
        var r1 = QuizRound.Create(lion, CardLibrary.All, rng, 3);
        int ai = r1.Options.IndexOf(lion), wi = ai == 0 ? 1 : 0;
        T.Ok(!r1.Guess(wi) && r1.WrongGuesses == 1 && !r1.Solved, "wrong guess recorded");
        T.Ok(!r1.Guess(wi) && r1.WrongGuesses == 1, "same wrong option not counted twice");
        T.Ok(r1.Guess(ai) && r1.Solved, "correct guess solves round");
        T.Ok(!r1.Guess(wi), "no guesses after solved");
        var few = QuizRound.Create(lion, new List<Card> { lion, dog }, rng, 3);
        T.Ok(few.Options.Count == 2, "small pool degrades gracefully");

        Console.WriteLine("[Loc]");
        int changed = 0; Loc.Changed += () => changed++;
        T.Ok(Loc.Current == Lang.English && Loc.Code == "EN", "default English");
        Loc.Cycle(); T.Ok(Loc.Current == Lang.Hindi && Loc.Code == "HI", "cycle -> Hindi");
        Loc.Cycle(); T.Ok(Loc.Current == Lang.Tamil, "cycle -> Tamil");
        Loc.Cycle(); T.Ok(Loc.Current == Lang.English && changed == 3, "cycle -> English (3 change events)");
        T.Ok(Loc.TextImage("lion", Lang.English, "name") == null, "English uses real text (no image)");
        T.Ok(Loc.TextImage("lion", Lang.Hindi, "fact") != null && Loc.TextImage("cat", Lang.Tamil, "name") != null, "Hindi/Tamil text images load");

        Console.WriteLine("[MeshKit cone winding]");
        var cone = MeshKit.Cone();
        bool outward = true, downward = true; int sideTris = 0, baseTris = 0;
        for (int i = 0; i < cone.triangles.Length; i += 3)
        {
            Vector3 a = cone.vertices[cone.triangles[i]], b = cone.vertices[cone.triangles[i + 1]], c = cone.vertices[cone.triangles[i + 2]];
            Vector3 n = Vector3.Cross(b - a, c - a);   // Unity: front face = clockwise, normal = cross(v1-v0, v2-v0)
            Vector3 centre = new Vector3((a.x + b.x + c.x) / 3f, (a.y + b.y + c.y) / 3f, (a.z + b.z + c.z) / 3f);
            if (a.y == 0 && b.y == 0 && c.y == 0) { baseTris++; if (n.y >= 0) downward = false; }
            else { sideTris++; if (n.x * centre.x + n.z * centre.z <= 0) outward = false; }
        }
        T.Ok(sideTris == 20 && baseTris == 20, "cone has 20 side + 20 base triangles");
        T.Ok(outward, "side faces point outward");
        T.Ok(downward, "base faces point down");

        Console.WriteLine("[AnimalFactory]");
        var allNodes = new Dictionary<string, List<GameObject>>();
        foreach (var c in CardLibrary.All)
        {
            var g = AnimalFactory.Build(c.Id);
            var list = new List<GameObject>(); Collect(g.transform, list);
            allNodes[c.Id] = list;
            var an = g.GetComponent<ProceduralAnimator>();
            T.Ok(list.Count >= 24, c.Id + ": " + list.Count + " parts");
            T.Ok(an != null && an.head != null && an.tail != null && an.torso != null, c.Id + ": animator wired (head/tail/torso)");
            bool finite = true, hasRenderer = true;
            foreach (var n in list)
            {
                var t = n.transform;
                if (float.IsNaN(t.localPosition.x + t.localPosition.y + t.localPosition.z + t.localScale.x + t.localScale.y + t.localScale.z + t.localRotation.w)) finite = false;
                var mf = n.GetComponent<MeshFilter>();
                if (mf != null && n.GetComponent<MeshRenderer>() != null && n.GetComponent<MeshRenderer>().sharedMaterial == null) hasRenderer = false;
            }
            T.Ok(finite && hasRenderer, c.Id + ": finite transforms, every mesh has a material");
            T.Ok(g.GetComponent<Collider>() == null, c.Id + ": root has no collider");
            if (exportDir != null) Export(exportDir, c.Id, g);
        }

#if ARF_VUFORIA
        Console.WriteLine("[App flow with simulated Vuforia]");
        // success path
        var go = new GameObject("app");
        var app = go.AddComponent<ARFlashcardsApp>();   // Awake runs on AddComponent, as in Unity
        T.Ok(Enum.GetName(Field(app, "mode").GetType(), Field(app, "mode")) == "Starting", "starts in Starting mode");
        Vuforia.VuforiaApplication.Instance.RaiseInit(Vuforia.VuforiaInitError.NONE);
        Vuforia.VuforiaApplication.Instance.RaiseStarted();
        T.Ok(Vuforia.ObserverFactory.Created.Count == 5, "5 runtime image targets created (one per card)");
        T.Ok((bool)Field(app, "targetsReady") && Field(app, "mode").ToString() == "AR", "AR mode active after Vuforia started");
        var tcs = new List<TrackedCard>();
        foreach (var itb in Vuforia.ObserverFactory.Created) tcs.Add(itb.GetComponent<TrackedCard>());
        T.Ok(tcs.TrueForAll(x => x != null && x.model != null && !x.model.activeSelf), "each target has a TrackedCard with its hidden model");
        var session = (FlashcardSession)Field(app, "session");
        Vuforia.ObserverBehaviour lionObs = Vuforia.ObserverFactory.Created[0];
        lionObs.Fire(Vuforia.Status.TRACKED);
        T.Ok(session.Current != null && session.Current.Id == "lion" && tcs[0].model.activeSelf, "tracking lion card -> model shown, session current = lion");
        Vuforia.ObserverFactory.Created[2].Fire(Vuforia.Status.EXTENDED_TRACKED);
        T.Ok(session.Current.Id == "dog" && session.Scanned == 2, "second card (extended tracking) -> dog, counter 2");
        Vuforia.ObserverFactory.Created[2].Fire(Vuforia.Status.NO_POSE);
        T.Ok(session.Current.Id == "lion" && !tcs[2].model.activeSelf, "losing dog falls back to lion, dog model hidden");
        lionObs.Fire(Vuforia.Status.NO_POSE);
        T.Ok(session.Current == null, "all lost -> no card");

        // mode toggle to viewer and back
        Call(app, "OnModeToggle");
        T.Ok(Field(app, "mode").ToString() == "Viewer" && session.Current != null, "mode button -> 3D viewer with a card selected");
        Call(app, "OnCardPicked", CardLibrary.Find("horse"));
        T.Ok(session.Current.Id == "horse", "viewer: picking horse selects it");
        Call(app, "OnModeToggle");
        T.Ok(Field(app, "mode").ToString() == "AR", "mode button -> back to AR");

        // failure path: license error (fresh app instance, as after a scene reload)
        Call(app, "OnDestroy");
        T.Ok(true, "first app destroyed / unhooked");
        Vuforia.ObserverFactory.Created.Clear();
        var app2 = new GameObject("app2").AddComponent<ARFlashcardsApp>();
        Vuforia.VuforiaApplication.Instance.RaiseInit(Vuforia.VuforiaInitError.LICENSE_ERROR);
        T.Ok(Field(app2, "mode").ToString() == "Viewer", "license error -> falls back to 3D viewer (never an empty screen)");
        T.Ok(((string)Field(app2, "vuforiaProblem")).ToLower().Contains("license"), "problem text mentions the license");
        T.Ok(((FlashcardSession)Field(app2, "session")).Current != null, "viewer shows an animal immediately");

#else
        Console.WriteLine("[App flow WITHOUT Vuforia package]");
        var go0 = new GameObject("app");
        var app0 = go0.AddComponent<ARFlashcardsApp>();
        T.Ok(Field(app0, "mode").ToString() == "Viewer", "no Vuforia package -> 3D viewer immediately (never an empty screen)");
        T.Ok(((string)Field(app0, "vuforiaProblem")).Contains("Setup Wizard"), "message points to the Setup Wizard");
        var sess0 = (FlashcardSession)Field(app0, "session");
        T.Ok(sess0.Current != null, "viewer shows an animal immediately");
        Call(app0, "OnCardPicked", CardLibrary.Find("horse"));
        T.Ok(sess0.Current.Id == "horse", "viewer: picking horse selects it");
        Call(app0, "OnModeToggle");
        T.Ok(Field(app0, "mode").ToString() == "Viewer", "AR toggle without Vuforia stays in viewer (and shows a message)");
        Call(app0, "OnSpeak", CardLibrary.Find("cat"));
        T.Ok(true, "speak does not crash outside Android");
        var ui0 = (UiController)Field(app0, "ui");
        ModelInteractionTests(app0);
        UiTests(ui0, sess0);
#endif

        return T.Done();
    }


    static void ModelInteractionTests(object app)
    {
        Console.WriteLine("[ModelInteraction]");
        var m = new GameObject("m").AddComponent<ModelInteraction>();
        m.baseScale = 0.2f;
        for (int i = 0; i < 40; i++) Call(m, "Update");
        T.Ok(Math.Abs(m.transform.localScale.x - 0.2f) < 0.02f, "pop-in finishes at base scale");
        ModelInteraction.RequestZoom(2f);
        for (int i = 0; i < 2; i++) Call(m, "Update");
        T.Ok(Math.Abs(m.transform.localScale.x - 0.4f) < 0.03f, "+ button doubles size");
        for (int i = 0; i < 10; i++) ModelInteraction.RequestZoom(2f);
        Call(m, "Update");
        T.Ok(m.transform.localScale.x <= 0.2f * 2.5f + 0.001f, "zoom is clamped to 2.5x");
    }

    static void UiTests(UiController ui, FlashcardSession sess)
    {
        Console.WriteLine("[IMGUI user interface]");
        Type ut = typeof(UiController);
        MethodInfo onGui = ut.GetMethod("OnGUI", BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var size in new[] { new[] { 1080, 1920 }, new[] { 720, 1600 }, new[] { 1440, 3120 }, new[] { 1920, 1080 } })
        {
            Screen.width = size[0]; Screen.height = size[1]; Screen.safeArea = new Rect(0, 0, size[0], size[1] - 80);
            foreach (var lang in new[] { Lang.English, Lang.Hindi, Lang.Tamil })
            {
                Loc.Current = lang;
                GUI.Reset(); onGui.Invoke(ui, null);
                if (GUI.boxes < 5 || GUI.buttons < 4) { T.Ok(false, "draws on " + size[0] + "x" + size[1] + " " + lang); }
            }
        }
        T.Ok(true, "OnGUI runs on 4 screen sizes x 3 languages without exceptions");
        Screen.width = 1080; Screen.height = 1920; Screen.safeArea = new Rect(0, 0, 1080, 1920); Loc.Current = Lang.English;
        GUI.Reset(); onGui.Invoke(ui, null);
        T.Ok(GUI.labelLog.Contains("AR Flashcards") && GUI.labelLog.Contains("3D VIEW"), "title + mode pill drawn");
        T.Ok(GUI.labelLog.Contains("SPEAK") && GUI.labelLog.Contains("QUIZ") && GUI.labelLog.Contains("NEXT"), "Speak / Quiz / Next buttons drawn in viewer mode");
        T.Ok(GUI.labelLog.Contains("+") && GUI.labelLog.Contains("-"), "zoom buttons drawn");
        T.Ok(GUI.lastBoxMaxX <= 1080.5f * 1f, "nothing drawn outside the screen width");

        // clicking Quiz opens the quiz, a wrong then right answer scores
        Card cur = sess.Current;
        Quiz(ui, onGui, cur);
    }

    static void Quiz(UiController ui, MethodInfo onGui, Card cur)
    {
        // find QUIZ button by scanning clicks over a grid (button rects are internal)
        bool opened = false;
        for (int y = 900; y < 1900 && !opened; y += 20)
            for (int x = 300; x < 800 && !opened; x += 40)
            {
                GUI.Reset(); GUI.clickAt = new Vector2(x, y); onGui.Invoke(ui, null); GUI.clickAt = null;
                var q = ut_quiz(ui);
                if (q != null) opened = true;
            }
        T.Ok(opened, "clicking QUIZ opens a question");
        if (!opened) return;
        GUI.Reset(); onGui.Invoke(ui, null);
        T.Ok(GUI.labelLog.Contains("Which animal is this?"), "quiz title drawn");
        var round = ut_quiz(ui);
        T.Ok(round.Answer == cur && round.Options.Count == 4, "quiz asks about the shown animal with 4 options");
        bool solved = false;
        for (int y = 1450; y < 1900 && !solved; y += 25)
            for (int x = 60; x < 1020 && !solved; x += 60)
            {
                GUI.Reset(); GUI.clickAt = new Vector2(x, y); onGui.Invoke(ui, null); GUI.clickAt = null;
                if (round.Solved) solved = true;
            }
        if (!solved) { Console.WriteLine("   wrong=" + round.WrongGuesses + " opts=" + round.Options.Count + " labels=" + string.Join(",", GUI.labelLog)); }
        T.Ok(solved, "tapping options eventually finds the right answer");
    }

    static QuizRound ut_quiz(UiController ui) { return (QuizRound)typeof(UiController).GetField("quiz", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ui); }

    static void Call(object o, string m, params object[] a)
    {
        var mi = o.GetType().GetMethod(m, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        mi.Invoke(o, a);
    }

    static void Collect(Transform t, List<GameObject> list)
    {
        foreach (var c in t.children) { list.Add(c.gameObject); Collect(c, list); }
    }

    static string F(float f) { return f.ToString("R", CultureInfo.InvariantCulture); }

    static void Export(string dir, string id, GameObject root)
    {
        var sb = new StringBuilder("[");
        var list = new List<Transform>(); var stack = new List<Transform>();
        Action<Transform> walk = null;
        walk = t => { list.Add(t); foreach (var c in t.children) walk(c); };
        walk(root.transform);
        for (int i = 0; i < list.Count; i++)
        {
            var t = list[i]; var g = t.gameObject;
            var mf = g.GetComponent<MeshFilter>(); var mr = g.GetComponent<MeshRenderer>();
            string shape = mf != null && mf.sharedMesh != null ? mf.sharedMesh.name : "";
            Color col = Color.white; if (mr != null && mr.sharedMaterial != null && mr.sharedMaterial.colors.ContainsKey("_BaseColor")) col = mr.sharedMaterial.colors["_BaseColor"];
            if (i > 0) sb.Append(",");
            sb.Append("{\"name\":\"" + g.name + "\",\"parent\":" + (t.parent == null ? -1 : list.IndexOf(t.parent)) + ",\"shape\":\"" + shape + "\"");
            sb.Append(",\"pos\":[" + F(t.localPosition.x) + "," + F(t.localPosition.y) + "," + F(t.localPosition.z) + "]");
            sb.Append(",\"rot\":[" + F(t.localRotation.x) + "," + F(t.localRotation.y) + "," + F(t.localRotation.z) + "," + F(t.localRotation.w) + "]");
            sb.Append(",\"scale\":[" + F(t.localScale.x) + "," + F(t.localScale.y) + "," + F(t.localScale.z) + "]");
            sb.Append(",\"color\":[" + F(col.r) + "," + F(col.g) + "," + F(col.b) + "]}");
        }
        sb.Append("]");
        File.WriteAllText(Path.Combine(dir, id + ".json"), sb.ToString());
    }
}
