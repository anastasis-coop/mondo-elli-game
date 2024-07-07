using UnityEngine;

namespace Room9
{
    public class Stopwatch : MonoBehaviour
    {
        public float ElapsedSeconds { get; private set; }

        private void Update() => ElapsedSeconds += Time.deltaTime;

        public void Reset() => ElapsedSeconds = 0;
    }
}