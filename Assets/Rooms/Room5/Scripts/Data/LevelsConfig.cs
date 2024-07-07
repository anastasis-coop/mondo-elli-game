using Common;
using UnityEngine;

namespace Room5
{
    [CreateAssetMenu(fileName = "Level Configs", menuName = "Room 5/Level Configs")]
    public class LevelsConfig : ScriptableObject
    {
        public enum ShelfConfig { Single, Double }
        
        [field: SerializeField] 
        public RoomLevel Level { get; private set; }
        
        [field: SerializeField]
        public ItemsSet ItemsSet { get; private set; }

        [field: SerializeField]
        public ShelfConfig ShelfConfiguration { get; private set; }

        [field: SerializeField, Min(0)]
        public int ActiveLights { get; private set; }

        [field: SerializeField, Min(0)]
        public int OnOffLightsPeriod { get; private set; }
        
        [field: Space]
        [field: SerializeField]
        public bool ShowTimer { get; private set; }
    }
}
