using UnityEngine;

namespace Room7
{
    public class LevelController : MonoBehaviour
    {
        [SerializeField]
        private RoomLevel level;

        [SerializeField]
        private MessageSystem messageSystem;

        public ObjectsController objectsController;
        public TilesController tilesController;
        public Timer timer;
        [SerializeField]
        private TimerBar timerBar;

        public Score score;
        public RestartButton restartButton;
        
        [SerializeField] 
        private BigElloSaysConfig[] _tutorialLevel01;
        [SerializeField] 
        private BigElloSaysConfig[] _tutorialLevel02;
        [SerializeField] 
        private BigElloSaysConfig[] _tutorialLevel11;
        [SerializeField] 
        private BigElloSaysConfig[] _tutorialLevel12;
        [SerializeField] 
        private BigElloSaysConfig[] _tutorialLevel21;
        [SerializeField] 
        private BigElloSaysConfig[] _tutorialLevel22;
        [SerializeField] 
        private BigElloSaysConfig[] _tutorialLevel31;
        [SerializeField] 
        private BigElloSaysConfig[] _tutorialLevel32;
        
        [SerializeField]
        private BigElloSaysConfig _endExercise;

        [SerializeField]
        private TutorialHighlight highlight;

        [SerializeField]
        private RoomEndPopup endPopup;

        private bool gameOver = false;
        private bool tutorialRedo = false;

        private void Start()
        {
#if UNITY_EDITOR
            if (!GameState.StartedFromThisScene)
#endif
                level = GameState.Instance.levelBackend.roomLevel;

            switch (level)
            {
                case RoomLevel.LEVEL_01:
                    messageSystem.SetCurrentMessagesBatch(_tutorialLevel01);
                    objectsController.PrepareGame(minorIs2: false, majorIs0: true);
                    break;
                case RoomLevel.LEVEL_02:
                    messageSystem.SetCurrentMessagesBatch(_tutorialLevel02);
                    objectsController.PrepareGame(minorIs2: true, majorIs0: true);
                    break;
                case RoomLevel.LEVEL_11:
                    messageSystem.SetCurrentMessagesBatch(_tutorialLevel11);
                    objectsController.PrepareGame(minorIs2: false, majorIs0: false);
                    break;
                case RoomLevel.LEVEL_12:
                    messageSystem.SetCurrentMessagesBatch(_tutorialLevel12);
                    objectsController.PrepareGame(minorIs2: true, majorIs0: false, bombAlwaysVisible: true);
                    break;
                case RoomLevel.LEVEL_21:
                    messageSystem.SetCurrentMessagesBatch(_tutorialLevel21);
                    break;
                case RoomLevel.LEVEL_22:
                    messageSystem.SetCurrentMessagesBatch(_tutorialLevel22);
                    break;
                case RoomLevel.LEVEL_31:
                    messageSystem.SetCurrentMessagesBatch(_tutorialLevel31);
                    break;
                case RoomLevel.LEVEL_32:
                    messageSystem.SetCurrentMessagesBatch(_tutorialLevel32);
                    break;
            }

            if (level < RoomLevel.LEVEL_21)
            {
                objectsController.gameObject.SetActive(true);
            }
            else
            {
                tilesController.gameObject.SetActive(true);
                tilesController.PrepareTutorial(vertical: true);
            }

            tutorialRedo = false;
            messageSystem.messageRead.AddListener(OnTutorialMessageRead);
            messageSystem.messageShown.AddListener(OnTutorialMessageShown);
            messageSystem.greenButtonClicked.AddListener(OnStartButtonPressed);
            messageSystem.redButtonClicked.AddListener(OnTutorialReDoButtonPressed);
            messageSystem.ShowNextMessage();
        }

        private void EnableTimerBar()
        {
            timerBar.gameObject.SetActive(true);
        }

        public void BigElloTalked()
        {
            if (gameOver || timer.itIsEnd)
            {
                messageSystem.oneShotRead.RemoveListener(BigElloTalked);

                endPopup.Show(saveResultsAndExit);
            }
            else
            {
                messageSystem.messageRead.RemoveListener(OnTutorialMessageRead);
                messageSystem.messageShown.RemoveListener(OnTutorialMessageShown);
                messageSystem.greenButtonClicked.RemoveListener(OnStartButtonPressed);
                messageSystem.redButtonClicked.RemoveListener(OnTutorialReDoButtonPressed);
                messageSystem.messageBatchFinished.RemoveListener(OnReDoBatchFinished);

                StartGame();
            }
        }

