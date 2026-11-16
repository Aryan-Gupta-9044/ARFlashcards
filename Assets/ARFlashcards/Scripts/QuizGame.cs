using System;
using System.Collections.Generic;

namespace ARFlashcards
{
    public sealed class QuizQuestion
    {
        public readonly string Prompt;
        public readonly string Answer;
        public readonly string[] Options;
        public QuizQuestion(string prompt, string answer, params string[] options)
        { Prompt = prompt; Answer = answer; Options = options; }
    }

    /// <summary>Curated, fully offline animal questions; each session shuffles questions and choices.</summary>
    public sealed class QuizGame
    {
        public List<QuizQuestion> Questions { get; private set; }
        public int Index { get; private set; }
        public int Score { get; private set; }
        public int Correct { get; private set; }
        public int Incorrect { get; private set; }
        public bool Answered { get; private set; }
        public int Selected { get; private set; } = -1;
        public bool Complete { get { return Index >= Questions.Count; } }
        public QuizQuestion Current { get { return Complete ? null : Questions[Index]; } }

        static readonly QuizQuestion[] Bank = {
            new QuizQuestion("What is a group of lions called?", "A pride", "A pride", "A pod", "A colony", "A herd"),
            new QuizQuestion("Which animal is the largest land animal?", "Elephant", "Elephant", "Horse", "Lion", "Dog"),
            new QuizQuestion("Which animal is known for a powerful sense of smell?", "Dog", "Dog", "Cat", "Horse", "Elephant"),
            new QuizQuestion("About how many hours a day can cats sleep?", "12 to 16 hours", "12 to 16 hours", "1 to 2 hours", "4 to 6 hours", "20 to 24 hours"),
            new QuizQuestion("Which animal can sleep standing up or lying down?", "Horse", "Horse", "Cat", "Lion", "Dog"),
            new QuizQuestion("Which animal has a long trunk?", "Elephant", "Elephant", "Horse", "Lion", "Cat"),
            new QuizQuestion("Which animal is a familiar companion often kept as a pet?", "Dog", "Dog", "Lion", "Elephant", "Horse"),
            new QuizQuestion("Which animal has whiskers and retractable claws?", "Cat", "Cat", "Elephant", "Horse", "Dog"),
            new QuizQuestion("Which animal is a big cat with a mane in many adult males?", "Lion", "Lion", "Dog", "Horse", "Cat"),
            new QuizQuestion("Which animal is commonly ridden and can gallop?", "Horse", "Horse", "Cat", "Elephant", "Lion"),
            new QuizQuestion("Which animal uses its trunk to grasp and move objects?", "Elephant", "Elephant", "Dog", "Lion", "Horse"),
            new QuizQuestion("Which animal often communicates with barks?", "Dog", "Dog", "Cat", "Horse", "Elephant"),
            new QuizQuestion("Which animal often communicates with meows?", "Cat", "Cat", "Lion", "Horse", "Dog"),
            new QuizQuestion("Which animal is a hoofed mammal?", "Horse", "Horse", "Cat", "Dog", "Lion"),
            new QuizQuestion("Which animal is a wild member of the cat family?", "Lion", "Lion", "Dog", "Horse", "Elephant"),
            new QuizQuestion("Which animal has very large ears and tusks?", "Elephant", "Elephant", "Cat", "Horse", "Dog"),
            new QuizQuestion("Which animal is usually smallest among these five?", "Cat", "Cat", "Elephant", "Horse", "Lion"),
            new QuizQuestion("Which animal's young one is called a foal?", "Horse", "Horse", "Lion", "Cat", "Dog"),
            new QuizQuestion("Which animal's young one is called a calf?", "Elephant", "Elephant", "Cat", "Horse", "Lion"),
            new QuizQuestion("Which animal's young one is called a kitten?", "Cat", "Cat", "Dog", "Horse", "Elephant"),
            new QuizQuestion("Which animal's young one is called a puppy?", "Dog", "Dog", "Lion", "Cat", "Horse"),
            new QuizQuestion("Which animal's young one is called a cub?", "Lion", "Lion", "Horse", "Dog", "Cat"),
            new QuizQuestion("Which animal has a mane around its head and neck?", "Lion", "Lion", "Elephant", "Cat", "Dog"),
            new QuizQuestion("Which animal can use its trunk like a flexible hand?", "Elephant", "Elephant", "Horse", "Cat", "Lion"),
            new QuizQuestion("Which animal is known for purring when content?", "Cat", "Cat", "Dog", "Horse", "Elephant"),
            new QuizQuestion("Which animal is often trained to follow commands?", "Dog", "Dog", "Cat", "Lion", "Elephant"),
            new QuizQuestion("Which animal has hooves rather than paws?", "Horse", "Horse", "Dog", "Cat", "Lion"),
            new QuizQuestion("Which animal is a herbivore with a long trunk?", "Elephant", "Elephant", "Lion", "Cat", "Dog"),
            new QuizQuestion("Which animal is a carnivorous wild cat?", "Lion", "Lion", "Horse", "Elephant", "Cat"),
            new QuizQuestion("Which animal is commonly called a feline pet?", "Cat", "Cat", "Horse", "Dog", "Elephant")
        };

        public static QuizGame Create(Random rng)
        {
            var shuffled = new List<QuizQuestion>(Bank);
            Shuffle(shuffled, rng);
            string previous = UnityEngine.PlayerPrefs.GetString("ARFlashcards.PreviousQuiz", "");
            var questions = new List<QuizQuestion>();
            foreach (QuizQuestion q in shuffled)
                if (!previous.Contains("\n" + q.Prompt + "\n")) questions.Add(q);
            if (questions.Count < 10) questions.AddRange(shuffled);
            if (questions.Count > 10) questions.RemoveRange(10, questions.Count - 10);
            string saved = "\n";
            foreach (QuizQuestion q in questions) saved += q.Prompt + "\n";
            UnityEngine.PlayerPrefs.SetString("ARFlashcards.PreviousQuiz", saved);
            UnityEngine.PlayerPrefs.Save();
            return new QuizGame { Questions = questions };
        }

        public List<string> ShuffledOptions(Random rng)
        {
            var options = new List<string>(Current.Options);
            Shuffle(options, rng);
            return options;
        }

        public void Submit(int selected, IList<string> options)
        {
            if (Answered || Complete || selected < 0 || selected >= options.Count) return;
            Answered = true; Selected = selected;
            if (options[selected] == Current.Answer) { Correct++; Score++; }
            else Incorrect++;
        }

        public void Next()
        {
            if (!Answered) return;
            Index++; Answered = false; Selected = -1;
        }

        static void Shuffle<T>(IList<T> list, Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--) { int j = rng.Next(i + 1); T t = list[i]; list[i] = list[j]; list[j] = t; }
        }
    }
}
