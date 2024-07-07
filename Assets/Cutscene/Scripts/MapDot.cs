using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Cutscene
{
    public class MapDot : MonoBehaviour
    {
        [SerializeField]
        private Image _ring;

        [SerializeField]
        private Ease _ease;

        [SerializeField]
        private float _finalScale;

        [SerializeField, Min(0)]
        private float _animationTime;

        [SerializeField, Min(0)]
        private float _restTime;

        private float _timer;

        private Tween _tween;

        private void OnEnable()
        {
            _timer = 0;
            StartTween();
        }

        private void Update()
        {
            if (_timer <= 0) return;

            _timer -= Time.deltaTime;

            if (_timer <= 0) StartTween();
        }

        private void OnDisable() => _tween?.Kill();

        private void StartWaiting()
        {
            _tween = null;
            _timer = _restTime;
        }

        private void StartTween()
        {
            _tween = CreateTween();
            _tween.OnComplete(StartWaiting);
            _tween.Play();
        }

        private Tween CreateTween()
        {
            var color = _ring.color;
            color.a = 1;
            _ring.color = color;
            _ring.transform.localScale = Vector3.one;

            var sequence = DOTween.Sequence();
            sequence.Append(_ring.transform.DOScale(_finalScale, _animationTime).SetEase(_ease));
            sequence.Insert(0, _ring.DOFade(0, _animationTime));

            return sequence;
        }
    }
}