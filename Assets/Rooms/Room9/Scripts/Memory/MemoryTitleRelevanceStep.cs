using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

namespace Room9
{
    public class MemoryTitleRelevanceStep : MonoBehaviour
    {
        [SerializeField]
        private MessageSystem bigEllo;

        [SerializeField]
        private Timer timer;

        [SerializeField]
        private GameObject timerLabelRoot;

        [SerializeField]
        private int timerSeconds = 300;

        [SerializeField]
        private Score score;

        [SerializeField]
        private ThemeIntro themeIntro;

        [SerializeField]
        private StepStart stepStart;

        [SerializeField]
        private LocalizedString localizedStepStartTitle;

        [SerializeField]
        private BigElloSaysConfig[] stepStartMessages;

        [SerializeField]
        private BigElloSaysConfig[] tutorials;

        [SerializeField]
        private LocalizedReadableStringAsset task;

        [SerializeField]
        private LocalizedReadableStringAsset info;

        [SerializeField]
        private TaskWidget taskWidget;

        [SerializeField]
        private MemoryTitles memoryTitles;

        [SerializeField]
        private StepEnd stepEnd;

        [SerializeField]
        private LocalizedString localizedStepEndTitle;

        [SerializeField]
        private LocalizedString localizedStepEndMessage;

        private ReadableString _theme;
        private MemoryExercise.Title[] _titles;
        private Action _callback;

        public void Show(ReadableString theme, MemoryExercise.Title[] titles, Action callback)
        {
            _theme = theme;
            _titles = titles;
            _callback = callback;

            string stepStartTitle = localizedStepStartTitle.GetLocalizedString();
            stepStart.Show(stepStartTitle, stepStartMessages, OnStepStartDone);
        }

        private void OnStepStartDone()
        {
            stepStart.Hide();
            themeIntro.Show(_theme, null, OnThemeIntroDone);
        }

        private void OnThemeIntroDone()
        {
            themeIntro.Hide();

            timer.timerCallback.AddListener(OnTimerExpired);

            timer.SetTime(timerSeconds);

            timerLabelRoot.SetActive(true);

            taskWidget.Show(task, info);

            var texts = Array.ConvertAll(_titles, t => t.TextAudio);

            memoryTitles.Show(texts, OnTitleRelevanceDone);

            bigEllo.messageBatchFinished.AddListener(OnBigElloBatchFinished);
            bigEllo.SetCurrentMessagesBatch(tutorials);
            bigEllo.ShowNextMessage();
        }

        private void OnBigElloBatchFinished()
        {
            bigEllo.messageBatchFinished.RemoveListener(OnBigElloBatchFinished);
            timer.activation = true;
        }

        public void OnTimerExpired()
        {
            if (timer.activation)
            {
                memoryTitles.OnTimerExpired();
            }
        }

        public void OnTitleRelevanceDone(Dictionary<int, bool> answers)
        {
            // Could be simpler but we need a way to discriminate missed from wrong
            Dictionary<int, bool> correct = new();

            for (int i = 0; i < _titles.Length; i++)
            {
                if (!answers.ContainsKey(i)) continue;

                var title = _titles[i];
                bool match = title.Relevance == answers[i];

                if (!title.Relevance)
                {
                    correct[i] = match;
                    continue;
                }

                bool anyRelevantDuplicates = Array.Exists(title.DuplicateOfIndex,
                        d => answers.ContainsKey(d) && answers[d]);

                bool firstRatedRelevant = !Array.Exists(title.DuplicateOfIndex,
                    d => answers.ContainsKey(d) && answers[d] && correct.ContainsKey(d));

                correct[i] = (match && firstRatedRelevant) || (!match && anyRelevantDuplicates);
            }

            score.MissedCounter = _titles.Length - answers.Count;
            score.RightCounter = correct.Count(c => c.Value);
            score.WrongCounter = answers.Count - score.RightCounter;

            timer.timerCallback.RemoveListener(OnTimerExpired);
            timer.activation = false;

            memoryTitles.ShowSolution(correct, OnContinueButtonPressed);
        }

        public void OnContinueButtonPressed()
        {
            memoryTitles.Hide();

            float percentage = score.RightPercentage;
            int stars = percentage < 0.4f ? 0 : percentage < 0.6f ? 1 : percentage < 0.8f ? 2 : 3;
            GameState.Instance.RoomStars += stars;

            string stepEndTitle = localizedStepEndTitle.GetLocalizedString();
            string stepEndMessage = localizedStepEndMessage.GetLocalizedString();
            //stepEnd.Show(stepEndTitle, stepEndMessage, stars, 3, OnStepEndDone);
        }

        private void OnStepEndDone()
        {
            taskWidget.Hide();
            stepEnd.Hide();
            _callback?.Invoke();
        }
    }
}
