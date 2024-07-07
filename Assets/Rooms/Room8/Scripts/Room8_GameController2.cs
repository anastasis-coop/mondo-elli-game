using Common;
using UnityEngine;

namespace Room8 {
    public class Room8_GameController2 : Room8_BaseGameController {

        public GameObject level2_1;
        public GameObject level2_2;

        public CompositeBooleanSwapper feedbackLight;
        
        public SetDescription[] setDescriptions;
        public GameObject[] screenList;

        public Letter[] letters;

        public override void GoToLevel() {
            switch (level) {
                case RoomLevel.LEVEL_21:
                    level2_1.SetActive(true);
                    break;
                case RoomLevel.LEVEL_22:
                    level2_2.SetActive(true);
                    break;
                default:
                    Debug.LogError($"{level} is invalid for this scene, defaulting to {RoomLevel.LEVEL_21}");
                    level2_1.SetActive(true);
                    break;
            }
        }
        
        public override void RightAnswer() {
            base.RightAnswer();
            feedbackLight.Set(true);
        }

        public override void WrongAnswer() {
            base.WrongAnswer();
            feedbackLight.Set(false);
        }

        private void OnValidate() {
            if (screenList.Length != setDescriptions.Length)
                screenList = new GameObject[setDescriptions.Length];
        }
    }
}
