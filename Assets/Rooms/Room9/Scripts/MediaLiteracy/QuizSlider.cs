using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Room9
{
    public class QuizSlider : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI titleLabel;

        [SerializeField]
        private ReadableLabel titleReadable;

        [SerializeField]
        private Slider slider;

        private Action<float> _callback;

        public void Show(LocalizedReadableStringAsset title, Action<float> callback)
        {
            _callback = callback;
            
            gameObject.SetActive(true);
            
            StopAllCoroutines();
            StartCoroutine(ShowRoutine(title));
        }

        private IEnumerator ShowRoutine(LocalizedReadableStringAsset localizedTitle)
        {
            var handle = localizedTitle.LoadAssetAsync();

            yield return handle;

            ReadableString title = handle.Result.Value;

            titleLabel.text = title?.String;
            titleReadable.AudioClip = title?.AudioClip;
        }

        public void OnContinueButtonPressed()
        {
            _callback?.Invoke(slider.value);
        }

        public void Hide()
        {
            _callback = null;
            gameObject.SetActive(false);
        }
    }
}