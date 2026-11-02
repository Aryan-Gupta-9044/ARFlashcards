using System;
using System.Collections.Generic;

namespace ARFlashcards
{
    /// <summary>
    /// Pure game state (no Unity dependency, unit-tested): which card is shown, which were scanned.
    /// </summary>
    public sealed class FlashcardSession
    {
        public event Action<Card> CurrentChanged;
        public event Action ProgressChanged;

        readonly HashSet<string> scanned = new HashSet<string>();
        readonly List<Card> visible = new List<Card>();   // oldest first, newest last

        public Card Current { get; private set; }
        public int Scanned { get { return scanned.Count; } }
        public int Total { get { return CardLibrary.All.Count; } }
        public bool Complete { get { return Scanned >= Total; } }

        /// <summary>A card became visible (tracked or selected). The newest visible card wins.</summary>
        public void Show(Card card)
        {
            if (card == null) return;
            visible.Remove(card);
            visible.Add(card);
            bool first = scanned.Add(card.Id);
            SetCurrent(card);
            if (first && ProgressChanged != null) ProgressChanged();
        }

        /// <summary>A card disappeared. Falls back to the newest card that is still visible.</summary>
        public void Hide(Card card)
        {
            if (card == null) return;
            visible.Remove(card);
            SetCurrent(visible.Count > 0 ? visible[visible.Count - 1] : null);
        }

        public void ClearVisible()
        {
            visible.Clear();
            SetCurrent(null);
        }

        public void ResetProgress()
        {
            scanned.Clear();
            if (ProgressChanged != null) ProgressChanged();
        }

        void SetCurrent(Card c)
        {
            if (ReferenceEquals(Current, c)) return;
            Current = c;
            if (CurrentChanged != null) CurrentChanged(c);
        }
    }
}
