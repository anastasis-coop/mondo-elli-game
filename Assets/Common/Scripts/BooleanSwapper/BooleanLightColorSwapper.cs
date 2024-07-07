using UnityEngine;

namespace Common
{
    [RequireComponent(typeof(Light))]
    public class BooleanLightColorSwapper : BooleanSwapper
    {
        [Space]
        [SerializeField, ColorUsage(false)]
        private Color _none = Color.white;
        
        [SerializeField, ColorUsage(false)]
        private Color _false = Color.white;

        [SerializeField, ColorUsage(false)]
        private Color _true = Color.white;

        private Light _light;

        private Light Light => _light == null ? (_light =  GetComponent<Light>()) : _light;

        public override bool CurrentValue => Light.color == _true;

        protected override void SetNone() => Light.color = _none;

        protected override void SetBool(bool value) => Light.color = value ? _true : _false;
    }
}
