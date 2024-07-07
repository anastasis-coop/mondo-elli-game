using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Common
{
    public abstract class Highlighter : MonoBehaviour
    {
        public bool IsHighlighted { get; private set; }

        protected abstract void InitIfNeeded();

        protected abstract bool EmphasizeLogic();

        protected abstract bool DeEmphasizeLogic();

        public void Emphasize()
        {
            InitIfNeeded();
            IsHighlighted = EmphasizeLogic();
        }

        public void DeEmphasize()
        {
            InitIfNeeded();
            IsHighlighted = !DeEmphasizeLogic();
        }
    }
}
