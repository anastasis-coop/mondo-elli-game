using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Room9
{
    public class ThemeIntro : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI themeLabel;

        [SerializeField]
        private TextMeshProUGUI tipLabel;

        [SerializeField]
        private ReadableLabel themeReadable;

        [SerializeField]
        private ReadableLabel tipReadable;

        [SerializeField]
        private GameObject tipRoot;

        private Action _callback;

        public void Show(ReadableString theme, ReadableString[] tips, Action callback)
        {
            themeLabel.text = theme?.String;
            themeReadable.AudioClip = theme?.AudioClip;

            tipRoot.SetActive(tips != null);

            if (tips != null)
            {
                var random = tips[Random.Range(0, tips.Length)];
                tipLabel.text = random?.String;
                tipReadable.AudioClip = random?.AudioClip;
            }
           
            _callback = callback;

            gameObject.SetActive(true);

            Invoke(nameof(OnContinueButtonPressed), 15);
        }

        public void OnContinueButtonPressed()
        {
            _callback?.Invoke();
        }

        public void Hide()
        {
            _callback = null;
            CancelInvoke(nameof(OnContinueButtonPressed));
            gameObject.SetActive(false);
        }
    }
}
