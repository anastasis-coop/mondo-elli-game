using System;
using TMPro;
using UnityEngine;

namespace Room9
{
    public class StepEnd : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI titleLabel;

        [SerializeField]
        private ReadableLabel titleReadableLabel;

        [SerializeField]
        private TextMeshProUGUI messageLabel;

        [SerializeField]
        private ReadableLabel messageReadableLabel;

        [SerializeField]
        private StarsRating stars;

        private Action _callback;

        public void Show(ReadableString title, ReadableString message, int starsCount, int starsMax, Action callback)
        {
            titleLabel.text = title.String;
            titleReadableLabel.AudioClip = title.AudioClip;
            messageLabel.text = message.String;
            messageReadableLabel.AudioClip = message.AudioClip;

            stars.Init(starsCount, starsMax);

            _callback = callback;

            gameObject.SetActive(true);
        }

        public void OnContinueButtonPressed()
        {
            _callback?.Invoke();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}