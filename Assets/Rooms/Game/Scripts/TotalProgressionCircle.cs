using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class TotalProgressionCircle : MonoBehaviour
    {
        [SerializeField]
        private Image currentIcon;

        [SerializeField]
        private Image[] otherIcons;

        [SerializeField]
        private Image currentFill;

        [SerializeField]
        private Image[] otherFills;

        [Serializable]
        private class IslandConfig
        {
            public Island Island;
            public Sprite Sprite;
            public Color Color;
        }

        [SerializeField]
        private IslandConfig[] islandSprites;

        public void Init(Island current, List<Island> completed, bool mediaLiteracy, float progress)
        {
            int otherCount = 0;

            foreach (IslandConfig config in islandSprites)
            {
                if (config.Island == Island.INTRODUZIONE && mediaLiteracy && current != Island.INTRODUZIONE) continue;
                if (config.Island == Island.MEDIA_LITERACY && (!mediaLiteracy || current == Island.INTRODUZIONE)) continue;
                
                bool isCompleted = completed.Contains(config.Island);

                if (config.Island == current)
                {
                    currentIcon.sprite = config.Sprite;

                    float scale = isCompleted ? 1 : progress;

                    currentFill.transform.localScale = Vector3.one * scale;
                    currentFill.color = config.Color;
                }
                else
                {
                    otherIcons[otherCount].sprite = config.Sprite;
                    otherFills[otherCount].color = isCompleted ? config.Color : Color.white;

                    otherCount++;
                }
            }
        }
    }
}
