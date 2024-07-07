using System;
using UnityEngine;

public class AnimatorAudioTrigger : MonoBehaviour
{
    [SerializeField]
    private AudioSource _source;
    
    [SerializeField, Range(0, 1)]
    private float _volume = 1;

    [SerializeField]
    private AudioClip[] _clips;

    public void PlayAudio(int audioIndex)
    {
        if (audioIndex < 0 || audioIndex >= _clips.Length)
            throw new ArgumentOutOfRangeException(nameof(audioIndex),"audioIndex must be non-negative and less than the number of available clips");
        
        _source.PlayOneShot(_clips[audioIndex], _volume);
    }
}
