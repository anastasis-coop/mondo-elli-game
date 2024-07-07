using System;
using UnityEngine;
using UnityEngine.Localization;

namespace Room9
{
    public class SnippetsStep : MonoBehaviour
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
        private LocalizedReadableStringAsset task;

        [SerializeField]
        private LocalizedReadableStringAsset info;

        [SerializeField]
        private TaskWidget taskWidget;

        [SerializeField]
        private SnippetsAssign snippetAssign;

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

            timer.activation = true;
            stopwatch.Reset();
            stopwatch.enabled = true;

            taskWidget.Show(task, info);

            snippetAssign.Show(_exercise.Texts, OnSnippetAssignDone);
        }

        private void OnSnippetAssignDone(int[] answer)
        {
            snippetAssign.Hide();

            _result.SnippetEstimates = answer;

            timer.activation = false;
            stopwatch.enabled = false;

            string stepEndTitle = localizedStepEndTitle.GetLocalizedString();
            string stepEndMessage = localizedStepEndMessage.GetLocalizedString();
            stepEnd.Show(readableStepEndTitle, readableStepEndMessage, 1, 1, OnStepEndDone);
        }

        private void OnStepEndDone()
        {
            taskWidget.Hide();
            stepEnd.Hide();

            _callback?.Invoke();
        }
    }
}
