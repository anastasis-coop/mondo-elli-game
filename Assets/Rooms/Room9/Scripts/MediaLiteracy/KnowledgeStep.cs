using System;
using UnityEngine;
using UnityEngine.Localization;

namespace Room9
{
    public class KnowledgeStep : MonoBehaviour
    {
        [SerializeField]
        private MessageSystem bigEllo;

        [SerializeField]
        private StepStart stepStart;

        [SerializeField]
        private LocalizedString localizedStepStartTitle;

        [SerializeField]
        private BigElloSaysConfig[] stepStartMessages;

        [SerializeField]
        private BigElloSaysConfig starTutorial;

        [SerializeField]
        private ThemeIntro themeIntro;

        [SerializeField]
        private QuizSlider knowledgeEstimate;

        [SerializeField]
        private LocalizedReadableStringAsset estimateQuestion;

        [SerializeField]
        private QuizChoice quizChoice;

        [SerializeField]
        private StepEnd stepEnd;

        [SerializeField]
        private LocalizedString localizedStepEndTitle;
        
        [SerializeField] private ReadableString readableStepEndTitle;

        [SerializeField]
        private LocalizedString localizedStepEndMessage;
        
        [SerializeField] private ReadableString readableStepEndMessage;

        private MediaLiteracyExercise _exercise;
        private MediaLiteracyResult _result;
        private Action _callback;
        private ReadableString[] _tips;

        private int _quizIndex;
        
        public void Show(MediaLiteracyExercise exercise, MediaLiteracyResult result, ReadableString[] tips, Action callback)
        {
            _exercise = exercise;
            _result = result;
            _callback = callback;
            _tips = tips;
            string stepStartTitle = localizedStepStartTitle.GetLocalizedString();
            stepStart.Show(stepStartTitle, stepStartMessages, OnStepStartDone);
        }

        private void OnStepStartDone()
        {
            stepStart.Hide();

            themeIntro.Show(_exercise.ThemeAudio, _tips, OnThemeIntroDone);
        }

        private void OnThemeIntroDone()
        {
            themeIntro.Hide();

            knowledgeEstimate.Show(estimateQuestion, OnKnowledgeEstimateDone);
        }

        private void OnKnowledgeEstimateDone(float answer)
        {
            knowledgeEstimate.Hide();

            _result.KnowledgeEstimate = (int)answer;

            _quizIndex = 0;
            _result.KnowledgeQuizzesAnswers = new bool[_exercise.KnowledgeQuizzes.Length];

            ShowQuiz(_quizIndex);
        }

        private void ShowQuiz(int index)
        {
            _quizIndex = index;

            var quiz = _exercise.KnowledgeQuizzes[index];

            quizChoice.Show(quiz.QuestionAudio, quiz.ChoicesAudio, OnQuizDone, quiz.Correct[0], true);
        }

        private void OnQuizDone(int answer)
        {
            quizChoice.Hide();

            bool correct = answer == _exercise.KnowledgeQuizzes[_quizIndex].Correct[0];

            _result.KnowledgeQuizzesAnswers[_quizIndex] = correct;

            if (_quizIndex < _exercise.KnowledgeQuizzes.Length - 1)
            {
                ShowQuiz(_quizIndex + 1);
            }
            else
            {
                int stars = _result.GetKnowledgeStars();

                string stepEndTitle = localizedStepEndTitle.GetLocalizedString();
                string stepEndMessage = localizedStepEndMessage.GetLocalizedString();
                stepEnd.Show(readableStepEndTitle, readableStepEndMessage, stars, 1, OnStepEndDone);
                bigEllo.ShowMessage(starTutorial);
            }
        }

        private void OnStepEndDone()
        {
            stepEnd.Hide();

            _callback?.Invoke();
        }
    }
}