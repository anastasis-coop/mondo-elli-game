using UnityEngine;

namespace Cutscene
{
    [CreateAssetMenu(fileName = "New Cutscene Binding", menuName = "Cutscenes/Binding")]
    public class CutsceneBinding : Binding
    {
        [field: Space]
        [field: SerializeField]
        public bool UseAlternativeParent { get; private set; }

        [field: SerializeField]
        public CutsceneAnimation CutsceneAnimation { get; private set; }
    }
}