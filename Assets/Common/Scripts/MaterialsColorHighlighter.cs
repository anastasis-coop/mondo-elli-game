using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Common
{
    public class MaterialsColorHighlighter : ColorHighlighter
    {
        [SerializeField]
        private MeshRenderer _renderer;

        private Material[] _cache;
        private Dictionary<Material, Color> _originals;

        protected override void InitIfNeeded()
        {
            if (_cache != null) return;

            _cache = _renderer.materials;
            _originals = _cache.ToDictionary(m => m, m => m.color);
        }

        protected override void AssignColor(Color color)
        {
            foreach (var mat in _cache) 
                mat.color = color;
            _renderer.materials = _cache;
        }

        protected override void Revert()
        {
            foreach (var mat in _cache) 
                mat.color = _originals[mat];
            _renderer.materials = _cache;
        }
        
#if UNITY_EDITOR
        [ContextMenu("Search Renderer in Children")]
        private void SearchInChildren()
        {
            _renderer = GetComponentInChildren<MeshRenderer>();
        }
        
        private void OnValidate()
        {
            if (_renderer != null) return;
            _renderer = GetComponent<MeshRenderer>();
        }
#endif
    }
}
