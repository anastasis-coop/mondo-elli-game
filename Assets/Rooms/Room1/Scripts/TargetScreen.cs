using System;
using System.Collections;
using Common;
using UnityEngine;
using UnityEngine.Rendering;

namespace Room1
{
    public class TargetScreen : MonoBehaviour
    {
        [SerializeField]
        private MeshRenderer _screenRenderer;

        [SerializeField]
        private string _colorPropertyName = "_AlbedoColor";
        
        [Space]
        [SerializeField, ColorUsage(false, false)]
        private Color _pulseColor;

        [SerializeField]
        private float _pulsePeriod;
        
        [Space]
        [SerializeField]
        private Transform _container;

        private ChildrenCentralizer _parentChildrenCentrilizer;

        private ItemsSet _prefabs;

        private Coroutine _coroutine;

        private SpriteRenderer _spriteRenderer;

        private void OnEnable()
        {
            if (_parentChildrenCentrilizer == null && transform.parent != null)
                _parentChildrenCentrilizer = transform.parent.GetComponent<ChildrenCentralizer>();

            if (_parentChildrenCentrilizer != null) _parentChildrenCentrilizer.RecalculateChildren();
        }
        
        private void OnDisable()
        {
            if (_parentChildrenCentrilizer != null) 
                _parentChildrenCentrilizer.RecalculateChildren();
        }

        private IEnumerator Pulse(float time, Action onEnd)
        {
            var id = Shader.PropertyToID(_colorPropertyName);
            var initialColor = _screenRenderer.material.GetColor(id);
            
            var from = initialColor;
            var to = _pulseColor;
            var actualTime = time - _pulsePeriod;
            for (float timer = 0, periodTimer = 0; timer <= actualTime; timer += Time.deltaTime, periodTimer += Time.deltaTime)
            {
                var materialColor = Color.Lerp(from, to, periodTimer / _pulsePeriod);
                _screenRenderer.material.SetColor(id, materialColor);
                if (periodTimer > _pulsePeriod)
                {
                    periodTimer -= _pulsePeriod;
                    (from, to) = (to, from);
                }

                yield return null;
            }

            from = _screenRenderer.material.GetColor(id);
            to = initialColor;
            for (float timer = 0; timer <= _pulsePeriod; timer += Time.deltaTime)
            {
                var materialColor = Color.Lerp(from, to, timer / _pulsePeriod);
                _screenRenderer.material.SetColor(id, materialColor);
                yield return null;
            }
            _screenRenderer.material.SetColor(id, initialColor);
            
            _coroutine = null;
            
            onEnd?.Invoke();
        }

        private void ShowByPrefab(GameObject prefab)
        {
                
            var obj = Instantiate(prefab, _container, false);
            foreach (var renderer in obj.GetComponentsInChildren<MeshRenderer>())
                renderer.shadowCastingMode = ShadowCastingMode.Off;
            obj.transform.rotation = Quaternion.Euler(0, 45, 0);
        }

        private void ShowBySprite(Sprite sprite)
        {
            if (_spriteRenderer == null)
                _spriteRenderer = _container.GetComponent<SpriteRenderer>() ?? _container.gameObject.AddComponent<SpriteRenderer>();

            _spriteRenderer.sprite = sprite;
        }
        
        public void Prepare(ItemsSet set)
        {
            _prefabs = set;
        }
        
        public void ChangeTarget(string targetPrefabName)
        {
            while (_container.childCount != 0)
            {
                var child = _container.GetChild(0);
                child.SetParent(null);
                Destroy(child.gameObject);
            }

            var item = _prefabs[targetPrefabName];

            ShowBySprite(item.Sprite);
            if (item.Sprite == null) ShowByPrefab(item.Prefab);
        }

        public void PulseForTime(float time, Action onEnd)
        {
            if (_coroutine != null)
            {
                onEnd?.Invoke();
                return;
            }

            _coroutine = StartCoroutine(Pulse(time, onEnd));
        }
    }
}
