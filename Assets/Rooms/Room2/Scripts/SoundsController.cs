using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Room2
{
    public class SoundsController : MonoBehaviour
    {

        public ResourcesManager resourcesManager;

        private List<AudioClip> audioClips = new();
        private AudioSource audioSource1;
        private AudioSource audioSource2;
        private List<int> randomOrder;

        private int currentIndex = 0;
        private bool prevState = false;

        public UnityEvent playFinished = new UnityEvent();
        public AudioClip CurrentSingleManagerAudioClipPlaying { get; set; }
        
        void Start()
        {
            audioSource1 = gameObject.AddComponent<AudioSource>();
            audioSource2 = gameObject.AddComponent<AudioSource>();
        }

        private void Update()
        {
            bool currState = isPlaying();
            if (prevState && !currState)
            {
                playFinished.Invoke();
            }
            prevState = currState;
        }

        public IEnumerator LoadAudioClips(string path)
        {
            audioClips.Clear();
            yield return resourcesManager.GetAudioClipsByFolderName(path, audioClips);

            ShuffleIndexes();
        }

        private string PlayAudioClip(int index, float delay = 0)
        {
            audioSource1.clip = audioClips[index];
            if (delay == 0)
            {
                audioSource1.Play();
            }
            else
            {
                audioSource1.PlayDelayed(delay);
            }
            return audioSource1.clip.name;
        }

        internal string PlayAudioClip(string name, float delay = 0)
        {
            int index = audioClips.FindIndex(clip => clip.name == name);
            if (index >= 0)
            {
                return PlayAudioClip(index, delay);
            }
            return "";
        }

        public string PlaySequentialAudioClip(float delay = 0)
        {
            int index = currentIndex;
            CurrentSingleManagerAudioClipPlaying = audioClips[index];
            currentIndex = (currentIndex == audioClips.Count - 1) ? 0 : currentIndex + 1;
            return PlayAudioClip(index, delay);
        }

        private void ShuffleIndexes()
        {
            randomOrder = new List<int>();
            for (int i = 0; i < audioClips.Count; i++)
            {
                randomOrder.Insert(Random.Range(0, randomOrder.Count), i);
            }
        }

        private int GetShuffledIndex()
        {
            int index = randomOrder[currentIndex];
            if (currentIndex < audioClips.Count - 1)
            {
                currentIndex++;
            }
            else
            {
                currentIndex = 0;
                ShuffleIndexes();
                if (randomOrder[0] == index)
                {
                    // Avoid next sound will be the same when shuffling
                    randomOrder.Reverse();
                }
            }
            return index;
        }

        public string PlayRandomAudioClip(float delay = 0)
        {
            return PlayAudioClip(Random.Range(0, audioClips.Count));
        }

        internal string PlayRandomAudioClipExcluding(string target, float delay = 0)
        {
            List<AudioClip> withoutTarget = audioClips.FindAll(clip => clip.name != target);
            return PlayAudioClip(withoutTarget[Random.Range(0, withoutTarget.Count)].name, delay);
        }

        public string PlayShuffledAudioClip(float delay = 0)
        {
            return PlayAudioClip(GetShuffledIndex(), delay);
        }

        public void PlayTwoRandomAudioClips(float delay)
        {
            PlayShuffledAudioClip();
            if (audioClips.Count > 1) // To avoid infinite loop
            {
                do
                {
                    audioSource2.clip = audioClips[Random.Range(0, audioClips.Count)];
                } while (audioSource2.clip == audioSource1.clip);
                audioSource2.PlayDelayed(delay);
            }
        }

        internal void PlayTwoRandomAudioClipsIncluding(string target, float delay = 0)
        {
            bool firstIsTarget = (Random.Range(0, 2) == 0);
            if (firstIsTarget)
            {
                audioSource2.clip = audioClips.Find(clip => clip.name == target);
                audioSource2.Play();
                PlayRandomAudioClipExcluding(target, delay);
            }
            else
            {
                PlayRandomAudioClipExcluding(target);
                audioSource2.clip = audioClips.Find(clip => clip.name == target);
                audioSource2.PlayDelayed(delay);
            }
        }

        internal void PlayTwoRandomAudioClipsExcluding(string target, float delay)
        {
            List<AudioClip> withoutTarget = audioClips.FindAll(clip => clip.name != target);
            PlayAudioClip(withoutTarget[Random.Range(0, withoutTarget.Count)].name);
            if (withoutTarget.Count > 1) // To avoid infinite loop
            {
                do
                {
                    audioSource2.clip = withoutTarget[Random.Range(0, withoutTarget.Count)];
                } while (audioSource2.clip == audioSource1.clip);
                audioSource2.PlayDelayed(delay);
            }
        }

        internal bool isPlaying()
        {
            return audioSource1.isPlaying || audioSource2.isPlaying;
        }

        public bool isAudioClipPlaying(string name)
        {
            return (audioSource1.isPlaying && audioSource1.clip.name == name) || (audioSource2.isPlaying && audioSource2.clip.name == name);
        }

    }
}
