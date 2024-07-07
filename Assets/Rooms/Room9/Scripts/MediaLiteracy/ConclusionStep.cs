using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;

namespace Room9
{
    public class ConclusionStep : MonoBehaviour
    {
        [SerializeField]
        private Timer timer;

        [SerializeField]
        private Stopwatch stopwatch;

        [SerializeField]
        private StepStart stepStart;

        [SerializeField, FormerlySerializedAs("localizedEditorStepStart")]
        private LocalizedString localizedStepStartTitle;

        [SerializeField]
        private BigElloSaysConfig[] stepStartMessages;

        [SerializeField]
        private LocalizedString localizedTagsStepStart;

        [SerializeField]
        private BigElloSaysConfig[] tagsStepStartMessages;

        [SerializeField]
        private LocalizedString localizedTitleStepStart;

        [SerializeField]
        private BigElloSaysConfig[] titleStepStartMessages;

        [SerializeField]
        private QuizMultipleChoice tagsQuiz;

        [SerializeField]
        private QuizChoice titleQuiz;

        [SerializeField]
        private StepEnd stepEnd;

        [SerializeField]
        private LocalizedString localizedTagsStepEndTitle;
        
        [SerializeField] private ReadableString readableTagsStepEndTitle;

        [SerializeField]
        private LocalizedString localizedTagsStepEndSuccess;
        
        [SerializeField] private ReadableString readableTagsStepEndSuccess;

        [SerializeField]
        private LocalizedString localizedTagsStepEndFail;
        
        [SerializeField] private ReadableString readableTagsStepEndFail;

        [SerializeField]
        private LocalizedString localizedTitleStepEndTitle;
        
        [SerializeField] private ReadableString readableStepEndTitle;

        [SerializeField]
        private LocalizedString localizedTitleStepEndSuccess;
        
        [SerializeField] private ReadableString readableStepEndSuccess;

        [SerializeField]
        private LocalizedString localizedTitleStepEndFail;
        
        [SerializeField] private ReadableString readableStepEndFail;

        [SerializeField]
        private Vector2 normalizedTimeRange;

        [SerializeField]
        private LocalizedString localizedTimeStarLostTitle;
        [SerializeField] private ReadableString readableTimeStarLostTitle;

        [SerializeField]
        private LocalizedString localizedTimeStarLostOver;
        
        [SerializeField] private ReadableString readableTimeStarLostOver;

        [SerializeField]
        private LocalizedString localizedTimeStarLostUnder;
        
        [SerializeField] private ReadableString readableTimeStarLostUnder;

        private MediaLiteracyExercise _exercise;
        private MediaLiteracyResult _result;
        private Action _callback;

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

            string tagsStepStartTitle = localizedTagsStepStart.GetLocalizedString();
            stepStart.Show(tagsStepStartTitle, tagsStepStartMessages, OnTagsStepStartDone);
        }

        private void OnTagsStepStartDone()
        {
            stepStart.Hide();

            if (!timer.itIsEnd) timer.activation = true;
            stopwatch.enabled = true;

            tagsQuiz.Show(_exercise.TagsQuiz.QuestionAudio, _exercise.TagsQuiz.ChoicesAudio, OnTagsDone, _exercise.TagsQuiz.Correct, true, 3);
        }

        private void OnTagsDone(HashSet<int> answers)
        {
            tagsQuiz.Hide();

            timer.activation = false;
            stopwatch.enabled = false;

            bool correct = answers.SetEquals(_exercise.TagsQuiz.Correct);

            answers.IntersectWith(_exercise.TagsQuiz.Correct);

            _result.TagsCorrectAnswers = answers.Count;

            int stars = correct ? 1 : 0;

            string tagsStepEndTitle = localizedTagsStepEndTitle.GetLocalizedString();
            string tagsStepEndSuccessMessage = localizedTagsStepEndSuccess.GetLocalizedString();
            string tagsStepEndFailMessage = localizedTagsStepEndFail.GetLocalizedString();
            var message = correct ? readableTagsStepEndSuccess : readableTagsStepEndFail;

            stepEnd.Show(readableTagsStepEndTitle, message, stars, 1, OnTagsStepEndDone);
        }

        private void OnTagsStepEndDone()
        {
            tagsQuiz.Hide();
            stepEnd.Hide();
            string titleStepStartTitle = localizedTitleStepStart.GetLocalizedString();
            stepStart.Show(titleStepStartTitle, titleStepStartMessages, OnTitleStepStartDone);
        }

        private void OnTitleStepStartDone()
        {
            stepStart.Hide();

            if (!timer.itIsEnd) timer.activation = true;
            stopwatch.enabled = true;

            titleQuiz.Show(_exercise.TitleQuiz.QuestionAudio, _exercise.TitleQuiz.ChoicesAudio, OnTitleQuizDone, _exercise.TitleQuiz.Correct[0], true);
        }

        private void OnTitleQuizDone(int answer)
        {
            titleQuiz.Hide();

            timer.activation = false;
            stopwatch.enabled = false;

            bool correct = answer == _exercise.TitleQuiz.Correct[0];

            _result.TitleCorrectAnswer = correct;
            _result.TimeEstimatesMet = !timer.itIsEnd;

            int stars = correct ? 1 : 0;
            string titleStepEndSuccessMessage = localizedTitleStepEndSuccess.GetLocalizedString();
            string titleStepEndFailMessage = localizedTitleStepEndFail.GetLocalizedString();
            string titleStepEndTitle = localizedTitleStepEndTitle.GetLocalizedString();
            var message = correct ? readableStepEndSuccess : readableStepEndFail;
            stepEnd.Show(readableStepEndTitle, message, stars, 1, OnTitleStepEndDone);
        }

        private void OnTitleStepEndDone()
        {
            stepEnd.Hide();

            string timeStarLostTitle = localizedTimeStarLostTitle.GetLocalizedString();
            string timeStarLostMessageOver = localizedTimeStarLostOver.GetLocalizedString();
            string timeStarLostMessageUnder = localizedTimeStarLostUnder.GetLocalizedString();

            float ratio = stopwatch.ElapsedSeconds / (float)timer.totalTime;

            if (ratio > normalizedTimeRange.y)
                stepEnd.Show( readableTimeStarLostTitle, readableTimeStarLostOver, 0, 1, Callback);
            else if (ratio < normalizedTimeRange.x)
                stepEnd.Show( readableTimeStarLostTitle, readableTimeStarLostUnder, 0, 1, Callback);
            else
                Callback();
        }

        private void Callback()
        {
            stepEnd.Hide();
            _callback?.Invoke();
        }
    }
}