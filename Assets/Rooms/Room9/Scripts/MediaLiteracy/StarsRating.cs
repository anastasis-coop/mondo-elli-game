using UnityEngine;

namespace Room9
{
    public class StarsRating : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] starHoles;

        [SerializeField]
        private GameObject[] stars;

        public void Init(int count, int max)
        {
            for (int i = 0; i < starHoles.Length; i++)
            {
                starHoles[i].SetActive(i < max);
                stars[i].SetActive(i < count);
            }
        }
    }
}