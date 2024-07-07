using System;
using UnityEngine;
using UnityEngine.Localization;

namespace Room9
{
    // HACK wrapper for localizing without having to implement a lot of stuff
    [CreateAssetMenu]
    public class ReadableStringAsset : ScriptableObject
    {
        [SerializeField]
        private ReadableString value;
        public ReadableString Value => value;

#if UNITY_EDITOR
        public void SetValue(string text, AudioClip clip)
        {
            value = new()
            {
                String = text,
                AudioClip = clip
            };
        }
#endif
    }

    [Serializable]
    public class LocalizedReadableStringAsset : LocalizedAsset<ReadableStringAsset>
    {

    }
}