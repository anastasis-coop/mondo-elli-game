using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Room9
{
    public class MemoryPreviewStep : MonoBehaviour
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
        private TextReview review;

        [SerializeField]
        private LocalizedReadableStringAsset reviewTask;

        [SerializeField]
        private GameObject titlePanel;

        [SerializeField]
        private TextMeshProUGUI titleLabel;

        [SerializeField]
        private ReadableLabel titleReadable;

        [SerializeField]
        private LocalizedReadableStringAsset titleTask;

        [SerializeField]
        private LocalizedReadableStringAsset info;

        [SerializeField]
        private TaskWidget taskWidget;

        [SerializeField]
        private StepEnd stepEnd;

        [SerializeField]
        private LocalizedString localizedStepEndTitle;

        [SerializeField]
        private LocalizedString localizedStepEndMessage;

        private ReadableString _theme;
        private MemoryExercise.PreviewQuiz[] _quizzes;
        private Action _callback;

        private int _quizIndex;

        public void Show(ReadableString theme, MemoryExercise.PreviewQuiz[] quizzes, Action callback)
        {
            _theme = theme;
            _quizzes = quizzes;
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

            taskWidget.Show(titleTask, info);

            _ = TryShowQuiz(0);

            bigEllo.messageBatchFinished.AddListener(OnBigElloBatchFinished);
            bigEllo.SetCurrentMessagesBatch(tutorials);
            bigEllo.ShowNextMessage();
        }

        private void OnBigElloBatchFinished()
        {
            bigEllo.messageBatchFinished.RemoveListener(OnBigElloBatchFinished);
            timer.activation = true;
        }

        private bool TryShowQuiz(int index)
        {
            if (index < 0 || index >= _quizzes.Length)
                return false;

            _quizIndex = index;

            var quiz = _quizzes[index];

            taskWidget.Show(reviewTask, info);

            review.Show(null, null, quiz.PreviewsAudio, OnReviewDone);

            return true;
        }

        private void OnReviewDone()
        {
            review.Hide();

            titleLabel.text = _quizzes[_quizIndex].TitleAudio?.String;
            titleReadable.AudioClip = _quizzes[_quizIndex].TitleAudio?.AudioClip;
            titlePanel.SetActive(true);
        }

        public void OnTitleDone(bool answer)
        {
            titlePanel.SetActive(false);

            var quiz = _quizzes[_quizIndex];

            if (answer == quiz.Solution)
                score.RightCounter++;
            else
                score.WrongCounter++;

            if (TryShowQuiz(_quizIndex + 1)) return;

            OnStepCompleted();
        }

        public void OnTimerExpired()
        {
            if (timer.activation)
            {
                review.Hide();
                titlePanel.SetActive(false);

                score.MissedCounter += _quizzes.Length - _quizIndex;
                OnStepCompleted();
            }
        }

        private void OnStepCompleted()
        {
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
