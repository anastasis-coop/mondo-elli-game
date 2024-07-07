using UnityEngine;

namespace Room1
{
    public class BlackHole1232 : MonoBehaviour
    {
        public ConveyorRoomforLevel1232 conveyorRoom;

        private void OnTriggerEnter(Collider other)
        {
            if (conveyorRoom != null)
            {
                conveyorRoom.onObjectExit(other.GetComponent<ObjectProperties1232>());
            }
        }

    }
}
