using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Room8
{
    public class Calculator : MonoBehaviour
    {
        public event Action<int> DigitClicked;

        public event Action Cleared;

        public event Action<IReadOnlyList<int>> Submitted;

        [SerializeField]
        private CalculatorScreen _screen;

        [SerializeField]
        private CalculatorButton _cancButton;

        [SerializeField]
        private CalculatorButton _equalButton;
        
        [SerializeField]
        private CalculatorDigitButton[] _digitsButtons;

        [SerializeField, Min(0)]
        private int _maximumSequenceLength;
        
        [SerializeField]
        private UnityEvent<IReadOnlyList<int>> _onSubmit;

        private readonly List<int> _sequence = new();

        public IReadOnlyList<int> CurrentSequence => _sequence;
        
        private void Awake() => _screen.ShowSequence(_sequence);

        private void OnEnable()
        {
            foreach (var digitButton in _digitsButtons)
            {
                digitButton.enabled = true;
                digitButton.onDigitClick.AddListener(OnDigitClick);
            }

            _cancButton.onClick.AddListener(Clear);
            _cancButton.enabled = true;

            _equalButton.onClick.AddListener(OnSubmit);
            _equalButton.enabled = true;
        }

        private void OnDisable()
        {
            foreach (var digitButton in _digitsButtons)
            {
                digitButton.enabled = false;
                digitButton.onDigitClick.RemoveListener(OnDigitClick);
            }

            _cancButton.onClick.RemoveListener(Clear);
            _cancButton.enabled = false;
            
            _equalButton.onClick.RemoveListener(OnSubmit);
            _equalButton.enabled = false;
        }

        private void OnSubmit()
        {
            _onSubmit.Invoke(_sequence);
            Submitted?.Invoke(_sequence);
        }

        private void OnDigitClick(int digit)
        {
            if (_sequence.Count >= _maximumSequenceLength) return;
            
            _sequence.Add(digit);
            _screen.ShowSequence(_sequence);
            DigitClicked?.Invoke(digit);
        }

        public void Clear()
        {
            _sequence.Clear();
            _screen.ShowSequence(_sequence);
            Cleared?.Invoke();
        }
    }
}
