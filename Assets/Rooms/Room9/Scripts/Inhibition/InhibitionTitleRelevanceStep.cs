using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Room9
{
    public class InhibitionTitleRelevanceStep : MonoBehaviour
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
        private TaskWidget taskWidget;

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
        private InhibitionTitles inhibitionTitles;

        [SerializeField]
        private InhibitionDistractions distractions;

        [SerializeField]
        private StepEnd stepEnd;

        [SerializeField]
        private LocalizedString localizedStepEndTitle;

        [SerializeField]
        private LocalizedString localizedStepEndMessage;

        private ReadableString _theme;
        private InhibitionExercise.Title[] _titles;
        private Action _callback;

        public void Show(ReadableString theme, InhibitionExercise.Title[] titles, Action callback)
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

            var titles = Array.ConvertAll(_titles, t => t.TextAudio);
            inhibitionTitles.Show(titles, OnInhibitionTitlesDone);
            distractions.Show(OnInterferenceClick);

            bigEllo.messageBatchFinished.AddListener(OnBigElloBatchFinished);
            bigEllo.SetCurrentMessagesBatch(tutorials);
            bigEllo.ShowNextMessage();
        }

        private void OnBigElloBatchFinished()
        {
            bigEllo.messageBatchFinished.RemoveListener(OnBigElloBatchFinished);
            timer.activation = true;
        }

        public void OnInterferenceClick()
        {
            score.MissedCounter++;
        }

        public void OnTimerExpired()
        {
            if (timer.activation)
                inhibitionTitles.OnTimerExpired();
        }

        private void OnInhibitionTitlesDone(Dictionary<int, bool> answers)
        {
            inhibitionTitles.Hide();
            distractions.Hide();

            for (int i = 0; i < _titles.Length; i++)
            {
                var title = _titles[i];

                if (answers.ContainsKey(i))
                {
                    if (answers[i] == title.Relevance)
                        score.RightCounter++;
                    else
                        score.WrongCounter++;
                }
                else
                {
                    score.MissedCounter++;
                }
            }

            timer.timerCallback.RemoveListener(OnTimerExpired);
            timer.activation = false;

            float percentage = score.RightPercentage;
            int stars = percentage < 0.4f ? 0 : percentage < 0.6f ? 1 : percentage < 0.8f ? 2 : 3;
            GameState.Instance.RoomStars += stars;
            string stepEndTitle = localizedStepEndTitle.GetLocalizedString();
            string stepEndMessage = localizedStepEndMessage.GetLocalizedString();
            //stepEnd.Show(stepEndTitle, stepEndMessage, stars, 3, OnStepEndDone);
        }

        private void OnStepEndDone()
        {
            timerLabelRoot.SetActive(false);
            taskWidget.Hide();

            stepEnd.Hide();
            _callback?.Invoke();
        }
    }
}