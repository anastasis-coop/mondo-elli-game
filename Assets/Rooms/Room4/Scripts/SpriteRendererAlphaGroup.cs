using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Room4
{
    [ExecuteInEditMode]
    public class SpriteRendererAlphaGroup : MonoBehaviour
    {
        private static int GetChildrenCount(Transform t)
        {
            if (t.childCount == 0) return 0;
            return t.childCount + t.Cast<Transform>().Sum(GetChildrenCount);
        }
        
        [SerializeField, Range(0, 1)]
        private float _alpha;

        private int _childrenCount;
        private SpriteRenderer[] _renderers;

        private TweenerCore<float, float, FloatOptions> _tween;

        public SpriteRenderer[] Renderers 
        {
            get
            {
                if (_renderers == null)
                {
                    _childrenCount = GetChildrenCount(transform);
                    _renderers = GetComponentsInChildren<SpriteRenderer>(true);
                }

                return _renderers;
            }
        }

        public float Alpha
        {
            get => _alpha;
            set
            {
                foreach (var renderer in Renderers)
                {
                    var color = renderer.material.color;
                    color.a = value;
                    renderer.material.color = color;
                }

                _alpha = value;
            }
        }

        private void Awake() => _renderers = Renderers; //Initialize here if not before

        private void Update()
        {
            if (GetChildrenCount(transform) == _childrenCount) return;
            _renderers = null;
            _renderers = Renderers;
        }

        public TweenerCore<float, float, FloatOptions> DOAlpha(float endValue, float duration)
        {
            DOKill();
            return _tween = DOTween.To(() => Alpha, v => Alpha = v, endValue, duration);
        }

        public void DOKill() => _tween?.Kill();
    }
}
