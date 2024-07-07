using UnityEngine;

namespace Room2
{
    public class PhoneScreenController : MonoBehaviour
    {
        [SerializeField]
        private GameObject _emissiveQuad;

        public bool IsOn
        {
            get => _emissiveQuad.activeSelf;
            set => _emissiveQuad.SetActive(value);
        }

#if UNITY_EDITOR
        [ContextMenu("Toggle")]
        private void Toggle()
        {
            if (_emissiveQuad == null) return;
            IsOn = !IsOn;
        }
#endif
    }
    
}
