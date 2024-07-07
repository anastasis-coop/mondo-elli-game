using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

namespace Room9
{
    public class InhibitionSnippetRelevanceStep : MonoBehaviour
    {
        [SerializeField]
        private MessageSystem bigEllo;

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
        private Timer timer;

        [SerializeField]
        private int timerSeconds  =300;

        [SerializeField]
        private Score score;

        [SerializeField]
        private GameObject timerLabelRoot;

        [SerializeField]
        private bool pickRelevant;
        [SerializeField]
        private bool pickMultiple;
        [SerializeField]
        private bool needToPickAll;

        [SerializeField]
        private InhibitionSnippets inhibitionSnippets;

        [SerializeField]
        private InhibitionDistractions distractions;

        [SerializeField]
        private StepEnd stepEnd;

        [SerializeField]
        private LocalizedString localizedStepEndTitle;

        [SerializeField]
        private LocalizedString localizedStepEndMessage;

        private ReadableString _theme;
        private InhibitionExercise.Text[] _texts;

        private Action _callback;

        private int _currentTextIndex;

        public void Show(ReadableString theme, InhibitionExercise.Text[] texts, Action callback)
        {
            _theme = theme;
            _texts = texts;
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

            _ = TryShowText(0);

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

        private bool TryShowText(int index)
        {
            if (index < 0 || index >= _texts.Length)
                return false;

            _currentTextIndex = index;

            var snippets = Array.ConvertAll(_texts[_currentTextIndex].Snippets, t => t.TextAudio);
            inhibitionSnippets.Show(snippets, pickMultiple, OnTextDone);

            return true;
        }

        private void OnTextDone(HashSet<int> selectedSnippets)
        {
            inhibitionSnippets.Hide();

            for (int i = 0; i < _texts[_currentTextIndex].Snippets.Length; i++)
            {
                var snippet = _texts[_currentTextIndex].Snippets[i];

                if (selectedSnippets.Contains(i))
                {
                    if (snippet.Relevance == pickRelevant)
                        score.RightCounter++;
                    else
                        score.WrongCounter++;
                }
                else if (needToPickAll && snippet.Relevance == pickRelevant)
                {
                    score.MissedCounter++;
                }
            }

            if (!TryShowText(_currentTextIndex + 1))
                OnSnippetsRelevanceDone();
        }

        public void OnInterferenceClick()
        {
            score.MissedCounter++;
        }

        private void OnTimerExpired()
        {
            if (_currentTextIndex < _texts.Length)
            {
                if (needToPickAll)
                {
                    for (int i = _currentTextIndex; i < _texts.Length; i++)
                    {
                        score.MissedCounter += _texts[i].Snippets.Count(s => s.Relevance == pickRelevant);
                    }
                }
                else
                {
                    score.MissedCounter += _texts.Length - _currentTextIndex;
                }
            }

            OnSnippetsRelevanceDone();
        }

        private void OnSnippetsRelevanceDone()
        {
            inhibitionSnippets.Hide();
            distractions.Hide();

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
            taskWidget.Hide();
            stepEnd.Hide();
            _callback?.Invoke();
        }
    }
}
