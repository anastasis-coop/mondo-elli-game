using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;

namespace Room1
{
    public class LevelControllerforLevel1232 : MonoBehaviour
    {
        [System.Serializable]
        private class Entry
        {
            [field: SerializeField] public RoomLevel Level { get; private set; }
            [field: SerializeField] public ConveyorLevelConfig Config { get; private set; }
        }

        [SerializeField]
        private List<Entry> _configs;

        public ConveyorRoomforLevel1232 conveyorRoom;
        public InterferenceforLevel1232 interference;
        public SpawnerforLevel1232 spawner;

        public RoomLevel level;

        [Header("Tutorial")]
        public MessageSystem messageSystem;
        
        public List<BigElloSaysConfig> TutorialMessages_1_2;
        public List<BigElloSaysConfig> TutorialMessages_2_2;
        public List<BigElloSaysConfig> TutorialMessages_3_2;

        [SerializeField]
        private BooleanMaterialSwapper lightMaterialSwapper;

        [SerializeField]
        private BooleanLightColorSwapper lightColorSwapper;

        [SerializeField]
        private ParticlesController tutorialInterference;

        private Dictionary<RoomLevel, ConveyorLevelConfig> _configsDict;

        private Dictionary<RoomLevel, ConveyorLevelConfig> Configs
            => _configsDict ??= _configs.ToDictionary(e => e.Level, e => e.Config);

        void Start()
        {
#if UNITY_EDITOR
            if (!GameState.StartedFromThisScene)
#endif
                level = GameState.Instance.levelBackend.roomLevel;
            
            if (!Configs.TryGetValue(level, out var config))
            {
                Debug.LogError("No configurations for the level " + level);
                return;
            }
            
            conveyorRoom.grayScale = config.GrayScale;
            conveyorRoom.ItemsSet = config.ItemsSet;

            spawner.levelIs12 = level == RoomLevel.LEVEL_12;
            spawner.addTargetAfterHalf = config.EnableExtraItemScreen;
            interference.canDoBubbles = config.UseType1Distactions;
            interference.canDoSmoke = config.UseType1Distactions;
            interference.canDoSparkles = config.UseType1Distactions;
            interference.canDoLight = config.UseType2Distactions;
            interference.canDoBouncingBalls = config.UseType2Distactions;
            interference.canDoRain = config.UseType3Distactions;
            interference.canDoSnowing = config.UseType3Distactions;

            conveyorRoom.Prepare();

            ShowTutorial();
        }

        private void ShowTutorial()
        {
            var tutorial = level switch
            {
                RoomLevel.LEVEL_12 => TutorialMessages_1_2,
                RoomLevel.LEVEL_22 => TutorialMessages_2_2,
                RoomLevel.LEVEL_32 => TutorialMessages_3_2,
                _ => null
            };

            messageSystem.greenButtonClicked.AddListener(StartButtonPressed);
            messageSystem.messageShown.AddListener(TutorialMessageShown);
            messageSystem.messageRead.AddListener(TutorialMessageRead);
            messageSystem.SetCurrentMessagesBatch(tutorial);
            messageSystem.ShowNextMessage();
        }

        private void TutorialMessageShown(int index)
        {
            if ((index == 2 && level > RoomLevel.LEVEL_12)
                || (index == 3 && level == RoomLevel.LEVEL_12))
            {
                lightMaterialSwapper.Set(false);
                lightColorSwapper.Set(false);
            }
            else if (index == 3 && level > RoomLevel.LEVEL_12
                || (index == 4 && level == RoomLevel.LEVEL_12))
            {
                tutorialInterference.StartParticles();
            }
        }

        private void TutorialMessageRead(int index)
        {
            messageSystem.ShowNextMessage();
        }

        private void StartButtonPressed()
        {
            messageSystem.greenButtonClicked.RemoveListener(StartButtonPressed);
            messageSystem.messageShown.RemoveListener(TutorialMessageShown);
            messageSystem.messageRead.RemoveListener(TutorialMessageRead);

            conveyorRoom.startAfterInstructions();
        }
    }
}