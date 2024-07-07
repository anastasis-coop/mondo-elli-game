using System;
using UnityEngine;
using UnityEngine.Localization;

namespace Room9
{
    [Serializable]
    public class ReadableString
    {
        [TextArea]
        public string String;
        public AudioClip AudioClip;

        public override string ToString() => String;
    }

}