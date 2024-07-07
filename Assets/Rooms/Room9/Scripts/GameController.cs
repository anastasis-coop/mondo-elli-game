using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Room9
{
    public class GameController : MonoBehaviour
    {
        public ReadOutLoud readOutLoud;
        
        [SerializeField]
        private InhibitionGame inhibitionGame;

        [SerializeField]
        private MemoryGame memoryGame;

        [SerializeField]
        private FlexibilityGame flexibilityGame;

        [SerializeField]
        private MediaLiteracyGame mediaLiteracyGame;

        [SerializeField]
        private Island currentIsland;

        [SerializeField]
        private RoomLevel currentLevel;

        [SerializeField]
        private Timer timer;

        [SerializeField]
        private Score score;

        public void Start()
        {
            InitCurrentIslandAndLevel();

            switch (currentIsland)
            {
                case Island.CONTROLLO_INTERFERENZA:
                case Island.INIBIZIONE_RISPOSTA:
                    inhibitionGame.Show(currentLevel, SaveResultsAndExit);
                    break;
                case Island.MEMORIA_LAVORO:
                    memoryGame.Show(currentLevel, SaveResultsAndExit);
                    break;
                case Island.FLESSIBILITA_COGNITIVA:
                    flexibilityGame.Show(currentLevel, SaveResultsAndExit);
                    break;
                default:
                    mediaLiteracyGame.Show(SaveResultsAndExit);
                    break;
            }
        }

        private void InitCurrentIslandAndLevel()
        {
#if UNITY_EDITOR
            if (GameState.StartedFromThisScene) return;
#endif
            currentIsland = GameState.Instance.levelBackend.island;
            currentLevel = GameState.Instance.levelBackend.roomLevel;
        }

        public void SaveResultsAndExit()
        {
            if (GameState.Instance.testMode)
            {
                GameState.Instance.LoadSceneAfterRoom();
            }
            else
            {
                // ML exercises are the only one that can finish before timer.totalTime
                int elapsedTime = timer.totalTime - (int)timer.timeLeft;

                GameState.Instance.levelBackend.ExitRoom(elapsedTime, score,
                () => {
                    GameState.Instance.LoadSceneAfterRoom();
                },
                err => {
                    Debug.Log(err.Message);
                });
            }
        }
    }
}
