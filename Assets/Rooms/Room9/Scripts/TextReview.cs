using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Room9
{
    public class TextReview : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI titleLabel;

        [SerializeField]
        private ReadableLabel titleReadable;

        [SerializeField]
        private GameObject tagLabelPrefab;

        [SerializeField]
        private Transform tagLabelsRoot;

        [SerializeField]
        private GameObject textLabelPrefab;

        [SerializeField]
        private Transform textLabelsRoot;

        [SerializeField]
        private GameObject cancelRoot;

        [SerializeField]
        private Color[] highlightColors;

        private List<GameObject> instances = new();
        private Action _continueCallback;
        private Action _cancelCallback;

        public void Show(ReadableString title, ReadableString[] tags, ReadableString[] texts,
            Action continueCallback, Action cancelCallback = null, bool highlight = false)
        {
            titleLabel.gameObject.SetActive(title != null);
            titleLabel.text = title?.String;
            titleReadable.AudioClip = title?.AudioClip;

            if (tags != null) SpawnLabels(tags, tagLabelPrefab, tagLabelsRoot, false);
            if (texts != null) SpawnLabels(texts, textLabelPrefab, textLabelsRoot, highlight);

            _continueCallback = continueCallback;
            _cancelCallback = cancelCallback;

            cancelRoot.SetActive(cancelCallback != null);

            gameObject.SetActive(true);
        }

        // HACK this is a workaround, find a better way
        public void AppendMultiClipLabel(string text, List<AudioClip> clips)
        {
            GameObject entry = Instantiate(textLabelPrefab, textLabelsRoot);
            entry.GetComponent<TextMeshProUGUI>().text = text;
            entry.GetComponent<ReadableLabel>().AudioClips = clips;

            instances.Add(entry);
        }

        private void SpawnLabels(ReadableString[] texts, GameObject prefab, Transform root, bool highlight)
        {
            int colorIndex = 0;

            foreach (ReadableString text in texts)
            {
                string str = text.String;

                if (highlight)
                {
                    string rgba = ColorUtility.ToHtmlStringRGBA(highlightColors[colorIndex]);
                    str = $"<mark=#{rgba}>{str}</mark>";

                    colorIndex = (colorIndex + 1) % highlightColors.Length;
                }

                GameObject entry = Instantiate(prefab, root);
                entry.GetComponent<TextMeshProUGUI>().text = str;
                entry.GetComponent<ReadableLabel>().AudioClip = text.AudioClip;

                instances.Add(entry);
            }
        }

        public void OnContinueButtonPressed()
        {
            _continueCallback?.Invoke();
        }

        public void OnCancelButtonPressed()
        {
            _cancelCallback?.Invoke();
        }

        public void Hide()
        {
            foreach (var entry in instances)
            {
                Destroy(entry);
            }

            instances.Clear();

            _continueCallback = null;
            gameObject.SetActive(false);
        }
    }
}
