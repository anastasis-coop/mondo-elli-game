using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Room9
{
    public class ProductionEditorCard : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI label;

        [SerializeField]
        private ReadableLabel readable;

        [SerializeField]
        private Image image;

        private ProductionEditor.CardConfig _cardConfig;
        public ProductionEditor.CardConfig CardConfig
        {
            get => _cardConfig;
            set
            {
                _cardConfig = value;

                if (_cardConfig == null) return;

                label.text = _cardConfig.TextAudio?.String;
                readable.AudioClip = _cardConfig.TextAudio?.AudioClip;
                label.color = _cardConfig.Color;
                image.color = _cardConfig.Color;
            }
        }

        private Action<ProductionEditorCard> _pressedCallback;

        public void Init(Action<ProductionEditorCard> onPressed)
        {
            _pressedCallback = onPressed;
        }

        public void OnPressed() => _pressedCallback?.Invoke(this);
    }
}