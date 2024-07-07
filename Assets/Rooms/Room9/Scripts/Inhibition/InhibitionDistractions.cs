using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Room9
{
    public class InhibitionDistractions : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] distractions;

        [SerializeField]
        private Vector2 activationSecondsRange;

        [SerializeField]
        private Vector2 durationSecondsRange;

        private Action _pressedCallback;
        private Coroutine _coroutine;

        public void Show(Action pressedCallback)
        {
            _pressedCallback = pressedCallback;

            gameObject.SetActive(true);

            _coroutine = StartCoroutine(DistractionRoutine());
        }

        private IEnumerator DistractionRoutine()
        {
            while (true)
            {
                float randomWait = Random.Range(activationSecondsRange.x, activationSecondsRange.y);
                yield return new WaitForSeconds(randomWait);

                int randomIndex = Random.Range(0, distractions.Length);
                GameObject distraction = distractions[randomIndex];
                distraction.SetActive(true);

                float randomDuration = Random.Range(durationSecondsRange.x, durationSecondsRange.y);
                yield return new WaitForSeconds(randomDuration);
                distraction.SetActive(false);
            }
        }

        public void OnDistractionPressed() => _pressedCallback?.Invoke();

        public void Hide()
        {
            StopCoroutine(_coroutine);

            foreach (var distraction in distractions)
                distraction.SetActive(false);

            _pressedCallback = null;

            gameObject.SetActive(false);
        }
    }
}
