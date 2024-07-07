using UnityEngine;

namespace Room3
{
    public class MultiGameObject : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] _objects;

        private void OnEnable()
        {
            foreach (var go in _objects)
                go.SetActive(true);
        }

        private void OnDisable()
        {
            foreach (var go in _objects)
                go.SetActive(false);
        }
    }
}
