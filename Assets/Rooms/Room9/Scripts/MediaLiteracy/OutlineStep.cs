using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Localization;

namespace Room9
{
    public class OutlineStep : MonoBehaviour
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
        private ReadableString[] outlineEntryStringsAudio;

        [SerializeField]
        private ReadableString[] outlineEntryStringsAudioCondensed;

        [SerializeField]
        private LocalizedReadableStringAsset sortTask;

        [SerializeField]
        private LocalizedReadableStringAsset sortInfo;

        [SerializeField]
        private TaskWidget taskWidget;

        [SerializeField]
        private OutlineSort outlineSort;

        [SerializeField]
        private BigElloSaysConfig[] tutorials;

        [SerializeField]
        private StepEnd stepEnd;

        [SerializeField]
        private LocalizedString localizedStepEndTitle;
        
        [SerializeField] private ReadableString readableStepEndTitle;

        [SerializeField]
        private LocalizedString localizedStepEndMessage;
        
        [SerializeField] private ReadableString readableStepEndMessage;

        [SerializeField]
        private LocalizedString localizedStepEndRetry;
        
        [SerializeField] private ReadableString readableStepEndRetry;

        private MediaLiteracyResult _result;
        private event Action _callback;

        private bool _help = false;

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
            taskWidget.Show(sortTask, sortInfo);
            outlineSort.Show(_help, outlineEntryStringsAudioCondensed, true, OnOutlineSorted);
            DOVirtual.DelayedCall(0.3f, delegate
            {
                bigEllo.SetCurrentMessagesBatch(tutorials);
                bigEllo.ShowNextMessage();
            });
        }

        private void OnOutlineSorted(int[] answers)
        {
            outlineSort.Hide();

            bool correct = true;

            for(int i = 0; i < outlineEntryStringsAudioCondensed.Length; i++)
            {
                if (answers[i] == i) continue;
                correct = false;
                break;
            }

            string stepEndTitle = localizedStepEndTitle.GetLocalizedString();

            if (!correct)
            {
                string stepEndRetryMessage = localizedStepEndRetry.GetLocalizedString();

                stepEnd.Show(readableStepEndTitle, readableStepEndRetry, 0, 2, OnRetryPressed);
            }
            else
            {
                _result.OutlineNeededHelp = _help;
                int stars = _result.GetOutlineStars();

                string stepEndMessage = localizedStepEndMessage.GetLocalizedString();

                stepEnd.Show(readableStepEndTitle, readableStepEndMessage, stars, 2, OnContinuePressed);
            }
        }

        private void OnRetryPressed()
        {
            stepEnd.Hide();
            taskWidget.Show(sortTask, sortInfo);
            outlineSort.Show(_help = true, outlineEntryStringsAudioCondensed, true, OnOutlineSorted);
        }

        private void OnContinuePressed()
        {
            taskWidget.Hide();
            stepEnd.Hide();
            _callback?.Invoke();
        }
    }
}
