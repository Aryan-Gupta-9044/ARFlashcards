using System.Collections.Generic;
using UnityEngine;

namespace ARFlashcards
{
    /// <summary>Offline discovery progress shared by AR scans and the 3D viewer.</summary>
    public static class DiscoveryJournal
    {
        const string KeyPrefix = "ARFlashcards.Discovered.";

        public static bool IsDiscovered(Card card)
        {
            return card != null && PlayerPrefs.GetInt(KeyPrefix + card.Id, 0) == 1;
        }

        public static bool Record(Card card)
        {
            if (card == null || IsDiscovered(card)) return false;
            PlayerPrefs.SetInt(KeyPrefix + card.Id, 1);
            PlayerPrefs.Save();
            return true;
        }

        public static int Count
        {
            get
            {
                int count = 0;
                foreach (Card card in CardLibrary.All) if (IsDiscovered(card)) count++;
                return count;
            }
        }

        public static bool Complete { get { return CardLibrary.All.Count > 0 && Count == CardLibrary.All.Count; } }
    }
}
