using System.Collections;
using TMPro;
using UnityEngine;

namespace Room9
{
    public class TaskWidget : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI taskLabel;

        [SerializeField]
        private ReadableLabel taskReadable;

        [SerializeField]
        private GameObject infoButton;

        [SerializeField]
        private TextMeshProUGUI infoLabel;

        [SerializeField]
        private ReadableLabel infoReadable;

        public void Show(LocalizedReadableStringAsset task, LocalizedReadableStringAsset info)
        {
            gameObject.SetActive(true);

            StopAllCoroutines();
            StartCoroutine(ShowLocalizedRoutine(task, info));
        }

        private IEnumerator ShowLocalizedRoutine(LocalizedReadableStringAsset localizedTask, LocalizedReadableStringAsset localizedInfo)
        {
            taskLabel.SetText(string.Empty);
            infoLabel.SetText(string.Empty);

            infoButton.SetActive(false);

            var taskHandle = localizedTask.LoadAssetAsync();
            yield return taskHandle;

            ReadableString task = taskHandle.Result.Value;

            if (localizedInfo == null || localizedInfo.IsEmpty)
            {
                Show(task, null);
            }
            else
            {
                var infoHandle = localizedInfo.LoadAssetAsync();
                yield return infoHandle;
                Show(task, infoHandle.Result.Value);
            }
        }

        public void Show(ReadableString task, ReadableString info)
        {
            taskLabel.text = task?.String;
            infoLabel.text = info?.String;

            taskReadable.AudioClip = task?.AudioClip;
            infoReadable.AudioClip = info?.AudioClip;
            
            infoButton.SetActive(info != null);

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
        }
    }
}