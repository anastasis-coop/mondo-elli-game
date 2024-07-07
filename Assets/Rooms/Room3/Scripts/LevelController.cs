using System.Collections.Generic;
using UnityEngine;

namespace Room3
{
    public class LevelController : MonoBehaviour
    {
        public RoomLevel level;
        public MessageSystem messageSystem;
        public AnimalsController animalsController;
        [SerializeField] private List<GameObject> _animalsGameObjects;
        public FindOppositeController findOppositeController;
        [SerializeField] private List<GameObject> _findOppositeGameObjects;
        public PathController pathController;
        [SerializeField] private List<GameObject> _pathGameObjects;
        public DifferencesController differencesController;
        [SerializeField] private List<GameObject> _differencesGameObjects;
        public Timer timer;
        public Score score;

        [SerializeField]
        private TimerBar timerBar;
        
        [SerializeField]
        private BigElloSaysConfig _endExerciseConfig;
        
        [SerializeField]
        private RoomEndPopup endPopup;

        
        [SerializeField]
        public BigElloSaysConfig[] Level01TutorialConfigs;
        [SerializeField]
        public BigElloSaysConfig[] Level02TutorialConfigs;
        [SerializeField]
        public BigElloSaysConfig[] Level11TutorialConfigs;
        [SerializeField]
        public BigElloSaysConfig[] Level12TutorialConfigs;
        [SerializeField]
        public BigElloSaysConfig[] Level21TutorialConfigs;
        [SerializeField]
        public BigElloSaysConfig[] Level22TutorialConfigs;
        [SerializeField]
        public BigElloSaysConfig[] Level31TutorialConfigs;
        [SerializeField]
        public BigElloSaysConfig[] Level32TutorialConfigs;

        void Start()
        {

#if UNITY_EDITOR
            if (!GameState.StartedFromThisScene)
#endif
                level = GameState.Instance.levelBackend.roomLevel;

            messageSystem.messageRead.AddListener(OnTutorialMessageRead);
            messageSystem.greenButtonClicked.AddListener(OnStartButtonClick);

            switch (level)
            {
                case RoomLevel.LEVEL_01:
                    messageSystem.SetCurrentMessagesBatch(Level01TutorialConfigs);
                    animalsController.gameObject.SetActive(true);
                    animalsController.prepareGame(false);
                    break;
                case RoomLevel.LEVEL_02:
                    messageSystem.SetCurrentMessagesBatch(Level02TutorialConfigs);
                    animalsController.gameObject.SetActive(true);
                    animalsController.prepareGame(true);
                    break;
                case RoomLevel.LEVEL_11:
                    messageSystem.SetCurrentMessagesBatch(Level11TutorialConfigs);
                    break;
                case RoomLevel.LEVEL_12:
                    messageSystem.SetCurrentMessagesBatch(Level12TutorialConfigs);
                    messageSystem.redButtonClicked.AddListener(() =>
                        messageSystem.ShowSpecificRangeOfMessages(2, 5));
                    break;
                case RoomLevel.LEVEL_21:
                    messageSystem.SetCurrentMessagesBatch(Level21TutorialConfigs);
                    pathController.gameObject.SetActive(true);
                    pathController.prepareGame(true);
                    break;
                case RoomLevel.LEVEL_22:
                    messageSystem.SetCurrentMessagesBatch(Level22TutorialConfigs);
                    messageSystem.redButtonClicked.AddListener(() =>
                        messageSystem.ShowSpecificRangeOfMessages(2, 10));
                    pathController.gameObject.SetActive(true);
                    pathController.prepareGame(false);
                    break;
                case RoomLevel.LEVEL_31:
                    messageSystem.SetCurrentMessagesBatch(Level31TutorialConfigs);
                    break;
                case RoomLevel.LEVEL_32:
                    messageSystem.SetCurrentMessagesBatch(Level32TutorialConfigs);
                    messageSystem.redButtonClicked.AddListener(() =>
                        messageSystem.ShowSpecificRangeOfMessages(2, 6));
                    break;
            }

            messageSystem.ShowNextMessage();
        }

        private void EnableTimerBar()
        {
            timerBar.gameObject.SetActive(true);
        }

        public void BigElloTalked()
        {
            if (timer.itIsEnd) return;

            EnableTimerBar();
            timer.Reset();
            timer.SetActivation(true);
            SetupButtonsAccordingToLevel(level);
            switch (level)
            {
                case RoomLevel.LEVEL_01:
                case RoomLevel.LEVEL_02:
                    animalsController.startGame();
                    break;
                case RoomLevel.LEVEL_11:
                    findOppositeController.gameObject.SetActive(true);
                    findOppositeController.prepareGame(8, false);
                    findOppositeController.startGame();
                    break;
                case RoomLevel.LEVEL_12:
                    findOppositeController.gameObject.SetActive(true);
                    findOppositeController.prepareGame(5, true);
                    findOppositeController.startGame();
                    break;
                case RoomLevel.LEVEL_21:
                case RoomLevel.LEVEL_22:
                    pathController.startGame();
                    break;
                case RoomLevel.LEVEL_31:
                    differencesController.gameObject.SetActive(true);
                    differencesController.prepareGame(8);
                    differencesController.Roll();
                    break;
                case RoomLevel.LEVEL_32:
                    differencesController.gameObject.SetActive(true);
                    differencesController.prepareGame(5);
                    differencesController.Roll();
                    break;
            }
        }

        private void SetupButtonsAccordingToLevel(RoomLevel roomLevel)
        {
            foreach (GameObject button in _animalsGameObjects)
            {
                button.SetActive(roomLevel == RoomLevel.LEVEL_01 || roomLevel == RoomLevel.LEVEL_02);
            }

            foreach (GameObject button in _findOppositeGameObjects)
            {
                button.SetActive(roomLevel == RoomLevel.LEVEL_11 || roomLevel == RoomLevel.LEVEL_12);
            }

            foreach (GameObject button in _pathGameObjects)
            {
                button.SetActive(roomLevel == RoomLevel.LEVEL_21 || roomLevel == RoomLevel.LEVEL_22);
            }

            foreach (GameObject button in _differencesGameObjects)
            {
                button.SetActive(roomLevel == RoomLevel.LEVEL_31 || roomLevel == RoomLevel.LEVEL_32);
            }
        }

        public void EndExercise()
        {
            messageSystem.oneShotRead.AddListener(OnEndExerciseRead);
            messageSystem.ShowMessage(_endExerciseConfig);
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

        private void OnTutorialMessageRead(int _)
        {
            messageSystem.TryShowNextMessage();
        }

        private void OnStartButtonClick()
        {
            messageSystem.messageRead.RemoveListener(OnTutorialMessageRead);
            messageSystem.greenButtonClicked.RemoveListener(OnStartButtonClick);
            BigElloTalked();
        }

        private void OnEndExerciseRead()
        {
            messageSystem.oneShotRead.RemoveListener(OnEndExerciseRead);

            endPopup.Show(saveResultsAndExit);
        }
    }

}
