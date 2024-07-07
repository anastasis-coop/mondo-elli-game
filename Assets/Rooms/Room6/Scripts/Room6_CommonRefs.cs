using UnityEngine;

namespace Room6 {
    public class Room6_CommonRefs : MonoBehaviour {
        public static Room6_GameController controller { get; private set; }

        private void Awake() {
            controller = GameObject.Find("GameController").GetComponent<Room6_GameController>();
        }
    }
}