        private void StartGame()
        {
            switch (level)
            {
                case RoomLevel.LEVEL_21:
                    tilesController.PrepareGame(minorIs2: false, majorIs3: false);
                    break;
                case RoomLevel.LEVEL_22:
                    tilesController.PrepareGame(minorIs2: true, majorIs3: false);
                    break;
                case RoomLevel.LEVEL_31:
                    tilesController.PrepareGame(minorIs2: false, majorIs3: true);
                    break;
                case RoomLevel.LEVEL_32:
                    tilesController.PrepareGame(minorIs2: true, majorIs3: true);
                    break;
            }

            messageSystem.oneShotRead.AddListener(BigElloTalked);
            EnableTimerBar();
            timer.Reset();
            timer.SetActivation(true);

            if (level < RoomLevel.LEVEL_21)
                objectsController.StartGame();
            else
                tilesController.StartGame();
        }

        public void EndExercise()
        {
            gameOver = true;
            timer.activation = false;
            if (objectsController != null)
                objectsController.gameObject.SetActive(false);
            if (tilesController != null)
                tilesController.gameObject.SetActive(false);

            messageSystem.ShowMessage(_endExercise);
        }

        private void saveResultsAndExit()
        {
            if (GameState.Instance.testMode)
            {
                GameState.Instance.LoadSceneAfterRoom();
            }
            else
            {
                GameState.Instance.levelBackend.ExitRoom(timer.totalTime, score,
                () => {
                    GameState.Instance.LoadSceneAfterRoom();
                },
                err => {
                    Debug.Log(err.Message);
                });
            }
        }

        private void OnTutorialMessageRead(int index)
        {
            messageSystem.ShowNextMessage();
        }

        private void OnTutorialMessageShown(int index)
        {
            bool isLevel21Tutorial = level == RoomLevel.LEVEL_21 || (tutorialRedo && level == RoomLevel.LEVEL_22);
            bool isLevel31Tutorial = level == RoomLevel.LEVEL_31 || (tutorialRedo && level == RoomLevel.LEVEL_32);

            if (!isLevel21Tutorial && !isLevel31Tutorial) return;

            if (index == 2)
            {
                var rightSpot = tilesController.FindOneRightAnswerSpot();
                highlight.MaskTransform(rightSpot.transform, false);
            }
            else if (index == 3)
            {
                tilesController.PrepareTutorial(vertical: false);
                var rightSpot = tilesController.FindOneRightAnswerSpot();
                highlight.MaskTransform(rightSpot.transform, false);
            }
            else if (index == 4 && isLevel31Tutorial)
            {
                tilesController.PrepareTutorial(vertical: false, red: true);
                var rightSpot = tilesController.FindOneRightAnswerSpot();
                highlight.MaskTransform(rightSpot.transform, false);
            }
            else if (index == 5 && isLevel31Tutorial)
            {
                tilesController.PrepareTutorial(vertical: true, red: true);
            }

        }

        private void OnStartButtonPressed()
        {
            StartGame();
        }

        private void OnTutorialReDoButtonPressed()
        {
            tutorialRedo = true;

            BigElloSaysConfig[] previousLevel =
                level == RoomLevel.LEVEL_12 ? _tutorialLevel11 :
                level == RoomLevel.LEVEL_22 ? _tutorialLevel21 :
                level == RoomLevel.LEVEL_32 ? _tutorialLevel31 : null;

            if (level >= RoomLevel.LEVEL_21)
                tilesController.PrepareTutorial(vertical: true);

            messageSystem.SetCurrentMessagesBatch(previousLevel);
            messageSystem.messageBatchFinished.AddListener(OnReDoBatchFinished);
            messageSystem.ShowSpecificRangeOfMessages(1, previousLevel.Length - 1);
        }

        private void OnReDoBatchFinished()
        {
            tutorialRedo = false;

            messageSystem.messageBatchFinished.RemoveListener(OnReDoBatchFinished);

            BigElloSaysConfig[] currentLevel =
                level == RoomLevel.LEVEL_12 ? _tutorialLevel12 :
                level == RoomLevel.LEVEL_22 ? _tutorialLevel22 :
                level == RoomLevel.LEVEL_32 ? _tutorialLevel32 : null;

            messageSystem.ShowMessage(currentLevel[^1]);
        }
    }
}