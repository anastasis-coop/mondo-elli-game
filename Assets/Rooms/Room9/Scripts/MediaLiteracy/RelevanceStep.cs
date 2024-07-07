using System;
using UnityEngine;
using UnityEngine.Localization;

namespace Room9
{
    public class RelevanceStep : MonoBehaviour
    {
        [SerializeField]
        private StepStart stepStart;

        [SerializeField]
        private LocalizedString localizedStepStartTitle;

        [SerializeField]
        private BigElloSaysConfig[] stepStartMessages;

        [SerializeField]
        private LocalizedReadableStringAsset assignTask;

        [SerializeField]
        private LocalizedReadableStringAsset assignInfo;

        [SerializeField]
        private LocalizedReadableStringAsset quizTask;

        [SerializeField]
        private LocalizedReadableStringAsset solutionTask;

        [SerializeField]
        private LocalizedReadableStringAsset solutionInfo;

        [SerializeField]
        private TaskWidget taskWidget;

        [SerializeField]
        private RelevanceAssign relevanceAssign;

        [SerializeField]
        private RelevanceEntry quizEntry;

        [SerializeField]
        private QuizChoice quizChoice;

        [SerializeField]
        private LocalizedReadableStringAsset quizTitle;

        [SerializeField]
        private LocalizedReadableStringAsset[] quizChoices;

        [SerializeField]
        private int zeroRelevanceCount;

        [SerializeField]
        private Solution relevanceSolution;

        [SerializeField]
        private StepEnd phaseEnd;

        [SerializeField]
        private LocalizedString localizedRelevancePhaseEndTitle;
        
        [SerializeField] private ReadableString readableRelevancePhaseEndTitle;

        [SerializeField]
        private LocalizedString localizedRelevancePhaseEndMessage;
        
        [SerializeField] private ReadableString readableRelevancePhaseEndMessage;

        private MediaLiteracyExercise _exercise;
        private MediaLiteracyResult _result;
        private Action _callback;
        private int _quizIndex;
        private int[] _answers;

        public void Show(MediaLiteracyExercise exercise, MediaLiteracyResult result, Action callback)
        {
            _exercise = exercise;
            _result = result;
            _callback = callback;

            _answers = new int[exercise.Texts.Length];
            string stepStartTitle = localizedStepStartTitle.GetLocalizedString();
            stepStart.Show(stepStartTitle, stepStartMessages, OnStepStartDone);
        }

        private void OnStepStartDone()
        {
            stepStart.Hide();
            taskWidget.Show(assignTask, assignInfo);
            relevanceAssign.Show(_exercise.Texts, OnAssignDone);
        }

        private void OnAssignDone(int[] answers)
        {
            relevanceAssign.Hide();

            _answers = answers;
            _result.RelevanceQuizzesAnswers = new int[zeroRelevanceCount];

            _quizIndex = 0;

            taskWidget.Show(quizTask, null);

            ShowQuiz(_quizIndex);
        }

        private void ShowQuiz(int index)
        {
            _quizIndex = index;

            int zeroRelevanceIndex = 0;

            for (int i = 0; i < _exercise.Texts.Length; i++)
            {
                if (_answers[i] != 0) continue;

                if (index == zeroRelevanceIndex)
                {
                    MediaLiteracyExercise.Text text = _exercise.Texts[i];

                    quizEntry.Init(i, text.TitleAudio, text.PreviewAudio, text.AuthorAudio);
                    quizChoice.Show(quizTitle, quizChoices, OnQuizDone);
                    break;
                }

                zeroRelevanceIndex++;
            }
        }

        private void OnQuizDone(int answer)
        {
            quizChoice.Hide();

            _result.RelevanceQuizzesAnswers[_quizIndex] = answer;

            if (_quizIndex < zeroRelevanceCount - 1)
            {
                ShowQuiz(_quizIndex + 1);
            }
            else
            {
                taskWidget.Show(solutionTask, solutionInfo);

                ReadableString[] titles = Array.ConvertAll(_exercise.Texts, t => t.TitleAudio);
                int[] solutions = Array.ConvertAll(_exercise.Texts, t => t.Relevance);
                relevanceSolution.Show(titles, _answers, solutions, OnSolutionDone);
            }
        }

        //TODO move points calc out of this
        private void OnSolutionDone(int[] points)
        {
            relevanceSolution.Hide();

            _result.RelevancePoints = points;

            int stars = _result.GetRelevanceStars();

            string relevancePhaseEndTitle = localizedRelevancePhaseEndTitle.GetLocalizedString();
            string relevancePhaseEndMessage = localizedRelevancePhaseEndMessage.GetLocalizedString();
            phaseEnd.Show(readableRelevancePhaseEndTitle, readableRelevancePhaseEndMessage, stars, 3, OnContinuePressed);
        }

        private void OnContinuePressed()
        {
            taskWidget.Hide();
            phaseEnd.Hide();

            _callback?.Invoke();
        }
    }
}