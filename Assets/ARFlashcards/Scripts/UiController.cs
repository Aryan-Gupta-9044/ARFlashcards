using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARFlashcards
{
    public enum StatusKind { Info, Scanning, Tracking, Error }

    /// <summary>
    /// Whole user interface drawn with Unity's built-in IMGUI (OnGUI). No uGUI / TextMeshPro / EventSystem /
    /// Input System package is needed, so it compiles in any Unity project. Layout uses a virtual 1080-unit
    /// wide canvas that is scaled to the screen, plus safe-area insets for notches.
    /// </summary>
    public sealed class UiController : MonoBehaviour
    {
        public event Action ModeToggleClicked;
        public event Action<Card> CardPicked;
        public event Action<Card> SpeakClicked;
        public event Action StartARClicked;
        public event Action ViewerClicked;
        public event Action HomeClicked;

        FlashcardSession session;
        AudioFx audio;
        bool viewerMode;

        string statusText = "";
        StatusKind statusKind = StatusKind.Info;
        string toastText;
        float toastUntil;

        // quiz
        QuizGame quizGame;
        List<string> quizOptions;
        bool journalOpen;
        bool homeVisible = true;
        bool cameraLoading;
        bool exitConfirm;
        bool abandonConfirm;
        readonly System.Random rng = new System.Random();

        // gfx
        bool gfxReady;
        Texture2D roundTex, circleTex;
        GUIStyle boxStyle, textStyle;
        readonly Dictionary<string, int> fitCache = new Dictionary<string, int>();

        static readonly Color Panel = new Color(0.06f, 0.08f, 0.16f, 0.90f);
        static readonly Color PanelLight = new Color(0.14f, 0.18f, 0.32f, 0.95f);
        static readonly Color Blue = new Color(0.25f, 0.55f, 1f, 1f);
        static readonly Color Green = new Color(0.25f, 0.78f, 0.45f, 1f);
        static readonly Color Red = new Color(0.92f, 0.32f, 0.32f, 1f);
        static readonly Color Amber = new Color(1f, 0.74f, 0.2f, 1f);
        static readonly Color White = Color.white;
        static readonly Color Muted = new Color(0.78f, 0.82f, 0.92f, 1f);

        // ------------------------------------------------------------------ API
        public void Init(FlashcardSession s, AudioFx fx)
        {
            session = s; audio = fx;
        }

        public void SetViewerMode(bool v) { viewerMode = v; }
        public void SetHome(bool visible) { homeVisible = visible; }
        public void SetCameraLoading(bool loading) { cameraLoading = loading; }

        public void SetStatus(string text, StatusKind kind) { statusText = text ?? ""; statusKind = kind; }

        public void Toast(string text) { Toast(text, 2.4f); }
        public void Toast(string text, float seconds) { toastText = text; toastUntil = Time.unscaledTime + seconds; }

        // ------------------------------------------------------------------ gfx helpers
        static Texture2D MakeRounded(int size, float radius)
        {
            var t = new Texture2D(size, size, TextureFormat.RGBA32, false);
            t.filterMode = FilterMode.Bilinear;
            t.wrapMode = TextureWrapMode.Clamp;
            var px = new Color32[size * size];
            float half = size * 0.5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x + 0.5f - half) - (half - radius);
                    float dy = Mathf.Abs(y + 0.5f - half) - (half - radius);
                    float d = Mathf.Sqrt(Mathf.Max(dx, 0f) * Mathf.Max(dx, 0f) + Mathf.Max(dy, 0f) * Mathf.Max(dy, 0f)) - radius;
                    float a = Mathf.Clamp01(0.5f - d);
                    px[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(a * 255f));
                }
            t.SetPixels32(px);
            t.Apply(false, false);
            return t;
        }

        void EnsureGfx()
        {
            if (gfxReady) return;
            roundTex = MakeRounded(64, 28f);
            circleTex = MakeRounded(32, 16f);
            boxStyle = new GUIStyle();
            boxStyle.normal.background = roundTex;
            boxStyle.border = new RectOffset(28, 28, 28, 28);
            textStyle = new GUIStyle();
            textStyle.clipping = TextClipping.Clip;
            textStyle.richText = false;
            gfxReady = true;
        }

        void Box(Rect r, Color c)
        {
            Color old = GUI.color;
            GUI.color = c;
            GUI.Box(r, GUIContent.none, boxStyle);
            GUI.color = old;
        }

        void Dot(Rect r, Color c)
        {
            Color old = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, circleTex, ScaleMode.StretchToFill, true);
            GUI.color = old;
        }

        int Fit(string text, float w, float h, int maxSize, int minSize, FontStyle fs)
        {
            string key = text + "|" + (int)w + "|" + (int)h + "|" + maxSize + "|" + (int)fs;
            int size;
            if (fitCache.TryGetValue(key, out size)) return size;
            textStyle.wordWrap = true;
            textStyle.fontStyle = fs;
            size = minSize;
            for (int s = maxSize; s >= minSize; s -= 2)
            {
                textStyle.fontSize = s;
                if (textStyle.CalcHeight(new GUIContent(text), w) <= h) { size = s; break; }
            }
            if (fitCache.Count > 400) fitCache.Clear();
            fitCache[key] = size;
            return size;
        }

        void Label(Rect r, string text, int size, Color c, FontStyle fs, TextAnchor anchor, bool wrap)
        {
            textStyle.fontSize = size;
            textStyle.fontStyle = fs;
            textStyle.alignment = anchor;
            textStyle.wordWrap = wrap;
            textStyle.normal.textColor = c;
            GUI.Label(r, text, textStyle);
        }

        /// <summary>Rounded button: draws the box + caption, returns true when clicked.</summary>
        bool Button(Rect r, string caption, int size, Color fill, Color textColor)
        {
            Box(r, fill);
            Label(r, caption, size, textColor, FontStyle.Bold, TextAnchor.MiddleCenter, false);
            bool clicked = GUI.Button(r, GUIContent.none, GUIStyle.none);
            if (clicked && audio != null) audio.PlayClick();
            return clicked;
        }

        // ------------------------------------------------------------------ OnGUI
        void OnGUI()
        {
            if (session == null) return;
            EnsureGfx();
            Event e = Event.current;
            DrawAll(e);
        }

        void DrawAll(Event e)
        {
            // virtual canvas
            float sw = Screen.width, sh = Screen.height;
            if (sw < 2f || sh < 2f) return;
            float scale = sw <= sh ? sw / 1080f : sh / 1000f;
            float vW = sw / scale, vH = sh / scale;

            Rect sa = Screen.safeArea;
            float insL = Mathf.Max(0f, sa.xMin / scale);
            float insR = Mathf.Max(0f, (sw - sa.xMax) / scale);
            float insT = Mathf.Max(24f, (sh - sa.yMax) / scale);
            float insB = Mathf.Max(16f, sa.yMin / scale);

            float cw = Mathf.Min(vW - insL - insR, 1080f);
            float cx = insL + (vW - insL - insR - cw) * 0.5f;
            float pad = 28f;
            float left = cx + pad, right = cx + cw - pad, width = right - left;

            Matrix4x4 oldM = GUI.matrix;
            Color oldC = GUI.color;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            GUI.color = White;

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                Event.current.Use();
                if (quizGame != null && !quizGame.Complete) abandonConfirm = true;
                else if (!homeVisible) { homeVisible = true; journalOpen = false; }
                else exitConfirm = true;
            }
            if (exitConfirm || abandonConfirm)
            {
                Rect modal = new Rect(vW * .10f, vH * .36f, vW * .80f, 300f);
                Box(modal, Panel);
                bool abandoning = abandonConfirm;
                Label(new Rect(modal.x + 24f, modal.y + 24f, modal.width - 48f, 100f), abandoning ? "Leave this quiz? Your current run will end." : "Are you sure you want to exit AR Flashcards?", 28, White, FontStyle.Bold, TextAnchor.MiddleCenter, true);
                if (Button(new Rect(modal.x + 22f, modal.yMax - 92f, modal.width * .46f, 64f), abandoning ? "CONTINUE QUIZ" : "KEEP LEARNING", 21, Blue, White)) { exitConfirm = false; abandonConfirm = false; }
                if (Button(new Rect(modal.xMax - modal.width * .46f - 22f, modal.yMax - 92f, modal.width * .46f, 64f), abandoning ? "LEAVE QUIZ" : "EXIT APP", 21, PanelLight, White))
                {
                    if (abandoning) { abandonConfirm = false; quizGame = null; homeVisible = true; if (HomeClicked != null) HomeClicked(); }
                    else { exitConfirm = false; Application.Quit(); }
                }
                GUI.matrix = oldM; GUI.color = oldC; return;
            }

            if (homeVisible)
            {
                float hw = Mathf.Min(vW - 56f, 900f);
                float hx = (vW - hw) * .5f;
                Rect hero = new Rect(hx, Mathf.Max(insT + 30f, vH * .18f), hw, Mathf.Min(570f, vH * .62f));
                Box(hero, Panel);
                Label(new Rect(hero.x + 36f, hero.y + 42f, hw - 72f, 76f), "AR Flashcards", 52, White, FontStyle.Bold, TextAnchor.MiddleCenter, false);
                Label(new Rect(hero.x + 48f, hero.y + 128f, hw - 96f, 72f), "Meet the animals. Discover something new.", 28, Muted, FontStyle.Normal, TextAnchor.MiddleCenter, true);
                if (cameraLoading)
                {
                    Label(new Rect(hero.x + 40f, hero.y + 230f, hw - 80f, 100f), statusText, 28, Amber, FontStyle.Bold, TextAnchor.MiddleCenter, true);
                    if (statusKind == StatusKind.Error)
                    {
                        if (Button(new Rect(hero.x + 48f, hero.yMax - 105f, hw - 96f, 72f), "RETRY CAMERA", 26, Blue, White))
                        { if (StartARClicked != null) StartARClicked(); }
                        if (Button(new Rect(hero.x + 48f, hero.yMax - 190f, hw - 96f, 72f), "OPEN 3D VIEWER", 26, PanelLight, White))
                        { cameraLoading = false; homeVisible = false; if (ViewerClicked != null) ViewerClicked(); }
                    }
                }
                else
                {
                    Label(new Rect(hero.x + 40f, hero.y + 228f, hw - 80f, 56f), "Discoveries  " + DiscoveryJournal.Count + " / " + CardLibrary.All.Count + "     •     Quiz sessions " + PlayerPrefs.GetInt("ARFlashcards.QuizSessions", 0), 22, Muted, FontStyle.Normal, TextAnchor.MiddleCenter, true);
                    if (Button(new Rect(hero.x + 48f, hero.y + 310f, hw - 96f, 82f), "START AR EXPERIENCE", 28, Blue, White))
                    { if (StartARClicked != null) StartARClicked(); }
                    if (Button(new Rect(hero.x + 48f, hero.y + 410f, (hw - 108f) / 2f, 72f), "3D ANIMAL VIEWER", 23, PanelLight, White))
                    { homeVisible = false; if (ViewerClicked != null) ViewerClicked(); }
                    if (Button(new Rect(hero.x + 60f + (hw - 108f) / 2f, hero.y + 410f, (hw - 108f) / 2f, 72f), "DISCOVERY JOURNAL", 22, PanelLight, White)) journalOpen = true;
                    if (Button(new Rect(hero.x + 48f, hero.y + 495f, hw - 96f, 58f), "EXIT APP", 20, new Color(.28f, .18f, .22f), White)) exitConfirm = true;
                }
                if (journalOpen) DrawJournal(hx + 20f, hw - 40f, hero.yMax + 300f);
                GUI.matrix = oldM; GUI.color = oldC;
                PointerInput.FeedGui(e, hero);
                return;
            }

            // ---- top bar
            float y = insT + 8f;
            Rect bar = new Rect(left, y, width, 104f);
            Box(bar, Panel);
            Label(new Rect(bar.x + 118f, bar.y + 10f, 350f, 52f), "AR Flashcards", 36, White, FontStyle.Bold, TextAnchor.MiddleLeft, false);
            string prog = "Scanned " + session.Scanned + " / " + session.Total + (session.Complete ? "  - all done!" : "");
            Label(new Rect(bar.x + 118f, bar.y + 60f, 350f, 36f), prog, 22, session.Complete ? Green : Muted, FontStyle.Normal, TextAnchor.MiddleLeft, false);

            Rect modeR = new Rect(bar.xMax - 20f - 170f, bar.y + 20f, 170f, 64f);
            Rect langR = new Rect(modeR.x - 12f - 100f, bar.y + 20f, 100f, 64f);
            Rect journalR = new Rect(langR.x - 12f - 156f, bar.y + 20f, 156f, 64f);
            if (Button(journalR, "JOURNAL", 22, PanelLight, White)) journalOpen = !journalOpen;
            Rect homeR = new Rect(bar.x + 14f, bar.y + 20f, 90f, 64f);
            if (Button(homeR, "HOME", 20, PanelLight, White))
            {
                if (quizGame != null && !quizGame.Complete) abandonConfirm = true;
                else { homeVisible = true; cameraLoading = false; journalOpen = false; quizGame = null; if (HomeClicked != null) HomeClicked(); }
            }
            if (Button(modeR, viewerMode ? "3D VIEW" : "AR MODE", 26, viewerMode ? Amber : Blue, viewerMode ? new Color(0.1f, 0.1f, 0.1f) : White))
            { if (ModeToggleClicked != null) ModeToggleClicked(); }
            if (Button(langR, Loc.Code, 28, PanelLight, White)) Loc.Cycle();
            y = bar.yMax + 12f;

            // ---- status pill
            if (statusText.Length > 0)
            {
                float stH = 64f;
                float tw = width - 80f;
                int ssz = Fit(statusText, tw, stH - 8f, 26, 18, FontStyle.Normal);
                Rect st = new Rect(left, y, width, stH);
                Box(st, Panel);
                Color dc = statusKind == StatusKind.Tracking ? Green : statusKind == StatusKind.Error ? Red
                         : statusKind == StatusKind.Scanning ? Amber : Blue;
                Dot(new Rect(st.x + 24f, st.y + stH * 0.5f - 11f, 22f, 22f), dc);
                Label(new Rect(st.x + 64f, st.y + 4f, tw, stH - 8f), statusText, ssz, White, FontStyle.Normal, TextAnchor.MiddleLeft, true);
                y = st.yMax + 12f;
            }

            // ---- viewer chips
            if (viewerMode)
            {
                int n = CardLibrary.All.Count;
                float gap = 10f, chipW = (width - gap * (n - 1)) / n, chipH = 80f;
                for (int i = 0; i < n; i++)
                {
                    Card c = CardLibrary.All[i];
                    bool sel = session.Current == c;
                    Rect cr = new Rect(left + i * (chipW + gap), y, chipW, chipH);
                    int csz = Fit(c.Name(Lang.English), chipW - 12f, chipH - 12f, 26, 16, FontStyle.Bold);
                    if (Button(cr, c.Name(Lang.English), csz, sel ? c.Accent : PanelLight, sel ? new Color(0.05f, 0.05f, 0.08f) : White))
                    { if (CardPicked != null) CardPicked(c); }
                }
                y += chipH + 12f;
            }

            // ---- bottom panel
            float bottom = vH - insB - 20f;
            float panelTop;
            if (journalOpen) panelTop = DrawJournal(left, width, bottom);
            else if (quizGame != null) panelTop = DrawQuiz(left, width, bottom);
            else panelTop = DrawInfo(left, width, bottom);

            // ---- zoom buttons (right edge, vertically centred between top stuff and panel)
            float zoomMid = (y + panelTop) * 0.5f;
            Rect plus = new Rect(right - 88f, zoomMid - 100f, 88f, 88f);
            Rect minus = new Rect(right - 88f, zoomMid + 12f, 88f, 88f);
            if (panelTop - y > 240f)
            {
                if (Button(plus, "+", 56, Panel, White)) ModelInteraction.RequestZoom(1.2f);
                if (Button(minus, "-", 56, Panel, White)) ModelInteraction.RequestZoom(1f / 1.2f);
            }

            // ---- toast
            if (toastText != null && Time.unscaledTime < toastUntil)
            {
                int tsz = Fit(toastText, width - 80f, 70f, 28, 18, FontStyle.Bold);
                Rect tr = new Rect(left + 20f, panelTop - 96f, width - 40f, 80f);
                Box(tr, new Color(0f, 0f, 0f, 0.82f));
                Label(new Rect(tr.x + 20f, tr.y + 4f, tr.width - 40f, tr.height - 8f), toastText, tsz, White, FontStyle.Bold, TextAnchor.MiddleCenter, true);
            }

            // ---- drag / pinch zone (what is left in the middle)
            Rect zone = new Rect(0f, y, vW, Mathf.Max(0f, panelTop - y));
            PointerInput.FeedGui(e, zone);

            GUI.matrix = oldM;
            GUI.color = oldC;

        }

        // ------------------------------------------------------------------ info card
        float DrawInfo(float left, float width, float bottom)
        {
            Card c = session.Current;
            if (c == null)
            {
                Rect hint = new Rect(left, bottom - 130f, width, 130f);
                Box(hint, Panel);
                string t = viewerMode ? "Tap an animal above to see it in 3D" : "Point the camera at a flashcard";
                int sz = Fit(t, width - 60f, 90f, 34, 20, FontStyle.Bold);
                Label(new Rect(hint.x + 30f, hint.y + 20f, width - 60f, 90f), t, sz, White, FontStyle.Bold, TextAnchor.MiddleCenter, true);
                return hint.y;
            }

            float h = 440f;
            Rect card = new Rect(left, bottom - h, width, h);
            Box(card, Panel);
            Box(new Rect(card.x + 28f, card.y + 20f, 90f, 10f), c.Accent);

            float ix = card.x + 32f, iw = card.width - 64f;
            Lang L = Loc.Current;
            Texture2D nameImg = Loc.TextImage(c.Id, L, "name");
            Texture2D factImg = Loc.TextImage(c.Id, L, "fact");

            Rect nameR = new Rect(ix, card.y + 38f, iw, 76f);
            if (L == Lang.English || nameImg == null)
                Label(nameR, c.Name(Lang.English), 58, White, FontStyle.Bold, TextAnchor.MiddleLeft, false);
            else DrawImage(nameR, nameImg, TextAnchor.MiddleLeft);

            Rect factR = new Rect(ix, nameR.yMax + 6f, iw, 150f);
            if (L == Lang.English || factImg == null)
            {
                string f = c.Fact(Lang.English);
                int fsz = Fit(f, factR.width, factR.height, 32, 20, FontStyle.Normal);
                Label(factR, f, fsz, Muted, FontStyle.Normal, TextAnchor.UpperLeft, true);
            }
            else DrawImage(factR, factImg, TextAnchor.UpperLeft);

            // buttons
            float by = card.yMax - 20f - 88f;
            int count = viewerMode ? 4 : 2;
            float gap = 14f, bw = (iw - gap * (count - 1)) / count;
            Rect b0 = new Rect(ix, by, bw, 88f);
            Rect b1 = new Rect(ix + (bw + gap), by, bw, 88f);
            if (Button(b0, "SPEAK", 30, Blue, White)) { if (SpeakClicked != null) SpeakClicked(c); }
            if (Button(b1, "QUIZ", 30, Amber, new Color(0.1f, 0.1f, 0.1f))) StartQuiz(c);
            if (viewerMode)
            {
                Rect b2 = new Rect(ix + 2 * (bw + gap), by, bw, 88f);
                Rect b3 = new Rect(ix + 3 * (bw + gap), by, bw, 88f);
                if (Button(b2, "NEXT", 26, PanelLight, White))
                {
                    int i = (CardLibrary.IndexOf(c) + 1) % CardLibrary.All.Count;
                    if (CardPicked != null) CardPicked(CardLibrary.All[i]);
                }
                if (Button(b3, "RESET VIEW", 21, PanelLight, White)) ModelInteraction.RequestReset();
            }
            return card.y;
        }

        float DrawJournal(float left, float width, float bottom)
        {
            float h = Mathf.Min(590f, bottom - 80f);
            Rect p = new Rect(left, bottom - h, width, h);
            Box(p, Panel);
            Label(new Rect(p.x + 28f, p.y + 14f, width - 180f, 58f), "Animal Discovery Journal", 36, White, FontStyle.Bold, TextAnchor.MiddleLeft, false);
            Label(new Rect(p.x + 28f, p.y + 72f, width - 56f, 42f), DiscoveryJournal.Count + " / " + CardLibrary.All.Count + " animals discovered", 25, Muted, FontStyle.Normal, TextAnchor.MiddleLeft, false);
            if (Button(new Rect(p.xMax - 132f, p.y + 18f, 100f, 64f), "CLOSE", 22, PanelLight, White)) journalOpen = false;
            float rowY = p.y + 128f;
            float rowH = Mathf.Min(68f, (h - 156f) / Mathf.Max(1, CardLibrary.All.Count));
            for (int i = 0; i < CardLibrary.All.Count; i++)
            {
                Card c = CardLibrary.All[i];
                bool found = DiscoveryJournal.IsDiscovered(c);
                Rect row = new Rect(p.x + 24f, rowY + i * (rowH + 5f), width - 48f, rowH);
                Box(row, found ? PanelLight : new Color(0.10f, 0.11f, 0.17f, 0.95f));
                Dot(new Rect(row.x + 16f, row.y + row.height * .5f - 10f, 20f, 20f), found ? Green : Muted);
                Label(new Rect(row.x + 52f, row.y + 4f, row.width * .34f, row.height - 8f), found ? c.Name(Loc.Current) : "Undiscovered animal", 23, found ? White : Muted, FontStyle.Bold, TextAnchor.MiddleLeft, false);
                string fact = found ? c.Fact(Lang.English) : "Scan this flashcard to reveal its fact.";
                Label(new Rect(row.x + row.width * .40f, row.y + 4f, row.width * .57f, row.height - 8f), fact, Fit(fact, row.width * .57f, row.height - 8f, 19, 13, FontStyle.Normal), Muted, FontStyle.Normal, TextAnchor.MiddleLeft, true);
            }
            if (DiscoveryJournal.Complete)
                Label(new Rect(p.x + 20f, p.yMax - 38f, width - 40f, 30f), "Achievement unlocked: Animal Explorer!", 22, Amber, FontStyle.Bold, TextAnchor.MiddleCenter, false);
            return p.y;
        }

        void DrawImage(Rect r, Texture2D tex, TextAnchor anchor)
        {
            float aspect = (float)tex.width / Mathf.Max(1, tex.height);
            float w = r.width, h = w / aspect;
            if (h > r.height) { h = r.height; w = h * aspect; }
            float x = r.x;
            float y = anchor == TextAnchor.MiddleLeft ? r.y + (r.height - h) * 0.5f : r.y;
            GUI.DrawTexture(new Rect(x, y, w, h), tex, ScaleMode.StretchToFill, true);
        }

        // ------------------------------------------------------------------ quiz
        void StartQuiz(Card c)
        {
            quizGame = QuizGame.Create(rng);
            quizOptions = quizGame.ShuffledOptions(rng);
            PlayerPrefs.SetInt("ARFlashcards.QuizSessions", PlayerPrefs.GetInt("ARFlashcards.QuizSessions", 0) + 1);
            PlayerPrefs.Save();
        }

        float DrawQuiz(float left, float width, float bottom)
        {
            float h = Mathf.Min(650f, bottom - 80f);
            Rect p = new Rect(left, bottom - h, width, h);
            Box(p, Panel);
            if (Button(new Rect(p.xMax - 132f, p.y + 18f, 100f, 64f), "CLOSE", 22, PanelLight, White))
            {
                if (!quizGame.Complete) abandonConfirm = true;
                else quizGame = null;
            }
            if (quizGame == null) return p.y;
            if (quizGame.Complete)
            {
                Label(new Rect(p.x + 28f, p.y + 90f, width - 56f, 70f), "Quiz complete!", 42, White, FontStyle.Bold, TextAnchor.MiddleCenter, false);
                Label(new Rect(p.x + 28f, p.y + 170f, width - 56f, 100f), "Score " + quizGame.Score + " / " + quizGame.Questions.Count + "    •    " + quizGame.Correct + " correct    •    " + quizGame.Incorrect + " incorrect", 28, Muted, FontStyle.Normal, TextAnchor.MiddleCenter, true);
                if (Button(new Rect(p.x + 30f, p.yMax - 112f, (width - 76f) / 2f, 76f), "PLAY AGAIN", 25, Blue, White)) StartQuiz(session.Current);
                if (Button(new Rect(p.x + 46f + (width - 76f) / 2f, p.yMax - 112f, (width - 76f) / 2f, 76f), "BACK TO LEARNING", 23, PanelLight, White)) quizGame = null;
                return p.y;
            }
            int qn = quizGame.Index + 1;
            Label(new Rect(p.x + 28f, p.y + 14f, width - 190f, 38f), "QUESTION " + qn + " / " + quizGame.Questions.Count + "     SCORE " + quizGame.Score, 22, Muted, FontStyle.Bold, TextAnchor.MiddleLeft, false);
            Rect progress = new Rect(p.x + 28f, p.y + 58f, width - 56f, 12f);
            Box(progress, PanelLight);
            Box(new Rect(progress.x, progress.y, progress.width * qn / quizGame.Questions.Count, progress.height), Green);
            Label(new Rect(p.x + 30f, p.y + 82f, width - 60f, 102f), quizGame.Current.Prompt, Fit(quizGame.Current.Prompt, width - 60f, 102f, 34, 22, FontStyle.Bold), White, FontStyle.Bold, TextAnchor.MiddleLeft, true);
            float gap = 14f, ow = (width - 76f - gap) / 2f, oh = 102f;
            float oy = p.y + 205f;
            for (int i = 0; i < quizOptions.Count; i++)
            {
                int col = i % 2, row = i / 2;
                Rect r = new Rect(p.x + 30f + col * (ow + gap), oy + row * (oh + gap), ow, oh);
                Color fill = PanelLight;
                if (quizGame.Answered && quizOptions[i] == quizGame.Current.Answer) fill = Green;
                else if (quizGame.Answered && i == quizGame.Selected) fill = Red;
                if (Button(r, quizOptions[i], 25, fill, White) && !quizGame.Answered)
                {
                    quizGame.Submit(i, quizOptions);
                    if (quizOptions[i] == quizGame.Current.Answer) { if (audio != null) audio.PlayCorrect(); }
                    else if (audio != null) audio.PlayWrong();
                }
            }
            if (quizGame.Answered && Button(new Rect(p.x + 30f, p.yMax - 72f, width - 60f, 54f), qn == quizGame.Questions.Count ? "SEE RESULTS" : "NEXT QUESTION", 22, Blue, White))
            {
                quizGame.Next();
                if (!quizGame.Complete) quizOptions = quizGame.ShuffledOptions(rng);
            }
            return p.y;
        }
    }
}
