using UnityEngine;

namespace Room1
{
    public class BlackHole : MonoBehaviour
    {
        public ConveyorRoom conveyorRoom;

        private void OnTriggerEnter(Collider other)
        {
            if (conveyorRoom != null)
            {
                conveyorRoom.onObjectExit(other.transform);
            }
        }

    }
}
