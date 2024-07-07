using System;
using TMPro;
using UnityEngine;

namespace Room9
{
    public class TimeEntry : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI valueLabel;

        private Vector2Int _range;

        public int Value { get; private set; }

        private event Action _valueChanged;

        public void Init(Vector2Int range, int value, Action valueChanged)
        {
            _valueChanged = valueChanged;
            _range = range;
            Value = value;

            valueLabel.text = value.ToString();
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