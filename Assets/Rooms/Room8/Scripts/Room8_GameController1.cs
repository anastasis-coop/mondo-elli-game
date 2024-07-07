using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Room8 {
    public class Room8_GameController1 : Room8_BaseGameController {

        public GameObject level0_1;
        public GameObject level0_2;
        public GameObject level1_1;
        public GameObject level1_2;

        public ParticleSystem rightParticles;
        public ParticleSystem wrongParticles;

        public override void GoToLevel() {
            switch (level) {
                case RoomLevel.LEVEL_01:
                    level0_1.SetActive(true);
                    break;
                case RoomLevel.LEVEL_02:
                    level0_2.SetActive(true);
                    break;
                case RoomLevel.LEVEL_11:
                    level1_1.SetActive(true);
                    break;
                case RoomLevel.LEVEL_12:
                    level1_2.SetActive(true);
                    break;
            }
        }

        public void ShowParticles(Vector3 position, bool success)
        {
            var particles = success ? rightParticles : wrongParticles;
            particles.transform.position = position;
            particles.Play();
        }
    }
}
