using UnityEngine;

namespace Room8 {
    public class Room8_CommonRefs : MonoBehaviour {
        public static Room8_BaseGameController controller { get; private set; }

        private void Awake() {
            controller = GameObject.Find("GameController").GetComponent<Room8_BaseGameController>();
        }
    }
}

