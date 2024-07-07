using UnityEngine;

namespace Common
{
    public class SpriteColorHighlighter : ColorHighlighter
    {
        [SerializeField]
        private SpriteRenderer _renderer;

        private Color? _original;
        
        protected override void InitIfNeeded() => _original ??= _renderer.color;

        protected override void AssignColor(Color color) => _renderer.color = color;

        protected override void Revert() => AssignColor(_original ?? Color.white);
        
#if UNITY_EDITOR
        [ContextMenu("Search Renderer in Children")]
        private void SearchInChildren()
        {
            _renderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        private void OnValidate()
        {
            if (_renderer != null) return;
            _renderer = GetComponent<SpriteRenderer>();
        }
#endif
    }
}
