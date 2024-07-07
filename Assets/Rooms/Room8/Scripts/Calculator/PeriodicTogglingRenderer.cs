using UnityEngine;

namespace Room8
{
    public class PeriodicTogglingRenderer : MonoBehaviour
    {
        [SerializeField, Min(0)]
        private float _frequency;

        private Renderer _renderer;

        private float _timer;

        private Renderer Renderer => _renderer ??= GetComponent<Renderer>();

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer > 0) return;

            _timer += 1 / _frequency;
            if (Renderer != null) Renderer.enabled = !Renderer.enabled;
        }
    }
}