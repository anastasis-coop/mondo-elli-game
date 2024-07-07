using Common;
using UnityEngine;

namespace Room1
{
    [CreateAssetMenu(fileName = "Conveyor Level Config", menuName = "Room 1/Conveyor Level Configs")]
    public class ConveyorLevelConfig : ScriptableObject
    {
        [field: Header("Common")]
        [field: SerializeField]
        public ItemsSet ItemsSet { get; private set; }
        
        [field: SerializeField]
        public bool ItemChangeEmphasis { get; private set; }
        
        [field: SerializeField, Min(0)]
        public float EmphasisAnimationTime { get; private set; }
        
        [field: SerializeField, Min(0)]
        public float ItemsSpawnPeriod { get; private set; }
        
        [field: SerializeField, Range(0, 1)]
        public float OddsOfRightItem { get; private set; }
        
        [field: SerializeField]
        public bool EnableExtraItemScreen { get; private set; }
        
        [field: SerializeField]
        public bool GrayScale { get; private set; }
        
        [field: SerializeField]
        public bool FreezeRotation { get; private set; }
        
        [field: SerializeField]
        public bool UseType1Distactions { get; private set; }
        
        [field: SerializeField]
        public bool UseType2Distactions { get; private set; }
        
        [field: SerializeField]
        public bool UseType3Distactions { get; private set; }
        
        [field: Header("Conveyor Belts")]
        [field: SerializeField, Min(1)] 
        public float ConveyorSpeedMultiplier { get; private set; } = 1;
        
        [field: SerializeField]
        public bool AllowBeltsToMove { get; private set; }
        
        [field: SerializeField]
        public bool AllowBeltsSpeedChange { get; private set; }
    }
}
