using System;
using UnityEngine;

namespace Room9
{
    [CreateAssetMenu]
    public class FlexibilityExercise : Exercise
    {
        [Serializable]
        public class Text
        {
            public ReadableString[] LayersAudio;
            public ReadableString[] Snippets;

            public int Solution;
        }

        public Text[] RelevanceTexts;
        public Text[] SnippetsTexts;
        public Text[] TimeTexts;
    }
}