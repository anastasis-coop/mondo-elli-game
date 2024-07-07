using UnityEngine;

namespace Cutscene
{
    public abstract class Binding : ScriptableObject
    {
        [field: SerializeField]
        public BigElloMessage Message { get; private set; }
    }
}
