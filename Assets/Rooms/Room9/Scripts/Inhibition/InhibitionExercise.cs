using System;
using UnityEngine;

namespace Room9
{
    [CreateAssetMenu]
    public class InhibitionExercise : Exercise
    {
        [Serializable]
        public class Title
        {
            public ReadableString TextAudio;
            public bool Relevance;
        }

        public Title[] Titles;

        [Serializable]
        public class Text
        {
            [Serializable]
            public class Snippet
            {
                public ReadableString TextAudio;
                public bool Relevance;
            }

            public Snippet[] Snippets;
        }

        public Text[] NegativeRelevanceTexts;
        public Text[] PositiveRelevanceTexts;
    }
}
