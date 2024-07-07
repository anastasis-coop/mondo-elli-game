using System;
using System.Text;
using UnityEngine;
using UnityEngine.Localization;

namespace Room9
{
    public class MemoryOutlineStep : MonoBehaviour
    {
        [SerializeField]
        private MessageSystem bigEllo;

        [SerializeField]
        private Timer timer;

        [SerializeField]
        private int timerSeconds = 300;

        [SerializeField]
        private GameObject timerLabelRoot;

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
        private LocalizedReadableStringAsset reviewTask;

        [SerializeField]
        private LocalizedReadableStringAsset sortTask;

        [SerializeField]
        private LocalizedReadableStringAsset info;

        [SerializeField]
        private TaskWidget taskWidget;

        [SerializeField]
        private TextReview review;

        [SerializeField]
        private OutlineSort outlineSort;

        [SerializeField]
        private Solution outlineSolution;

        [SerializeField]
        private StepEnd stepEnd;

        [SerializeField]
        private LocalizedString localizedStepEndTitle;

        [SerializeField]
        private LocalizedString localizedStepEndMessage;

        private ReadableString _theme;
        private MemoryExercise.OutlineParagraph[] _paragraphs;
        private Action _callback;

        public void Show(ReadableString theme, MemoryExercise.OutlineParagraph[] paragraphs, Action callback)
        {
            _theme = theme;
            _paragraphs = paragraphs;
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

            taskWidget.Show(reviewTask, info);
            review.Show(null, null, Array.ConvertAll(_paragraphs, p => p.TextAudio), OnReviewDone);

            timer.SetTime(timerSeconds);
            timerLabelRoot.SetActive(true);

            timer.timerCallback.AddListener(OnTimerExpired);

            bigEllo.messageBatchFinished.AddListener(OnBigElloBatchFinished);
            bigEllo.SetCurrentMessagesBatch(tutorials);
            bigEllo.ShowNextMessage();
        }

        private void OnBigElloBatchFinished()
        {
            bigEllo.messageBatchFinished.RemoveListener(OnBigElloBatchFinished);
            timer.activation = true;
        }

        private void OnReviewDone()
        {
            review.Hide();

            var titles = Array.ConvertAll(_paragraphs, p => p.TitleAudio);

            taskWidget.Show(sortTask, info);
            outlineSort.Show(false, titles,false, OnOutlineSortDone);
        }

        public void OnTimerExpired()
        {
            if (timer.activation)
            {
                review.Hide();
                outlineSort.OnTimerExpired();
            }
        }

        public void OnOutlineSortDone(int[] answers)
        {
            outlineSort.Hide();

            ReadableString[] titles = new ReadableString[_paragraphs.Length];
            int[] solution = new int[_paragraphs.Length];

            for (int i = 0; i < _paragraphs.Length; i++)
            {
                titles[i] = _paragraphs[i].TitleAudio;
                solution[i] = i;

                if (answers[i] == solution[i])
                    score.RightCounter++;
                else
                    score.WrongCounter++;
            }

            outlineSolution.Show(titles, answers, solution, OnOutlineSolutionDone);
        }

        public void OnOutlineSolutionDone(int[] _)
        {
            outlineSolution.Hide();

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
