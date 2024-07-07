using UnityEngine;

namespace Room2
{
    public class DemoController : MonoBehaviour
    {
        public GameObject Phone4;
        public GameObject Phone5;

        public AudioClip RingtonePhone4;
        public AudioClip RingtonePhone5;

        public AudioSource audio;

        public void PlayAudio(AudioClip clip, bool loop)
        {
            audio.clip = clip;
            audio.Play();
            audio.loop = loop;
        }

        public void DemoEnded()
        {
            if (audio.loop || !audio.isPlaying)
            {
                Phone4.SetActive(false);
                Phone5.SetActive(false);
                audio.Stop();
            }
        }

    }
}
