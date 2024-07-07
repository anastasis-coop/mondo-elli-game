using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Room5
{
    public class LevelController : MonoBehaviour
    {
        [System.Serializable]
        public class LevelConfigAndTutorial
        {
            [field: SerializeField]
            public RoomLevel Level { get; private set; }

            [field: SerializeField]
            public LevelsConfig Config { get; private set; }
            
            public BigElloSaysConfig[] tutorial;
        }

        public LevelConfigAndTutorial[] Configs;

        public MessageSystem MessageSystem;

        public GameObject SecondShelf;
        public LampsCableController LampsController;
        
        public CreateSelectionFeedback createSelectionFeedback;
        public RoomLevel level;

        private Dictionary<RoomLevel, LevelConfigAndTutorial> _configs;

        private Dictionary<RoomLevel, LevelConfigAndTutorial> ConfigsDict => _configs ??= Configs.ToDictionary(c => c.Level, c => c);

        public bool ShowingTutorial { get; private set; }

        private void Start()
        {
#if UNITY_EDITOR
            if (!GameState.StartedFromThisScene)
#endif
                level = GameState.Instance.levelBackend.roomLevel;

            var config = ConfigsDict[level].Config;
            var tutorial = ConfigsDict[level].tutorial;
            
            createSelectionFeedback.Config = config;
            SecondShelf.SetActive(config.ShelfConfiguration == LevelsConfig.ShelfConfig.Double);
            LampsController.gameObject.SetActive(!SecondShelf.activeSelf && config.ActiveLights > 0);

            createSelectionFeedback.BigElloPresenting = true;


            createSelectionFeedback.gameObject.SetActive(true);

            MessageSystem.SetCurrentMessagesBatch(tutorial);
            MessageSystem.messageRead.AddListener(OnTutorialMessageRead);
            MessageSystem.messageShown.AddListener(OnTutorialMessageShown);
            MessageSystem.greenButtonClicked.AddListener(OnStartButtonPressed);
            MessageSystem.ShowNextMessage();
        }

        private void OnTutorialMessageRead(int index)
        {
            MessageSystem.ShowNextMessage();
        }

        private void OnTutorialMessageShown(int index)
        {
        }

        private void OnStartButtonPressed()
        {
            MessageSystem.messageRead.RemoveListener(OnTutorialMessageRead);
            MessageSystem.greenButtonClicked.RemoveListener(OnStartButtonPressed);

            createSelectionFeedback.BigElloTalked();
        }
    }
}