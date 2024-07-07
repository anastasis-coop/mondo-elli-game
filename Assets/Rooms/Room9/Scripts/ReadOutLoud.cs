using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Room9
{
    public class ReadOutLoud : MonoBehaviour
    {
        [SerializeField]
        private Image buttonImage;

        [SerializeField]
        private Graphic fullScreenGraphic;

        [SerializeField]
        private Sprite activeSprite, inactiveSprite;

        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private GraphicRaycaster raycaster;

        private bool _active;

        private Coroutine routine;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        private void SetActive(bool active)
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                audioSource.Stop();
                audioSource.clip = null;

                //TODO return?
            }

            _active = active;

            fullScreenGraphic.raycastTarget = active;
            buttonImage.sprite = active ? activeSprite : inactiveSprite;
        }

        public void OnButtonPressed()
        {
            SetActive(!_active);
        }

        public void ForceStop()
        {
            SetActive(false);
        }

        public void OnPointerClick(BaseEventData baseData)
        {
            var data = (PointerEventData)baseData;

            List<RaycastResult> results = new();
            raycaster.Raycast(data, results);

            SetActive(false);

            foreach (var result in results)
            {
                var text = result.gameObject?.GetComponent<ReadableLabel>();

                if (text == null) continue;

                routine = StartCoroutine(ReadLabel(text));
                break;
            }
        }

        private IEnumerator ReadLabel(ReadableLabel text)
        {
            foreach (AudioClip clip in text.AudioClips)
                yield return PlayClip(clip);
        }

        private IEnumerator PlayClip(AudioClip audioClip)
        {
            audioSource.Stop();
            audioSource.clip = audioClip;
            audioSource.Play();

            yield return new WaitWhile(() => audioSource.isPlaying);
            audioSource.clip = null;
        }

        public void Hide()
        {
            audioSource.Stop();
            audioSource.clip = null;
            if (routine != null) StopCoroutine(routine);

            SetActive(false);
            gameObject.SetActive(false);
        }
    }
}