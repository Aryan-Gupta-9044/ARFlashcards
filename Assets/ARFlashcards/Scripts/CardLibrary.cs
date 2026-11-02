using System.Collections.Generic;
using UnityEngine;

namespace ARFlashcards
{
    /// <summary>UI / content languages. Hindi and Tamil text is pre-rendered (see Loc.cs).</summary>
    public enum Lang { English = 0, Hindi = 1, Tamil = 2 }

    /// <summary>One flashcard: id, localized name + fact, accent colour.</summary>
    public sealed class Card
    {
        public readonly string Id;
        public readonly string[] Names;   // index = (int)Lang
        public readonly string[] Facts;
        public readonly Color Accent;

        public Card(string id, string[] names, string[] facts, Color accent)
        {
            Id = id; Names = names; Facts = facts; Accent = accent;
        }

        public string Name(Lang l) { return Names[(int)l]; }
        public string Fact(Lang l) { return Facts[(int)l]; }
        public string Speech(Lang l) { return Names[(int)l] + ". " + Facts[(int)l]; }
    }

    /// <summary>
    /// The deck. To add a card: add an entry here, add a card image in Resources/ARFlashcards/Cards/&lt;id&gt;.bytes
    /// (a PNG renamed to .bytes) and a model case in AnimalFactory.
    /// </summary>
    public static class CardLibrary
    {
        public static readonly List<Card> All = new List<Card>
        {
            new Card("lion",
                new[] { "Lion", "शेर", "சிங்கம்" },
                new[] { "Lions live in groups called prides.",
                        "शेर प्राइड नामक समूहों में रहते हैं।",
                        "சிங்கங்கள் 'பிரைட்' எனப்படும் குழுக்களாக வாழ்கின்றன." },
                new Color(0.93f, 0.62f, 0.16f)),
            new Card("elephant",
                new[] { "Elephant", "हाथी", "யானை" },
                new[] { "Elephants are the largest land animals on Earth.",
                        "हाथी धरती का सबसे बड़ा स्थलीय जानवर है।",
                        "யானைகள் பூமியின் மிகப்பெரிய நிலவாழ் விலங்குகள்." },
                new Color(0.35f, 0.58f, 0.86f)),
            new Card("dog",
                new[] { "Dog", "कुत्ता", "நாய்" },
                new[] { "A dog's sense of smell is far stronger than a human's.",
                        "कुत्तों की सूंघने की शक्ति इंसानों से कहीं ज़्यादा तेज़ होती है।",
                        "நாய்களின் மோப்ப சக்தி மனிதர்களை விட மிகவும் அதிகம்." },
                new Color(0.25f, 0.70f, 0.45f)),
            new Card("cat",
                new[] { "Cat", "बिल्ली", "பூனை" },
                new[] { "Cats sleep for about 12 to 16 hours a day.",
                        "बिल्लियाँ दिन में लगभग 12 से 16 घंटे सोती हैं।",
                        "பூனைகள் ஒரு நாளைக்கு சுமார் 12 முதல் 16 மணி நேரம் தூங்குகின்றன." },
                new Color(0.80f, 0.40f, 0.82f)),
            new Card("horse",
                new[] { "Horse", "घोड़ा", "குதிரை" },
                new[] { "Horses can sleep both lying down and standing up.",
                        "घोड़े लेटकर और खड़े होकर, दोनों तरह से सो सकते हैं।",
                        "குதிரைகள் படுத்தும் நின்றும் தூங்க முடியும்." },
                new Color(0.88f, 0.34f, 0.36f)),
        };

        public static Card Find(string id)
        {
            for (int i = 0; i < All.Count; i++)
                if (All[i].Id == id) return All[i];
            return null;
        }

        public static int IndexOf(Card c) { return All.IndexOf(c); }
    }
}
