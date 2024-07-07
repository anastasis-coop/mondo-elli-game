using UnityEngine;

namespace Cutscene
{
    public class SubAnimatorInterface : MonoBehaviour
    {
        [SerializeField]
        private Animator _animator;

        public void SetTrigger(string name) => _animator.SetTrigger(name);
        public void SetBool(string name, bool value) => _animator.SetBool(name, value);
    }
}