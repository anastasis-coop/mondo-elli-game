using UnityEngine;

namespace Common
{
    public abstract class ColorHighlighter : Highlighter
    {
        [SerializeField]
        private Color _highlightColor = Color.white;

        protected abstract void AssignColor(Color color);

        protected abstract void Revert();

        protected override bool EmphasizeLogic()
        {
            AssignColor(_highlightColor);
            return true;
        }

        protected override bool DeEmphasizeLogic()
        {
            Revert();
            return true;
        }
    }
}
