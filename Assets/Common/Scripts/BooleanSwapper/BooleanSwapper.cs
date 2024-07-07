using UnityEngine;

namespace Common
{
    public abstract class BooleanSwapper : MonoBehaviour
    {
        private enum DefaultState
        {
            None,
            True,
            False,
        }
        
        [SerializeField]
        private DefaultState _default;

        [Space]
        [SerializeField, Min(0), Tooltip("If 0 it won't go back to default state")]
        private float _returnToDefaultTime;
        
        private float _backToDefaultTimer;

        private bool _ignoreReturnToDefaultTime;
        
        public abstract bool CurrentValue { get; }

        protected virtual void Awake() => Set(_default);

        private void Update()
        {
            if (_backToDefaultTimer == 0 || _ignoreReturnToDefaultTime) return;

            _backToDefaultTimer -= Time.deltaTime;

            if (_backToDefaultTimer > 0) return;

            _backToDefaultTimer = 0;
            Set(DefaultState.None);
        }

        private void Set(DefaultState state)
        {
            if (state == DefaultState.None) SetNone();
            else
            {
                SetBool(state == DefaultState.True);
                _backToDefaultTimer = _returnToDefaultTime;
            }
        }

        protected abstract void SetNone();

        protected abstract void SetBool(bool value);

        public void Set(bool value) => Set(value ? DefaultState.True : DefaultState.False);

        /// <summary>
        /// Toggles between the <c>true</c> and <c>false</c> materials. If the state of
        /// the BooleanMaterialSwapper is <c>None</c>, it toggles directly to <c>true</c>.
        /// </summary>
        /// <returns>The new value</returns>
        public bool Toggle()
        {
            var value = !CurrentValue;
            Set(value);
            return value;
        }

#if UNITY_EDITOR
        private void OnValidate() => Set(_default);
#endif
    }
}