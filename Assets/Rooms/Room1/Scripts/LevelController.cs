using System.Collections.Generic;
using System.Linq;
using Common;
using UnityEngine;

namespace Room1
{
    public class LevelController : MonoBehaviour
    {
        [System.Serializable]
        private class Entry
        {
            [field: SerializeField] public RoomLevel Level { get; private set; }
            [field: SerializeField] public ConveyorLevelConfig Config { get; private set; }
        }

        [SerializeField]
        private List<Entry> _configs;

        public Spawner spawner;

        public ConveyorRoom conveyorRoom;
        public Interferences interferences;

        public RoomLevel level;

        [Header("Tutorial")] 
        public MessageSystem messageSystem;
        
        public List<BigElloSaysConfig> TutorialMessages_0_1;
        public List<BigElloSaysConfig> TutorialMessages_0_2;
        public List<BigElloSaysConfig> TutorialMessages_1_1;
        public List<BigElloSaysConfig> TutorialMessages_2_1;
        public List<BigElloSaysConfig> TutorialMessages_3_1;

        [SerializeField]
        private TargetScreen tutorialScreen;

        [SerializeField]
        private BooleanMaterialSwapper lightMaterialSwapper;

        [SerializeField]
        private BooleanLightColorSwapper lightColorSwapper;

        [SerializeField]
        private ParticlesController tutorialInterference;

        private Dictionary<RoomLevel, ConveyorLevelConfig> _configsDict;

        private Dictionary<RoomLevel, ConveyorLevelConfig> Configs
            => _configsDict ??= _configs.ToDictionary(e => e.Level, e => e.Config);

        private void Start()
        {
#if UNITY_EDITOR
            if (!GameState.StartedFromThisScene)
#endif
                level = GameState.Instance.levelBackend.roomLevel;

            if (!Configs.TryGetValue(level, out var config))
            {
                Debug.LogError("No configurations for the level " + level + ". Fallback to " + _configs[0].Level);
                config = _configs[0].Config;
                return;
            }
            
            conveyorRoom.ItemsSet = config.ItemsSet;
            
            conveyorRoom.highlightChangeOfTarget = config.ItemChangeEmphasis;
            conveyorRoom.grayScale = config.GrayScale;
            conveyorRoom.changeAnimationTime = config.EmphasisAnimationTime;
            
            conveyorRoom.useSound = config.UseType2Distactions;
            conveyorRoom.trackSpeedMultiplier = config.ConveyorSpeedMultiplier;

            spawner.delay = config.ItemsSpawnPeriod;
            spawner.rightPercent = (int)(config.OddsOfRightItem * 100);
            spawner.addTargetsAfterHalf = config.EnableExtraItemScreen;
            spawner.freezeRotation = config.FreezeRotation;

            interferences.grayScale = config.GrayScale;
            interferences.canAccelerate = config.AllowBeltsSpeedChange;
            interferences.canDecelerate = config.AllowBeltsSpeedChange;
            interferences.canDoBubbles = config.UseType1Distactions;
            interferences.canDoSmoke = config.UseType1Distactions;
            interferences.canDoSparkles = config.UseType1Distactions;
            interferences.canDoLight = config.UseType2Distactions;
            interferences.canDoBouncingBalls = config.UseType2Distactions;
            interferences.canDoFlickering = config.UseType2Distactions;
            interferences.canDoRain = config.UseType3Distactions;
            interferences.canDoSnowing = config.UseType3Distactions;
            
            conveyorRoom.Prepare();

            ShowTutorial();
        }

        private void ShowTutorial()
        {
            var tutorial = level switch
            {
                RoomLevel.LEVEL_01 => TutorialMessages_0_1,
                RoomLevel.LEVEL_02 => TutorialMessages_0_2,
                RoomLevel.LEVEL_11 => TutorialMessages_1_1,
                RoomLevel.LEVEL_21 => TutorialMessages_2_1,
                RoomLevel.LEVEL_31 => TutorialMessages_3_1,
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
            if (index == 2)
            {
                tutorialScreen.PulseForTime(4, null);
            }
            else if (index == 3 && level < RoomLevel.LEVEL_11
                || index == 4 && level >= RoomLevel.LEVEL_11)
            {
                lightMaterialSwapper.Set(false);
                lightColorSwapper.Set(false);
            }
            else if (index == 5 && level >= RoomLevel.LEVEL_11)
            {
                tutorialInterference.StartParticles();
            }
        }

        private void TutorialMessageRead(int index)
        {
            if (index == 5 && level >= RoomLevel.LEVEL_11)
            {
                tutorialInterference.StopParticles();
            }

            messageSystem.ShowNextMessage();
        }

        private void StartButtonPressed()
        {
            messageSystem.greenButtonClicked.RemoveListener(StartButtonPressed);
            messageSystem.messageShown.RemoveListener(TutorialMessageShown);
            messageSystem.messageRead.RemoveListener(TutorialMessageRead);

            conveyorRoom.startMovingAfterInstructions();
        }
    }
}