using System;
using DG.Tweening;
using UnityEngine;

namespace Cutscene
{
    [RequireComponent(typeof(RectTransform))]
    public class RollingDownUI : MonoBehaviour
    {
        private enum Position
        {
            Down = -1,
            Rest = 0,
            Up = 1
        }

        public event Action MovementEnded;

        [SerializeField]
        private Position _useAwakePositionAs = Position.Rest;

        [SerializeField]
        private Position _initialPosition = Position.Rest;

        [SerializeField, Min(0)]
        private float _span;

        [SerializeField, Min(0)]
        private float _animationTime;

        [SerializeField]
        private Ease _ease;

        private float _up;
        private float _rest;
        private float _down;

        private Tween _tween;

        private void Awake()
        {
            var rectTransform = (RectTransform)transform;
            switch (_useAwakePositionAs)
            {
                case Position.Down:
                    _down = rectTransform.anchoredPosition.y;
                    _up = _down + _span;
                    break;
                case Position.Up:
                    _up = rectTransform.anchoredPosition.y;
                    _down = _up - _span;
                    break;
                case Position.Rest:
                    _up = rectTransform.anchoredPosition.y + _span / 2;
                    _down = _up - _span;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("Unknown Position of value " + _useAwakePositionAs);
            }

            _rest = (_down + _up) / 2;

            rectTransform.anchoredPosition = new(rectTransform.anchoredPosition.x, PositionToValue(_initialPosition));
        }

        private float PositionToValue(Position position)
        {
            return position switch
            {
                Position.Down => _down,
                Position.Up => _up,
                _ => _rest
            };
        }

        private void OnTweenEnd()
        {
            _tween = null;
            MovementEnded?.Invoke();
        }

        private Tween MoveAt(Position position)
        {
            if (_tween != null)
            {
                _tween.Pause();
                _tween.Kill();
            }

            var rectTransform = (RectTransform)transform;

            var target = PositionToValue(position);

            return _tween = rectTransform.DOAnchorPosY(target, _animationTime).SetEase(_ease).OnComplete(OnTweenEnd);
        }

        [ContextMenu("Show")]
        public Tween Show() => MoveAt(Position.Rest);

        [ContextMenu("Move Up")]
        public Tween MoveUp() => MoveAt(Position.Up);

        [ContextMenu("Move Down")]
        public Tween MoveDown() => MoveAt(Position.Down);
    }
}