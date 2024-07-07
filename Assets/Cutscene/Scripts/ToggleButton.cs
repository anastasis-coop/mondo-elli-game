using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Cutscene
{
    [RequireComponent(typeof(Button))]
    public class ToggleButton : MonoBehaviour
    {
        [SerializeField]
        private bool _isOn;

        [SerializeField]
        private Sprite _onSprite;

        [SerializeField]
        private Sprite _offSprite;

        [SerializeField]
        public UnityEvent<bool> OnToggle;
        
        private Button _button;

        public bool interactable
        {
            get => _button.interactable;
            set => _button.interactable = value;
        }

        private void InitButton() => _button = GetComponent<Button>();

        private void UpdateGraphic()
        {
            var image = _button.targetGraphic as Image;
            if (image == null) return;
            image.sprite = _isOn ? _onSprite : _offSprite;
        }
        
        private void Awake()
        {
            InitButton();
            UpdateGraphic();
        }

        private void OnEnable() => _button.onClick.AddListener(OnClick);
        private void OnDisable() => _button.onClick.RemoveListener(OnClick);

        private void OnClick()
        {
            _isOn = !_isOn;
            UpdateGraphic();
            OnToggle.Invoke(_isOn);
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_button == null) InitButton();
            UpdateGraphic();
        }
#endif
    }
}
