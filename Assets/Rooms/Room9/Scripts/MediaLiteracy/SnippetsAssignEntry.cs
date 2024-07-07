using System;
using TMPro;
using UnityEngine;

namespace Room9
{
    public class SnippetsAssignEntry : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI titleLabel;

        [SerializeField]
        private ReadableLabel titleReadable;

        [SerializeField]
        private TextMeshProUGUI previewLabel;

        [SerializeField]
        private ReadableLabel previewReadable;

        [SerializeField]
        private TextMeshProUGUI authorLabel;

        [SerializeField]
        private ReadableLabel authorReadable;

        [SerializeField]
        private TextMeshProUGUI snippetsCountLabel;

        [SerializeField]
        private string snippetsCountFormat;

        [SerializeField]
        private TextMeshProUGUI valueLabel;

        private Vector2Int _range;

        public int TextIndex { get; private set; }
        public int Value { get; private set; }

        private Action _valueChanged;

        public void Init(int textIndex, ReadableString title, ReadableString preview, ReadableString author, Vector2Int range, Action valueChanged)
        {
            titleLabel.text = title?.String;
            previewLabel.text = preview?.String;
            authorLabel.text = author?.String;

            titleReadable.AudioClip = title?.AudioClip;
            previewReadable.AudioClip = preview?.AudioClip;
            authorReadable.AudioClip = author?.AudioClip;

            _range = range;
            _valueChanged = valueChanged;

            TextIndex = textIndex;
            Value = range.x;
            valueLabel.text = Value.ToString();

            snippetsCountLabel.text = string.Format(snippetsCountFormat, range.y);
        }
        public void OnMinusButtonPressed()
        {
            Value = Mathf.Max(_range.x, Value - 1);
            valueLabel.text = Value.ToString();

            _valueChanged?.Invoke();
        }

        public void OnPlusButtonPressed()
        {
            Value = Mathf.Min(_range.y, Value + 1);
            valueLabel.text = Value.ToString();

            _valueChanged?.Invoke();
        }
    }
}
