using System;
using UnityEngine;

namespace Room9
{
    [CreateAssetMenu]
    public class MemoryExercise : Exercise
    {
        [Serializable]
        public class Title
        {
            public ReadableString TextAudio;
            public bool Relevance;
            public int[] DuplicateOfIndex;
        }

        public Title[] Titles;

        [Serializable]
        public class OutlineParagraph
        {
            public ReadableString TextAudio;
            public ReadableString TitleAudio;
        }

        public OutlineParagraph[] OutlineParagraphs;

        [Serializable]
        public class PreviewQuiz
        {
            public ReadableString[] PreviewsAudio;
            public ReadableString TitleAudio;
            public bool Solution;
        }

        public PreviewQuiz[] PreviewQuizzes;
    }
}