using System;
using UnityEngine;
using UnityEngine.Localization;

namespace Room9
{
    public class TimeStep : MonoBehaviour
    {
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
        private TimeAssign timeAssign;

        [SerializeField]
        private StepEnd stepEnd;

        [SerializeField]
        private LocalizedString localizedStepEndTitle;

        [SerializeField] private ReadableString readableStepEndTitle;

        [SerializeField]
        private LocalizedString localizedStepEndMessage;
        
        [SerializeField] private ReadableString readableStepEndMessage;

        private MediaLiteracyResult _result;
        private Action _callback;

        public void Show(MediaLiteracyResult result, Action callback)
        {
            _result = result;
            _callback = callback;

            string stepStartTitle = localizedStepStartTitle.GetLocalizedString();
            stepStart.Show(stepStartTitle, stepStartMessages, OnStepStartDone);
        }

        private void OnStepStartDone()
        {
            stepStart.Hide();

            taskWidget.Show(task, info);
            timeAssign.Show(OnTimeAssignDone);
        }

        private void OnTimeAssignDone(int snippetMinutes, int productionMinutes, int revisionMinutes)
        {
            timeAssign.Hide();

            _result.SnippetMinutes = snippetMinutes;
            _result.ProductionMinutes = productionMinutes;
            _result.RevisionMinutes = revisionMinutes;

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