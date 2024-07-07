using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Room8
{
    public class CalculatorScreen : MonoBehaviour
    {
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        
        [SerializeField]
        private MeshRenderer _screenRenderer;

        [SerializeField]
        private TMP_Text _text;

        [SerializeField]
        private GameObject _caretObject;

        [SerializeField]
        private bool _showCaret;

        private float _colorPulseTimer;

        private void Awake()
        {
            ShowCaret(_showCaret);
            _text.text = "";
        }

        private void Update()
        {
            if (_colorPulseTimer <= 0) return;

            _colorPulseTimer -= Time.deltaTime;

            if (_colorPulseTimer > 0) return;
            
            _screenRenderer.material.SetColor(EmissionColor, Color.white);
        }

        private void ShowCaret(bool show)
        {
            _showCaret = show;
            if (_caretObject != null) _caretObject.SetActive(_showCaret);
        }

        public void ShowSequence(IEnumerable<int> sequence) => _text.text = string.Join("", sequence);

        public void Clear() => _text.text = "";

        public void PulseColor(Color color, float timer)
        {
            _screenRenderer.material.SetColor(EmissionColor, color);
            _colorPulseTimer = timer;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ShowCaret(_showCaret);
        }
#endif
    }
}
