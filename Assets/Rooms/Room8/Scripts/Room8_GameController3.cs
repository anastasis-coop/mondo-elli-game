using UnityEngine;

namespace Room8 {
    public class Room8_GameController3 : Room8_BaseGameController {

        public GameObject level3_1;
        public GameObject level3_2;

        public override void GoToLevel() {
            switch (level) {
                case RoomLevel.LEVEL_31:
                    level3_1.SetActive(true);
                    break;
                case RoomLevel.LEVEL_32:
                    level3_2.SetActive(true);
                    break;
            }
        }
    }
}
