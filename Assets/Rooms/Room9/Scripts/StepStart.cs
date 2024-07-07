using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Room9
{
    public class StepStart : MonoBehaviour
    {
        [SerializeField]
        private MessageSystem bigEllo;

        [SerializeField]
        private TextMeshProUGUI titleLabel;

        private Action _callback;

        public void Show(string title, BigElloSaysConfig[] messages, Action callback)
        {
            titleLabel.text = title;

            _callback = callback;

            gameObject.SetActive(true);

            bigEllo.SetCurrentMessagesBatch(messages);
            bigEllo.messageBatchFinished.AddListener(OnBigElloMessagesFinished);

            if (messages.Length > 0)
            { 
                bigEllo.ShowNextMessage();
            }
            else
                Invoke(nameof(OnBigElloMessagesFinished), 2);
        }

        private void OnBigElloMessagesFinished()
        {
            CancelInvoke(nameof(OnBigElloMessagesFinished));
            bigEllo.messageBatchFinished.RemoveListener(OnBigElloMessagesFinished);
            _callback?.Invoke();
        }

        public void Hide()
        {
            CancelInvoke(nameof(OnBigElloMessagesFinished));
            bigEllo.messageBatchFinished.RemoveListener(OnBigElloMessagesFinished);
            _callback = null;
            gameObject.SetActive(false);
        }
    }
}
