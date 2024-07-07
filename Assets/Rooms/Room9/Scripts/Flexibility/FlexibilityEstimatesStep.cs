using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace Room9
{
    public class FlexibilityEstimatesStep : MonoBehaviour
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
        private FlexibilityEstimates estimates;

        [SerializeField]
        private LocalizedReadableStringAsset task;

        [SerializeField]
        private LocalizedReadableStringAsset info;

        [SerializeField]
        private TaskWidget taskWidget;

        [SerializeField]
        private LocalizedString localizedProperty;

        [SerializeField]
        private Vector2Int propertyRange;

        [SerializeField]
        private int correctDelta;

        [SerializeField]
        private StepEnd stepEnd;

        [SerializeField]
        private LocalizedString localizedStepEndTitle;

        [SerializeField]
        private LocalizedString localizedStepEndMessage;

        private int _depth;
        private bool _mergeSnippets;
        private ReadableString _theme;
        private FlexibilityExercise.Text[] _texts;
        private Action _callback;
        private int _textIndex;

        public void Show(ReadableString theme, FlexibilityExercise.Text[] texts, int depth, bool mergeSnippets, Action callback)
        {
            _depth = depth;
            _mergeSnippets = mergeSnippets;
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

            _ = TryShowText(0);
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

            _textIndex = index;

            var text = _texts[index];

            taskWidget.Show(task, info);
            string property = localizedProperty.GetLocalizedString();
            estimates.Show(property, propertyRange, text.LayersAudio, _depth, text.Snippets, _mergeSnippets, OnEstimatesDone);

            return true;
        }

        private void OnEstimatesDone(int[] answers)
        {
            estimates.Hide();

            int solution = _texts[_textIndex].Solution;

            if (answers.Length > 0)
            {
                // HACK only using last estimate here sice the requirements might change
                if (Mathf.Abs(answers[^1] - solution) <= correctDelta)
                    score.RightCounter++;
                else
                    score.WrongCounter++;
            }
            else
            {
                score.MissedCounter++;
            }

            if (!timer.itIsEnd && TryShowText(_textIndex + 1)) return;

            if (timer.itIsEnd)
            {
                // HACK magic layer count
                score.MissedCounter += _texts.Length - _textIndex; 
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

        public void OnTimerExpired()
        {
            if (timer.activation)
            {
                estimates.OnTimerExpired();
            }
        }

        private void OnStepEndDone()
        {
            taskWidget.Hide();
            stepEnd.Hide();
            _callback?.Invoke();
        }
    }
}
