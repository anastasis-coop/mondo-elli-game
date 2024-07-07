using System.Linq;
using UnityEngine;

namespace Common
{
    public class MaterialHighlighter : Highlighter
    {
        [SerializeField]
        private MeshRenderer _renderer;

        [Space]
        [SerializeField]
        private Material _override;

        private BooleanMaterialSwapper _swapper;
        private Material[] _original;
        private Material[] _cache;
        
        public Material ExternalOverride { get; set; }

        protected override void InitIfNeeded()
        {
            if (_original != null || _swapper != null) return;
            
            _swapper = GetComponent<BooleanMaterialSwapper>();
            _original = _renderer.materials;
            _cache = _original.ToArray();
        }

        protected override bool EmphasizeLogic()
        {
            var over = ExternalOverride;
            if (over == null) over = _override;
            
            for (var i = 0; i < _cache.Length; i++) _cache[i] = over;
            _renderer.materials = _cache;
            return true;
        }

        protected override bool DeEmphasizeLogic()
        {
            for (var i = 0; i < _cache.Length; i++) _cache[i] = _original[i];
            _renderer.materials = _cache;
            
            if (_swapper != null) _swapper.Set(_swapper.CurrentValue);
            return true;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_renderer != null) return;

            _renderer = GetComponent<BooleanMaterialSwapper>()?.Renderer ? 
                    GetComponent<BooleanMaterialSwapper>()?.Renderer : 
                    GetComponent<MeshRenderer>();
        }
#endif
    }
}
