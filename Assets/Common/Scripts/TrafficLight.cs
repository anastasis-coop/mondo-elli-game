using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    [RequireComponent(typeof(MeshRenderer))]
    public class TrafficLight : MonoBehaviour
    {
        [Serializable]
        private class StateConfig
        {
            [field: SerializeField]
            public int MaterialIndex { get; private set; }

            [field: SerializeField]
            public Material Material { get; private set; }

            [field: SerializeField]
            public Light Light { get; private set; }
        }
        
        public enum State
        {
            Off,
            Red,
            Yellow,
            Green
        }

        [SerializeField]
        private State _initialState;

        [Header("Configurations")]
        [SerializeField]
        private StateConfig _red;
        
        [SerializeField]
        private StateConfig _yellow;
        
        [SerializeField]
        private StateConfig _green;

        [Space]
        [SerializeField]
        private Material _offMaterial;

        private Dictionary<State, StateConfig> _onStatesConfigs;

        private State _currentState;

        private MeshRenderer _renderer;

        private Material[] _materialsCache;

        private MeshRenderer Renderer => _renderer ??= GetComponent<MeshRenderer>();

        private Dictionary<State, StateConfig> OnStatesConfigs
        {
            get
            {
                return _onStatesConfigs ??= new()
                {
                    [State.Red] = _red,
                    [State.Yellow] = _yellow,
                    [State.Green] = _green
                };
            }
        }

        public State CurrentState
        {
            get => _currentState;
            set
            {
                if (_currentState == value) return;

                if (_materialsCache == null) _materialsCache = Renderer.materials;
                
                _currentState = value;
                
                foreach (var state in OnStatesConfigs.Values)
                {
                    _materialsCache[state.MaterialIndex] = _offMaterial;
                    if (state.Light != null) state.Light.enabled = false;
                }

                if (value != State.Off)
                {
                    var state = OnStatesConfigs[value];
                    _materialsCache[state.MaterialIndex] = state.Material;
                    if (state.Light != null) state.Light.enabled = true;
                }

                Renderer.materials = _materialsCache;
            }
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_offMaterial == null) return;
            CurrentState = _initialState;
        }
#endif
    }
}
