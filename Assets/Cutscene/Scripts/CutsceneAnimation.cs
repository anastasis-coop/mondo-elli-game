using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Cutscene
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CutsceneAnimation : MonoBehaviour
    {
        public event Action<bool> SafeToRemoveChanged;
        [SerializeField]
        private string _startTrigger = "Start";
        [SerializeField]
        private string _stopTrigger = "Stop";

        [Space]
        [SerializeField]
        private CanvasGroup _canvasGroup;

        [SerializeField, Min(0)]
        private float _fadeTime;

        [SerializeField]
        private bool _visibleAtStart;

        [SerializeField]
        private CutsceneStep[] _steps;

        private int _latestStep = -1;
        private int _startId;
        private int _stopId;
        private Animator _animator;
        private Tween _showTween;

        private readonly Stack<IEnumerator<ICutsceneStep>> _stepsStack = new();

        public bool IsSafeToRemove { get; set; }

        private void Awake()
        {
            if (_canvasGroup != null) _canvasGroup.alpha = _visibleAtStart ? 1 : 0;
            _animator ??= GetComponent<Animator>();
            _startId = Animator.StringToHash(_startTrigger);
            _stopId = Animator.StringToHash(_stopTrigger);
            foreach (var step in _steps)
                step.Prepare();

            IsSafeToRemove = true;
        }

        private void ResetSteps()
        {
            _latestStep = -1;
            _stepsStack.Clear();
            foreach (var step in _steps) step.Prepare();
        }

        private void Show(bool show, TweenCallback callback)
        {
            if (_showTween != null && !_showTween.IsComplete()) _showTween.Complete();

            if (_canvasGroup == null)
            {
                callback?.Invoke();
                return;
            }

            if (_fadeTime > 0)
            {
                _canvasGroup.alpha = show ? 0 : 1;
                _showTween = _canvasGroup.DOFade(show ? 1 : 0, _fadeTime).OnComplete(callback);
            }
            else
            {
                _canvasGroup.alpha = show ? 1 : 0;
                callback?.Invoke();
            }
        }

        [ContextMenu("Show")]
        public void Show() => Show(true, null);

        [ContextMenu("Hide")]
        public void Hide() => Show(false, null);

        [ContextMenu("Show And Start")]
        public void ShowAndStart()
        {
            ResetSteps();
            Show(true, () => Play(true));
        }

        [ContextMenu("Start")]
        public void Play() => Play(false);

        public void Play(bool onlyAnimator)
        {
            if (!onlyAnimator) ResetSteps();
            IsSafeToRemove = true;
            if (_animator != null) _animator.SetTrigger(_startId);
        }

        [ContextMenu("Stop")]
        public void Stop()
        {
            ResetSteps();
            if (_animator != null) _animator.SetTrigger(_stopId);
        }

        public bool NextCutsceneStep()
        {
            while (_stepsStack.Count > 0)
            {
                var enumerator = _stepsStack.Pop();
                if (!enumerator.MoveNext()) continue;

                _stepsStack.Push(enumerator);

                if (enumerator.Current == null) return true;

                enumerator.Current.Prepare();
                _stepsStack.Push(enumerator.Current.Execute());

                return true;
            }

            if (_latestStep < _steps.Length - 1)
            {
                var enumerator = _steps[++_latestStep].Execute();

                if (!enumerator.MoveNext()) return true;

                _stepsStack.Push(enumerator);
                if (enumerator.Current == null) return true;

                enumerator.Current.Prepare();
                _stepsStack.Push(enumerator.Current.Execute());
                return true;

            }

            return false;
        }

        public void SetUnsafe()
        {
            IsSafeToRemove = false;
            SafeToRemoveChanged?.Invoke(IsSafeToRemove);
        }

        public void SetSafe()
        {
            IsSafeToRemove = true;
            SafeToRemoveChanged?.Invoke(IsSafeToRemove);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
        }

        [ContextMenu("Increase Cutscene Step")]
        private void IncreaseCutsceneStep() => NextCutsceneStep();
#endif
    }
}