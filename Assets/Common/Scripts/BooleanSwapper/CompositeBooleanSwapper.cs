using System.Reflection;
using UnityEngine;

namespace Common
{
    public class CompositeBooleanSwapper : BooleanSwapper
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly MethodInfo SetNoneMethod = typeof(BooleanSwapper).GetMethod("SetNone", Flags);
        private static readonly MethodInfo SetBoolMethod = typeof(BooleanSwapper).GetMethod("SetBool", Flags);

        private static readonly object[] False = { false };
        private static readonly object[] True = { true };
        
        [Space]
        [SerializeField]
        private BooleanSwapper[] _swappers;

        public override bool CurrentValue => _swappers.Length > 0 && _swappers[0].CurrentValue;

        protected override void Awake()
        {
            base.Awake();
            var fieldInfo = typeof(BooleanSwapper).GetField("_ignoreReturnToDefaultTime", Flags);
            foreach (var swapper in _swappers) fieldInfo?.SetValue(swapper, true);
        }

        protected override void SetNone()
        {
            foreach (var swapper in _swappers) 
                SetNoneMethod.Invoke(swapper, null);
        }

        protected override void SetBool(bool value)
        {
            foreach (var swapper in _swappers) 
                SetBoolMethod.Invoke(swapper, value ? True : False);
        }
    }
}