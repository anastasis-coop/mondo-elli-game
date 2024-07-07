using UnityEngine;

namespace Room4
{
    public class GameController : MonoBehaviour
    {
        public Timer levelTimer;
        public Score levelScore;

        public GameObject energyBar;

        public BigElloSays messagePanel;
        public FeedbackController feedback;
        public Bomb bomb;

        public Sprite stripedBackground;
        public Sprite normalBackground;

        public RestartButton restartButton;

        public GameObject level0_1;
        public GameObject level0_2;
        public GameObject level1_1;
        public GameObject level1_2;
        public GameObject level2_1;
        public GameObject level2_2;
        public GameObject level3_1;
        public GameObject level3_2;

        [SerializeField]
        private RoomEndPopup endPopup;

        public RoomLevel level;

        public static GameController Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void Start()
        {

#if UNITY_EDITOR
            if (!GameState.StartedFromThisScene)
#endif
            level = GameState.Instance.levelBackend.roomLevel;

            switch (level)
            {
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
                case RoomLevel.LEVEL_21:
                    level2_1.SetActive(true);
                    break;
                case RoomLevel.LEVEL_22:
                    level2_2.SetActive(true);
                    break;
                case RoomLevel.LEVEL_31:
                    level3_1.SetActive(true);
                    break;
                case RoomLevel.LEVEL_32:
                    level3_2.SetActive(true);
                    break;
            }
        }

        public void RightAnswer(Vector3 position)
        {
            levelScore.RightCounter++;
            feedback.rightAnswerFeedback(position);
        }

        public void WrongAnswer(Vector3 position)
        {
            levelScore.WrongCounter++;
            feedback.wrongAnswerFeedback(position);
        }

        public void MissedAnswer()
        {
            levelScore.MissedCounter++;
            feedback.missedAnswerFeedback();
        }

        public void levelEnd()
        {
            endPopup.Show(SaveResultsAndExit);
        }

        private void SaveResultsAndExit()
        {
            if (GameState.Instance.testMode)
            {
                GameState.Instance.LoadSceneAfterRoom();
            }
            else
            {
                GameState.Instance.levelBackend.ExitRoom(levelTimer.totalTime, levelScore,
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
