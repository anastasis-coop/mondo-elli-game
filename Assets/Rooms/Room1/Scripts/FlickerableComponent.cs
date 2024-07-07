using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

namespace Room1
{
    public class FlickerableComponent : MonoBehaviour
    {
        [SerializeField]
        private MeshRenderer _renderer;

        [SerializeField]
        private string _emissionColorProperty = "_Color";

        [SerializeField]
        private Light _light;
        
        [SerializeField, Min(0)]
        private float _onLightIntensity;

        [SerializeField, Min(0)]
        private float _offLightIntensity;

        [SerializeField, Min(0)]
        private float _minTimer;

        [SerializeField]
        private float _maxTimer;

        private Color? _initialRendererColor;
        private Color? _initialLightColor;
        
        private Coroutine _coroutine;
        
        private void Initialize()
        {
            if (_initialRendererColor.HasValue && _initialLightColor.HasValue) return;
            _initialRendererColor =
                _renderer == null ? Color.black : _renderer.material.GetColor(_emissionColorProperty);
            _initialLightColor = _light == null ? Color.black : _light.color;
        }

        private void SetMaterial(Color color)
        {
            if (_renderer == null) return;
            _renderer.material.SetColor(_emissionColorProperty, color);
        }

        private void SetLight(Color color, float intensity)
        {
            if (_light == null) return;
            _light.color = color;
            _light.intensity = intensity;
        }

        private IEnumerator FlickerRoutine(Color color)
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(_minTimer, _maxTimer));
                SetMaterial(color);
                SetLight(color, _onLightIntensity);
                yield return new WaitForSeconds(Random.Range(_minTimer, _maxTimer));
                SetMaterial(_initialRendererColor ?? Color.black);
                SetLight(_initialLightColor ?? Color.black, _offLightIntensity);
            }
        }

        public void StartFlickering(Color color)
        {
            //Makes sure it is initialized
            Initialize();
            
            //Makes sure it can flicker
            gameObject.SetActive(true);
            enabled = true;
            
            StopFlickering();
            _coroutine = StartCoroutine(FlickerRoutine(color));
        }

        public void StopFlickering()
        {
            if (_coroutine != null) StopCoroutine(_coroutine);
            _coroutine = null;
            
            //Makes sure it is initialized
            Initialize();
            
            SetMaterial(_initialRendererColor ?? Color.black);
            SetLight(_initialLightColor ?? Color.black, _offLightIntensity);
        }
    }
    
}
