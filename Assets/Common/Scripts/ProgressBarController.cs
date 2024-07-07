using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class ProgressBarController : MonoBehaviour
    {
        [System.Serializable]
        private class ImageGroup
        {
            [SerializeField]
            private Image[] images;

            public bool Enabled
            {
                set
                {
                    foreach (Image image in images)
                        image.enabled = value;
                }
            }
        }

        [SerializeField]
        private Sprite[] areaIcons;

        [SerializeField]
        private Image[] roomIconImages;

        [SerializeField]
        private ImageGroup[] roomImages;

        [SerializeField]
        private Image[] codingImages;

        [SerializeField]
        private GameObject[] roomIconRoots;

        [SerializeField]
        private GameObject[] codingRoots;

        public void Init(int areaIndex, int roomCount, int codingCount)
        {
            foreach (Image image in roomIconImages)
                image.sprite = areaIcons[areaIndex];

            for (int i = roomIconRoots.Length - 1; i >= 0; i--) //Reverse because we start by removing the last room
                roomIconRoots[i].SetActive(i < roomCount);

            for (int i = 0; i < codingRoots.Length; i++)
                codingRoots[i].SetActive(i < codingCount);

            Fill(0, 0);
        }

        public void Fill(int codingIndex, float fillAmount)
        {
            for (int i = 0; i < roomImages.Length; i++)
            {
                roomImages[i].Enabled = i < codingIndex;
            }

            for (int i = 0; i < codingImages.Length; i++)
            {
                codingImages[i].fillAmount = i < codingIndex ? 1 : 0;
            }

            if (codingIndex < codingImages.Length)
            {
                codingImages[codingIndex].fillAmount = fillAmount;
            }
        }
    }
}
