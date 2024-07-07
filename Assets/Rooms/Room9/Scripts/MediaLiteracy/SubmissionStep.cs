using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace Room9
{
    public class SubmissionStep : MonoBehaviour
    {
        [SerializeField]
        private StepStart stepStart;

        [SerializeField]
        private LocalizedString localizedStepStartTitle;

        [SerializeField]
        private BigElloSaysConfig[] stepStartMessages;

        [SerializeField]
        private TextMeshProUGUI reviewLabel;

        [SerializeField]
        private TextReview review;

        [SerializeField]
        private ReadableString reviewTaskAudio;

        [SerializeField]
        private LocalizedReadableStringAsset reviewTask;

        [SerializeField]
        private TaskWidget taskWidget;

        [SerializeField]
        private QuizChoice quizChoice;

        [SerializeField]
        private QuizChoice yesNoChoices;

        [SerializeField]
        private QuizMultipleChoice tipsQuiz;

        [SerializeField]
        private GameObject tipPrefab;

        [SerializeField]
        private SubmissionStars stars;

        private MediaLiteracyExercise _exercise;
        private MediaLiteracyResult _result;
        private Action _callback;
        private int _quizIndex;

        public void Show(MediaLiteracyExercise exercise, MediaLiteracyResult result, Action callback)
        {
            _exercise = exercise;
            _result = result;
            _callback = callback;

            string stepStartTitle = localizedStepStartTitle.GetLocalizedString();
            stepStart.Show(stepStartTitle, stepStartMessages, OnStepStartDone);
        }

        private void OnStepStartDone()
        {
            stepStart.Hide();

            var title = _exercise.TitleQuiz.CorrectChoices[0];

            taskWidget.Show(reviewTaskAudio, null);

            review.Show(title, _exercise.TagsQuiz.CorrectChoices, null, OnReviewDone);
            review.AppendMultiClipLabel(_result.ProductionResult, _result.ProductionAudio);
        }

        public void OnReviewDone()
        {
            taskWidget.Hide();
            review.Hide();

            _quizIndex = 0;
            _result.SubmissionQuizzesAnswers = new int[_exercise.SubmissionQuizzes.Length];

            ShowQuiz(_quizIndex);
        }

        private void ShowQuiz(int index)
        {
            _quizIndex = index;

            var quiz = _exercise.SubmissionQuizzes[index];

            if (quiz.ChoicesAudio.Length == 2)
            {
                // HACK assuming all 2 choices quizzes are yes no
                yesNoChoices.Show(quiz.QuestionAudio, quiz.ChoicesAudio, OnQuizDone);
            }
            else
            {
                quizChoice.Show(quiz.QuestionAudio, quiz.ChoicesAudio, OnQuizDone);
            }
        }

        private void OnQuizDone(int answer)
        {
            yesNoChoices.Hide();
            quizChoice.Hide();

            _result.SubmissionQuizzesAnswers[_quizIndex] = answer;

            if (_quizIndex < _exercise.SubmissionQuizzes.Length - 1)
            {
                ShowQuiz(_quizIndex + 1);
            }
            else
            {
                tipsQuiz.Show(_exercise.TipsQuizTitle, _exercise.Tips, OnTipsQuizDone);
            }
        }

        private void OnTipsQuizDone(HashSet<int> answers)
        {
            tipsQuiz.Hide();

            _result.SubmissionTipsAnswers = new List<int>(answers).ToArray();

            stars.Show(_result, OnStepEndDone);
        }

        private void OnStepEndDone()
        {
            stars.Hide();

            _callback?.Invoke();
        }
    }
}
