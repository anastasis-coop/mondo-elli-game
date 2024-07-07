using System;
using TMPro;
using UnityEngine;

namespace Room9
{
    public class FlexibilityEntry : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI maxLabel;

        [SerializeField]
        private string maxLabelFormat;

        [SerializeField]
        private TextMeshProUGUI valueLabel;

        private Vector2Int _range;

        public int Value { get; private set; }

        private Action _valueChanged;

        public void Init(Vector2Int range, Action valueChanged)
        {
            _range = range;
            _valueChanged = valueChanged;

            // HACK to avoid passing a current value from outside
            if (Value < range.x) Value = range.x;
            if (Value > range.y) Value = range.y;

            valueLabel.text = Value.ToString();

            maxLabel.text = string.Format(maxLabelFormat, range.y);
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