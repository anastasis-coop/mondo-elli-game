using System;
using UnityEngine;

namespace Game
{
    [Serializable]
    public class CodingSubPath
    {
        public Vector3Int StartDirection;
        public Vector3[] SuggestedPath;

        public int Count => SuggestedPath != null ? SuggestedPath.Length : 0;
        public Vector3? Start => Count > 0 ? SuggestedPath[0] : null;
        public Vector3? End => Count > 1 ? SuggestedPath[Count - 1] : null;

        public Vector3 EndDirection => (SuggestedPath[^1] - SuggestedPath[^2]).normalized;
    }

    [CreateAssetMenu]
    public class CodingPath : ScriptableObject
    {
        public Vector3Int StartDirection;
        public Vector3[] SuggestedPath;
        public int MinSubPathCount = 1;

        public int Count => SuggestedPath != null ? SuggestedPath.Length : 0;
        public Vector3? Start => Count > 0 ? SuggestedPath[0] : null;
        public Vector3? End => Count > 1 ? SuggestedPath[Count - 1] : null;
    }

}
