using System.Collections.Generic;
using UnityEngine;

namespace Room9
{
    public class ReadableLabel : MonoBehaviour
    {
        public List<AudioClip> AudioClips = new();
        public AudioClip AudioClip
        {
            set
            {
                AudioClips.Clear();
                AudioClips.Add(value);
            }
        }
    }
}
