using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Room1
{
    public class FunnelFailSafe : MonoBehaviour
    {
        [SerializeField]
        private ConveyorRoom conveyorRoom;

        [SerializeField]
        private float checkIntervalSeconds = 2;

        private float _elapsedSeconds;
        private Queue<GameObject> _queue = new();

        private void OnCollisionEnter(Collision collision)
        {
            _queue.Enqueue(collision.gameObject);
        }

        private void Update()
        {
            _elapsedSeconds += Time.deltaTime;

            if (_elapsedSeconds < checkIntervalSeconds) return;

            _elapsedSeconds = 0;

            while (_queue.TryDequeue(out GameObject stuck))
            {
                if (stuck == null) continue;

                conveyorRoom.onObjectExit(stuck.transform);
            }
        }
    }
}