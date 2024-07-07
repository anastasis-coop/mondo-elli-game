using UnityEngine;

namespace Room1
{
    public class PositionChanger : MonoBehaviour
    {

        public float speed;
        public int currentPosition;
        private Vector3[] positions = { new Vector3(-4.9f, -7.46f, -1.52f), new Vector3(9.0f, -5.0f, -15.0f) };

        void Update()
        {
            if (currentPosition >= 0 && currentPosition <= positions.Length)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, positions[currentPosition], Time.deltaTime * speed);
            }
        }

    }
}
