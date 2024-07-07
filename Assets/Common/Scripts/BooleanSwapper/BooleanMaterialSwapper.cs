using System;
using Room5;
using UnityEngine;

namespace Common
{
    [RequireComponent(typeof(MeshRenderer))]
    public class BooleanMaterialSwapper : BooleanSwapper
    {
        [Space]
        [SerializeField]
        private Material _none;

        [SerializeField]
        private Material _false;

        [SerializeField]
        private Material _true;

        private MeshRenderer _renderer;

        public MeshRenderer Renderer => _renderer == null ? (_renderer = GetComponent<MeshRenderer>()) : _renderer;

        public override bool CurrentValue => Renderer.sharedMaterial == _true;

        protected override void SetNone()
        {
            if (_none == null) Debug.LogError($"No Default Material in {name}");

            if (TryGetComponent<MaterialHighlighter>(out var highlighter) && highlighter.IsHighlighted) return;
            
            Renderer.sharedMaterial = _none;
        }

        protected override void SetBool(bool value)
        {
            if (_false == null && !value) Debug.LogError($"No False Material in {name}");
            if (_true == null && value) Debug.LogError($"No True Material in {name}");
            
            if (TryGetComponent<MaterialHighlighter>(out var highlighter) && highlighter.IsHighlighted) return;
            
            Renderer.sharedMaterial = value ? _true : _false;
        }
    }
}
