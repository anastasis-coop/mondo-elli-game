using UnityEngine;
using UnityEngine.Events;

namespace Room8
{
    public class CalculatorDigitButton : CalculatorButton
    {
        [SerializeField, Range(0, 9)]
        private int _digit;
        
        [field: SerializeField]
        public UnityEvent<int> onDigitClick { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            onClick.AddListener(() => onDigitClick.Invoke(_digit));
        }
    }
}
