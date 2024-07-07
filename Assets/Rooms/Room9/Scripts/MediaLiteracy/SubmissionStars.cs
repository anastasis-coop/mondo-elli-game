using System;
using TMPro;
using UnityEngine;

namespace Room9
{
    public class SubmissionStars : MonoBehaviour
    {
        [Serializable]
        private class Entry
        {
            public string Title;
            public TextMeshProUGUI TitleLabel;
            public int MaxStars;
            public StarsRating StarsRating;

            public void Init(int stars)
            {
                TitleLabel.text = Title;
                StarsRating.Init(stars, MaxStars);
            }
        }

        [SerializeField]
        private Entry knowledgeEntry;

        [SerializeField]
        private Entry outlineEntry;

        [SerializeField]
        private Entry relevanceEntry;

        [SerializeField]
        private Entry timeEntry;

        [SerializeField]
        private Entry snippetsEntry;

        [SerializeField]
        private Entry productionEntry;

        [SerializeField]
        private Entry conclusionEntry;

        [SerializeField]
        private TextMeshProUGUI totalStarsLabel;

        private Action _callback;

        public void Show(MediaLiteracyResult result, Action callback)
        {
            _callback = callback;

            knowledgeEntry.Init(result.GetKnowledgeStars());
            outlineEntry.Init(result.GetOutlineStars());
            relevanceEntry.Init(result.GetRelevanceStars());
            timeEntry.Init(result.GetTimeStars());
            snippetsEntry.Init(result.GetSnippetsStars());
            productionEntry.Init(result.GetProductionStars());
            conclusionEntry.Init(result.GetConclusionStars());

            totalStarsLabel.text = result.GetTotalStars().ToString();

            gameObject.SetActive(true);
        }

        public void OnContinueButtonPressed() => _callback?.Invoke();

        public void Hide()
        {
            _callback = null;

            gameObject.SetActive(false);
        }
    }
}