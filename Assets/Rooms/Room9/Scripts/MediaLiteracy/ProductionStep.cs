using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace Room9
{
    public class ProductionStep : MonoBehaviour
    {
        [SerializeField]
        private Timer timer;

        [SerializeField]
        private Stopwatch stopwatch;

        [SerializeField]
        private StepStart stepStart;

        [SerializeField]
        private LocalizedString localizedStepStartTitle;

        [SerializeField]
        private BigElloSaysConfig[] stepStartMessages;

        [SerializeField]
        private LocalizedString localizedPickStepStartTitle;

        [SerializeField]
        private BigElloSaysConfig[] pickStepStartMessages;

        [SerializeField]
        private LocalizedString localizedEditorStepStartTitle;

        [SerializeField]
        private BigElloSaysConfig[] editorStepStartMessages;

        [SerializeField]
        private LocalizedReadableStringAsset pickTask;

        [SerializeField]
        private LocalizedReadableStringAsset pickInfo;

        [SerializeField]
        private LocalizedReadableStringAsset editorTask;

        [SerializeField]
        private LocalizedReadableStringAsset editorInfo;

        [SerializeField]
        private TaskWidget taskWidget;

        [SerializeField]
        private ProductionPick productionPick;

        [SerializeField]
        private ProductionEditor productionEditor;

        [SerializeField]
        private StepEnd stepEnd;

        [SerializeField]
        private LocalizedString localizedPickStepEndTitle;
        
        [SerializeField] private ReadableString readablePickStepEndTitle;

        [SerializeField]
        private LocalizedString localizedPickStepEndMessage;
        
        [SerializeField] private ReadableString readablePickStepEndMessage;

        [SerializeField]
        private LocalizedString localizedSnippetsStepFailTitle;
        
        [SerializeField] private ReadableString readableSnippetsStepFailTitle;

        [SerializeField]
        private LocalizedString localizedSnippetsStepFailMessage;
        
        [SerializeField] private ReadableString readableSnippetsStepFailMessage;

        [SerializeField]
        private LocalizedString localizedEditorStepEndTitle;
        
        [SerializeField] private ReadableString readableEditorStepEndTitle;

        [SerializeField]
        private LocalizedString localizedEditorStepEndMessage;
        
        [SerializeField] private ReadableString readableEditorStepEndMessage;

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
            string pickStepStartTitle = localizedPickStepStartTitle.GetLocalizedString();
            stepStart.Show(pickStepStartTitle, pickStepStartMessages, OnPickStepStartDone);
        }

        private void OnPickStepStartDone()
        {
            stepStart.Hide();

            if (!timer.itIsEnd) timer.activation = true;
            stopwatch.enabled = true;

            taskWidget.Show(pickTask, pickInfo);
            productionPick.Show(_exercise.Texts, _result.SnippetEstimates, OnPickDone);
        }

        private void OnPickDone(List<int>[] choices)
        {
            productionPick.Hide();

            _result.ProductionPicks = choices;

            for (int textIndex = 0; textIndex < choices.Length; textIndex++)
            {
                foreach (int snippetIndex in choices[textIndex])
                {
                    _result.ProductionPoints += _exercise.Texts[textIndex].Snippets[snippetIndex].Relevance;
                }
            }

            timer.activation = false;
            stopwatch.enabled = false;

            int stars = 1;

            string pickStepEndTitle = localizedPickStepEndTitle.GetLocalizedString();
            string pickStepEndMessage = localizedPickStepEndMessage.GetLocalizedString();
            stepEnd.Show(readablePickStepEndTitle, readablePickStepEndMessage, stars, 1, OnPickStepEndDone);
        }

        private void OnPickStepEndDone()
        {
            stepEnd.Hide();

            bool estimatesCorrect = true;

            for (int textIndex = 0; textIndex < _exercise.Texts.Length; textIndex++)
            {
                int estimate = _result.SnippetEstimates[textIndex];
                int picks = _result.ProductionPicks[textIndex].Count;

                const int MAX_DELTA = 1;

                if (Mathf.Abs(estimate - picks) > MAX_DELTA)
                {
                    estimatesCorrect = false;
                    break;
                }
            }

            _result.SnippetEstimatesMet = estimatesCorrect;

            if (estimatesCorrect)
                ShowEditorStepStart();
            else
            {
                string snippetsStepFailTitle = localizedSnippetsStepFailTitle.GetLocalizedString();
                string snippetsStepFailMessage = localizedSnippetsStepFailMessage.GetLocalizedString();
                stepEnd.Show(readableSnippetsStepFailTitle, readableSnippetsStepFailMessage, 0, 1, ShowEditorStepStart);
            }
        }

        private void ShowEditorStepStart()
        {
            taskWidget.Hide();
            stepEnd.Hide();
            string editorStepStartTitle = localizedEditorStepStartTitle.GetLocalizedString();
            stepStart.Show(editorStepStartTitle, editorStepStartMessages, OnEditorStepStartDone);
        }

        private void OnEditorStepStartDone()
        {
            stepStart.Hide();

            if (!timer.itIsEnd) timer.activation = true;
            stopwatch.enabled = true;

            taskWidget.Show(editorTask, editorInfo);
            productionEditor.Show(_exercise.Texts, _result.ProductionPicks, OnEditorDone);
        }

        private void OnEditorDone(string result, List<AudioClip> audio)
        {
            productionEditor.Hide();

            timer.activation = false;
            stopwatch.enabled = false;

            _result.ProductionResult = result;
            _result.ProductionAudio = audio;

            int stars = _result.GetProductionStars();

            string editorStepEndTitle = localizedEditorStepEndTitle.GetLocalizedString();
            string editorStepEndMessage = localizedEditorStepEndMessage.GetLocalizedString();
            stepEnd.Show(readableEditorStepEndTitle, readableEditorStepEndMessage, stars, 2, OnEditorStepEndDone);
        }

        private void OnEditorStepEndDone()
        {
            taskWidget.Hide();
            stepEnd.Hide();
            _callback?.Invoke();
        }
    }
}