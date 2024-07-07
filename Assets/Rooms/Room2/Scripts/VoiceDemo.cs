using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;

namespace Room2
{
    public class VoiceDemo : MonoBehaviour
    {

        public LocalizedAudioClip ManVoiceDemo;
        public LocalizedAudioClip WomanVoiceDemo;

        public UnityEvent OnPlayFinished = new UnityEvent();

        private AudioSource audioSource;
        private bool prevState;

        // Start is called before the first frame update
        void Start()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            prevState = false;
        }

        public void PlayManVoiceDemo()
        {
            audioSource.clip = ManVoiceDemo.LoadAsset();
            audioSource.Play();
        }

        public void PlayWomanVoiceDemo()
        {
            audioSource.clip = WomanVoiceDemo.LoadAsset();
            audioSource.Play();
        }

        void Update()
        {
            bool state = audioSource.isPlaying;
            if (prevState && !state)
            {
                OnPlayFinished.Invoke();
            }
            prevState = state;
        }

    }
}
