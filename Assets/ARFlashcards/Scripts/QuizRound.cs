using System;
using System.Collections.Generic;

namespace ARFlashcards
{
    /// <summary>
    /// One multiple-choice question: "which animal is this?". Pure logic (unit-tested).
    /// A round gives 1 point only if the first guess is right.
    /// </summary>
    public sealed class QuizRound
    {
        public Card Answer { get; private set; }
        public List<Card> Options { get; private set; }
        public bool Solved { get; private set; }
        public int WrongGuesses { get; private set; }

        readonly HashSet<int> wrongIdx = new HashSet<int>();

        public static QuizRound Create(Card answer, IList<Card> pool, Random rng, int optionCount)
        {
            var r = new QuizRound { Answer = answer, Options = new List<Card>() };
            var others = new List<Card>();
            foreach (var c in pool) if (c != null && c != answer) others.Add(c);

            r.Options.Add(answer);
            int need = Math.Min(optionCount - 1, others.Count);
            for (int i = 0; i < need; i++)
            {
                int k = rng.Next(others.Count);
                r.Options.Add(others[k]);
                others.RemoveAt(k);
            }
            // Fisher-Yates shuffle so the answer is not always first
            for (int i = r.Options.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                Card t = r.Options[i]; r.Options[i] = r.Options[j]; r.Options[j] = t;
            }
            return r;
        }

        public bool IsWrongPicked(int index) { return wrongIdx.Contains(index); }

        /// <summary>Returns true when the guess is correct.</summary>
        public bool Guess(int index)
        {
            if (Solved || index < 0 || index >= Options.Count || wrongIdx.Contains(index)) return false;
            if (Options[index] == Answer) { Solved = true; return true; }
            wrongIdx.Add(index);
            WrongGuesses++;
            return false;
        }
    }
}
