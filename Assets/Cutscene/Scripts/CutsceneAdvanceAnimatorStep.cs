using System.Collections.Generic;
using UnityEngine;

namespace Cutscene
{
    [RequireComponent(typeof(Animator))]
    public class CutsceneAdvanceAnimatorStep : CutsceneStep
    {
        [SerializeField]
        private string _nextTrigger = "Next";

        [SerializeField]
        private string _backToSequenceStart = "Reset";

        [SerializeField, Min(1)]
        private int _steps = 1;

        private int _currentStep = -1; 
        
        private Animator _animator;

        private Animator Animator => _animator ??= GetComponent<Animator>();

        public override void Prepare()
        {
            if (_currentStep >= 0) Animator.SetTrigger(_backToSequenceStart);
            _currentStep = -1;
        }

        public override IEnumerator<ICutsceneStep> Execute()
        {
            while (_currentStep < _steps)
            {
                Animator.SetTrigger(_nextTrigger);
                _currentStep++;
                yield return null;
            }
        }
    }
}