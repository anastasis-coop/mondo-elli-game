using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Room8
{
    [RequireComponent(typeof(AudioSource))]
    public class CalculatorButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField]
        private AudioClip _pressClip;
        [SerializeField]
        private AudioClip _relaseClip;

        [SerializeField, Range(0, 1)]
        private float _volume = 1;
        
        [Space]
        [SerializeField, Min(0)]
        private float _buttonRun;

        private AudioSource _audioSource;
        
        private Vector3 _originalPosition;

        private AudioSource AudioSource 
            => _audioSource ??= GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();

        [field: SerializeField]
        public UnityEvent onClick { get; private set; }
        
        protected virtual void Awake() => _originalPosition = transform.position;

        private void ResetPosition() => transform.position = _originalPosition;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!enabled) return;
            transform.position -= _buttonRun * transform.forward;
            AudioSource.pitch = 1f;
            AudioSource.PlayOneShot(_pressClip, _volume);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!enabled) return;
            ResetPosition();
            AudioSource.pitch = 1.2f;
            AudioSource.PlayOneShot(_relaseClip, _volume);
            onClick.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData) => ResetPosition();
    }
}